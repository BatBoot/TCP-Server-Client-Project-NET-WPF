using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using Tic_Tac_Toe_Multiplayer.Model;
using System.Windows.Input;
using Tic_Tac_Toe_Multiplayer.Handlers;
using System.Threading;

namespace Tic_Tac_Toe_Multiplayer.ViewModels
{
    public class MainWindowViewModel 
    {
        #region Messages
        public GameClient Player { get; }                                          /*Todo Keep track of the client and the changes in status or game information*/
                                                                                   //! I am terribly sorry for the monstrosities I have coded                                                            
        public IList<IList<ChangableProperty<string>>> GetTableInfo { get; }       /*Todo Matrix connecting the view table with the model table stored in GameLogic and changing properties*/
        public IList<IList<ChangableProperty<bool>>> UnlockButton { get; }         /*Todo Keep track if the button is available a.k.a plauers did not fill it*/
        public ICommand MakeMove { get; }                                          /*Todo Command handler that handles the button press from the view*/
        public string PlayerLabel { get; }                                         /*Todo Dynamic label for the player*/
        public string OpponentLabel { get; }                                       /*Todo Dynamic label for the opponent*/
        public ChangableProperty<string> DisplayServerStatus { get; }              /*Todo Changable property used to dynamically display the server status*/
        public ChangableProperty<string> DisplayTurnCount { get; }                 /*Todo Changable property used to dynamically display the turn count*/
        public ChangableProperty<int> PlayerWinCount { get; }                      /*Todo Changable property used to dynamically display player win count*/
        public ChangableProperty<int> OpponentWinCount { get; }                    /*Todo Changable property used to dynamically display opponent win count*/
        public ChangableProperty<bool> UnlockTable { get; }                        /*Todo Availability matrix for locking the buttons after being press by any player*/

        #endregion

        #region Functions

        public async Task AsyncClientRun()                                        /*Todo Asyncronious method for handling client connection to the server*/
        {
                await Player.PlayerConnect();
        }

        public MainWindowViewModel()                                              /*Todo Constructor that initialises the viewmodel and generates game info*/
        {
            PlayerLabel = "";
            OpponentLabel = "";
            Player = new GameClient();
            MakeMove = new DelegateCommand(Proccess_Move);                        /*Todo Create delegate for button command*/

            DisplayServerStatus = new();                                          /*Todo Initialise every changable property*/
            DisplayTurnCount = new();
            PlayerWinCount = new();
            OpponentWinCount = new();
            UnlockTable = new();

            Task.Run(AsyncClientRun);                                              /*Todo Start thread operation for each dynamic Property*/
            Task.Run(UpdateGameStatus);
            Task.Run(UpdateTurnCount);
            Task.Run(UpdatePlayerWinCount);
            Task.Run(UpdateOpponentWinCount);
            Task.Run(UpdateTableContent);

            while (!Player.Connected || Player.Get_ID == 0) continue;              /*Todo Wait for the player to connect and to get the ID from the server*/

            if (Player.Get_ID == 1)                                                /*Todo Based on the player ID assign the corresponding label for both players*/
            {
                PlayerLabel = $"Player X Wins:\n You";
                OpponentLabel = $"Player O Wins:\n Opponent";
            }
            else if(Player.Get_ID == 2) 
            {
                PlayerLabel = $"Player O Wins:\n You";
                OpponentLabel = $"Player X Wins:\n Opponent";
            }

            GetTableInfo = new List<IList<ChangableProperty<string>>>             /*Todo Initialise matrix that handles the contents of the buttons*/
            {
                new List<ChangableProperty<string>> {new ChangableProperty<string>(), new ChangableProperty<string>(), new ChangableProperty<string>()},
                new List<ChangableProperty<string>> {new ChangableProperty<string>(), new ChangableProperty<string>(), new ChangableProperty<string>()},
                new List<ChangableProperty<string>> {new ChangableProperty<string>(), new ChangableProperty<string>(), new ChangableProperty<string>()}
            };

            UnlockButton = new List<IList<ChangableProperty<bool>>>              /*Todo Initialise matrix that handles the availability of the buttons*/
            {                                                                    /*Todo Set everything as available at the start*/
                new List<ChangableProperty<bool>> {new ChangableProperty<bool> { Value = true}, new ChangableProperty<bool> { Value = true }, new ChangableProperty<bool> { Value = true } },
                new List<ChangableProperty<bool>> {new ChangableProperty<bool> { Value = true}, new ChangableProperty<bool> { Value = true }, new ChangableProperty<bool> { Value = true } },
                new List<ChangableProperty<bool>> {new ChangableProperty<bool> { Value = true}, new ChangableProperty<bool> { Value = true }, new ChangableProperty<bool> { Value = true } }
            };

        }


        private void Proccess_Move(object obj)                                  /*Todo Delegate function for handling button presses*/
        {
            Button button = (Button)obj;                                        /*Todo Handle the button object*/
            Player.Game.Table[Grid.GetRow(button)][Grid.GetColumn(button)] = Player.Game.PlayerSymbol.ToString(); /*Todo Change game info based on the button position*/
            string server_msg = $"{Grid.GetRow(button)} {Grid.GetColumn(button)} {Player.Game.PlayerSymbol}";     /*Todo Create move package for the server*/
            Player?.ClientHandlerWrite(Player.Client.GetStream() ,server_msg);  /*Todo Send move package to server*/
            Player.Game.TurnCount++;                                            /*Todo Increment turn count*/
            Player.Game.Check_EndGame(Grid.GetRow(button), Grid.GetColumn(button), Player.Game.PlayerSymbol);
        }

        #endregion

        #region Threading

        public async Task UpdatePlayerWinCount()                                /*Todo Asynchronous threaded function that constantly updates the Player Win Count*/
        {
            await Task.Yield();                                                 /*Todo  Push task in the background*/
            while (true)                                                        /*Todo As long as the application*/
            {
                if (Player.Connected)                                           /*Todo Wait for player to be connected*/
                {
                    if (Player.Get_ID == 1) PlayerWinCount.Value = Player.Game.XWin_Count;     /*Todo  Dynamically asign corresponding labels to player and opponent*/
                    else if (Player.Get_ID == 2) PlayerWinCount.Value = Player.Game.OWin_Count;

                }                                                               /*! Would not want my pc to blow*/
                Thread.Sleep(25);                                               /*Todo Relax the thread*/
            }
        }

        public async Task UpdateOpponentWinCount()                             /*Todo Asynchronous threaded function that constantly updates the Player Win Count*/
        {
            await Task.Yield();                                                /*Todo  Push task in the background*/
            while (true)
            {
                if (Player.Connected)
                {
                    if (Player.Get_ID == 1) OpponentWinCount.Value = Player.Game.OWin_Count;  /*Todo  Dynamically asign corresponding win count to player and opponent*/
                    else if (Player.Get_ID == 2) OpponentWinCount.Value = Player.Game.XWin_Count;
                }
                Thread.Sleep(25);
            }
        }
        public async Task UpdateTurnCount()                                     /*Todo Asynchronous threaded function that constantly updates the Turn Count*/
        {
            await Task.Yield();
            while (true)
            {
                if (Player.Connected) DisplayTurnCount.Value = $"Turn: {Player.Game.TurnCount}";  /*Todo Turn count is based on the game stored turncount*/
                Thread.Sleep(25);
            }
        }

        public async Task UpdateTableContent()                                  /*Todo Asynchronous threaded function that constantly updates button contents and their availability*/
        {
            await Task.Yield();
            while(true)
            {                                                                   /*! Inefficient but necessary*/
                if (Player.Connected && Player.Game.Table is not null)          /*Todo Iterate through the matrix storing the game table information*/
                {
                    for (int i = 0; i < 3; i++)
                        for (int j = 0; j < 3; j++)
                        {
                            GetTableInfo[i][j].Value = Player.Game.Table[i][j];  /*Todo Update displayed value*/
                            if (GetTableInfo[i][j].Value != "") UnlockButton[i][j].Value = false;  /*Todo If button content is not empty then it was pressed so lock it*/
                            else UnlockButton[i][j].Value = true;
                        }
                }
                Thread.Sleep(100);                                              /*Todo Slightly longer thread relaxation for a slighyly more expensive task */
            }
        }
        public async Task UpdateGameStatus()                                    /*Todo Asynchronous threaded function that constantly updates the Game Status*/
        {
            await Task.Yield();
            while (true)
            {
                if (Player.Connected)
                {
                    DisplayServerStatus.Value = Player.ServerStatus;           /*Todo Update server status*/
                    if (DisplayServerStatus.Value == "Server Status: Another Player Needed" || (Player.Get_ID == 1 && Player.Game.TurnCount % 2 != 0) || (Player.Get_ID == 2 && Player.Game.TurnCount % 2 == 0)) 
                        UnlockTable.Value = false;                              /*Todo Check is all players are in and lock the player table based on the turn count*/
                    else UnlockTable.Value = true;                              /*! Ugly if case but extremly useful*/
                    
                }
                Thread.Sleep(25);
            }
        }


    #endregion
}


}
