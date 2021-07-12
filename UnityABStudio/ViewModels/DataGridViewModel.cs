namespace SoarCraft.QYun.UnityABStudio.ViewModels {
    using System.Collections.ObjectModel;
    using CommunityToolkit.Mvvm.ComponentModel;
    using Contracts.ViewModels;
    using Core.Contracts.Services;
    using Core.Models;

    public class DataGridViewModel : ObservableRecipient, INavigationAware {
        private readonly ISampleDataService _sampleDataService;

        public ObservableCollection<SampleOrder> Source { get; } = new();

        public DataGridViewModel(ISampleDataService sampleDataService) {
            this._sampleDataService = sampleDataService;
        }

        public async void OnNavigatedTo(object parameter) {
            this.Source.Clear();

            // Replace this with your actual data
            var data = await this._sampleDataService.GetGridDataAsync();

            foreach (var item in data) {
                this.Source.Add(item);
            }
        }

        public void OnNavigatedFrom() {
        }
    }
}
