using PoputkaKGAMT.ViewModel;
using Firebase.Database;
using Microsoft.Maui.Storage;

namespace PoputkaKGAMT;

public partial class ProfilePage : ContentPage
{
	public ProfilePage()
	{
		InitializeComponent();
        BindingContext = new Profile_ViewModel();
    }

    // Автоматичкеская загрузка при октрытии старницы
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is Profile_ViewModel vm)
            vm.LoadProfilInfo();  
    }
}