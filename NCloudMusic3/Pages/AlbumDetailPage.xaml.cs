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
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NCloudMusic3.Pages
{
    class MiscData : NotifyPropertyChanged
    {
        private bool showToggleButton;
        private bool isExpended = false;

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
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AlbumDetailPage : Page
    {
        public MusicList AlbumData { get; set; } = new ();

        MiscData MiscData { get; set; } = new();

        public AlbumDetailPage()
        {
            this.InitializeComponent();

            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            

            if (e.Parameter is ulong aid)
            {
                AlbumData.Update(App.AlbumListCache[aid]);
            }
        }

        private void descriptionBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MiscData.ShowToggleButton = descriptionBlock.ActualHeight > 50;
        }
    }
}
