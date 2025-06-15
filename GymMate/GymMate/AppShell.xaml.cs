namespace GymMate
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("resttimer", typeof(Views.RestTimerPage));
        }
    }
}
