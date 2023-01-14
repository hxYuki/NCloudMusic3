// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

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
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NCloudMusic3.Controls
{
    public sealed partial class MusicList : UserControl
    {
        public RangeObservableCollection<Music> Musics { get => (RangeObservableCollection<Music>)GetValue(MusicsProperty); set => SetValue(MusicsProperty, value); }
        public static readonly DependencyProperty MusicsProperty =
            DependencyProperty.Register(
                nameof(Musics),
                typeof(RangeObservableCollection<Music>),
                typeof(MusicList),
                new PropertyMetadata(null, (s, e) =>
                {
                    //(s as MusicList).With(cls =>
                    //{
                    //    cls.list.ItemsSource = cls.Musics;
                    //});

                }));
        public UIElement Header { get => (UIElement)GetValue(HeaderProperty); set => SetValue(HeaderProperty, value); }
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(
                nameof(Header),
                typeof(UIElement),
                typeof(MusicList),
                new PropertyMetadata(null));

        //private RangeObservableCollection<Music> musicItems { get; set; } = new();
        public MusicList()
        {
            this.InitializeComponent();
        }

        private void AlbumButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            App.Instance.NavigateToAlbum((ulong)(sender as HyperlinkButton).Tag);
        }
        private void MusicTapped(object sender, TappedRoutedEventArgs e)
        {
            var music = Music.Get(sender.As<ListViewItem>().Tag.As<ulong>());

            App.Instance.SetPlayList(Musics);
            App.Instance.PlayMusic(music);
        }

        private void list_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            App.Instance.Player.MediaOpened += Player_MediaOpened;
        }

        private void Player_MediaOpened(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (Musics.Contains(App.Instance.Playing.CurrentPlay))
                    list.SelectedItem = App.Instance.Playing.CurrentPlay;
                // TODO: 列表上下文菜单选项记忆。
            });
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            App.Instance.Player.MediaOpened -= Player_MediaOpened;
        }
    }
}
