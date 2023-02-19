using Microsoft.UI.Xaml;
using NCloudMusic3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCloudMusic3.Helpers
{
    public static class XamlHelper
    {
        public static bool CollectionToVisibility<T>(IList<T> ls) => ls.Count > 0;
        public static bool CollectionToVisibility<T>(T[] ls) => ls.Length > 0;
        public static string SecondsToString(double milliseconds)
        {
            var ts = TimeSpan.FromMilliseconds(milliseconds);
            if (ts.TotalHours > 1)
                return ts.ToString("g");
            else return ts.ToString(@"mm\:ss");
        }
        public static bool Reverse(bool boolean) => !boolean;
        public static string JoinArtists(Artist[] artists)
        {
            if (artists is not null && artists.Length > 0)
            {
                return string.Join(" / ", artists.Select(a => a.Name));
            }
            else return null;
        }
        public static Visibility WhenStringIsNullOrEmpty(string str) => string.IsNullOrEmpty(str) ? Visibility.Visible : Visibility.Collapsed;
        public static Visibility WhenStringIsNotNullNorEmpty(string str) => string.IsNullOrEmpty(str) ? Visibility.Collapsed : Visibility.Visible;
        public static Visibility WhenObjectIsNull(object obj) => obj is null ? Visibility.Visible : Visibility.Collapsed;
        public static Visibility WhenObjectIsNotNull(object obj) => obj is not null ? Visibility.Visible : Visibility.Collapsed;
    }
}
