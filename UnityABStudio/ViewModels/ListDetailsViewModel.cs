namespace SoarCraft.QYun.UnityABStudio.ViewModels {
    using System.Collections.ObjectModel;
    using System.Linq;
    using CommunityToolkit.Mvvm.ComponentModel;
    using Contracts.ViewModels;
    using Core.Contracts.Services;
    using Core.Models;

    public class ListDetailsViewModel : ObservableRecipient, INavigationAware {
        private readonly ISampleDataService _sampleDataService;
        private SampleOrder _selected;

        public SampleOrder Selected {
            get { return this._selected; }
            set { _ = this.SetProperty(ref this._selected, value); }
        }

        public ObservableCollection<SampleOrder> SampleItems { get; private set; } = new();

        public ListDetailsViewModel(ISampleDataService sampleDataService) {
            this._sampleDataService = sampleDataService;
        }

        public async void OnNavigatedTo(object parameter) {
            this.SampleItems.Clear();

            // Replace this with your actual data
            var data = await this._sampleDataService.GetListDetailsDataAsync();

            foreach (var item in data) {
                this.SampleItems.Add(item);
            }
        }

        public void OnNavigatedFrom() {
        }

        public void EnsureItemSelected() {
            if (this.Selected == null) {
                this.Selected = this.SampleItems.First();
            }
        }
    }
}
