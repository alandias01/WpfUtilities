﻿using System.Collections.Generic;
using System.Collections.Specialized;

namespace WpfUtilities.Collections
{
    public class ObservableQueue<T> : Queue<T>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public ObservableQueue(IEnumerable<T> items) : base(items)
        {            
        }
        
        public new void Enqueue(T item)
        {
            base.Enqueue(item);
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add, item));
        }

        public new T Dequeue()
        {
            var item = base.Dequeue();
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove, item));
            return item;
        }
    }
}
