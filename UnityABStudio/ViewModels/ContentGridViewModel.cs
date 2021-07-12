namespace SoarCraft.QYun.UnityABStudio.ViewModels {
    using System.Collections.ObjectModel;
    using System.Windows.Input;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using Contracts.Services;
    using Contracts.ViewModels;
    using Core.Contracts.Services;
    using Core.Models;

    public class ContentGridViewModel : ObservableRecipient, INavigationAware {
        private readonly INavigationService _navigationService;
        private readonly ISampleDataService _sampleDataService;
        private ICommand _itemClickCommand;

        public ICommand ItemClickCommand => this._itemClickCommand ?? (this._itemClickCommand = new RelayCommand<SampleOrder>(this.OnItemClick));

        public ObservableCollection<SampleOrder> Source { get; } = new();

        public ContentGridViewModel(INavigationService navigationService, ISampleDataService sampleDataService) {
            this._navigationService = navigationService;
            this._sampleDataService = sampleDataService;
        }

        public async void OnNavigatedTo(object parameter) {
            this.Source.Clear();

            // Replace this with your actual data
            var data = await this._sampleDataService.GetContentGridDataAsync();
            foreach (var item in data) {
                this.Source.Add(item);
            }
        }

        public void OnNavigatedFrom() {
        }

        private void OnItemClick(SampleOrder clickedItem) {
            if (clickedItem != null) {
                this._navigationService.SetListDataItemForNextConnectedAnimation(clickedItem);
                _ = this._navigationService.NavigateTo(typeof(ContentGridDetailViewModel).FullName, clickedItem.OrderID);
            }
        }
    }
}
