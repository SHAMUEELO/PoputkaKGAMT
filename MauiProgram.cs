
using Microsoft.Extensions.Logging;
using PoputkaKGAMT.Services;
using PoputkaKGAMT.ViewModel;
using SimpleToolkit.SimpleShell;


namespace PoputkaKGAMT
{
    public interface IBackgroundUpdateService
    {
        void Start(int intervalSeconds = 10);
        void Stop();

        // Событие, чтобы viewModel-и узнавали об обновлении поездок
        event Action OnTripsUpdated;
    }

    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSimpleShell()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            

#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services
                .AddSingleton<IBackgroundUpdateService, BackgroundUpdateService>()
                .AddSingleton<TripService>()
                .AddSingleton<UserService>()
                .AddSingleton<PlaceService>()
                .AddSingleton<FellowTravelerService>();

            builder.Services.AddTransient<SearchResult_ViewModel>();
            builder.Services.AddTransient<TravelHistory_ViewModel>();

            return builder.Build();
        }
    }
}
