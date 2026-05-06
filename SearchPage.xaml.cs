using PoputkaKGAMT.ViewModel;

namespace PoputkaKGAMT;

public partial class SearchPage : ContentPage
{
	public SearchPage()
	{
		InitializeComponent();
		BindingContext = new Search_ViewModel();
	}

    // јвтоматичкеска€ загрузка при октрытии старницы
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is Search_ViewModel vm)
        {
            var place = Preferences.Get("SelectedPlace", "");

            if (!string.IsNullOrWhiteSpace(place))
            {
                var target = Preferences.Get("PlaceTarget", "");

                if (target == "Departure")
                    vm.DeparturePlaceSearchPage = place;
                else if (target == "Arrive")
                    vm.ArrivePlaceSearchPage = place;

                Preferences.Remove("SelectedPlace");
                Preferences.Remove("PlaceTarget");
            }

            vm.LoadPlace();

        }
    }
}