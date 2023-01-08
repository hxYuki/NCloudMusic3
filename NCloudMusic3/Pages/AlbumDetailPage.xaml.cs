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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NCloudMusic3.Pages
{
    class MiscData : NotifyPropertyChanged
    {
        private bool showToggleButton;
        private bool isExpended = false;
        private GridLength blockHeight = new GridLength(41);
        private string extendButtonText;

        public bool ShowToggleButton
        {
            get => showToggleButton; set
            {
                showToggleButton = value; RaisePropertyChanged();
            }
        }

        public bool IsExpended
        {
            get => isExpended; set
            {
                isExpended = value; RaisePropertyChanged();
            }
        }

        public GridLength BlockHeight
        {
            get => blockHeight; set
            {
                blockHeight = value; RaisePropertyChanged();
            }
        }

        public string ExtendButtonText
        {
            get => extendButtonText; set
            {
                extendButtonText = value; RaisePropertyChanged();
            }
        }
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AlbumDetailPage : Page
    {
        public MusicList AlbumData { get; set; } = new();
        RangeObservableCollection<Music> AlbumMusicDetail { get; set; } = new();
        RangeObservableCollection<ulong> AlbumMusicId { get; set; } = new();

        MiscData MiscData { get; set; } = new();

        public AlbumDetailPage()
        {
            this.InitializeComponent();

            MiscData.ExtendButtonText = Resources["expendButtonText"].ToString();

            AlbumMusicId.CollectionChanged += async (e, s) =>
            {
                var ls = await App.Instance.GetOrRequestNewMusic(AlbumMusicId);
                AlbumMusicDetail.Clear();
                foreach(var i in 0..ls.Count)
                {
                    ls[i].Num = i+1;
                    AlbumMusicDetail.Add(ls[i]);
                }
            };
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);



            if (e.Parameter is ulong aid)
            {
                AlbumData.Update(App.AlbumListCache[aid]);

                SetMusicList(aid);
            }
        }

        private async void SetMusicList(ulong aid)
        {
            var ls = await App.Instance.GetMusicList(aid, (ls) => ReplaceMusicList(aid, ls), DispatcherQueue);
            ReplaceMusicList(aid, ls);
            async void ReplaceMusicList(ulong aid, List<ulong> ls)
            {
                await App.Instance.PrepareMusic(ls);

                // ObservableCollection 似乎行为与 Set 一致。
                if (AlbumMusicId.SequenceEqual(ls))
                {
                    return;
                }

                AlbumMusicId.Clear();
                AlbumMusicId.AddRange(ls);

                if (aid == App.Instance.User.LikeListId)
                {
                    //AlbumMusicId.AddRange(App.Instance.LikeSongList);
                    App.Instance.LikeSongList.Clear();
                    App.Instance.LikeSongList.AddRange(ls);
                }
            }
        }

        private void descriptionBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MiscData.ShowToggleButton = descriptionBlock.ActualHeight > 50;
        }

        private void toggleExtendButton_Click(object sender, RoutedEventArgs e)
        {
            if (MiscData.BlockHeight.IsAuto)
            {
                MiscData.BlockHeight = new GridLength(41);
                MiscData.ExtendButtonText = Resources["expendButtonText"].ToString();
            }
            else
            {
                MiscData.BlockHeight = GridLength.Auto;
                MiscData.ExtendButtonText = Resources["collapseButtonText"].ToString();

            }
        }

        private void MusicTapped(object sender, TappedRoutedEventArgs e)
        {
            var music = App.SongCache[sender.As<ListViewItem>().Tag.As<ulong>()];

            App.Instance.SetPlayList(AlbumMusicDetail);
            App.Instance.PlayMusic(music);
        }

        
    }
    public static class H
    {
        public static bool CollectionToVisibility<T>(IList<T> ls) => ls.Count > 0;
        public static bool CollectionToVisibility<T>(T[] ls) => ls.Length > 0;
        public static string SecondsToString(double milliseconds)
        {
            var ts = TimeSpan.FromMilliseconds(milliseconds);
            if(ts.TotalHours>1)
                return ts.ToString("g");
            else return ts.ToString(@"mm\:ss");
        }
        public static bool Reverse(bool boolean) => !boolean;
    }
}
