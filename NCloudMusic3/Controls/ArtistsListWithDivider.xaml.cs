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
using static System.Runtime.InteropServices.JavaScript.JSType;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NCloudMusic3.Controls
{
    public sealed partial class ArtistsListWithDivider : UserControl
    {
        public Artist[] Artists { get => (Artist[])GetValue(ArtistsProperty); set=>SetValue(ArtistsProperty, value); }
        public static readonly DependencyProperty ArtistsProperty =
            DependencyProperty.Register(
                nameof(Artists),
                typeof(Artist[]),
                typeof(ArtistsListWithDivider),
                new PropertyMetadata(null, (s, e) =>
                {
                    if(e.NewValue is Artist[] ats)
                    {
                        (s as ArtistsListWithDivider).With(el =>
                        {
                            el.button.Content = ats[0].Name;
                            el.button.Tag = ats[0].Id;
                            
                            el.artistsList.ItemsSource = H.ArtistsNoHead(ats);
                        });
                    }
                })
            );


        public ArtistsListWithDivider()
        {
            this.InitializeComponent();

            artistsList.DataContext = this;
        }

        private void button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;

            //App.Instance.NavigateToAlbum((ulong)(sender as Button).Tag);
        }
    }

    public static class H
    {
        public static Artist[] ArtistsNoHead(Artist[] artists)
        {
            return artists.Skip(1).ToArray();
        }
    }
}
