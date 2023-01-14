using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCloudMusic3.Helpers
{
    internal static class IEnumerableExtension
    {
        static Random Random = new ((int)(DateTime.Now.ToBinary() % int.MaxValue));
        public static IEnumerable<T> GetShuffled<T>(this IEnumerable<T> source)
        {
            var ls = source.ToArray();
            int n = ls.Length;
            while (n > 0)
            {
                n--;
                int k = Random.Next(n);

                (ls[n], ls[k]) = (ls[k], ls[n]);
            }
            return ls;
        }

        public static int RollingNextIndex<T>(this IEnumerable<T> source, int i) {
            if (i + 1 < source.Count())
            {
                return i + 1;
            }
            else return 0;
        }
        public static int RollingPreviousIndex<T>(this IEnumerable<T> source, int i) {
            if (i > 0)
            {
                return i - 1;
            }
            else return source.Count() - 1;
        }

    }
}
