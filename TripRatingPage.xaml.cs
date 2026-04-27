using PoputkaKGAMT.ViewModel;

namespace PoputkaKGAMT;

public partial class TripRatingPage : ContentPage
{
	public TripRatingPage()
	{
		InitializeComponent();
        BindingContext = new TripRating_ViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is TripRating_ViewModel vm)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await vm.LoadTravelersCommand.ExecuteAsync(null);

                }
                catch (Exception ex)
                {

                    await Shell.Current.DisplayAlertAsync("Ошибка загрузки", ex.Message, "OK");
                }
            });
        }
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