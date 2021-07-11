using System.Threading.Tasks;

namespace UnityABStudio.Activation {
    public interface IActivationHandler {
        bool CanHandle(object args);

        Task HandleAsync(object args);
    }
}
