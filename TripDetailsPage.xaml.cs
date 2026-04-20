using PoputkaKGAMT.ViewModel;

namespace PoputkaKGAMT;

public partial class TripDetailsPage : ContentPage
{
	public TripDetailsPage()
	{
		InitializeComponent();
		BindingContext = new TripDetails_ViewModel();

    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is TripDetails_ViewModel vm)
        {
            vm.LoadTripCommand.Execute(null);
        }
    }

    protected override bool OnBackButtonPressed()
    {
        if (BindingContext is TripDetails_ViewModel vm)
        {
            vm.GoBackSearchResult();
        }
        return true;
    }
}