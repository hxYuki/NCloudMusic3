using NCloudMusic3.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NCloudMusic3.Helpers
{
    internal static class JsonProcesser
    {
        public static Artist ParseArtist(this JToken jToken)
        {
            var r = Artist.Create(((ulong)jToken["id"]), jToken["name"].ToString());

            if (jToken["picUrl"] is not null)
            {
                r.PictureUrl = jToken["picUrl"].ToString();
            }
            if (jToken["alias"] is not null && jToken["alias"].HasValues)
            {
                r.Alias = jToken["alias"].ToObject<string[]>();
            }
            if (jToken["tns"] is not null && jToken["tns"].HasValues)
            {
                r.Translations = jToken["tns"].ToObject<string[]>();
            }
            if (jToken["transNames"] is not null && jToken["transNames"].HasValues)
            {
                r.Translations = jToken["transNames"].ToObject<string[]>();
            }

            return r;
        }
        public static Album ParseAlbum(this JToken jToken)
        {
            var r = Album.Create(((ulong)jToken["id"]), jToken["name"].ToString(), jToken["picUrl"].ToString());
            if (jToken["description"] is not null)
            {
                r.Description = jToken["description"].ToString();
            }
            if (jToken["publishTime"] is not null)
            {
                r.PublishTime = (DateTime.FromBinary((long)jToken["publishTime"]));
            }
            if (jToken["transNames"] is not null && jToken["transNames"].HasValues)
            {
                r.Translations = jToken["transNames"].ToObject<string[]>();
            }
            if (jToken["artists"] is not null && jToken["artists"].HasValues)
            {
                r.Artists = jToken["artists"].Select(j => j.ParseArtist()).ToArray();
            }

            return r;
        }
        public static Music ParseMusic(this JToken jToken)
        {
            var r = Music.Create(((ulong)jToken["id"]));

            r.Title = jToken["name"].ToString();
            r.Artists = jToken["ar"].Select(j => j.ParseArtist()).ToArray();
            r.Album = jToken["al"].ParseAlbum();
            if (jToken["tns"].IsAvailableArray())
                r.Translation = jToken["tns"].ToObject<string[]>();
            r.CloudInfo = (Music.CloudType)((int)jToken["t"]);
            if (jToken["alia"].IsAvailableArray())
                r.Alias = jToken["alia"].ToObject<string[]>();
            r.MaxQuality = jToken switch
            {
                JToken j when j["hr"] is not null => MusicQuality.HiRes,
                JToken j when j["sq"] is not null => MusicQuality.Lossless,
                JToken j when j["h"] is not null => MusicQuality.ExHigh,
                JToken j when j["m"] is not null => MusicQuality.Higher,
                JToken j when j["l"] is not null => MusicQuality.Standard,
                null => MusicQuality.Standard,
                _ => throw new NotImplementedException(),
            };

            return r;
        }
        public static MusicList ParseMusicList(this JToken jToken)
        {
            return MusicList.Create(((ulong)jToken["id"])).With(l =>
            {
                l.Name = jToken["name"].ToString();
                l.CoverImgUrl = jToken["coverImgUrl"].ToString();
                l.Description = jToken["description"]?.ToString();
                l.TrackCount = ((int)jToken["trackCount"]);
                l.IsFromSubscribe = ((bool)jToken["subscribed"]);
                l.Creator = jToken["creator"].ParseUser();
                l.CreateTime = DateTime.FromBinary((long)jToken["createTime"]);
            });
            
        }
        public static User ParseUser(this JToken jToken)
        {
            return User.Create(((ulong)jToken["userId"])).With(u =>
            {
                u.UserId = ((ulong)jToken["userId"]);
                u.Nickname = jToken["nickname"].ToString();
                u.Signature = jToken["signature"].ToString();
                u.AvatarUrl = jToken["avatarUrl"].ToString();
                if(jToken["backgroundUrl"] is not null)
                    u.BackgroudUrl = jToken["backgroundUrl"].ToString();
                if(jToken["createTime"] is not null)
                    u.CreateTime = DateTime.FromBinary((long)jToken["createTime"]);

            });

        }
        public static bool IsAvailableArray(this JToken jToken)
        {
            return jToken is not null && jToken.HasValues;
        }
    }
}
