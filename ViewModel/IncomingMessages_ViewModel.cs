using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoputkaKGAMT.Models;
using PoputkaKGAMT.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace PoputkaKGAMT.ViewModel
{
    partial class IncomingMessages_ViewModel : ObservableObject
    {

        [RelayCommand]
        public async Task OnMainPage()
        {
            await Shell.Current.GoToAsync("//SearchPage");
        }
    }
}
