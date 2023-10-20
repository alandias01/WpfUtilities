using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;

namespace WpfUtilities.Collections
{
    public class BulkObservableCollection<T> : ObservableCollection<T>
    {
        private bool shouldRaiseCollectionChanged = true;
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (this.shouldRaiseCollectionChanged)
                base.OnCollectionChanged(e);
        }

        public void AddRange(IEnumerable<T> list)
        {
            this.shouldRaiseCollectionChanged = false;
            foreach (T item in list)
            {
                this.Add(item);
            }
            this.shouldRaiseCollectionChanged = true;
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
