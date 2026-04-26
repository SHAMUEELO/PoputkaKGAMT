using PoputkaKGAMT.ViewModel;

namespace PoputkaKGAMT;

public partial class TripRatingPage : ContentPage
{
	public TripRatingPage()
	{
		InitializeComponent();
        BindingContext = new TripRating_ViewModel();
    }
    protected override bool OnBackButtonPressed()
    {
        if (BindingContext is TripRating_ViewModel vm)
        {
            vm.GoBackTripDetails();
        }
        return true;
    }
}