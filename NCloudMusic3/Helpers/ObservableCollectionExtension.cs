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

        public IDisposable BatchUpdate(NotifyCollectionChangedAction updateAction = NotifyCollectionChangedAction.Reset)
        {
            _suppressNotification = true;
            return new UpdateToken(this, updateAction);
        }

        private class UpdateToken : IDisposable
        {
            private RangeObservableCollection<T> oc;
            private NotifyCollectionChangedAction actionType;
            public UpdateToken(RangeObservableCollection<T> collection, NotifyCollectionChangedAction actionType = NotifyCollectionChangedAction.Reset)
            {
                oc = collection ?? throw new ArgumentNullException(nameof(collection));
                this.actionType = actionType;
            }
            public void Dispose()
            {
                oc._suppressNotification = false;
                oc.OnCollectionChanged(new NotifyCollectionChangedEventArgs(actionType));
            }
        }
    }
}