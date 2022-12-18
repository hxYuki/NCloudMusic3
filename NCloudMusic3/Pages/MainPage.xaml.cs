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
        internal UserProfile UserProfile => App.Instance.User;
        internal ulong LikeListId => App.Instance.LikeListId;

        public ObservableCollection<MusicList> CreatedList => App.Instance.AlbumList;
        public ObservableCollection<MusicList> LikedList => App.Instance.SubscribeAlbumList;

        public MainPage()
        {
            this.InitializeComponent();



            
            //contentFrame.Navigate(typeof(Pages.HomePage));
            root.SelectedItem = Home;

            CreatedList.CollectionChanged += (s, e)=> { UpdateUserPlayList(); };
            LikedList.CollectionChanged += (s, e) => { UpdateUserPlayList(); };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            (e.Parameter as Window).With(wd =>
            {
                wd.ExtendsContentIntoTitleBar = true; // enable custom titlebar
                wd.SetTitleBar(AppTitleBar);      // set user ui element as titlebar
            });
        }


        private void root_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                // TODO: navigate to settings
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
                        contentFrame.Navigate(typeof(AlbumDetailPage), id);

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
    }
}
