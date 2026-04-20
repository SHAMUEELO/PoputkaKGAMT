using PoputkaKGAMT.ViewModel;


namespace PoputkaKGAMT
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Dispatcher.Dispatch(async () =>
            {
                string userKey = Preferences.Get("CurrentUserKey", "");
                if (!string.IsNullOrEmpty(userKey))
                {
                    await Shell.Current.GoToAsync("//SearchPage");
                }
            });
        }

    }
}
