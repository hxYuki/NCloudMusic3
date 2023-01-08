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

            foreach(var i in 0..ls.Length)
            {
                var swp = Random.Next(0, ls.Length);

                (ls[i], ls[swp]) = (ls[swp], ls[i]);
            }
            return ls;
        }
    }
}
