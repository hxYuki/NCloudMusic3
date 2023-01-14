using NCloudMusic3.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace NCloudMusic3.Helpers
{
    public class CacheHolder<TKey, TValue> where TValue : CacheHolder<TKey, TValue>, new()
    {
        protected static Dictionary<TKey, TValue> cache = new();
        protected TKey key;

        protected DateTime LastGetTime { get; set; }

        public static bool TryGet(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            return cache.TryGetValue(key, out value);
        }

        public static TValue Get(TKey key)
        {
            if (!cache.TryGetValue(key, out TValue t))
                throw new KeyNotFoundException();
            return t;
        }

        public static TValue Create(TKey key)
        {
            if(!cache.ContainsKey(key))
                cache[key] = new TValue() { key=key};

            return cache[key];
        }
        public static TValue CreateOrUpdate(TKey key, Action<TValue> updateFuntion)
        {
            if (!cache.ContainsKey(key))
                cache[key] = new TValue() { key = key };

            updateFuntion(cache[key]);

            return cache[key];
        }

        public static async Task SaveCache(StorageFolder cacheFolder)
        {
            var cachefile = await cacheFolder.CreateFileAsync(typeof(TValue).Name+".t.cache", CreationCollisionOption.ReplaceExisting);

            await FileIO.WriteTextAsync(cachefile, JsonSerializer.Serialize(cache, new JsonSerializerOptions() { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault }));
        }
        public static async Task LoadCache(StorageFolder cacheFolder)
        {
            var cachefile = await cacheFolder.TryGetItemAsync(typeof(TValue).Name + ".t.cache");

            if (cachefile is not null)
            {
                var txt = File.ReadAllText(cachefile.Path);

                cache = JsonSerializer.Deserialize<Dictionary<TKey, TValue>>(txt);
            }
        }
    }
}
