using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tic_Tac_Toe_Multiplayer.Handlers
{
    /// <summary>
    /// Handler Class for creating changable properties that bind to data
    //! Source: [https://daedtech.com/wpf-and-notifying-property-change/] 
    /// </summary>
    public class ChangableProperty<T> : NotificationHandler         //Todo Implements the notification helper class
    {
        private T _value;                                           //Todo Generic class able to generate properties that are able to dynamically change on runtime
        public T Value                                              //Todo refreshing the GUI contents
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChange();                                 //Todo Everytime the property is set the function that notifies the change is called, refreshing
            }                                                       //Todo the displayed GUI data
        }
    }
}
