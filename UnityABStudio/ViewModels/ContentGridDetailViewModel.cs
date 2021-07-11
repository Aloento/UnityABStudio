using System;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;

using UnityABStudio.Contracts.ViewModels;
using UnityABStudio.Core.Contracts.Services;
using UnityABStudio.Core.Models;

namespace UnityABStudio.ViewModels {
    public class ContentGridDetailViewModel : ObservableRecipient, INavigationAware {
        private readonly ISampleDataService _sampleDataService;
        private SampleOrder _item;

        public SampleOrder Item {
            get { return _item; }
            set { SetProperty(ref _item, value); }
        }

        public ContentGridDetailViewModel(ISampleDataService sampleDataService) {
            _sampleDataService = sampleDataService;
        }

        public async void OnNavigatedTo(object parameter) {
            if (parameter is long orderID) {
                var data = await _sampleDataService.GetContentGridDataAsync();
                Item = data.First(i => i.OrderID == orderID);
            }
        }

        public void OnNavigatedFrom() {
        }
    }
}
