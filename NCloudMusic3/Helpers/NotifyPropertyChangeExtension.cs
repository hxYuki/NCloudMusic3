using NCloudMusic3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCloudMusic3.Helpers
{
    public static class NotifyPropertyChangeExtension
    {
        public static void Update<T>(this T o, T source) where T : NotifyPropertyChanged
        {
            foreach(var p in o.GetType().GetProperties())
            {
                p.SetValue(o, p.GetValue(source));
            }
        }
    }
}
