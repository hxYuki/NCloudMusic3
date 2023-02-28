// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using NCloudMusic3.Models;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using NCloudMusic3.Helpers;
using CommunityToolkit.Mvvm.Input;
using DotNext.Collections.Generic;
using System.Reflection.Metadata;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NCloudMusic3.Controls {
    public partial class ItemsVM : ObservableObject {
        [ObservableProperty]
        private bool reserved = false;

        [ObservableProperty]
        Music music;

        public RangeObservableCollection<ItemsVM> Collections { get; set; }

    }
    public partial class ReplicationMusicVM : ObservableObject {

        public RangeObservableCollection<ItemsVM> Music { get; set; }

        public ReplicationMusicVM(IEnumerable<Music> m) {
            //Music = new(m.OrderBy(m => m.Title).Select(m => new ItemsVM() { Reserved = false, Music = m }));

            var gid = m.OrderBy(m => m.Title).GroupBy(m => m.Id).Where(g => g.Key!=0 && g.Count() > 1);
            var t = m.OrderBy(m => m.Title).GroupBy(m => new { m.Title, m.Album, m.Artists });
            var res = t.Select(g => {

                var theg = gid.Where(idg => g.Any(m => m.Id == idg.Key)).FirstOrDefault()?.Except(g) ?? Enumerable.Empty<Music>();

                return g.Concat(theg);
            }).Where(g => g.Count() > 1)
            .Select(g => {
                bool first = true;
                return new ItemsVM() { Collections = new(g.OrderByDescending(m => m.BitRate).Select(m => {
                    var t = new ItemsVM() { Reserved = first, Music = m };
                    first &= false;
                    return t;
                })) };
            });
            Music = new(res);
        }


    }
    public sealed partial class HandleReplicationDialog : ContentDialog {
        ReplicationMusicVM datacontext;
        public HandleReplicationDialog(IEnumerable<Music> source) {
            this.InitializeComponent();
            datacontext = new ReplicationMusicVM(source);

            XamlRoot = App.Instance.m_window.Content.XamlRoot;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e) {
            //datacontext.SelectMusicCommand.Execute((sender as CheckBox).Tag);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            this.Hide();
        }
    }
}
