using PoputkaKGAMT.ViewModel;

namespace PoputkaKGAMT;

public partial class FellowTravelerPage : ContentPage
{
	public FellowTravelerPage()
	{
		InitializeComponent();
		BindingContext = new FellowTraveler_ViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is FellowTraveler_ViewModel vm)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await vm.LoadFellowTravelersCommand.ExecuteAsync(null);
                    
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
        if (BindingContext is FellowTraveler_ViewModel vm)
        {
            vm.GoBackTripsDetails();
        }
        return true;
    }
}