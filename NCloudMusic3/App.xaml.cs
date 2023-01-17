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
using Windows.Media.Playback;
using Windows.Media.Core;
using Windows.ApplicationModel.VoiceCommands;
using Windows.UI.Core;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;
using Windows.Storage.Pickers;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NCloudMusic3
{
    public class PlayingInfomation : NotifyPropertyChanged
    {
        private RangeObservableCollection<Music> playList = new();
        private int playingIndex;
        private Music currentPlay;
        private bool isPlaying = false;
        private bool isShuffled;
        private bool isLooping;
        private bool isLiked;
        private double duration;
        private double position;
        private bool stopAfterThisPiece;

        public RangeObservableCollection<Music> PlayList
        {
            get => playList; set
            {
                playList = value; RaisePropertyChanged();
            }
        }

        public int PlayingIndex
        {
            get => playingIndex; set
            {
                playingIndex = value; RaisePropertyChanged();
            }
        }
        public Music CurrentPlay
        {
            get => currentPlay; set
            {
                currentPlay = value; RaisePropertyChanged();
            }
        }
        public bool IsPlaying
        {
            get => isPlaying; set
            {
                isPlaying = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(IsPaused));
            }
        }
        public bool IsPaused
        {
            get => !isPlaying; set
            {
                isPlaying = !value; RaisePropertyChanged(); RaisePropertyChanged(nameof(IsPlaying));
            }
        }
        public bool IsShuffled
        {
            get => isShuffled; set
            {
                isShuffled = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(NotShuffled));
            }
        }
        public bool NotShuffled
        {
            get => !isShuffled; set
            {
                isShuffled = !value; RaisePropertyChanged(); RaisePropertyChanged(nameof(IsShuffled));
            }
        }
        public bool IsLooping
        {
            get => isLooping; set
            {
                isLooping = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(NotLooping));
            }
        }
        public bool NotLooping
        {
            get => !isLooping; set
            {
                isLooping = !value; RaisePropertyChanged(); RaisePropertyChanged(nameof(IsLooping));
            }
        }
        public bool IsLiked
        {
            get => isLiked; set
            {
                isLiked = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(NotLiked));
            }
        }
        public bool NotLiked
        {
            get => !isLiked; set
            {
                isLiked = !value; RaisePropertyChanged(); RaisePropertyChanged(nameof(IsLiked));
            }
        }

        public bool StopAfterThisPiece
        {
            get => stopAfterThisPiece; set
            {
                stopAfterThisPiece = value; RaisePropertyChanged();
            }
        }

        public double Duration
        {
            get => duration; set
            {
                duration = value;
                RaisePropertyChanged();
            }
        }
        public double Position
        {
            get => position; set
            {
                position = value;
                RaisePropertyChanged();
            }
        }
    }
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;
        public StorageFolder LocalCacheFolder = ApplicationData.Current.LocalCacheFolder;

        public Config AppConfig = new();
        //const string MusicCacheFile = "music.cache";
        //const string AlbumCacheFile = "album.cache";

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
            m_window.appWindow.Closing += AppWindow_Closing;

            LoadSettings();

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Instance.m_window);
            // Associate the HWND with the file picker
            WinRT.Interop.InitializeWithWindow.Initialize(FolderPicker, hwnd);

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

            await LoadCache();

            LoadPlayingStatus();
            LikeSongList.CollectionChanged += async (s, e) =>
            {
                await PrepareMusic(s as IEnumerable<ulong>);
            };


        }
        private void LoadSettings()
        {
            var settings = LocalSettings.Values["Settings"];
            if (settings is string config)
            {
                AppConfig = JsonSerializer.Deserialize<Config>(config);
            }
            else
            {
                AppConfig = Config.Default;
                SaveConfig();
            }
        }
        public void SaveConfig()
        {
            LocalSettings.Values["Settings"] = JsonSerializer.Serialize(AppConfig);
        }
        private async void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            args.Cancel = true;
            SaveConfig();
            await SavePlayingStatus();

            m_window.Close();
        }

        public async Task SavePlayingStatus()
        {
            var d = new JsonObject
            {
                ["playlist"] = new JsonArray(Playing.PlayList.Select(x => JsonValue.Create(x.Id)).ToArray()),
                ["playingIndex"] = Playing.PlayingIndex,
                ["random"] = Playing.IsShuffled,
                ["loop"] = Playing.IsLooping,
                ["isLiked"] = Playing.IsLiked
            };

            var f = await LocalCacheFolder.CreateFileAsync("playing.cache", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(f, d.ToString());
        }
        public async void LoadPlayingStatus()
        {
            var f = await LocalCacheFolder.TryGetItemAsync("playing.cache");
            if (f == null)
                return;

            JsonNode jd;
            try
            {
                var d = await File.ReadAllTextAsync(f.Path);
                jd = JsonObject.Parse(d);
            }
            catch
            {
                return;
            }
            Playing.With(e =>
            {
                e.PlayList.Clear();
                e.PlayList.AddRange(jd["playlist"].Deserialize<List<ulong>>().Select(id => Music.Get(id)).ToList());
                e.PlayingIndex = jd["playingIndex"].GetValue<int>();
                e.IsShuffled = jd["random"].GetValue<bool>();
                e.IsLooping = jd["loop"].GetValue<bool>();
                e.IsLiked = jd["isLiked"].GetValue<bool>();
                e.CurrentPlay = e.PlayList[e.PlayingIndex];

                SetPlayerSource(e.CurrentPlay);
            });
        }
        public async Task SaveCache()
        {
            await Music.SaveCache(LocalCacheFolder);
        }
        public async Task LoadCache()
        {
            await Music.LoadCache(LocalCacheFolder);
        }

        internal MainWindow m_window;
        private static App instance;

        public FolderPicker FolderPicker { get; private set; } = new ();
        public LoginDialog Login;

        internal UserProfile UserProf { get; private set; } = new();


        /// <summary>
        /// 请用于可能较少歌曲不在缓存中的情况
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<List<Music>> GetOrRequestNewMusic(IEnumerable<ulong> ids)
        {
            var list = new List<Music>();
            foreach (var id in ids)
            {
                if (Music.TryGet(id, out Music value))
                {
                    list.Add(value);
                }
                else
                {
                    list.Add((await FetchMusicNotCached(new[] { id }))[0]);
                }
            }

            return list;
        }


        public RangeObservableCollection<ulong> LikeSongList = new();

        public Dictionary<string, ulong> LocalMusicList = new();

        public RangeObservableCollection<MusicList> MyPlaylistsList = new();
        public RangeObservableCollection<MusicList> SubscribedPlaylistsList = new();

        public MediaPlayer Player = new();

        public MediaPlaybackSession PlaybackSession => Player.PlaybackSession;

        public PlayingInfomation Playing { get; set; } = new();

        // TODO 或许应该将原播放列表加入Playing并在关闭时保存
        List<Music> OriginMusicList = new();

        public void SetPlayList(IEnumerable<Music> list)
        {
            Playing.PlayList.Clear();

            if (Playing.IsShuffled)
            {
                OriginMusicList = list.ToList();

                Playing.PlayList.AddRange(list.GetShuffled());
            }
            else
                Playing.PlayList.AddRange(list);
        }
        public void NextMusic()
        {
            if (Playing.CurrentPlay is null || Playing.PlayList.Count == 0) return;

            PlayMusic(Playing.PlayList[Playing.PlayList.RollingNextIndex(Playing.PlayingIndex)]);
        }
        public void PreviousMusic()
        {
            if (Playing.CurrentPlay is null || Playing.PlayList.Count == 0) return;

            PlayMusic(Playing.PlayList[Playing.PlayList.RollingPreviousIndex(Playing.PlayingIndex)]);
        }
        public void InsertToNext(Music music)
        {
            if (Playing.PlayList.Contains(music))
            {
                Playing.PlayList.Move(Playing.PlayList.IndexOf(music), Playing.PlayingIndex + 1);
            }
            else Playing.PlayList.Insert(Playing.PlayingIndex + 1, music);

            Playing.PlayingIndex = Playing.PlayList.IndexOf(Playing.CurrentPlay);
        }
        public void TogglePause()
        {
            if (Playing.CurrentPlay is null) return;
            if (Playing.IsPlaying)
                Player.Pause();
            else Player.Play();
            Playing.IsPlaying = Playing.IsPaused;
        }
        public void ToggleLoop() { }//TODO 待实现
        public void ToggleStopAfterwards() { }//TODO 待实现
        public void ToggleShuffled()
        {
            if (Playing.IsShuffled)
            {
                Playing.PlayList.Clear();
                Playing.PlayList.AddRange(OriginMusicList);
                Playing.IsShuffled = false;
            }
            else
            {
                OriginMusicList = Playing.PlayList.ToList();
                Playing.PlayList.Clear();
                Playing.PlayList.AddRange(OriginMusicList.GetShuffled());
                Playing.IsShuffled = true;
            }

            Playing.PlayingIndex = Playing.PlayList.IndexOf(Playing.CurrentPlay);
        }
        public void ToggleLike() { } //TODO 待实现
        public async void PlayMusic(Music music)
        {
            // if no music in list, add to list
            if (Playing.PlayList.Count == 0)
                Playing.PlayList.Add(music);


            if (LikeSongList.Contains(music.Id))
            {
                Playing.IsLiked = true;
            }

            Playing.PlayingIndex = Playing.PlayList.IndexOf(music);
            Playing.CurrentPlay = music;
            Playing.IsPlaying = true;
            if (await SetPlayerSource(music))
            {
                Player.Play();
            }
            else { NextMusic(); }
            //Playing.Duration = Player.NaturalDuration.TotalSeconds;
        }

        private async Task<bool> SetPlayerSource(Music music)
        {
            if (music.LocalPath != null)
            {
                // play from local
                var localMusic = await StorageFile.GetFileFromPathAsync(music.LocalPath);
                Player.Source = MediaSource.CreateFromStorageFile(localMusic);
            }
            else
            {
                // fetch from remote
                var res = await api.RequestAsync(CloudMusicApiProviders.SongUrlV1, new()
                {
                    ["id"] = music.Id.ToString()
                });

                var uri = res["data"][0]["url"].ToString();
                try
                {
                    Player.Source = MediaSource.CreateFromUri(new Uri(uri));
                }
                catch
                {
                    Player.Source = null;
                    RaiseSetPlayerSourceError(music);
                    return false;
                }
            }
            return true;
        }

        TeachingTip tipHost = null;
        public void SetTipHost(TeachingTip tip) => tipHost = tip;
        public void SendTip(string Title, string Message)
        {
            if (tipHost is null) throw new ArgumentNullException(nameof(tipHost) + "未设置TipHost");

            if (tipHost.IsOpen)
            {
                tipHost.IsOpen = false;
            }
            tipHost.Title = Title;
            tipHost.Subtitle = Message;

            tipHost.IsOpen = true;
        }

        public void RaiseSetPlayerSourceError(Music source)
        {
            SendTip("播放失败", $"歌曲: {source.Title} 获取url失败");
        }

        Frame rootFrame = null;
        public void SetFrame(Frame frame) { rootFrame = frame; }
        public void PageNavigateTo(Type type, object parameter = null)
        {
            if (parameter is null)
                rootFrame.Navigate(type);
            else rootFrame.Navigate(type, parameter);
        }

        public void NavigateToAlbum(ulong albumId)
        {
            PageNavigateTo(typeof(AlbumDetailPage), albumId);
        }

        public async void GetUserinfo()
        {
            if (LocalSettings.Values["CookieCount"] is null)
            {
                return;
            }
            var result = await api.RequestAsync(CloudMusicApiProviders.LoginStatus, null, false);

            UserProf.SetModel(result["profile"].ParseUser());
            UserProf.IsLoginUser = true;

            await UpdateMusicLists();

            LikeSongList.AddRange(await GetMusicList(UserProf.LikeListId));
        }

        public async Task UpdateMusicLists()
        {
            if (UserProf.IsNotLoginUser)
            {
                throw new Exception("User Not Logged in.");
            }
            var queries = new Dictionary<string, object>()
            {
                ["uid"] = UserProf.UserId
            };
            //var likeList = await api.RequestAsync(CloudMusicApiProviders.Likelist, queries, false);
            var createdLists = await api.RequestAsync(CloudMusicApiProviders.UserPlaylist, queries, false);

            var (subscribedlists, createdlists) = createdLists["playlist"].Select(pl => pl.ParseMusicList())
                .Aggregate((new List<MusicList>(), new List<MusicList>()), (acc, el) =>
                    {
                        if (el.IsFromSubscribe)
                            acc.Item1.Add(el);
                        else acc.Item2.Add(el);

                        return acc;
                    });

            UserProf.LikeListId = createdlists[0].Id;


            MyPlaylistsList.AddRange(createdlists);
            SubscribedPlaylistsList.AddRange(subscribedlists);

            return;
        }

        // 获取歌单中的歌曲列表（优先返回缓存，并随后调用更新回调）
        public async Task<List<ulong>> GetMusicList(ulong playlistId, Action<List<ulong>> updateCallback = null, DispatcherQueue dispatcher = null)
        {
            string cachename = $"{playlistId}.list";

            StorageFile cache;
            try { cache = await LocalCacheFolder.GetFileAsync(cachename); }
            catch { cache = null; }

            if (cache != null && cache.DateCreated.AddDays(15) > DateTimeOffset.Now)
            {
                dispatcher?.TryEnqueue(async () =>
                {
                    List<ulong> list = await GetListFromNetworkAndCache(playlistId, cachename);

                    updateCallback?.Invoke(list);
                });
                var txt = await FileIO.ReadTextAsync(cache);
                return JsonSerializer.Deserialize<List<ulong>>(txt);
            }
            var list = await GetListFromNetworkAndCache(playlistId, cachename);

            return list;

            async Task<List<ulong>> GetListFromNetworkAndCache(ulong playlistId, string cachename)
            {
                var list = (await api.RequestAsync(CloudMusicApiProviders.PlaylistDetail, new() { ["id"] = playlistId.ToString() }, false))["playlist"]["trackIds"].Select(t => ((ulong)t["id"])).ToList();
                var newf = await LocalCacheFolder.CreateFileAsync(cachename, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(newf, JsonSerializer.Serialize(list));
                return list;
            }
        }

        public async Task<List<Music>> FetchMusicNotCached(IEnumerable<ulong> mids)
        {
            var musicDetail = await api.RequestAsync(CloudMusicApiProviders.SongDetail,
                    new()
                    {
                        ["ids"] = string.Join(",", mids.Select(id => id.ToString()))
                    }, false);

            var musicList = musicDetail["songs"].Select(s => s.ParseMusic()).ToList();

            return musicList;
        }

        // 获取不在缓存中的歌曲，加入缓存。
        public async Task PrepareMusic(IEnumerable<ulong> mids)
        {
            List<ulong> toFetch = new();
            foreach (var m in mids)
            {
                if (Music.TryGet(m, out var _))
                {
                }
                else toFetch.Add(m);
            }

            if (toFetch.Count == 0)
                return;

            var musicDetail = await api.RequestAsync(CloudMusicApiProviders.SongDetail,
                    new()
                    {
                        ["ids"] = string.Join(",", toFetch.Select(id => id.ToString()))
                    }, false);

            var musicList = musicDetail["songs"].Select(s => s.ParseMusic()).ToList();

        }

        internal async Task<MusicList> GetPlaylistInfo(ulong playlistId)
        {
            if (MusicList.TryGet(playlistId, out var ls)) return ls;

            var t = await api.RequestAsync(CloudMusicApiProviders.PlaylistDetail, new()
            {
                ["id"] = playlistId.ToString(),
            });
            // TODO 查看返回值结构
            throw new NotImplementedException();
        }

        internal async Task<(Album, List<Music>)> GetAlbumInfo(ulong aid)
        {
            var t = await api.RequestAsync(CloudMusicApiProviders.Album, new()
            {
                ["id"] = aid.ToString(),
            });

            var al = t["album"].ParseAlbum();
            //    Album.CreateOrUpdate(aid, a =>
            //{
            //    a.Name = t["album"]["name"].ToString();
            //    a.Description = t["album"]["description"].ToString();
            //    a.PictureUrl = t["album"]["picUrl"].ToString();
            //    a.Artists = t["album"]["artists"].Select(ar => Artist.Create((ulong)ar["id"], ar["name"].ToString()).With(a =>
            //    {
            //        a.PictureUrl = ar["picUrl"].ToString();
            //    })).ToArray();
            //});
            var ml = t["songs"].Select(j => j.ParseMusic()).ToList();

            return (al, ml);
        }

        //public async void FetchAlbums()
    }
}
