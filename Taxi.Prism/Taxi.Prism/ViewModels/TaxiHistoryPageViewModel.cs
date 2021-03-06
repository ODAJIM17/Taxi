﻿using Prism.Commands;
using Prism.Navigation;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Taxi.Common.Models;
using Taxi.Common.Services;

namespace Taxi.Prism.ViewModels
{
    public class TaxiHistoryPageViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IApiService _apiService;
        private TaxiResponse _taxi;
        private DelegateCommand _checkPlateCommand;
        private bool isRunning;
        private List<TripItemViewModel> _details;

        public bool IsRunning
        {
            get => isRunning;
            set => SetProperty(ref isRunning, value);
        }

        public List<TripItemViewModel> Details
        {
            get => _details;
            set => SetProperty(ref _details, value);
        }

        public TaxiHistoryPageViewModel(
            INavigationService navigationService,
            IApiService apiService) : base(navigationService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            Title = "Taxi History";
        }

        public TaxiResponse Taxi
        {
            get => _taxi;
            set => SetProperty(ref _taxi, value);
        }

      
        public string Plate { get; set; }

        public DelegateCommand CheckPlateCommand => _checkPlateCommand ?? (_checkPlateCommand = new DelegateCommand(CheckPlateAsync));

        private async void CheckPlateAsync()
        {
            if (string.IsNullOrEmpty(Plate))
            {
                await App.Current.MainPage.DisplayAlert(
                    "Error",
                    "Please enter a plate number.",
                    "Accept");
                return;
            }

            Regex regex = new Regex(@"^([A-Za-z]{3}\d{3})$");
            if (!regex.IsMatch(Plate))
            {
                await App.Current.MainPage.DisplayAlert(
                    "Error",
                    "The plate must start with three letters and end with three numbers.",
                    "Accept");
                return;
            }

            IsRunning = true;
            var url = App.Current.Resources["UrlAPI"].ToString();
            var connection = await _apiService.CheckConnectionAsync(url);
            if (!connection)
            {
                IsRunning = false;
                await App.Current.MainPage.DisplayAlert("Error", "Please check your internet connection.", "Accept");
                return;
            }

            Response response = await _apiService.GetTaxiAsync(Plate, url, "api", "/Taxis");
            IsRunning = false;


            if (!response.IsSuccess)
            {
                await App.Current.MainPage.DisplayAlert(
                    "Error",
                    response.Message,
                    "Accept");
                return;
            }  

            Taxi = (TaxiResponse)response.Result;
            Details = Taxi.Trips.Select(t => new TripItemViewModel(_navigationService)
            {
                EndDate = t.EndDate,
                Id = t.Id,
                Qualification = t.Qualification,
                Remarks = t.Remarks,
                Source = t.Source,
                SourceLatitude = t.SourceLatitude,
                SourceLongitude = t.SourceLongitude,
                StartDate = t.StartDate,
                Target = t.Target,
                TargetLatitude = t.TargetLatitude,
                TargetLongitude = t.TargetLongitude,
                TripDetails = t.TripDetails,
                User = t.User

            }).ToList();
        }
    }
}
