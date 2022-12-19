using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCloudMusic3.Helpers
{
    public static class RangeExtension
    {
        public static RangeIntEnumerator GetEnumerator(this Range range)
        {
            return new RangeIntEnumerator(range);
        }
    }

    public struct RangeIntEnumerator
    {
        private readonly Range range;
        private int _cur;


        public RangeIntEnumerator(Range range)
        {
            this.range = range;
            _cur = range.Start.Value - 1;
        }

        public int Current => _cur;

        public bool MoveNext()
        {
            _cur++;
            return _cur < range.End.Value;
        }

        public void Reset()
        {
            _cur = range.Start.Value;
        }
    }
}
