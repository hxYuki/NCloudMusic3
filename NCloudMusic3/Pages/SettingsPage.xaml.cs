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
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NCloudMusic3.Pages
{
    class SettingsVM : ViewModel<Config>
    {

        //new Config Model => App.Instance.AppConfig;
        public MusicQuality PlayingQuality
        {
            get => App.Instance.AppConfig.PlayingQuality;
            set
            {
                App.Instance.AppConfig.PlayingQuality = value;
                RaisePropertyChanged();
            }
        }
        public MusicQuality DownloadQuality
        {
            get => App.Instance.AppConfig.DownloadQuality;
            set
            {
                App.Instance.AppConfig.DownloadQuality = value;
                RaisePropertyChanged();
            }
        }
        public RangeObservableCollection<string> LocalMusicFolders
        {
            get;
            set;
        } = new();
        public bool IsFolderListEmpty => LocalMusicFolders.Count == 0;
        public int SeletedIndex => 0;

        public SettingsVM()
        {
            LocalMusicFolders.CollectionChanged += (s, arg) => { 
                RaisePropertyChanged(nameof(IsFolderListEmpty));
                RaisePropertyChanged(nameof(SeletedIndex));
                App.Instance.AppConfig.LocalMusicFolders = (s as IEnumerable<string>).ToList();
            };
        }
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
            //SettingsVM.LocalMusicFolders.CollectionChanged += (s, a) =>
            //{
                
                //if (a.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                //{
                //    libList.DeselectRange(new(0, uint.MaxValue));

                //    libList.SelectedIndex = 0;
                //}
                //if((s as IEnumerable<string>).Count() > 0)
                //    libList.SelectedItem = (s as IEnumerable<string>).First();
            //};
        }

        private async void AddMusicLibrary(object sender, TappedRoutedEventArgs e)
        {
            var folder = await App.Instance.FolderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                SettingsVM.LocalMusicFolders.Add(folder.Path);
            }
        }

        private void RemoveMusicLibrary(object sender, TappedRoutedEventArgs e)
        {
            if ((sender as Button).Tag is string st)
            {
                SettingsVM.LocalMusicFolders.Remove(st);
            }
        }

        private void libList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //(sender as ListView).SelectedIndex = 0;
        }
    }
}
