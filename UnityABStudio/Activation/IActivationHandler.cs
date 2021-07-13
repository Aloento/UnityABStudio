namespace SoarCraft.QYun.UnityABStudio.Activation {
    using System.Threading.Tasks;

    public interface IActivationHandler {
        bool CanHandle(object args);

        Task HandleAsync(object args);
    }
}
