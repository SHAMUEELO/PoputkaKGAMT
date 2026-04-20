using PoputkaKGAMT.ViewModel;

namespace PoputkaKGAMT;

public partial class MyTravelPage : ContentPage
{
	public MyTravelPage()
	{
		InitializeComponent();
        BindingContext = new MyTravel_ViewModel();

    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is MyTravel_ViewModel vm)
        {
            vm.LoadMyTripDataCommand.Execute(null);
        }
    }
    protected override bool OnBackButtonPressed()
    {
        if (BindingContext is MyTravel_ViewModel vm)
        {
            vm.GoBackProfileButton();
        }
        return true; 
    }
}