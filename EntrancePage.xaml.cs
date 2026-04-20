using PoputkaKGAMT.ViewModel;

namespace PoputkaKGAMT;

public partial class EntrancePage : ContentPage
{
	public EntrancePage()
	{
		InitializeComponent();
        BindingContext = new Entrance_ViewModel();
    }


}