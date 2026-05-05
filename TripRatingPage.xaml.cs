using PoputkaKGAMT.ViewModel;

namespace PoputkaKGAMT;

public partial class TripRatingPage : ContentPage
{
	public TripRatingPage()
	{
		InitializeComponent();
        BindingContext = new TripRating_ViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is TripRating_ViewModel vm)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await vm.LoadTravelersCommand.ExecuteAsync(null);
                    ResetStars();
                }
                catch (Exception ex)
                {

                    await Shell.Current.DisplayAlertAsync("Ошибка загрузки", ex.Message, "OK");
                }
            });
        }
    }


    // Обработчик нажатия на звезду
    private async void OnStarTapped(object sender, TappedEventArgs e)
    {
        if (sender is Image image && e.Parameter is string ratingStr)
        {
            if (int.TryParse(ratingStr, out int rating))
            {
                // Устанавливаем рейтинг в ViewModel
                if (BindingContext is TripRating_ViewModel vm)
                {
                    vm.Estimate = rating;
                    UpdateStarsVisual(rating); // Обновляем визуал звёзд
                }
            }
        }
    }

    // Обновляет визуальное состояние звёзд
    private void UpdateStarsVisual(int rating)
    {
        // Зажигаем звёзды до выбранной
        FirstStar.Source = rating >= 1 ? "darkbluestaricon.png" : "darkbluestarholeicon.png";
        SecondStar.Source = rating >= 2 ? "darkbluestaricon.png" : "darkbluestarholeicon.png";
        ThirdStar.Source = rating >= 3 ? "darkbluestaricon.png" : "darkbluestarholeicon.png";
        FourStar.Source = rating >= 4 ? "darkbluestaricon.png" : "darkbluestarholeicon.png";
        FiveStar.Source = rating >= 5 ? "darkbluestaricon.png" : "darkbluestarholeicon.png";
    }

    // Сброс всех звёзд (вызывается при открытии формы/страницы)
    public void ResetStars()
    {
        UpdateStarsVisual(0);
    }


    protected override bool OnBackButtonPressed()
    {
        if (BindingContext is TripRating_ViewModel vm)
        {
            vm.GoBackTripDetails();
        }
        return true;
    }
}