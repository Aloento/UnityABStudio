using System;

namespace UnityABStudio.Contracts.Services {
    public interface IPageService {
        Type GetPageType(string key);
    }
}
