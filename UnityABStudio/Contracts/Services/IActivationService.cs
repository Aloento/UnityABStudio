using System.Threading.Tasks;

namespace UnityABStudio.Contracts.Services {
    public interface IActivationService {
        Task ActivateAsync(object activationArgs);
    }
}
