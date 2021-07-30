namespace SoarCraft.QYun.UnityABStudio.Core.Services {
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Memory;

    public class CacheService {
        private readonly IMemoryCache cache;

        public CacheService(IMemoryCache cache) => this.cache = cache;

        public bool IsFileCached(string filePath) {
            return false;
        }

        public async Task<string> GetFileMD5Async(string filePath) {
            if (!File.Exists(filePath))
                return string.Empty;

            Stream stream = File.OpenRead(filePath);
            if (stream.Length > 1000000) {
                var tmp = new byte[1000000];
                _ = await stream.ReadAsync(tmp.AsMemory());
                return BitConverter.ToString(MD5.Create().ComputeHash(tmp)).Replace("-", "");
            }

            return BitConverter.ToString(await MD5.Create().ComputeHashAsync(stream)).Replace("-", "");
        }

        public Task<bool> TryPutAsync(object key, object value, int seconds = byte.MaxValue) => Task.Run(() => {
            if (this.cache.TryGetValue(key, out _))
                return false;

            _ = this.cache.Set(key, value, TimeSpan.FromSeconds(seconds));
            return true;
        });

        public Task<(bool, T)> TryGetValue<T>(object key) where T : class => Task.Run(() =>
            this.cache.TryGetValue(key, out var res) ? (true, res as T) : (false, null));

        public void Remove(object key) => this.cache.Remove(key);
    }
}
