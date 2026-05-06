using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoputkaKGAMT.ViewModel
{
    public partial class Place_ViewModel : ObservableObject
    {

        [RelayCommand]
        public async Task GoBack()
        {
            string previousPage = Preferences.Get("PreviousPage", "");
            // Если ссылка пуста, то по умолчанию SearchPage
            if (string.IsNullOrEmpty(previousPage))
                previousPage = "SearchPage";

            await Shell.Current.GoToAsync($"//{previousPage}");
        }

        [RelayCommand]
        public async Task GoNaberezhnyeChelnyPage()
        {
            await Shell.Current.GoToAsync("//NaberezhnyeChelnyPage");
        }
        [RelayCommand]
        public async Task GoOtherCityPage()
        {
            await Shell.Current.GoToAsync("//OtherCityPage");
        }

        [RelayCommand]
        public async Task GoVillagePage()
        {
            await Shell.Current.GoToAsync("//VillagePage");
        }
    }
}
