using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCloudMusic3.Helpers
{
    internal static class WithExtension
    {
        public delegate void ActionRef<T>(ref T self) where T : struct;
        //public static void WithAs<U, T>(this U source, Action<T> action) where U : struct, T:class
        //{
        //    action(source as T);
        //}


        //public delegate void ActionRef<T>(ref T self) where T : struct;
        public static ref T With<T>(ref this T self, ActionRef<T> action) where T : struct
        {
            action(ref self);
            return ref self;
        }
        public static T With<T>(this T self, Action<T> action) where T : class
        {
            action(self);
            return self;
        }
        public static R With<T,R>(this T self, Func<T,R> action) where T : class
        {
            return action(self);
        }
    }
}
