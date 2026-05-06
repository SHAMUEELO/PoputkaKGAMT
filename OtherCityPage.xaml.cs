using PoputkaKGAMT.ViewModel;

namespace PoputkaKGAMT;

public partial class OtherCityPage : ContentPage
{
	public OtherCityPage()
	{
		InitializeComponent();
        BindingContext = new OtherCity_ViewModel();
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
        if (BindingContext is OtherCity_ViewModel vm)
        {
            vm.GoBack();
        }
        return true;
    }
}