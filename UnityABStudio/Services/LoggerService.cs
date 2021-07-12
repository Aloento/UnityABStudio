namespace SoarCraft.QYun.UnityABStudio.Services {
    using Serilog;

    public class LoggerService {
        public ILogger Logger { get; }

        public LoggerService() {
            Logger = new LoggerConfiguration()
                .WriteTo.File(@"C:\CaChe\UnityABStudio.log")
                .CreateLogger();

            Logger.Information("Hello, UnityABStudio!");
        }
    }
}
