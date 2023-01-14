// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using NCloudMusic3.Helpers;
using NCloudMusic3.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT;
using NCloudMusic3.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NCloudMusic3.Pages
{
    
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlayListDetailPage : Page
    {
        public MusicListVM MusicListInfo { get; set; } = new();
        RangeObservableCollection<Music> ListMusicDetail { get; set; } = new();
        RangeObservableCollection<ulong> ListMusicId { get; set; } = new();

        public PlayListDetailPage()
        {
            this.InitializeComponent();

            

            ListMusicId.CollectionChanged += async (e, s) =>
            {
                var ls = await App.Instance.GetOrRequestNewMusic(ListMusicId);
                ListMusicDetail.Clear();
                foreach (var i in 0..ls.Count)
                {
                    ls[i].Num = i + 1;
                    ListMusicDetail.Add(ls[i]);
                }
            };
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);


            if (e.Parameter is ulong pid)
            {
                
                var pl = await App.Instance.GetPlaylistInfo(pid);
                MusicListInfo.SetModel(pl);

                SetMusicList(pid);
            }
            else if (e.Parameter is AlbumNavigator { Id: ulong albumId })
            {
                // TODO 
                var (info, list) = await App.Instance.GetAlbumInfo(albumId);
            }
        }

        private async void SetMusicList(ulong aid)
        {
            var ls = await App.Instance.GetMusicList(aid, ls => ReplaceMusicList(aid, ls), DispatcherQueue);
            ReplaceMusicList(aid, ls);
            async void ReplaceMusicList(ulong aid, List<ulong> ls)
            {
                // ObservableCollection 似乎行为与 Set 一致。
                if (ListMusicId.SequenceEqual(ls))
                {
                    return;
                }

                await App.Instance.PrepareMusic(ls);

                ListMusicId.Clear();
                ListMusicId.AddRange(ls);

                if (aid == App.Instance.UserProf.LikeListId)
                {
                    //AlbumMusicId.AddRange(App.Instance.LikeSongList);
                    App.Instance.LikeSongList.Clear();
                    App.Instance.LikeSongList.AddRange(ls);
                }
            }
        }

        

        
    }
    public static class H
    {
        public static bool CollectionToVisibility<T>(IList<T> ls) => ls.Count > 0;
        public static bool CollectionToVisibility<T>(T[] ls) => ls.Length > 0;
        public static string SecondsToString(double milliseconds)
        {
            var ts = TimeSpan.FromMilliseconds(milliseconds);
            if (ts.TotalHours > 1)
                return ts.ToString("g");
            else return ts.ToString(@"mm\:ss");
        }
        public static bool Reverse(bool boolean) => !boolean;

    }
}
