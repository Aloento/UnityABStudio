namespace SoarCraft.QYun.UnityABStudio.Core.Helpers {
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public static class Json {
        public static async Task<T> ToObjectAsync<T>(string value) => await Task.Run(() => {
            return JsonConvert.DeserializeObject<T>(value);
        });

        public static async Task<string> StringifyAsync(object value) => await Task.Run(() => {
            return JsonConvert.SerializeObject(value);
        });
    }
}
