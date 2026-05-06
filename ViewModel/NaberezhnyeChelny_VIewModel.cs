using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoputkaKGAMT.ViewModel
{
    public partial class NaberezhnyeChelny_VIewModel : ObservableObject
    {
        [RelayCommand]
        public async Task GoBack()
        {
            await Shell.Current.GoToAsync("//PlacePage");
        }

    }
}
