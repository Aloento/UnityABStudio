namespace SoarCraft.QYun.UnityABStudio.Services {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CommunityToolkit.Mvvm.ComponentModel;
    using Contracts.Services;
    using Microsoft.UI.Xaml.Controls;
    using ViewModels;
    using Views;

    public class PageService : IPageService {
        private readonly Dictionary<string, Type> _pages = new();

        public PageService() {
            this.Configure<OverViewModel, OverViewPage>();
            this.Configure<ListDetailsViewModel, ListDetailsPage>();
            this.Configure<DataGridViewModel, DataGridPage>();
            this.Configure<ContentGridViewModel, ContentGridPage>();
            this.Configure<ContentGridDetailViewModel, ContentGridDetailPage>();
            this.Configure<SettingsViewModel, SettingsPage>();
        }

        public Type GetPageType(string key) {
            Type pageType;
            lock (this._pages) {
                if (!this._pages.TryGetValue(key, out pageType)) {
                    throw new ArgumentException($"Page not found: {key}. Did you forget to call PageService.Configure?");
                }
            }

            return pageType;
        }

        private void Configure<VM, V>()
            where VM : ObservableObject
            where V : Page {
            lock (this._pages) {
                var key = typeof(VM).FullName;
                if (this._pages.ContainsKey(key)) {
                    throw new ArgumentException($"The key {key} is already configured in PageService");
                }

                var type = typeof(V);
                if (this._pages.Any(p => p.Value == type)) {
                    throw new ArgumentException($"This type is already configured with key {this._pages.First(p => p.Value == type).Key}");
                }

                this._pages.Add(key, type);
            }
        }
    }
}
