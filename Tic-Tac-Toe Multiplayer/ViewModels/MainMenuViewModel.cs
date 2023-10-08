using System.Windows.Input;
using Tic_Tac_Toe_Multiplayer.Handlers;

namespace Tic_Tac_Toe_Multiplayer.ViewModels
{
    /// <summary>
    /// View Model for the Main Menu
    /// </summary>
    public class MainMenuViewModel
    {
        public ICommand MatchMake { get; set; }                                     //Todo Handle two commands [Play, Quit]

        public ICommand QuitApp { get; set; } 

        public MainMenuViewModel()                                                  //Todo Create new viewmodel and assign command delegates
        {
            MatchMake = new DelegateCommand(Execute_MatchMake);
            QuitApp = new DelegateCommand(Execute_QuitApp); 
        }
                                                                                    //Todo Method that creates a new viewmodel for the game [Play action]
        private void Execute_MatchMake(object obj) => App.Current.MainWindow.DataContext = new MainWindowViewModel();                                  

        private void Execute_QuitApp(object obj) => App.Current.Shutdown();         //Todo Method that closes the application [Quit action]

    }
}
