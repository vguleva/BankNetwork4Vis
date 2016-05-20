using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBankingModel.classes
{
    internal class EventValue<T> // todo eventIterator
    {
        //public delegate void MethodContainer(T item);
        //public event MethodContainer OnAdd;
        //public event MethodContainer OnRemove;

        public delegate void ValueChangedDelegate();

        public event ValueChangedDelegate OnChange;

        // todo public Plus(){val++; OnChange();}
    

    private T _value;
        /*
        public EventValue(T value)
        {
            _value = value;
        }
         */

        
        public T Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnChange();
            }
        }

        public T ToEventFreeType()
        {
            return _value;
        }
        /*
        static public operator implicit
        private DWORD(int value)
        {
            return new DWORD(value);
        }

        static public operator implicit int(DWORD value)
        {
            return value.Value;
        }
         * 
         */
    }
}
