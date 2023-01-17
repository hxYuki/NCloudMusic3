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
using NCloudMusic3.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NCloudMusic3.Pages
{
    class SettingsVM : ViewModel<Config>
    {

        //new Config Model => App.Instance.AppConfig;
        public MusicQuality PlayingQuality { 
            get => App.Instance.AppConfig.PlayingQuality;
            set { 
                App.Instance.AppConfig.PlayingQuality = value;
                RaisePropertyChanged(); } }
        public MusicQuality DownloadQuality { 
            get => App.Instance.AppConfig.DownloadQuality; 
            set { 
                App.Instance.AppConfig.DownloadQuality = value;
                RaisePropertyChanged(); } }
        public RangeObservableCollection<string> LocalMusicFolders
        {
            get;
            set;
        } = new();
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public List<EnumItemsSource<MusicQuality>> MusicQualities
        {
            get => EnumItemsSource<MusicQuality>.ToList();
        }

        SettingsVM SettingsVM { get; set; } = new();

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //SettingsVM.SetModel(App.Instance.AppConfig);
            SettingsVM.LocalMusicFolders.AddRange(App.Instance.AppConfig.LocalMusicFolders);
            SettingsVM.LocalMusicFolders.CollectionChanged += (s, a) =>
            {
                App.Instance.AppConfig.LocalMusicFolders = (s as IEnumerable<string>).ToList();

            };
        }

        private void AddMusicLibrary(object sender, TappedRoutedEventArgs e)
        {

        }
    }
}
