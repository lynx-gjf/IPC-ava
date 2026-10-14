using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using IPC.ViewModels;
using IPC.Views;

namespace IPC;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
<<<<<<< HEAD
<<<<<<< HEAD
=======
#if DEBUG
        this.AttachDeveloperTools();
#endif
>>>>>>> b4523ab5dc8a5e1747eefa24d2edd3b11a29e22a
=======
#if DEBUG
        this.AttachDeveloperTools();
#endif
>>>>>>> b4523ab (Initial commit)
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}