using CommunityToolkit.Mvvm.ComponentModel;
using PoputkaKGAMT.Models;
using PoputkaKGAMT.Services;
using PoputkaKGAMT.ViewModel;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace PoputkaKGAMT;


public partial class SearchResultPage : ContentPage
{
    private SearchResult_ViewModel SearchResultVM;

    public SearchResultPage(SearchResult_ViewModel viewModel)
	{
		InitializeComponent();
        this.SearchResultVM = viewModel;
        BindingContext = SearchResultVM;
    }

    private void OpenDriverResult(object sender, TappedEventArgs e)
    {
        DriverReusultScroll.IsVisible = true;
        PassengerReusultScroll.IsVisible = false;

        if (BindingContext is SearchResult_ViewModel vm)
        {
            if (vm.IsAllEmptyDriver == true)
            {
                DriverReusultScrollNotFind.IsVisible = true;
            }
            else { DriverReusultScrollNotFind.IsVisible = false; }
        }
        PassengerReusultScrollNotFind.IsVisible = false;

        DriverLabel.TextColor = Color.FromArgb("#214484");
        PassengerLabel.TextColor = Colors.Black;

        WheelImage.Source = "wheeliconresult.png";
        PassengerImage.Source = "graypassengericonresult.png";

        DriverBoxView.Color = Color.FromArgb("#214484");
        PassengerBoxView.Color = Colors.Black;     
    }

    private void OpenPassengerReusult(object sender, TappedEventArgs e)
    {
        DriverReusultScroll.IsVisible = false;
        PassengerReusultScroll.IsVisible = true;

        if (BindingContext is SearchResult_ViewModel vm)
        {
            if(vm.IsAllEmptyPassenger == true)
            {
                PassengerReusultScrollNotFind.IsVisible = true;
            }
            else { PassengerReusultScrollNotFind.IsVisible = false; }
        }
        DriverReusultScrollNotFind.IsVisible = false;

        DriverLabel.TextColor = Colors.Black;
        PassengerLabel.TextColor = Color.FromArgb("#214484"); 

        WheelImage.Source = "graywheeliconresult.png";
        PassengerImage.Source = "passengericonresult.png";

        DriverBoxView.Color = Colors.Black;
        PassengerBoxView.Color = Color.FromArgb("#214484");
    }

    //private DateTime lastLoad = DateTime.MinValue;
    //protected override void OnAppearing()
    //{
        //if (DateTime.Now - lastLoad > TimeSpan.FromSeconds(5)) // раз в 5 сек
       // {
          //  vm.LoadData();
        //    lastLoad = DateTime.Now;
      //  }
    //}
    protected override void OnAppearing()
    {
        base.OnAppearing();

        SearchResultVM.OnNavigatedTo();

        OpenDriverResult(null, null);

        bool parametersValue = App.Parameters;

        if (BindingContext is SearchResult_ViewModel vm)
        {
            vm.LoadDataCommand.Execute(null); 
        }
    }

    protected override void OnDisappearing()
    {

        SearchResultVM.OnNavigatedFrom();

        App.Parameters = false;
    }

    protected override bool OnBackButtonPressed()
    {
        if (BindingContext is SearchResult_ViewModel vm)
        {
            vm.GoSearch();
        }
        return true;
    }
}