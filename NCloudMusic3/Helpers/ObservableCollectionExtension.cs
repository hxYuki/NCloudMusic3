using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace NCloudMusic3.Helpers
{
    public class RangeObservableCollection<T> : ObservableCollection<T>
    {
        protected bool _suppressNotification = false;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_suppressNotification)
                base.OnCollectionChanged(e);
        }

        public void AddRange(IEnumerable<T> ts)
        {
            using (this.BatchUpdate())
            {
                foreach (T item in ts)
                {
                    this.Items.Add(item);
                }
            }
        }

        public IDisposable BatchUpdate()
        {
            _suppressNotification = true;
            return new UpdateToken(this);
        }

        private class UpdateToken : IDisposable
        {
            private RangeObservableCollection<T> oc;
            public UpdateToken(RangeObservableCollection<T> cl)
            {
                oc = cl ?? throw new ArgumentNullException(nameof(cl));
            }
            public void Dispose()
            {
                oc._suppressNotification = false;
                oc.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
    }
}