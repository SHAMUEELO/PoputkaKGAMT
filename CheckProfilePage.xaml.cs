namespace PoputkaKGAMT;
using PoputkaKGAMT.ViewModel;

public partial class CheckProfilePage : ContentPage
{
	public CheckProfilePage()
	{
		InitializeComponent();
		BindingContext = new CheckProfile_ViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is CheckProfile_ViewModel vm)
        {
            vm.UserProfileLoad();
        }
    }

    protected override bool OnBackButtonPressed()
    {
        if (BindingContext is CheckProfile_ViewModel vm)
        {
            vm.GoBack();
        }
        return true;
    }
}