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
            Routing.RegisterRoute("sessions", typeof(Views.SessionsPage));
            Routing.RegisterRoute("sessionEditor", typeof(Views.SessionEditorPage));
            Routing.RegisterRoute("sessionDetail", typeof(Views.SessionDetailPage));
            Routing.RegisterRoute("progress", typeof(Views.ProgressPage));
        }
    }
}
