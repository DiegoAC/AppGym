namespace GymMate
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("resttimer", typeof(Views.RestTimerPage));
            Routing.RegisterRoute("routines", typeof(Views.RoutinesPage));
            Routing.RegisterRoute("routineDetail", typeof(Views.RoutineDetailPage));
        }
    }
}
