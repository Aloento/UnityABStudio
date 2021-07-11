using System;
using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using UnityABStudio.Contracts.ViewModels;
using UnityABStudio.Core.Contracts.Services;
using UnityABStudio.Core.Models;

namespace UnityABStudio.ViewModels {
    public class DataGridViewModel : ObservableRecipient, INavigationAware {
        private readonly ISampleDataService _sampleDataService;

        public ObservableCollection<SampleOrder> Source { get; } = new ObservableCollection<SampleOrder>();

        public DataGridViewModel(ISampleDataService sampleDataService) {
            _sampleDataService = sampleDataService;
        }

        public async void OnNavigatedTo(object parameter) {
            Source.Clear();

            // Replace this with your actual data
            var data = await _sampleDataService.GetGridDataAsync();

            foreach (var item in data) {
                Source.Add(item);
            }
        }

        public void OnNavigatedFrom() {
        }
    }
}
