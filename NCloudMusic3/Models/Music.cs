using NCloudMusic3.Helpers;
using NeteaseCloudMusicApi;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Foundation.Diagnostics;
using Windows.Storage;

using DotNext;

namespace NCloudMusic3.Models
{
    // TODO 将所有数据获取移动到此处。。使用LRU缓存
    public class Music : CacheHolder<ulong, Music>, IEquatable<Music>
    {
        static Dictionary<string, Music> LocalOnlyCache = new();
        static Dictionary<string, ulong> Matched = new();

        public ulong Id { get => key; set => key = value; }
        public int Num { get; set; }
        public string Title { get; set; }
        public string[] Translation { get; set; }
        public Artist[] Artists { get; set; }
        public string[] Alias { get; set; }
        public Album Album { get; set; }
        public MusicQuality MaxQuality { get; set; }
        public CloudType CloudInfo { get; set; }
        public string Path { get; set; }
        public string LocalPath { get; set; }

        public async void Match()
        {
            if (string.IsNullOrWhiteSpace(LocalPath)) throw new InvalidOperationException("cannot 'Match' non-local files");
            if (!LocalOnlyCache.ContainsKey(LocalPath)) return;

            var keywords = new List<string>
            {
                Title
            };
            keywords.AddRange(Artists.Select(x => x.Name));
            var searchResult = await App.Instance.api.RequestAsync(CloudMusicApiProviders.Cloudsearch, new() { ["keywords"] = keywords });


        }

        public bool Equals(Music other)
        {
            return Id.Equals(other.Id);
        }

        public enum CloudType
        {
            CloudOnly = 1, InfoOnly = 2, Normal = 0
        }

        //public static Music Create(ulong id)
        //{
        //    if (!g.ContainsKey(id))
        //        g[id] = new Music() { Id = id };
        //    return g[id];
        //}

        // TODO: 匹配线上资源信息， 曲名/专辑/艺术家 方式搜索
        public static Music CreateLocal(string path)
        {
            if (!LocalOnlyCache.ContainsKey(path))
            {
                var tagFile = TagLib.File.Create(path);

                if (tagFile.Properties.MediaTypes != TagLib.MediaTypes.Audio)
                    throw new InvalidOperationException("non audio file got");
                
                var t = new Music
                {
                    LocalPath = path,
                    Title = tagFile.Tag.Title,
                    Album = new() { Name = tagFile.Tag.Album.Trim() },
                    Artists = tagFile.Tag.Artists.Select(ar => new Artist() { Name = ar.Trim() }).ToArray(),
                };

                LocalOnlyCache[path] = t;
            }

            return LocalOnlyCache[path];
        }
        public static Music GetLocal(string path)
        {
            if(Matched.TryGetValue(path, out var id))
                return cache[id];
            if(LocalOnlyCache.ContainsKey(path))
                return LocalOnlyCache[path];
            return CreateLocal(path);
        }
        new public static async Task SaveCache(StorageFolder cacheFolder)
        {
            await CacheHolder<ulong, Music>.SaveCache(cacheFolder);

            var cachefile = await cacheFolder.CreateFileAsync(typeof(Music).Name + ".local.t.cache", CreationCollisionOption.ReplaceExisting);
            var matchfile = await cacheFolder.CreateFileAsync(typeof(Music).Name + ".match.t.cache", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(cachefile, JsonSerializer.Serialize(LocalOnlyCache, options));
            await FileIO.WriteTextAsync(matchfile, JsonSerializer.Serialize(Matched, options));
        }
        new public static async Task LoadCache(StorageFolder cacheFolder)
        {
            await CacheHolder<ulong, Music>.LoadCache(cacheFolder);

            var cachefile = await cacheFolder.TryGetItemAsync(typeof(Music).Name + ".local.t.cache");
            var matchfile = await cacheFolder.TryGetItemAsync(typeof(Music).Name + ".match.t.cache");
            if (cachefile is not null)
            {
                var txt = File.ReadAllText(cachefile.Path);
                LocalOnlyCache = JsonSerializer.Deserialize<Dictionary<string, Music>>(txt, options);
            }
            if(matchfile is not null)
            {
                var t2 = File.ReadAllText(matchfile.Path);
                Matched = JsonSerializer.Deserialize<Dictionary<string, ulong>>(t2, options);
            }
        }
        // 尝试匹配线上资源，返回成功匹配的结果
        //public static IAsyncEnumerable<Music> TryMatch(params string[] paths)
        //{
        //    foreach (var localpath in paths)
        //    {


        //    }
        //}
    }

    public class Album : CacheHolder<ulong, Album>
    {
        //static Dictionary<ulong, Album> g = new();

        public ulong Id { get => key; set => key = value; }
        public string Name { get; set; }
        public string PictureUrl { get; set; }

        public Artist[] Artists { get; set; }
        public string[] Translations { get; set; }
        public DateTime PublishTime { get; set; }

        public string Description { get; set; }

        public static Album Create(ulong id, string name, string pictureUrl)
        {
            if (!cache.ContainsKey(id))
            {
                cache[id] = new Album() { Id = id, Name = name, PictureUrl = pictureUrl };
            }
            return cache[id];

            //return new Album() { Id = id, Name = name, PictureUrl = pictureUrl };
        }

    }
    public class AlbumNavigator
    {
        public ulong Id { get; set; }
    }
    public class Artist : CacheHolder<ulong, Artist>
    {
        //static Dictionary<ulong, Artist> g = new();

        public ulong Id { get => key; set => key = value; }
        public string Name { get; set; }
        public string PictureUrl { get; set; }
        public string[] Alias { get; set; }
        public string[] Translations { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public static Artist Create(ulong id, string name, string[] alias = null)
        {
            if (!cache.ContainsKey(id))
            {
                cache[id] = new Artist() { Id = id, Name = name, Alias = alias ?? Array.Empty<string>() };
            }
            return cache[id];
        }

        //public static Artist UpdateOrCreate(ulong id, Action<Artist> updater)
        //{
        //    if (!g.ContainsKey(id))
        //    {
        //        var t = new Artist() { Id = id };
        //        g[id] = t;
        //    }
        //    updater(g[id]);
        //    return g[id];
        //}
    }
}
