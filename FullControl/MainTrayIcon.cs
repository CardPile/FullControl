using Parser.Events;
using Parser;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FullControl;
internal class MainTrayIcon : ApplicationContext
{
    internal MainTrayIcon()
    {
        trayIcon = new NotifyIcon()
        {
            Icon = Properties.Resources.AppIcon,
            ContextMenuStrip = new ContextMenuStrip()
            {
                Items = { new ToolStripMenuItem("Exit", null, Exit) }
            },
            Text = "Full control",
            Visible = true
        };

        watcher = new LogFileWatcher(GetPlayerLogLocation(), false);

        dispatcher = new MatcherDispatcher();
        dispatcher.Connect(watcher);
        dispatcher.AddMatcher<GreToClientEventMatcher>().FullControlChangeEvent += FullControlChangeEvent;

        pollTimer = new System.Threading.Timer(Poll, null, 0, 500);
    }

    private void Exit(object? sender, EventArgs e)
    {
        pollTimer.Change(Timeout.Infinite, Timeout.Infinite);
        trayIcon.Visible = false;
        Application.Exit();
    }

    private void Poll(object? state)
    {
        watcher.Poll();

        if (tick == holdFullControlTick + HOLD_FULL_CONTROL_DELTA)
        {
            if (!holdFullControl)
            {
                Debug.WriteLine("Gonna send full control keys");
                SendFullControlKeysToArena();
            }
            else
            {
                Debug.WriteLine("Not sending full control keys");
            }
        }

        ++tick;
    }

    private void FullControlChangeEvent(object? sender, HoldFullControlChangeEvent e)
    {
        holdFullControl = e.HoldFullControl;
        holdFullControlTick = tick;
    }

    private void SendFullControlKeysToArena()
    {
        Process? mtgaProcess = Process.GetProcessesByName("MTGA").FirstOrDefault();
        if (mtgaProcess == null)
        {
            return;
        }

        SetForegroundWindow(mtgaProcess.MainWindowHandle);

        SendKeys.SendWait("^+");

        Debug.WriteLine("Sent full control keys");
    }

    private string GetPlayerLogLocation()
    {
        if (OperatingSystem.IsMacOS())
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (string.IsNullOrEmpty(userProfile))
            {
                throw new InvalidOperationException("UserProfile is null. Cannot resolve path to Player.log");
            }
            return Path.Combine(userProfile, "Library", "Logs", "com.wizards.mtga", "Wizards Of The Coast", "MTGA", "Player.log");
        }
        else
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (string.IsNullOrEmpty(localAppData))
            {
                throw new InvalidOperationException("LocalAppData is null. Cannot resolve path to Player.log");
            }
            localAppData = localAppData.Replace("Roaming", "LocalLow");  // There is a beter way to do it, but this is simpler
            return Path.Combine(localAppData, "Wizards Of The Coast", "MTGA", "Player.log");
        }
    }

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

    [DllImport("User32.dll")]
    private static extern int SetForegroundWindow(IntPtr point);

    private NotifyIcon trayIcon;

    private LogFileWatcher watcher;
    private MatcherDispatcher dispatcher;

    private System.Threading.Timer pollTimer;

    private int tick = 0;

    private bool holdFullControl = false;
    private int holdFullControlTick = 0;

    private static int HOLD_FULL_CONTROL_DELTA = 2;
}
