// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using NCloudMusic3.Helpers;
using NCloudMusic3.Models;
using NCloudMusic3.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NCloudMusic3.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        internal UserProfile UserProfile => App.Instance.UserProf;

        public PlayingInfomation Playing => App.Instance.Playing;

        public ObservableCollection<MusicList> CreatedList => App.Instance.MyPlaylistsList;
        public ObservableCollection<MusicList> LikedList => App.Instance.SubscribedPlaylistsList;

        

        public MainPage()
        {
            this.InitializeComponent();
            //contentFrame.Navigate(typeof(Pages.HomePage));
            root.SelectedItem = Home;
            currentPlayList.ItemsSource = Playing.PlayList;

            CreatedList.CollectionChanged += List_CollectionChanged;
            LikedList.CollectionChanged += List_CollectionChanged;

            App.Instance.Player.MediaOpened += Player_MediaOpened;
            App.Instance.Player.MediaEnded += Player_MediaEnded;
            App.Instance.Player.MediaFailed += Player_MediaFailed;
            App.Instance.PlaybackSession.NaturalDurationChanged += PlaybackSession_NaturalDurationChanged;
            App.Instance.PlaybackSession.PositionChanged += PlaybackSession_PositionChanged;
            App.Instance.PlaybackSession.PositionChanged += SliderValueUpdate;
        }

        private void SliderValueUpdate(Windows.Media.Playback.MediaPlaybackSession sender, object args)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if(!operatingSlider)
                    slider.Value = App.Instance.Player.PlaybackSession.Position.TotalMilliseconds;
            });
        }

        private void Player_MediaFailed(Windows.Media.Playback.MediaPlayer sender, Windows.Media.Playback.MediaPlayerFailedEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void Player_MediaOpened(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                currentPlayList.ScrollIntoView(Playing.CurrentPlay);
                currentPlayList.SelectedItem = Playing.CurrentPlay;
            });
        }

        private bool operatingSlider = false;
        private void PlaybackSession_PositionChanged(Windows.Media.Playback.MediaPlaybackSession sender, object args)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                Playing.Position = sender.Position.TotalMilliseconds;
                    
            });
        }

        private void PlaybackSession_NaturalDurationChanged(Windows.Media.Playback.MediaPlaybackSession sender, object args)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                Playing.Duration = sender.NaturalDuration.TotalMilliseconds;
            });
        }

        private void Player_MediaEnded(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                Playing.IsPlaying = false;

                if (Playing.StopAfterThisPiece)
                    return;

                if(Playing.IsLooping)
                {
                    App.Instance.PlayMusic(Playing.CurrentPlay);
                }

                App.Instance.NextMusic();
            });
        }

        private void List_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateUserPlayList();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            CreatedList.CollectionChanged -= List_CollectionChanged;
            LikedList.CollectionChanged -= List_CollectionChanged;

            App.Instance.Player.MediaOpened -= Player_MediaOpened;
            App.Instance.Player.MediaEnded -= Player_MediaEnded;
            
            App.Instance.PlaybackSession.NaturalDurationChanged -= PlaybackSession_NaturalDurationChanged;
            App.Instance.PlaybackSession.PositionChanged -= PlaybackSession_PositionChanged;
            App.Instance.PlaybackSession.PositionChanged -= SliderValueUpdate;

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            (e.Parameter as Window).With(wd =>
            {
                wd.SetTitleBar(AppTitleBar);      // set user ui element as titlebar
                /*wd.ExtendsContentIntoTitleBar = true;*/ // enable custom titlebar
            });

            App.Instance.SetFrame(contentFrame);
            App.Instance.SetTipHost(rootTips);
        }

        
        private void UpdateUserPlayList()
        {
            var t = createdListTitle.MenuItems[0];
            createdListTitle.MenuItems.Clear();
            createdListTitle.MenuItems.Add(t);
            foreach (var ls in 1..CreatedList.Count)
            {
                createdListTitle.MenuItems.Add(new NavigationViewItem() { Content = CreatedList[ls].Name, Tag = CreatedList[ls].Id, Icon = new ImageIcon() { Source = new BitmapImage(new Uri(CreatedList[ls].CoverImgUrl)) } });
            }

            likedListTitle.MenuItems.Clear();
            foreach (var ls in LikedList)
            {
                likedListTitle.MenuItems.Add(new NavigationViewItem() { Content = ls.Name, Tag = ls.Id, Icon = new ImageIcon() { Source = new BitmapImage(new Uri(ls.CoverImgUrl)) } });
            }
        }

        private void root_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                contentFrame.Navigate(typeof(SettingsPage));

            }
            else
            {
                var selectedItem = args.SelectedItem as NavigationViewItem;
                if (selectedItem != null)
                {
                    if (selectedItem.Tag is string tag)
                    {
                        if (tag == "ListContainer")
                            return;
                        string pageName = "NCloudMusic3.Pages." + tag;
                        Type pageType = Type.GetType(pageName);
                        contentFrame.Navigate(pageType);

                    }
                    if (selectedItem.Tag is ulong id)
                    {
                        contentFrame.Navigate(typeof(PlayListDetailPage), id);

                    }
                    // TODO: 修改用户信息导航，仅保留登录退出功能。喜欢的音乐 同样使用专辑页面-通过参数决定页面内容。
                }
            }
        }

        private void root_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if(args.InvokedItemContainer.Tag is string tag && tag == "ListContainer")
            {
                UpdateUserPlayList();
            }
        }


        private void NextMusicButton(object sender, TappedRoutedEventArgs e)
        {
            App.Instance.NextMusic();
        }

        private void PreviousMusicButton(object sender, TappedRoutedEventArgs e)
        {
            App.Instance.PreviousMusic();
        }

        private void TogglePlayButton(object sender, TappedRoutedEventArgs e)
        {
            App.Instance.TogglePause();
        }

        private void ToggleShuffledButton(object sender, TappedRoutedEventArgs e)
        {
            App.Instance.ToggleShuffled();
        }

        private void ToggleLoopButton(object sender, TappedRoutedEventArgs e)
        {
            App.Instance.ToggleLoop();
        }

        private void currentPlayList_Loaded(object sender, RoutedEventArgs e)
        {
            currentPlayList.ScrollIntoView(Playing.CurrentPlay);
            currentPlayList.SelectedItem = Playing.CurrentPlay;
        }

        private void ListViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            
            App.Instance.PlayMusic(Music.Get((ulong)(sender as ListViewItem).Tag));
        }

        private void root_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            contentFrame.GoBack();
        }


        private void slider_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            operatingSlider = false;
            operating.Text = "Idling";
            App.Instance.Player.Position = TimeSpan.FromMilliseconds((sender as Slider).Value);
            App.Instance.PlaybackSession.PositionChanged += SliderValueUpdate;
        }

        private void slider_ManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
            App.Instance.PlaybackSession.PositionChanged -= SliderValueUpdate;
            App.Instance.Player.Position = TimeSpan.FromMilliseconds(slider.Value);
        }

        private void slider_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (operatingSlider)
                operating.Text = (sender as Slider).Value.ToString();
        }

        private void slider_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            operatingSlider = true;
        }
    }
}
