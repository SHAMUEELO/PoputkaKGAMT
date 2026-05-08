using PoputkaKGAMT.ViewModel;

namespace PoputkaKGAMT;

public partial class SettingPage : ContentPage
{
	public SettingPage()
	{
		InitializeComponent();
		BindingContext = new Setting_ViewModel();

    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is Setting_ViewModel vm)
        {
            vm.LoadData();
        }
    }

    protected override bool OnBackButtonPressed()
    {
        if(BindingContext is Setting_ViewModel vm)
        {
            vm.GoBack();
        }
        return true; 
    }
}