using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Plugin.Firebase;
using Plugin.LocalNotification;
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
                .UseLocalNotification()
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
            builder.Services.AddTransient<ViewModels.RestTimerViewModel>();
            builder.Services.AddTransient<Views.RestTimerPage>();
            builder.Services.AddTransient<ViewModels.RoutinesViewModel>();
            builder.Services.AddTransient<Views.RoutinesPage>();
            builder.Services.AddTransient<ViewModels.RoutineDetailViewModel>();
            builder.Services.AddTransient<Views.RoutineDetailPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
