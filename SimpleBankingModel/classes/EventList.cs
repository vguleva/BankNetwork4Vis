﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBankingModel.classes
{
    sealed class EventList<T>:List<T>
    {
        public delegate void MethodContainer(T item);
        public event MethodContainer OnAdd;
        public event MethodContainer OnRemove;

        public new void Add(T item)
        {
            OnAdd(item);
            base.Add(item);
        }

        public new void Remove(T item)
        {
            OnRemove(item);
            base.Remove(item);
        }

        public new void RemoveAll(Predicate<T> match)
        {
            var itemsToRemove = FindAll(match);
            foreach (var item in itemsToRemove)
                Remove(item);
        }
        
    }
}
