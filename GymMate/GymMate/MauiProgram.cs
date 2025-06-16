using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Plugin.Firebase;
using Plugin.Firebase.CloudMessaging;
using Plugin.LocalNotification;
using Plugin.Firebase.Firestore;
using Microcharts.Maui;
using CommunityToolkit.Maui;
using GymMate.Services;
using GymMate.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Networking;

namespace GymMate
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseFirebaseApp()
                .UseFirebaseCloudMessaging()
                .UseLocalNotification()
                .UseMicrocharts()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Inter-Regular.ttf", "Inter");
                });

            builder.ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<CollectionView, CollectionViewHandler2>();
                handlers.AddHandler<CarouselView, CarouselViewHandler2>();
            });

            builder.Services.AddSingleton<IFirebaseFirestore>(_ => CrossFirebaseFirestore.Current);
            builder.Services.AddSingleton<INotificationService, NotificationService>();
            builder.Services.AddSingleton<IFirebaseAuthService, FirebaseAuthService>();
            builder.Services.AddSingleton<IRealtimeDbService, RealtimeDbService>();
            builder.Services.AddSingleton<IRoutinesService, RoutinesService>();
            builder.Services.AddSingleton<ISessionsService, SessionsService>();
            builder.Services.AddSingleton<IClassBookingService, ClassBookingService>();
            builder.Services.AddSingleton<IProgressPhotoService, ProgressPhotoService>();
            builder.Services.AddSingleton<IFollowService, FollowService>();
            builder.Services.AddSingleton<LocalDbService>();
            builder.Services.AddSingleton<IFeedService, FeedService>();
            builder.Services.AddTransient<ViewModels.RestTimerViewModel>();
            builder.Services.AddTransient<Views.RestTimerPage>();
            builder.Services.AddTransient<ViewModels.RoutinesViewModel>();
            builder.Services.AddTransient<Views.RoutinesPage>();
            builder.Services.AddTransient<ViewModels.RoutineDetailViewModel>();
            builder.Services.AddTransient<Views.RoutineDetailPage>();
            builder.Services.AddTransient<ViewModels.SessionsViewModel>();
            builder.Services.AddTransient<Views.SessionsPage>();
            builder.Services.AddTransient<ViewModels.SessionEditorViewModel>();
            builder.Services.AddTransient<Views.SessionEditorPage>();
            builder.Services.AddTransient<ViewModels.SessionDetailViewModel>();
            builder.Services.AddTransient<Views.SessionDetailPage>();
            builder.Services.AddTransient<ViewModels.ClassesViewModel>();
            builder.Services.AddTransient<Views.ClassesPage>();
            builder.Services.AddTransient<ViewModels.ClassDetailViewModel>();
            builder.Services.AddTransient<Views.ClassDetailPage>();
            builder.Services.AddTransient<ViewModels.PhotosViewModel>();
            builder.Services.AddTransient<Views.PhotosPage>();
            builder.Services.AddTransient<ViewModels.PhotoDetailViewModel>();
            builder.Services.AddTransient<Views.PhotoDetailPage>();
            builder.Services.AddTransient<ViewModels.FeedViewModel>();
            builder.Services.AddTransient<Views.FeedPage>();
            builder.Services.AddTransient<ViewModels.UserSearchViewModel>();
            builder.Services.AddTransient<Views.UserSearchPage>();
            builder.Services.AddTransient<ViewModels.ProfileViewModel>();
            builder.Services.AddTransient<Views.ProfilePage>();
            builder.Services.AddTransient<ViewModels.CommentsViewModel>();
            builder.Services.AddTransient<Views.CommentsPage>();
            builder.Services.AddTransient<ViewModels.ProgressViewModel>();
            builder.Services.AddTransient<Views.ProgressPage>();
            builder.Services.AddTransient<ViewModels.SettingsViewModel>();
            builder.Services.AddTransient<Views.SettingsPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            var app = builder.Build();
            var localDb = app.Services.GetRequiredService<LocalDbService>();
            localDb.InitAsync().GetAwaiter().GetResult();
            var routines = app.Services.GetRequiredService<IRoutinesService>();
            var sessionsService = app.Services.GetRequiredService<ISessionsService>();
            Connectivity.ConnectivityChanged += async (s, e) =>
            {
                if (e.NetworkAccess == NetworkAccess.Internet)
                {
                    var uid = app.Services.GetRequiredService<IFirebaseAuthService>().CurrentUserUid;
                    if (!string.IsNullOrEmpty(uid))
                    {
                        await routines.SyncPendingAsync(uid);
                        await sessionsService.SyncPendingAsync(uid);
                    }
                }
            };
            return app;
        }
    }
}
