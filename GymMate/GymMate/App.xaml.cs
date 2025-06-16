using Plugin.Firebase.Crashlytics;
using System.Globalization;
using Microsoft.Maui.Storage;
using System.Linq;

namespace GymMate
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            var code = Preferences.Get("LanguageCode", null) ?? CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var dict = Application.Current.Resources.MergedDictionaries
                .First(md => md.Keys.Contains(code));
            Application.Current.Resources.MergedDictionaries.Remove(dict);
            Application.Current.Resources.MergedDictionaries.Add(dict);
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