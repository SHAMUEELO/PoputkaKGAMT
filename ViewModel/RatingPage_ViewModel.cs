using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoputkaKGAMT.ViewModel
{
    partial class RatingPage_ViewModel : ObservableObject
    {
        [RelayCommand]
        public async Task GoBackProfileButton()
        {
            await Shell.Current.GoToAsync("//ProfilePage");
        }
    }
}
