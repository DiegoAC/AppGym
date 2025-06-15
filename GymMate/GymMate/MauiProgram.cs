using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Plugin.Firebase;
using Plugin.Firebase.CloudMessaging;
using Plugin.LocalNotification;
using Microcharts.Maui;
using CommunityToolkit.Maui;
using GymMate.Services;

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
                });

            builder.ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<CollectionView, CollectionViewHandler2>();
                handlers.AddHandler<CarouselView, CarouselViewHandler2>();
            });

            builder.Services.AddSingleton<IFirebaseAuthService, FirebaseAuthService>();
            builder.Services.AddSingleton<IRealtimeDbService, RealtimeDbService>();
            builder.Services.AddSingleton<INotificationService, NotificationService>();
            builder.Services.AddSingleton<IClassBookingService, ClassBookingService>();
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
            builder.Services.AddTransient<ViewModels.ProgressViewModel>();
            builder.Services.AddTransient<Views.ProgressPage>();
            builder.Services.AddTransient<ViewModels.SettingsViewModel>();
            builder.Services.AddTransient<Views.SettingsPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
