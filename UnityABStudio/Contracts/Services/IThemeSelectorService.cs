namespace SoarCraft.QYun.UnityABStudio.Contracts.Services {
    using System.Threading.Tasks;
    using Microsoft.UI.Xaml;

    public interface IThemeSelectorService {
        ElementTheme Theme { get; }

        Task InitializeAsync();

        Task SetThemeAsync(ElementTheme theme);

        Task SetRequestedThemeAsync();
    }
}
