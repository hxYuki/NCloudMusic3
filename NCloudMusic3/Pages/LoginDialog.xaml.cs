// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT;
using NCloudMusic3.Helpers;
using NeteaseCloudMusicApi;
using System.Text.RegularExpressions;
using System.Diagnostics;
using PInvoke;
using System.Text.Json.Nodes;
using System.Text.Json;
using NCloudMusic3.Models;
using System.Threading.Tasks;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NCloudMusic3.Pages
{
    internal class LoginDialogViewModel : NotifyPropertyChanged
    {
        private bool isLoading = false;

        public bool IsLoading
        {
            get => isLoading; set
            {
                isLoading = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsIdling));

            }
        }

        public bool IsIdling
        {
            get => !isLoading; set
            {
                isLoading = !value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsLoading));
            }
        }
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginDialog : ContentDialog
    {
        public LoginDialog()
        {
            this.InitializeComponent();

            this.Opened += LoginDialog_Opened;
            
        }
        internal LoginDialogViewModel LoginDialogVM { get; set; } = new();

        private void LoginDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            if (App.Instance.LocalSettings.Values["defaultId"] is not null)
            {
                idBox.Text = App.Instance.LocalSettings.Values["defaultId"] as string;
            }
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if(idBox.Text == "")
            {
                args.Cancel = true;
                errorMessageBox.Text = "«Î ‰»Î” œ‰ªÚ ÷ª˙∫≈¬Î°£";

                App.Instance.LocalSettings.Values["defaultId"] = idBox.Text;

                return;
            }
            else if(passwordBox.Password == "")
            {
                args.Cancel = true;
                errorMessageBox.Text = "«Î ‰»Î√‹¬Î°£";
                return;
            }

            Regex rx = PhoneRegex();

            var isEmail = !rx.IsMatch(idBox.Text);
            var queries = new Dictionary<string, object>();
            ContentDialogButtonClickDeferral deferral = args.GetDeferral();
            if(isEmail)
            {
                queries["email"] = idBox.Text.Trim();
            }else queries["phone"] = idBox.Text.Trim();
            queries["password"] = passwordBox.Password.Trim();

            
            var results = await App.Instance.api.RequestAsync(isEmail?CloudMusicApiProviders.Login:CloudMusicApiProviders.LoginCellphone, queries, false);
            
            if (((int)results["code"]) != 200)
            {
                errorMessageBox.Text = results["message"].ToString();

                args.Cancel = true;
                deferral.Complete();
                return;
            }
                
            JsonSerializerOptions serdeOptions = new() { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault };
            App.Instance.LocalSettings.Values["CookieCount"] = App.Instance.api.Cookies.Count;
            foreach (var i in 0..App.Instance.api.Cookies.Count)
            {
                App.Instance.LocalSettings.Values["Cookies-" + i] = JsonSerializer.Serialize(App.Instance.api.Cookies[i], serdeOptions);
            }

            
            //args.Cancel = true;
            deferral.Complete();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (idBox.Text == "")
            {
                errorMessageBox.Text = "«Î ‰»Î” œ‰ªÚ ÷ª˙∫≈¬Î°£";

                App.Instance.LocalSettings.Values["defaultId"] = idBox.Text;

                return;
            }
            else if (passwordBox.Password == "")
            {
                errorMessageBox.Text = "«Î ‰»Î√‹¬Î°£";
                return;
            }

            errorMessageBox.Text = "";

            LoginDialogVM.IsLoading = true;


            Regex rx = PhoneRegex();

            var isPhone = rx.IsMatch(idBox.Text);
            var queries = new Dictionary<string, object>();

            if (isPhone)
            {
                queries["phone"] = idBox.Text.Trim();
            }
            else queries["email"] = idBox.Text.Trim();
            queries["password"] = passwordBox.Password.Trim();

            var results = await App.Instance.api.RequestAsync(isPhone ? CloudMusicApiProviders.LoginCellphone : CloudMusicApiProviders.Login, queries, false);

            if (((int)results["code"]) != 200)
            {
                errorMessageBox.Text = results["message"].ToString();
                LoginDialogVM.IsLoading = false;
                return;
            }

            JsonSerializerOptions serdeOptions = new() { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault };
            App.Instance.LocalSettings.Values["CookieCount"] = App.Instance.api.Cookies.Count;
            foreach (var i in 0..App.Instance.api.Cookies.Count)
            {
                App.Instance.LocalSettings.Values["Cookies-" + i] = JsonSerializer.Serialize(App.Instance.api.Cookies[i], serdeOptions);
            }

            LoginDialogVM.IsLoading = false;
            this.Hide();
        }

        [GeneratedRegex("^1\\d{10}")]
        private static partial Regex PhoneRegex();

        private void CustomCloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
