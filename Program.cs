using Avalonia;
using System;

namespace IPC;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
<<<<<<< HEAD
<<<<<<< HEAD
            .WithDeveloperTools()
=======
>>>>>>> b4523ab5dc8a5e1747eefa24d2edd3b11a29e22a
=======
>>>>>>> b4523ab (Initial commit)
            .LogToTrace();
}
