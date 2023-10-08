using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Tic_Tac_Toe_Multiplayer.Handlers
{
    /// <summary>
    /// Helper Class for handling notification sending for changeable GUI data 
    //! Source: [https://learn.microsoft.com/en-us/dotnet/desktop/wpf/data/how-to-implement-property-change-notification?view=netframeworkdesktop-4.8]
    /// </summary>
    public class NotificationHandler : INotifyPropertyChanged                       //Todo Implements INotifyPropertyChanged to handle property changes notifications
    {
        public event PropertyChangedEventHandler? PropertyChanged;                  //Todo Declare event for change in property data

        public void OnPropertyChange([CallerMemberName] string propertyname = null) //Todo Function used to refresh the contents of GUI after change in property
        {                                                                           //Todo Takes in the name of the property that calls it automatically ([CallerMemberName])
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }                                                                           //Todo Invoke method of event to refresh displayed content binded to the current property
    }
}
