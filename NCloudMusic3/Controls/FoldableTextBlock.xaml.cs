// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
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

namespace NCloudMusic3.Controls
{
    public sealed partial class FoldableTextBlock : UserControl
    {
        public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
        public static DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string), typeof(FoldableTextBlock), new PropertyMetadata(null));

        private MiscData MiscData { get; set; } = new();

        public FoldableTextBlock()
        {
            this.InitializeComponent();
            MiscData.ExtendButtonText = Resources["expendButtonText"].ToString();
            
            
        }

        private void descriptionBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(MiscData.ShowToggleButton is null)
            {
                var t = new TextBlock() { Text = "a\nb", FontSize = descriptionBlock.FontSize, FontFamily = descriptionBlock.FontFamily };
                t.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                MiscData.ShowToggleButton = descriptionBlock.ActualHeight > t.DesiredSize.Height;

                descriptionBlock.MaxLines = 2;
                descriptionBlock.TextTrimming = TextTrimming.WordEllipsis;
            }
        }

        private void toggleExtendButton_Click(object sender, RoutedEventArgs e)
        {
            if (MiscData.IsExpended)
            {
                descriptionBlock.MaxLines = 2;
                MiscData.ExtendButtonText = Resources["expendButtonText"].ToString();
            }
            else
            {
                descriptionBlock.MaxLines = 0;
                MiscData.ExtendButtonText = Resources["collapseButtonText"].ToString();
            }
            MiscData.IsExpended = !MiscData.IsExpended;
        }
    }
    class MiscData : NotifyPropertyChanged
    {
        private bool? showToggleButton;
        private bool isExpended = false;
        private double blockHeight = 41;
        private string extendButtonText;

        public bool? ShowToggleButton
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

        public double BlockHeight
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
}
