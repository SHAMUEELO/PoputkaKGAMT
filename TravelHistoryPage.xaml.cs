using PoputkaKGAMT.ViewModel;

namespace PoputkaKGAMT;

public partial class TravelHistoryPage : ContentPage
{
    private TravelHistory_ViewModel TravelHistoryVM;

    public TravelHistoryPage(TravelHistory_ViewModel viewModel)
	{
		InitializeComponent();
        this.TravelHistoryVM = viewModel;
        BindingContext = TravelHistoryVM;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        TravelHistoryVM.OnNavigatedTo();

        if (BindingContext is TravelHistory_ViewModel vm)
        {
            vm.LoadTripCommand.Execute(null);
        }
    }

    protected override void OnDisappearing()
    {
        TravelHistoryVM.OnNavigatedFrom();
    }
}