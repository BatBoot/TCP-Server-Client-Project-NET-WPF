using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Tic_Tac_Toe_Multiplayer.Handlers
{

    /// <summary>
    /// Universal Handle for Data Binding of Methods
    //! Source: [https://www.wpftutorial.net/DelegateCommand.html]
    /// </summary>
    public class DelegateCommand : ICommand                                             //Todo Handler class implements ICommand interface
    {
        private readonly Predicate<object> _canExecute;                                 //Todo Uses a Predicate and Action variables
        private readonly Action<object> _execute;

        public event EventHandler? CanExecuteChanged;                                   //Todo EventHandler for execution on request by user

        public DelegateCommand(Action<object> execute) :                                //Todo Constructors that generates a command delegate object
          this(execute, null) { }

        public DelegateCommand(Action<object> execute, Predicate<object> canExecute)   
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)                                      //Todo Method that checks if command can be executed
        {
            if (_canExecute == null)
            {
                return true;
            }

            return _canExecute(parameter);
        }

        public void Execute(object? parameter)                                         //Todo Method that executes the command
        {
            _execute(parameter);
        }

        public void RaiseCanExecuteChanged()                                          //Todo Method for updating availability of the command
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }
    }
}
