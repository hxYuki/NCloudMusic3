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
using Microsoft.UI.Xaml.Shapes;
using System.Text.Json.Nodes;
using System.Diagnostics.CodeAnalysis;

namespace NCloudMusic3.Models {
    // TODO 将所有数据获取移动到此处。。使用LRU缓存
    public class Music : CacheHolder<ulong, Music>, IEquatable<Music> {
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
        public int BitRate { get; set; }

        public string? Key163 { get; set; }

        public static Dictionary<string, List<string>> LocalDuplications { get; set; } = new();

        public async Task<bool> Match() {
            if (string.IsNullOrWhiteSpace(LocalPath)) throw new InvalidOperationException("cannot 'Match' non-local files");
            if (Matched.ContainsKey(LocalPath)) return true;
            if (Key163 is not null && Key163Helper.Decrypt(Key163, out var json)) {
                if (!json.StartsWith("music:")) return false;

                Func<string, JsonNodeOptions?, JsonDocumentOptions, JsonNode> jnodeParse = JsonNode.Parse;
                var jres = jnodeParse.TryInvoke(json[6..], null, default);

                if (!jres.TryGet(out var j))
                    return false;

                Id = ((ulong)j["musicId"]);
                Title = j["musicName"].ToString();


                if (Id != 0 && cache.ContainsKey(Id)) {
                    if (!string.IsNullOrEmpty(cache[Id].LocalPath) || cache[Id].LocalPath == "!") {
                        LocalDuplications.TryAdd(Title, new() { cache[Id].LocalPath });
                        cache[Id].LocalPath = "!";

                        LocalDuplications[Title].Add(LocalPath);
                    }
                    else
                        cache[Id].LocalPath = LocalPath;

                    Matched[LocalPath] = Id;

                    this.PublicShallowCopy(cache[Id], nameof(Id), nameof(Num), nameof(LocalPath), nameof(BitRate));
                    return true;
                }

                var aid = ((ulong)j["albumId"]);
                if (aid != 0) {
                    Album = Album.Create(aid, j["album"].ToString(), j["albumPic"].ToString());
                }
                else {
                    Album.Name = j["album"].ToString();
                    Album.PictureUrl = j["albumPic"].ToString();
                }
                Artists = j["artist"].AsArray().Select(p => (p[0].ToString(), ((ulong)p[1]))).Select(p => {
                    if (p.Item2 != 0)
                        return Artist.Create(p.Item2, p.Item1);
                    else
                        return new() { Name = p.Item1 };
                }).ToArray();

                Alias = j["alias"]?.AsArray().Select(a => a.ToString()).ToArray();
                Translation = j["transNames"]?.AsArray().Select(j => j.ToString()).ToArray();

                if (Id != 0) {
                    cache[Id] = this;
                    Matched[LocalPath] = Id;

                    this.PublicShallowCopy(cache[Id], nameof(Id), nameof(Num), nameof(LocalPath), nameof(BitRate));
                    return true;
                }
                else
                    return false;
            }
            return false;
            // TODO: 使用搜索的方式匹配 Title/Album/Artist(s)
            var keywords = new List<string>
            {
                Title
            };
            keywords.AddRange(Artists.Select(x => x.Name));
            var searchResult = await App.Instance.api.RequestAsync(CloudMusicApiProviders.Cloudsearch, new() { ["keywords"] = keywords });

            throw new NotImplementedException();
        }

        public bool Equals(Music other) {
            if(LocalPath.Equals(other.LocalPath))
                return Id.Equals(other.Id);
            else return false;
        }

        public enum CloudType {
            CloudOnly = 1, InfoOnly = 2, Normal = 0
        }

        private static Music FromTagFile(TagLib.File file) {
            return new Music {
                LocalPath = file.Name,
                Title = file.Tag.Title,
                Album = new() { Name = file.Tag.Album?.Trim() },
                Artists = file.Tag.Artists.Select(ar => new Artist() { Name = ar.Trim() }).ToArray(),
                Key163 = file.Tag.Comment ?? file.Tag.Description,
                BitRate = file.Properties.AudioBitrate
            };
        }
        // TODO: 匹配线上资源信息， 曲名/专辑/艺术家 方式搜索
        [Obsolete]
        public static Music CreateLocal(string path) {
            if (CanGetByLocalPath(path, out var ret)) {
                return ret;
            }

            var tagFile = TagLib.File.Create(path);

            if (tagFile.Properties.MediaTypes != TagLib.MediaTypes.Audio)
                throw new InvalidOperationException("non audio file got");

            var t = FromTagFile(tagFile);

            LocalOnlyCache[path] = t;

            return LocalOnlyCache[path];
        }
        public static Music CreateLocalFromFile(IStorageFile file) {
            if (CanGetByLocalPath(file.Path, out var ret)) {
                return ret;
            }

            var tagFile = TagLib.File.Create(file.ToTagLibFileAbstraction());

            if (tagFile.Properties.MediaTypes != TagLib.MediaTypes.Audio)
                throw new InvalidOperationException("non audio file got");

            var t = FromTagFile(tagFile);

            LocalOnlyCache[file.Path] = t;

            return LocalOnlyCache[file.Path];
        }
        public static Music GetLocal(string path) {
            if (CanGetByLocalPath(path, out var ret))
                return ret;
            return CreateLocal(path);
        }
        public static bool CanGetByLocalPath(string path, [MaybeNullWhen(false)] out Music music) {
            if (Matched.TryGetValue(path, out var id)) {
                music = cache[id];
                return true;
            }
            return LocalOnlyCache.TryGetValue(path, out music);
        }
        new public static async Task SaveCache(StorageFolder cacheFolder) {
            await CacheHolder<ulong, Music>.SaveCache(cacheFolder);

            var cachefile = await cacheFolder.CreateFileAsync(typeof(Music).Name + ".local.t.cache", CreationCollisionOption.ReplaceExisting);
            var matchfile = await cacheFolder.CreateFileAsync(typeof(Music).Name + ".match.t.cache", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(cachefile, JsonSerializer.Serialize(LocalOnlyCache, options));
            await FileIO.WriteTextAsync(matchfile, JsonSerializer.Serialize(Matched, options));
        }
        new public static async Task LoadCache(StorageFolder cacheFolder) {
            await CacheHolder<ulong, Music>.LoadCache(cacheFolder);

            var cachefile = await cacheFolder.TryGetItemAsync(typeof(Music).Name + ".local.t.cache");
            var matchfile = await cacheFolder.TryGetItemAsync(typeof(Music).Name + ".match.t.cache");
            if (cachefile is not null) {
                var txt = File.ReadAllText(cachefile.Path);
                LocalOnlyCache = JsonSerializer.Deserialize<Dictionary<string, Music>>(txt, options);
            }
            if (matchfile is not null) {
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

    public class Album : CacheHolder<ulong, Album> {
        //static Dictionary<ulong, Album> g = new();

        public ulong Id { get => key; set => key = value; }
        public string Name { get; set; }
        public string PictureUrl { get; set; }

        public Artist[] Artists { get; set; }
        public string[] Translations { get; set; }
        public DateTime PublishTime { get; set; }

        public string Description { get; set; }

        public static Album Create(ulong id, string name, string pictureUrl) {
            if (!cache.ContainsKey(id)) {
                cache[id] = new Album() { Id = id, Name = name, PictureUrl = pictureUrl };
            }
            return cache[id];

            //return new Album() { Id = id, Name = name, PictureUrl = pictureUrl };
        }

    }
    public class AlbumNavigator {
        public ulong Id { get; set; }
    }
    public class Artist : CacheHolder<ulong, Artist> {
        //static Dictionary<ulong, Artist> g = new();

        public ulong Id { get => key; set => key = value; }
        public string Name { get; set; }
        public string PictureUrl { get; set; }
        public string[] Alias { get; set; }
        public string[] Translations { get; set; }

        public override string ToString() {
            return Name;
        }

        public static Artist Create(ulong id, string name, string[] alias = null) {
            if (!cache.ContainsKey(id)) {
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
