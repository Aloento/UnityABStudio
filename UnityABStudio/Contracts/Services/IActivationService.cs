namespace UnityABStudio.Contracts.Services {
    using System.Threading.Tasks;

    public interface IActivationService {
        Task ActivateAsync(object activationArgs);
    }
}
