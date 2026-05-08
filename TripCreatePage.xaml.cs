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
        OnlyDriverFormBorder1.IsVisible = true;
        OnlyDriverFormBorder2.IsVisible = true;
        OnlyDriverFormBorder3.IsVisible = true;
        OnlyDriverFormLabel1.IsVisible = true;
        OnlyDriverFormLabel2.IsVisible = true;
        OnlyDriverFormLabel3.IsVisible = true;
        OnlyDriverFormLabel4.IsVisible = true;

        if (OnlyDriverForm.IsVisible == true && OnlyDriverFormBorder.IsVisible == true && OnlyDriverFormLabel1.IsVisible == true &&
            OnlyDriverFormLabel2.IsVisible == true && OnlyDriverFormLabel3.IsVisible == true && OnlyDriverFormBorder1.IsVisible == true &&
            OnlyDriverFormBorder2.IsVisible == true && OnlyDriverFormBorder3.IsVisible == true && OnlyDriverFormLabel4.IsVisible == true)
        { App.IsPassenger = false; }
    }

    private void OpenPassengerForm(object sender, TappedEventArgs e)
    {
        DriverButton.IsVisible = false;
        PassengerButton.IsVisible = false;
        FormScrollView.IsVisible = true;
        OnlyDriverForm.IsVisible = false;
        OnlyDriverFormBorder.IsVisible = false;
        OnlyDriverFormBorder1.IsVisible = false;
        OnlyDriverFormBorder2.IsVisible = false;
        OnlyDriverFormBorder3.IsVisible = false;
        OnlyDriverFormLabel1.IsVisible = false;
        OnlyDriverFormLabel2.IsVisible = false;
        OnlyDriverFormLabel3.IsVisible = false;
        OnlyDriverFormLabel4.IsVisible = false;

        if (OnlyDriverForm.IsVisible == false && OnlyDriverFormBorder.IsVisible == false && OnlyDriverFormLabel1.IsVisible == false &&
            OnlyDriverFormLabel2.IsVisible == false && OnlyDriverFormLabel3.IsVisible == false && OnlyDriverFormBorder1.IsVisible == false &&
            OnlyDriverFormBorder2.IsVisible == false && OnlyDriverFormBorder3.IsVisible == false && OnlyDriverFormLabel4.IsVisible == false)
        { App.IsPassenger = true; }
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

        if (BindingContext is TripCreate_ViewModel vm)
        {
            if (vm.NavigateToPlacePage == true)
                return;
        }

        DriverButton.IsVisible = true;
        PassengerButton.IsVisible = true;
        FormScrollView.IsVisible = false;
        OnlyDriverForm.IsVisible = false;
        OnlyDriverFormBorder.IsVisible = false;
        OnlyDriverFormBorder1.IsVisible = false;
        OnlyDriverFormBorder2.IsVisible = false;
        OnlyDriverFormBorder3.IsVisible = false;
        OnlyDriverFormLabel1.IsVisible = false;
        OnlyDriverFormLabel2.IsVisible = false;
        OnlyDriverFormLabel3.IsVisible = false;
        OnlyDriverFormLabel4.IsVisible = false;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TripCreate_ViewModel vm)
        {
            var place = Preferences.Get("SelectedPlace", "");

            if (!string.IsNullOrWhiteSpace(place))
            {
                var target = Preferences.Get("PlaceTarget", "");

                if (target == "Departure")
                    vm.DeparturePlace = place;
                else if (target == "Arrive")
                    vm.ArrivePlace = place;

                Preferences.Remove("SelectedPlace");
                Preferences.Remove("PlaceTarget");
            }

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

    private async void Entry_PriceCompleted(object sender, EventArgs e)
    {
        if (BindingContext is TripCreate_ViewModel vm)
        {
            if (!int.TryParse(vm.Price, out int price) || price <= 0)
            {
                await Shell.Current.DisplayAlertAsync("Внимание", "Цена не может быть отрицательной!", "OK");
                vm.Price = "0";
            }
        }
    }
    private async void Entry_CarNumberCompleted(object sender, EventArgs e)
    {
        if (BindingContext is TripCreate_ViewModel vm)
        {
            string value = vm.CarNumber?.Trim() ?? "";

            if (!int.TryParse(value, out int number))
            {
                await Shell.Current.DisplayAlertAsync("Внимание", "Номер автомобиля должен содержать только цифры!", "OK");
                vm.CarNumber = "0";
                return;
            }
            if (number <= 0)
            {
                await Shell.Current.DisplayAlertAsync("Внимание", "Отрицательное или нулевое значение недопустимо!", "OK");
                vm.CarNumber = "0";
                return;
            }
            if (value.Length != 3)
            {
                await Shell.Current.DisplayAlertAsync("Внимание", "Номер автомобиля должен содержать ровно 3 цифры!", "OK");
                vm.CarNumber = "0";
                return;
            }
        }
    }
}