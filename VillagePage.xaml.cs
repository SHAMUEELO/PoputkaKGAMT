using PoputkaKGAMT.ViewModel;

namespace PoputkaKGAMT;

public partial class VillagePage : ContentPage
{
	public VillagePage()
	{
		InitializeComponent();
        BindingContext = new Village_ViewModel();
    }

    private async void OnLetterClicked(object sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            switch (btn.Text)
            {
                case "А":
                    await MainScroll.ScrollToAsync(AnchorA, ScrollToPosition.Start, true);
                    break;
                case "Б":
                    await MainScroll.ScrollToAsync(AnchorB, ScrollToPosition.Start, true);
                    break;
                case "В":
                    await MainScroll.ScrollToAsync(AnchorV, ScrollToPosition.Start, true);
                    break;
                case "Е":
                    await MainScroll.ScrollToAsync(AnchorE, ScrollToPosition.Start, true);
                    break;
                case "З":
                    await MainScroll.ScrollToAsync(AnchorZ, ScrollToPosition.Start, true);
                    break;
                case "И":
                    await MainScroll.ScrollToAsync(AnchorI, ScrollToPosition.Start, true);
                    break;
                case "К":
                    await MainScroll.ScrollToAsync(AnchorK, ScrollToPosition.Start, true);
                    break;
                case "Л":
                    await MainScroll.ScrollToAsync(AnchorL, ScrollToPosition.Start, true);
                    break;
                case "М":
                    await MainScroll.ScrollToAsync(AnchorM, ScrollToPosition.Start, true);
                    break;
                case "Н":
                    await MainScroll.ScrollToAsync(AnchorN, ScrollToPosition.Start, true);
                    break;
                case "О":
                    await MainScroll.ScrollToAsync(AnchorO, ScrollToPosition.Start, true);
                    break;
                case "П":
                    await MainScroll.ScrollToAsync(AnchorP, ScrollToPosition.Start, true);
                    break;
                case "С":
                    await MainScroll.ScrollToAsync(AnchorS, ScrollToPosition.Start, true);
                    break;
                case "Т":
                    await MainScroll.ScrollToAsync(AnchorT, ScrollToPosition.Start, true);
                    break;
                case "У":
                    await MainScroll.ScrollToAsync(AnchorY, ScrollToPosition.Start, true);
                    break;
                case "Х":
                    await MainScroll.ScrollToAsync(AnchorH, ScrollToPosition.Start, true);
                    break;
                case "Ч":
                    await MainScroll.ScrollToAsync(AnchorCH, ScrollToPosition.Start, true);
                    break;
                case "Ш":
                    await MainScroll.ScrollToAsync(AnchorSH, ScrollToPosition.Start, true);
                    break;
                case "Я":
                    await MainScroll.ScrollToAsync(AnchorYA, ScrollToPosition.Start, true);
                    break;
            }
        }
    }

    private async void OnPlaceTapped(object sender, TappedEventArgs e)
    {
        if (sender is Label label)
        {
            Preferences.Set("SelectedPlace", label.Text); // сохраняем место

            string previousPage = Preferences.Get("PreviousPage", "");
            // Если ссылка пуста, то по умолчанию SearchPage
            if (string.IsNullOrEmpty(previousPage))
                previousPage = "SearchPage";

            await Shell.Current.GoToAsync($"//{previousPage}");
        }
    }
    protected override bool OnBackButtonPressed()
    {
        if (BindingContext is Village_ViewModel vm)
        {
            vm.GoBack();
        }
        return true;
    }
}