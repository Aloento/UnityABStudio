namespace SoarCraft.QYun.UnityABStudio.ViewModels {
    using System.Linq;
    using CommunityToolkit.Mvvm.ComponentModel;
    using Contracts.ViewModels;
    using Core.Contracts.Services;
    using Core.Models;

    public class ContentGridDetailViewModel : ObservableRecipient, INavigationAware {
        private readonly ISampleDataService _sampleDataService;
        private SampleOrder _item;

        public SampleOrder Item {
            get { return this._item; }
            set { _ = this.SetProperty(ref this._item, value); }
        }

        public ContentGridDetailViewModel(ISampleDataService sampleDataService) {
            this._sampleDataService = sampleDataService;
        }

        public async void OnNavigatedTo(object parameter) {
            if (parameter is long orderID) {
                var data = await this._sampleDataService.GetContentGridDataAsync();
                this.Item = data.First(i => i.OrderID == orderID);
            }
        }

        public void OnNavigatedFrom() {
        }
    }
}
