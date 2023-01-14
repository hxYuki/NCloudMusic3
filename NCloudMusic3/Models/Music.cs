using NCloudMusic3.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCloudMusic3.Models
{
    // TODO 将所有数据获取移动到此处。。使用LRU缓存
    public class Music : CacheHolder<ulong, Music>, IEquatable<Music>
    {
        //static Dictionary<ulong, Music> g = new();

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
        //public static Album UpdateOrCreate(ulong id, Action<Album> updater)
        //{
        //    if (!g.ContainsKey(id))
        //    {
        //        var t = new Album() { Id = id };
        //        g[id] = t;
        //    }
        //    updater(g[id]);
        //    return g[id];
        //}

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
