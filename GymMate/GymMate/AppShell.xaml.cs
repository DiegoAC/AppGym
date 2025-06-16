namespace GymMate
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("login", typeof(Views.LoginPage));
            Routing.RegisterRoute("register", typeof(Views.RegisterPage));
            Routing.RegisterRoute("resttimer", typeof(Views.RestTimerPage));
            Routing.RegisterRoute("routines", typeof(Views.RoutinesPage));
            Routing.RegisterRoute("routineDetail", typeof(Views.RoutineDetailPage));
            Routing.RegisterRoute("sessions", typeof(Views.SessionsPage));
            Routing.RegisterRoute("sessionEditor", typeof(Views.SessionEditorPage));
            Routing.RegisterRoute("sessionDetail", typeof(Views.SessionDetailPage));
            Routing.RegisterRoute("classes", typeof(Views.ClassesPage));
            Routing.RegisterRoute("classDetail", typeof(Views.ClassDetailPage));
            Routing.RegisterRoute("photos", typeof(Views.PhotosPage));
            Routing.RegisterRoute("photoDetail", typeof(Views.PhotoDetailPage));
            Routing.RegisterRoute("progress", typeof(Views.ProgressPage));
            Routing.RegisterRoute("feed", typeof(Views.FeedPage));
            Routing.RegisterRoute("comments", typeof(Views.CommentsPage));
            Routing.RegisterRoute("userSearch", typeof(Views.UserSearchPage));
            Routing.RegisterRoute("profile", typeof(Views.ProfilePage));
            Routing.RegisterRoute("settings", typeof(Views.SettingsPage));
        }
    }
}
