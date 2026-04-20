using PoputkaKGAMT.ViewModel;

namespace PoputkaKGAMT;

public partial class SearchPage : ContentPage
{
	public SearchPage()
	{
		InitializeComponent();
		BindingContext = new Search_ViewModel();
	}

    // јвтоматичкеска€ загрузка при октрытии старницы
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is Search_ViewModel vm)
            vm.LoadPlace();
    }
}