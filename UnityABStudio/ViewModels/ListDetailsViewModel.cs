namespace UnityABStudio.ViewModels {
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;

    using CommunityToolkit.Mvvm.ComponentModel;

    using UnityABStudio.Contracts.ViewModels;
    using UnityABStudio.Core.Contracts.Services;
    using UnityABStudio.Core.Models;

    public class ListDetailsViewModel : ObservableRecipient, INavigationAware {
        private readonly ISampleDataService _sampleDataService;
        private SampleOrder _selected;

        public SampleOrder Selected {
            get { return _selected; }
            set { _ = SetProperty(ref _selected, value); }
        }

        public ObservableCollection<SampleOrder> SampleItems { get; private set; } = new ObservableCollection<SampleOrder>();

        public ListDetailsViewModel(ISampleDataService sampleDataService) {
            _sampleDataService = sampleDataService;
        }

        public async void OnNavigatedTo(object parameter) {
            SampleItems.Clear();

            // Replace this with your actual data
            var data = await _sampleDataService.GetListDetailsDataAsync();

            foreach (var item in data) {
                SampleItems.Add(item);
            }
        }

        public void OnNavigatedFrom() {
        }

        public void EnsureItemSelected() {
            if (Selected == null) {
                Selected = SampleItems.First();
            }
        }
    }
}
