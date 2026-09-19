using System.Text.RegularExpressions;

namespace DesktopServerSetupGo
{
    // Turns a stock php.ini into a development one by editing each setting where it already
    // lives. An earlier version appended its settings as a block at the end of the file. PHP
    // honours the last definition, so the stock "memory_limit = 128M" a developer finds and
    // edits had no effect, and the file read as if nothing had been configured at all.
    //
    // Lines are matched up to (?<eol>...) rather than "$": in .NET "$" only matches before
    // "\n", and php.ini ships with CRLF, so a pattern ending in "$" never matches a line.
    internal static class PhpIniPatcher
    {
        private const RegexOptions Lines = RegexOptions.Multiline | RegexOptions.IgnoreCase;
        private const string OwnBlock = "; --- DesktopServer: added settings ---";

        // Only the ones whose DLL is really in ext/ get enabled, so one list covers PHP 5.6
        // through 8.4: 7.4 calls GD "gd2" where 8.x calls it "gd", 7.4 has no php_zip.dll,
        // and only 5.x still carries the old "mysql" API that legacy projects are written for.
        // mbstring stays ahead of exif - before PHP 7.4 exif will not load without it.
        private static readonly string[] WantedExtensions = {
            "bz2", "curl", "fileinfo", "ftp", "gd", "gd2", "gettext", "gmp", "intl",
            "mbstring", "exif", "mysql", "mysqli", "openssl", "pdo_mysql", "pdo_pgsql",
            "pdo_sqlite", "pgsql", "soap", "sockets", "sodium", "sqlite3", "xsl", "zip"
        };

        // "overrides" are applied after the developer-mode defaults: an edition passes the
        // values it wants different, and any settings of its own.
        public static string Apply(string ini, string phpDir, params (string Key, string Value)[] overrides)
        {
            string extDir = Path.Combine(phpDir, "ext");

            // Drop the block older builds appended, or its values would still win.
            ini = Regex.Replace(ini, @"(?s)\r?\n; --- DesktopServer: .*$", "");

            // Absolute, because the stock relative "ext" resolves against Apache's working
            // directory rather than the PHP folder, and then no extension loads at all.
            ini = Set(ini, "extension_dir", $"\"{extDir.Replace("\\", "/")}\"");

            foreach (var ext in WantedExtensions)
            {
                if (File.Exists(Path.Combine(extDir, $"php_{ext}.dll")))
                    ini = Enable(ini, "extension", ext);
            }

            // opcache is a Zend extension; loading it with extension= silently fails.
            bool hasOpcache = File.Exists(Path.Combine(extDir, "php_opcache.dll"));
            if (hasOpcache) ini = Enable(ini, "zend_extension", "opcache");

            // --- Developer mode ---
            (string Key, string Value)[] developerMode = {
                ("display_errors", "On"),
                ("display_startup_errors", "On"),
                ("error_reporting", "E_ALL"),
                ("log_errors", "On"),
                ("memory_limit", "512M"),
                ("upload_max_filesize", "2048M"),
                ("post_max_size", "2048M"),
                ("max_execution_time", "600"),
                ("max_input_time", "600"),
                ("date.timezone", "Asia/Bangkok"),
            };
            foreach (var (key, value) in developerMode) ini = Set(ini, key, value);

            if (hasOpcache)
            {
                ini = Set(ini, "opcache.enable", "1");
                ini = Set(ini, "opcache.enable_cli", "1");
                // Pick up edits on the next request instead of caching them for a
                // minute - the single most important opcache setting for development.
                ini = Set(ini, "opcache.validate_timestamps", "1");
                ini = Set(ini, "opcache.revalidate_freq", "0");
            }

            // cURL on Windows comes without a CA list, so every https:// call fails with "SSL
            // certificate problem" until it is given one. openssl.cafile is left alone on
            // purpose: unset, PHP's own streams verify against the Windows certificate store,
            // which also knows the root of a company proxy or an antivirus.
            string caBundle = Path.Combine(phpDir, "extras", "ssl", "cacert.pem");
            if (File.Exists(caBundle))
                ini = Set(ini, "curl.cainfo", $"\"{caBundle.Replace("\\", "/")}\"");

            foreach (var (key, value) in overrides) ini = Set(ini, key, value);

            return ini;
        }

        // Rewrites a setting where it already is - the live line if there is one, otherwise
        // the commented-out stock line - so the file ends up with exactly one definition of
        // it, in the place a developer will look for it.
        private static string Set(string ini, string key, string value)
        {
            string k = Regex.Escape(key);
            string line = $"{key} = {value}";

            var live = new Regex($@"^[ \t]*{k}[ \t]*=[^\r\n]*(?<eol>\r?\n|\z)", Lines);
            if (live.IsMatch(ini))
            {
                // Per-directory sections are the developer's own overrides; leave them be.
                Match scoped = Regex.Match(ini, @"^[ \t]*\[(PATH|HOST)=", Lines);
                int limit = scoped.Success ? scoped.Index : ini.Length;
                bool done = false;
                return live.Replace(ini, m =>
                {
                    if (m.Index >= limit) return m.Value;
                    if (done) return "";
                    done = true;
                    return line + m.Groups["eol"].Value;
                });
            }

            // A stock line has nothing between the ";" and the name, which tells it apart from
            // the indented examples in the file's own docs (session.save_path has two). Only
            // PHP 5.6 writes its stock lines with a space, so that spelling is the fallback.
            // The last match wins: ;extension_dir comes twice, Unix first, Windows second.
            Match? stock = null;
            foreach (string gap in new[] { "", @"[ \t]*" })
            {
                foreach (Match m in Regex.Matches(ini, $@"^[ \t]*;{gap}{k}[ \t]*=[^\r\n]*", Lines)) stock = m;
                if (stock != null) break;
            }
            if (stock != null)
                return ini.Substring(0, stock.Index) + line + ini.Substring(stock.Index + stock.Length);

            return AppendToOwnBlock(ini, line);
        }

        // Uncomments the stock line for an extension. That keeps the spelling this PHP
        // version expects - "php_curl.dll" before 7.2, plain "curl" from then on - and the
        // stock load order. No space is allowed after the ";": that is what separates a real
        // ";extension=mysqli" from the ";   extension=mysqli" example in the file's own docs.
        private static string Enable(string ini, string directive, string name)
        {
            var rx = new Regex(
                $@"^[ \t]*(?<off>;)?{directive}[ \t]*=[ \t]*(php_)?{Regex.Escape(name)}(\.dll)?[ \t]*(;[^\r\n]*)?(?<eol>\r?\n|\z)",
                Lines);
            bool found = false;
            ini = rx.Replace(ini, m =>
            {
                Group off = m.Groups["off"];
                if (!found)
                {
                    found = true;
                    return off.Success ? m.Value.Remove(off.Index - m.Index, 1) : m.Value;
                }
                // A second live copy makes PHP warn "Module already loaded" on every start.
                return off.Success ? m.Value : "";
            });
            if (found) return ini;

            // The template never mentions this one: put it after the last extension it does.
            // Without a modern-style line to go by, use the file name - every version loads that.
            bool modern = Regex.IsMatch(ini, @"^[ \t]*;?extension[ \t]*=[ \t]*(?!php_)\w+[ \t]*(;|\r?$)", Lines);
            string line = $"{directive}={(modern ? name : $"php_{name}.dll")}";

            Match? last = null;
            foreach (Match m in Regex.Matches(ini, @"^[ \t]*;?(zend_)?extension[ \t]*=[^\r\n]*(?<eol>\r?\n|\z)", Lines)) last = m;
            if (last == null) return AppendToOwnBlock(ini, line);

            string eol = last.Groups["eol"].Value;
            return eol.Length > 0
                ? ini.Insert(last.Index + last.Length, line + eol)
                : ini + "\r\n" + line;
        }

        // For anything the file has no line for at all. Apply() strips this block before it
        // starts, so running the installer again does not stack copies of it.
        private static string AppendToOwnBlock(string ini, string line)
        {
            if (!ini.Contains(OwnBlock))
                ini = ini.TrimEnd('\r', '\n') + "\r\n\r\n" + OwnBlock + "\r\n";
            return ini + line + "\r\n";
        }
    }
}
