using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Database;
using Firebase.Database.Query;
using PoputkaKGAMT.Models;
using PoputkaKGAMT.Services;
using System.Text.RegularExpressions;

namespace PoputkaKGAMT.ViewModel
{
    partial class TripRating_ViewModel : ObservableObject
    {
        [RelayCommand]
        public async Task GoBackTripDetails()
        {
            await Shell.Current.GoToAsync("//TripDetailsPage");
        }
    }
}
