namespace DesktopServerSetupGo
{
    // Checks an installation folder before anything has been written to it.
    //
    // The rules come from what PHP for Windows and the uninstaller actually do. Both were
    // reproduced against the bundled PHP builds rather than assumed.
    internal static class InstallPath
    {
        // Null when the folder is usable, otherwise the reason to show the person.
        public static string? Reject(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return "Please choose an installation folder.";

            // PHP for Windows opens its extension DLLs through the ANSI file APIs, which fails
            // for anything outside plain ASCII. A Thai or Cyrillic folder leaves php.ini
            // unreadable; an accented Latin one reads the ini but still loads no extension at
            // all, even where the system code page can represent the character. Either way the
            // install reports success and the stack then runs on PHP's built-in defaults, with
            // no curl, no mysqli and no opcache.
            foreach (char c in path)
            {
                if (c < ' ' || c > '~')
                    return "This folder name contains characters that PHP for Windows cannot open.\n\n"
                         + "PHP would load none of its extensions. Please choose a folder whose "
                         + "whole path uses plain English letters and digits.\n\n"
                         + @"Spaces are fine, for example C:\My Dev Server.";
            }

            string full;
            try
            {
                full = Path.GetFullPath(path);
            }
            catch
            {
                return "That is not a valid folder path. Please choose another one.";
            }

            if (full.StartsWith(@"\\"))
                return "Please choose a folder on a local drive. The servers are started from "
                     + "this folder and cannot be run from a network share.";

            // The uninstaller deletes the installation folder and everything under it. Handed a
            // drive root it would take the whole drive with it, so refuse that here rather than
            // let it be discovered at uninstall time.
            string root = Path.GetPathRoot(full) ?? "";
            if (string.Equals(full.TrimEnd('\\') + @"\", root, StringComparison.OrdinalIgnoreCase))
                return "Please install into a folder rather than the root of a drive.\n\n"
                     + "Uninstalling removes the installation folder and everything inside it, "
                     + "which at a drive root would mean the entire drive.";

            return null;
        }
    }
}
