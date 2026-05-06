using PoputkaKGAMT.ViewModel;

namespace PoputkaKGAMT;

public partial class NaberezhnyeChelnyPage : ContentPage
{
	public NaberezhnyeChelnyPage()
	{
		InitializeComponent();
        BindingContext = new NaberezhnyeChelny_VIewModel();
    }

    private async void OnPlaceTapped(object sender, TappedEventArgs e)
    {
        if (sender is Label label)
        {
            Preferences.Set("SelectedPlace", label.Text); // сохран€ем место

            string previousPage = Preferences.Get("PreviousPage", "");
            // ≈сли ссылка пуста, то по умолчанию SearchPage
            if (string.IsNullOrEmpty(previousPage))
                previousPage = "SearchPage";

            await Shell.Current.GoToAsync($"//{previousPage}");
        }
    }
    protected override bool OnBackButtonPressed()
    {
        if (BindingContext is NaberezhnyeChelny_VIewModel vm)
        {
            vm.GoBack();
        }
        return true;
    }
}