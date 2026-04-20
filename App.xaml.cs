using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Threading;

namespace PoputkaKGAMT
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            UserAppTheme = AppTheme.Light;

            CultureInfo culture = new CultureInfo("ru-RU");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;


        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

       public static bool Parameters { get; set; } = false;
       public static bool IsPassenger { get; set; } = false;
   

    }
}