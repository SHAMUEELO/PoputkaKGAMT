using PoputkaKGAMT.ViewModel;

namespace PoputkaKGAMT;

public partial class RegistrationPage : ContentPage
{
	public RegistrationPage()
	{
		InitializeComponent();
        BindingContext = new Registration_ViewModel();
    }


    protected override bool OnBackButtonPressed()
    {
        if (BindingContext is Registration_ViewModel vm)
        {
            vm.GoEntrace();
        }
        return true;
    }
}