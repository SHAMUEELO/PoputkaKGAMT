using PoputkaKGAMT.ViewModel;

namespace PoputkaKGAMT;

public partial class RatingPage : ContentPage
{
	public RatingPage()
	{
		InitializeComponent();
		BindingContext = new RatingPage_ViewModel();

    }
}