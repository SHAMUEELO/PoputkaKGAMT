using PoputkaKGAMT.ViewModel;

namespace PoputkaKGAMT;

public partial class IncomingMessagesPage : ContentPage
{
	public IncomingMessagesPage()
	{
		InitializeComponent();
        BindingContext = new IncomingMessages_ViewModel();
    }

    protected override bool OnBackButtonPressed()
    {
        if (BindingContext is IncomingMessages_ViewModel vm)
        {
            vm.OnMainPage();
        }
        return true;
    }
}