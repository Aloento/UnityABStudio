namespace SoarCraft.QYun.UnityABStudio.Contracts.Services {
    using System;
    using System.Collections.Generic;
    using Microsoft.UI.Xaml.Controls;

    public interface INavigationViewService {
        IList<object> MenuItems { get; }

        object SettingsItem { get; }

        void Initialize(NavigationView navigationView);

        void UnregisterEvents();

        NavigationViewItem GetSelectedItem(Type pageType);
    }
}
