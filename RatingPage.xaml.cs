using PoputkaKGAMT.ViewModel;

namespace PoputkaKGAMT;

public partial class RatingPage : ContentPage
{
	public RatingPage()
	{
		InitializeComponent();
		BindingContext = new RatingPage_ViewModel();

    }

    protected override bool OnBackButtonPressed()
    {
        if (BindingContext is RatingPage_ViewModel vm)
        {
            vm.GoBackProfileButton();
        }
        return true;
    }
}