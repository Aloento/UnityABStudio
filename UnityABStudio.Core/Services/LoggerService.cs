namespace SoarCraft.QYun.UnityABStudio.Core.Services {
    using Serilog;

    public class LoggerService {
        public ILogger Logger { get; }

        public LoggerService() {
            this.Logger = new LoggerConfiguration()
                .WriteTo.Async(a => a.File(@"C:\CaChe\UnityABStudio.log"))
                .CreateLogger();

            this.Logger.Information("-------------------------------------");
            this.Logger.Information("Hello, SoarCraft.QYun.UnityABStudio!");
        }
    }
}
