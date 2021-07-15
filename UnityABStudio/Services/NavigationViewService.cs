namespace SoarCraft.QYun.UnityABStudio.Services {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts.Services;
    using Helpers;
    using Microsoft.UI.Xaml.Controls;
    using ViewModels;

    public class NavigationViewService : INavigationViewService {
        private readonly INavigationService _navigationService;
        private readonly IPageService _pageService;
        private NavigationView _navigationView;

        public IList<object> MenuItems
            => this._navigationView.MenuItems;

        public object SettingsItem
            => this._navigationView.SettingsItem;

        public NavigationViewService(INavigationService navigationService, IPageService pageService) {
            this._navigationService = navigationService;
            this._pageService = pageService;
        }

        public void Initialize(NavigationView navigationView) {
            this._navigationView = navigationView;
            this._navigationView.BackRequested += this.OnBackRequested;
            this._navigationView.ItemInvoked += this.OnItemInvoked;
        }

        public void UnregisterEvents() {
            this._navigationView.BackRequested -= this.OnBackRequested;
            this._navigationView.ItemInvoked -= this.OnItemInvoked;
        }

        public NavigationViewItem GetSelectedItem(Type pageType)
            => this.GetSelectedItem(this._navigationView.MenuItems, pageType);

        private void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
            => this._navigationService.GoBack();

        private void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
            if (args.IsSettingsInvoked) {
                _ = this._navigationService.NavigateTo(typeof(SettingsServic).FullName);
            } else {
                var selectedItem = args.InvokedItemContainer as NavigationViewItem;
                var pageKey = selectedItem.GetValue(NavHelper.NavigateToProperty) as string;

                if (pageKey != null) {
                    _ = this._navigationService.NavigateTo(pageKey);
                }
            }
        }

        private NavigationViewItem GetSelectedItem(IEnumerable<object> menuItems, Type pageType) {
            foreach (var item in menuItems.OfType<NavigationViewItem>()) {
                if (this.IsMenuItemForPageType(item, pageType)) {
                    return item;
                }

                var selectedChild = this.GetSelectedItem(item.MenuItems, pageType);
                if (selectedChild != null) {
                    return selectedChild;
                }
            }

            return null;
        }

        private bool IsMenuItemForPageType(NavigationViewItem menuItem, Type sourcePageType) {
            var pageKey = menuItem.GetValue(NavHelper.NavigateToProperty) as string;
            if (pageKey != null) {
                return this._pageService.GetPageType(pageKey) == sourcePageType;
            }

            return false;
        }
    }
}
