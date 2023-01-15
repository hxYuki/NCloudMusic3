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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NCloudMusic3.Pages
{
    class Data : NotifyPropertyChanged
    {
        private Album albumInfo;

        public Album AlbumInfo
        {
            get => albumInfo; set
            {
                albumInfo = value; RaisePropertyChanged();
            }
        }

    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AlbumDetailPage : Page
    {
        public AlbumDetailPage()
        {
            this.InitializeComponent();
        }

        RangeObservableCollection<Music> Musics { get; set; } = new();
        Data Data { get; set; }=new();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            list.Musics = Musics;

            if (e.Parameter is ulong albumId)
            {
                // TODO 
                var (info, list) = await App.Instance.GetAlbumInfo(albumId);
                Data.AlbumInfo = info;
                Musics.AddRange(list);
            }
        }
    }
}
