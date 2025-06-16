using Plugin.Firebase.Crashlytics;

namespace GymMate
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Application.Current.UserAppTheme = AppTheme.Unspecified;
            Application.Current.RequestedThemeChanged += (s, e) =>
            {
                MainPage = new AppShell();
            };
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var shell = new AppShell();
#if DEBUG
            var grid = new Grid();
            var crashBtn = new Button { IsVisible = false };
            crashBtn.Clicked += async (s, e) => await FirebaseCrashlytics.DefaultInstance.CrashAsync();
            grid.Children.Add(shell);
            grid.Children.Add(crashBtn);
            return new Window(grid);
#else
            return new Window(shell);
#endif
        }
    }
}