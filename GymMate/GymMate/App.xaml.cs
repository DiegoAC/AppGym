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
            return new Window(new AppShell());
        }
    }
}