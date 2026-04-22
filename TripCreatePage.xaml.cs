using PoputkaKGAMT.ViewModel;

namespace PoputkaKGAMT;

public partial class TripCreatePage : ContentPage
{
    public TripCreatePage()
    {
        InitializeComponent();
        BindingContext = new TripCreate_ViewModel();

    }

    private void OpenDriverForm(object sender, TappedEventArgs e)
    {
        DriverButton.IsVisible = false;
        PassengerButton.IsVisible = false;
        FormScrollView.IsVisible = true;
        OnlyDriverForm.IsVisible = true;
        OnlyDriverFormBorder.IsVisible = true;

        if (OnlyDriverForm.IsVisible == true && OnlyDriverFormBorder.IsVisible == true) { App.IsPassenger = false; }
    }

    private void OpenPassengerForm(object sender, TappedEventArgs e)
    {
        DriverButton.IsVisible = false;
        PassengerButton.IsVisible = false;
        FormScrollView.IsVisible = true;
        OnlyDriverForm.IsVisible = false;
        OnlyDriverFormBorder.IsVisible = false;

        if (OnlyDriverForm.IsVisible == false && OnlyDriverFormBorder.IsVisible == false) { App.IsPassenger = true; }
    }


    private void OnBackImageButton(object sender, TappedEventArgs e)
    {
        DriverButton.IsVisible = true;
        PassengerButton.IsVisible = true;
        FormScrollView.IsVisible = false;
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        DriverButton.IsVisible = true;
        PassengerButton.IsVisible = true;
        FormScrollView.IsVisible = false;
        OnlyDriverForm.IsVisible = false;
        OnlyDriverFormBorder.IsVisible = false;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TripCreate_ViewModel vm)
        {
            vm.LoadData();
        }
    }

    protected override bool OnBackButtonPressed()
    {
        DriverButton.IsVisible = true;
        PassengerButton.IsVisible = true;
        FormScrollView.IsVisible = false;
        
        if (BindingContext is TripCreate_ViewModel vm)
        {
            vm.OnMainPage();
        }
        return true;
    }
}