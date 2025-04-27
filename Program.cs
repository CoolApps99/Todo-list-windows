// Program.cs
using TodoListApp;

static class Program
{
    private static Mutex _mutex; // Keep reference to prevent GC

    [STAThread]
    static void Main()
    {
        _mutex = new Mutex(true, "{8F6F0AC4-B9A1-45FD-A8CF-72F04E6BDE8F}");

        if (_mutex.WaitOne(TimeSpan.Zero, true))
        {
            Application.Run(new MainForm());
            _mutex.ReleaseMutex();
        }
        else
        {
            MessageBox.Show("Application already running");
        }
    }
}
