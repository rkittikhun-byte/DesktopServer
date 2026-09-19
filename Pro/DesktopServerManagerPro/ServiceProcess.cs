using System.Diagnostics;
using System.Net.NetworkInformation;

namespace DesktopServerManagerPro;

// Tells this installation's own processes apart from everyone else's, and answers
// whether a service actually came up.
//
// All of this used to go by process name alone. On a machine that already runs another
// DesktopServer - which is the normal case, since people install an edition next to the
// one they already have - that meant three separate faults:
//
//   * the manager showed "Running" for an Apache it had never started, because some
//     other httpd.exe existed;
//   * Stop would have killed that other server, including a live site;
//   * a start that failed because the port was taken still reported success, since
//     nothing checked after launching.
//
// Every question here is therefore scoped to processes whose executable actually lives
// under this installation's folder.
internal static class ServiceProcess
{
    private static string Prefix(string root)
    {
        try { return Path.GetFullPath(root).TrimEnd('\\') + "\\"; }
        catch { return root.TrimEnd('\\') + "\\"; }
    }

    private static string? PathOf(Process p)
    {
        // Throws for processes owned by another user, and for a 32-bit view of a 64-bit
        // process. Unknown means "not ours", which is the safe answer for both callers:
        // we neither claim it is running nor kill it.
        try { return p.MainModule?.FileName; }
        catch { return null; }
    }

    public static List<Process> Owned(string root, string processName)
    {
        string prefix = Prefix(root);
        var mine = new List<Process>();

        foreach (var p in Process.GetProcessesByName(processName))
        {
            string? path = PathOf(p);
            if (path != null && path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) mine.Add(p);
            else p.Dispose();
        }
        return mine;
    }

    public static bool IsRunning(string root, string processName)
    {
        var mine = Owned(root, processName);
        foreach (var p in mine) p.Dispose();
        return mine.Count > 0;
    }

    public static void StopOwned(string root, string processName, Action<string>? log = null)
    {
        var mine = Owned(root, processName);
        if (mine.Count == 0)
        {
            log?.Invoke($"No {processName} from this installation is running.");
            return;
        }

        foreach (var p in mine)
        {
            try
            {
                int pid = p.Id;
                p.Kill(true);            // the whole tree: httpd and mysqld both fork children
                p.WaitForExit(5000);
                log?.Invoke($"{processName} stopped (pid {pid}).");
            }
            catch (Exception ex) { log?.Invoke($"Could not stop {processName}: {ex.Message}"); }
            finally { p.Dispose(); }
        }
    }

    public static bool IsPortInUse(int port)
    {
        try
        {
            foreach (var ep in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners())
                if (ep.Port == port) return true;
        }
        catch { }
        return false;
    }

    // When the port is busy and it is not us, say what is on it so the log can name
    // something the person can actually go and close.
    public static string? ForeignPortHolder(int port, string root, params string[] likelyNames)
    {
        if (!IsPortInUse(port)) return null;

        string prefix = Prefix(root);
        string? foreign = null;
        bool ours = false;

        foreach (var name in likelyNames)
        {
            foreach (var p in Process.GetProcessesByName(name))
            {
                string? path = PathOf(p);
                p.Dispose();
                if (path == null) continue;

                if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) ours = true;
                else foreign ??= path;
            }
        }

        if (foreign != null) return foreign;

        // Our own service already has the port. That is not a conflict to report - the
        // caller's "is it running" check covers it - so do not invent a stranger.
        return ours ? null : "another program";
    }

    // Null when the service is up and listening; otherwise a sentence worth showing.
    public static async Task<string?> ConfirmStarted(
        string root, string processName, int port, int timeoutMs = 10000)
    {
        DateTime deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        bool everAlive = false;

        while (DateTime.UtcNow < deadline)
        {
            bool alive = IsRunning(root, processName);
            everAlive |= alive;

            if (alive && IsPortInUse(port)) return null;
            if (!alive && everAlive) break;     // it launched and then gave up
            await Task.Delay(250);
        }

        if (!IsRunning(root, processName))
            return everAlive
                ? $"{processName} started and then stopped - check its log."
                : $"{processName} did not start.";

        return $"{processName} is running but nothing is listening on port {port}.";
    }
}
