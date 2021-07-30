namespace SoarCraft.QYun.UnityABStudio.Core.Services {
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using AssetReader;
    using Microsoft.Extensions.Caching.Memory;

    public class CacheService {
        private readonly IMemoryCache cache;

        public CacheService(IMemoryCache cache) => this.cache = cache;

        public async Task<bool> TryCacheFileAsync(SerializedFile serialized) {
            var fullPath = Path.GetFullPath(serialized.originalPath);
            var (res, _) = await IsFileCached(fullPath);
            if (res)
                return true;

            return await this.TryPutAsync(fullPath,
                new BundleCache(File.OpenRead(fullPath).Length, await this.GetFileMD5Async(fullPath), serialized));
        }

        public async Task<(bool, SerializedFile)> TryGetCachedFileAsync(string filePath) {
            if (!File.Exists(filePath))
                return (false, null);

            var (res, file) = await IsFileCached(Path.GetFullPath(filePath));
            return res ? (true, file) : (false, null);
        }

        private async Task<(bool, SerializedFile)> IsFileCached(string filePath) {
            var (res, cache) = await TryGetValue<BundleCache>(filePath);
            if (res) {
                if (cache.Size == File.OpenRead(filePath).Length) {
                    if (cache.MD5 == await GetFileMD5Async(filePath))
                        return (true, cache.Serialized);
                }
            }

            return (false, null);
        }

        private async Task<string> GetFileMD5Async(string filePath) {
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

    internal class BundleCache {
        public long Size;
        public string MD5;
        public SerializedFile Serialized;

        public BundleCache(long size, string md5, SerializedFile serialized) {
            this.Size = size;
            this.MD5 = md5;
            this.Serialized = serialized;
        }
    }
}
