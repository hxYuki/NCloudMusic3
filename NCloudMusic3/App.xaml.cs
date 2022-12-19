// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

using NeteaseCloudMusicApi;
using NCloudMusic3.Pages;
using NCloudMusic3.Helpers;
using System.Net;
using System.Text.Json;
using NCloudMusic3.Models;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;
using NCloudMusic3.ViewModels;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NCloudMusic3
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;
        public StorageFolder LocalCacheFolder = ApplicationData.Current.LocalCacheFolder;

        const string MusicCacheFile = "music.cache";

        public CloudMusicApi api;

        public static App Instance
        {
            get
            {
                if (instance == null)
                    instance = Application.Current as App;
                return instance;
            }
        }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();


        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();

            Login = new();

            var cookieCount = LocalSettings.Values["CookieCount"];
            if (cookieCount != null)
            {
                var cookies = new CookieCollection();
                JsonSerializerOptions serdeOptions = new() { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault };
                foreach (var i in 0..(int)cookieCount)
                {
                    cookies.Add(JsonSerializer.Deserialize<Cookie>(LocalSettings.Values["Cookies-" + i] as string, serdeOptions));
                }
                api = new CloudMusicApi(cookies);
            }
            else
                api = new CloudMusicApi();

            GetUserinfo();

            var cachefile = await LocalCacheFolder.TryGetItemAsync(MusicCacheFile);
            if(cachefile is not null)
            {
                var txt = File.ReadAllText(cachefile.Path);

                SongListCache = JsonSerializer.Deserialize<ConcurrentDictionary<ulong, Music>>(txt);
            }

            LikeSongList.CollectionChanged += UpdateLikeSoneList;
        }

        

        private void UpdateLikeSoneList(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            FetchMusic(sender as IEnumerable<ulong>);
        }

        internal Window m_window;
        private static App instance;

        public bool LoginState = false;

        public LoginDialog Login;

        internal UserProfile User { get; private set; } = new();

        // 全局歌曲缓存，歌曲id - 歌曲对象
        public static ConcurrentDictionary<ulong, Music> SongListCache = new();

        // 全局歌单缓存
        public static ConcurrentDictionary<ulong, MusicList> AlbumListCache = new();

        public RangeObservableCollection<ulong> LikeSongList = new();
        public ulong LikeListId { get; set; }
        public RangeObservableCollection<MusicList> AlbumList = new();
        public RangeObservableCollection<MusicList> SubscribeAlbumList = new();


        public async void GetUserinfo()
        {
            if (LocalSettings.Values["CookieCount"] is null)
            {
                return;
            }
            var result = await api.RequestAsync(CloudMusicApiProviders.LoginStatus, null, false);


            var prof = result["profile"];
            {
                User.IsLoginUser = true;
                User.UserId = ((ulong)prof["userId"]);
                User.Nickname = prof["nickname"].ToString();
                User.AvatarUrl = prof["avatarUrl"].ToString();
                User.BackgroudUrl = prof["backgroundUrl"].ToString();
                User.Signature = prof["signature"].ToString();
                User.CreateTime = DateTime.FromBinary((long)prof["createTime"]);
            }

            UpdateMusicLists();
        }

        public async void UpdateMusicLists()
        {
            if (User.IsNotLoginUser)
            {
                throw new Exception("User Not Logged in.");
            }
            var queries = new Dictionary<string, object>()
            {
                ["uid"] = User.UserId
            };
            var likeList = await api.RequestAsync(CloudMusicApiProviders.Likelist, queries, false);
            var createdLists = await api.RequestAsync(CloudMusicApiProviders.UserPlaylist, queries, false);

            var li = likeList["ids"].ToObject<ulong[]>();

            using (LikeSongList.BatchUpdate())
            {
                foreach (var i in 0..li.Length)
                {
                    if (i < LikeSongList.Count)
                    {
                        LikeSongList[i] = li[i];
                    }
                    else
                    {
                        LikeSongList.Add(li[i]);
                    }
                }
                if (li.Length < LikeSongList.Count)
                    foreach (var i in li.Length..LikeSongList.Count)
                    {
                        LikeSongList.RemoveAt(i);
                    }
            }

            var (subscribedlists, createdlists) = createdLists["playlist"].Select(pl =>
            {

                var el = new MusicList()
                {
                    Id = ((ulong)pl["id"]),
                    Name = pl["name"].ToString(),
                    CoverImgUrl = pl["coverImgUrl"].ToString(),
                    Description = pl["description"]?.ToString(),
                    TrackCount = ((int)pl["trackCount"]),
                    IsFromSubscribe = ((bool)pl["subscribed"]),
                    Creator = new User().With(u =>
                    {

                        u.UserId = ((ulong)pl["creator"]["userId"]);
                        u.Nickname = pl["creator"]["nickname"].ToString();
                        u.Signature = pl["creator"]["signature"].ToString();
                        u.AvatarUrl = pl["creator"]["avatarUrl"].ToString();
                    
                    }) // TODO: 多个重复用户数据可以优化
                    ,
                    CreateTime = DateTime.FromBinary((long)pl["createTime"])
                };

                AlbumListCache[el.Id] = el;

                return el;
            }).Aggregate((new List<MusicList>(), new List<MusicList>()), (acc, el) => 
            {
                if (el.IsFromSubscribe)
                    acc.Item1.Add(el);
                else acc.Item2.Add(el);

                return acc;
            });
            User.LikeListId = createdlists[0].Id;
            AlbumList.AddRange(createdlists);
            SubscribeAlbumList.AddRange(subscribedlists);

            return;
        }

        public async void FetchMusic(IEnumerable<ulong> mids)
        {
            List<ulong> toFetch = new();
            foreach (var m in mids)
            {
                if (SongListCache.TryGetValue(m, out var _))
                {
                }
                else toFetch.Add(m);
            }

            if (toFetch.Count == 0)
                return;

            var likedMusic = await api.RequestAsync(CloudMusicApiProviders.SongDetail,
                    new()
                    {
                        ["ids"] = string.Join(",", toFetch.Select(id => id.ToString()))
                    }, false);

            var musicList = likedMusic["songs"].Select(s => new Music()
            {
                Id = ((ulong)s["id"]),
                Title = s["name"].ToString(),
                Album = new()
                {
                    Id = ((ulong)s["al"]["id"]),
                    Name = s["al"]["name"].ToString(),
                    PictureUrl = s["al"]["picUrl"].ToString()
                },
                Artists = s["ar"].Select(ar => new Artist()
                {
                    Id = ((ulong)ar["id"]),
                    Name = ar["name"].ToString(),
                    Alias = new Artist[1] // TODO: check artist alias
                }).ToArray(),
                Translation = s["tns"]?.ToObject<string[]>(),
                CloudInfo = (Music.CloudType)((int)s["t"]),
                Alias = s["alia"].ToObject<string[]>()
            }).ToList();

            foreach (var s in musicList)
            {
                SongListCache.TryAdd(s.Id, s);
            }

            
            var cachefile = await LocalCacheFolder.CreateFileAsync(MusicCacheFile, CreationCollisionOption.ReplaceExisting);

            await FileIO.WriteTextAsync(cachefile, JsonSerializer.Serialize(SongListCache));
            
        }

        //public async void FetchAlbums()
    }
}
