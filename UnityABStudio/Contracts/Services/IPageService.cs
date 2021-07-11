namespace UnityABStudio.Contracts.Services {
    using System;

    public interface IPageService {
        Type GetPageType(string key);
    }
}
