using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Tic_Tac_Toe_Multiplayer.ViewModels;

namespace Tic_Tac_Toe_Multiplayer.Model
{
    /// <summary>
    /// Main class that stores the game informations
    /// </summary>
    public class GameLogic
    {
        #region Variables
        public IList<IList<string>> Table { get; set; }                                          //Todo Property that stores the table informations
        public int XWin_Count { get; set; }                                                      //Todo Properties that store the win count of both
        public int OWin_Count { get; set; }                                                      //Todo player and opponent
        public int TurnCount { get; set; }                                                       //Todo Property used to store the turn count
        public Symbols PlayerSymbol { get; set; }                                                //Todo Property that stores the client symbol based on ID
        public string PlayerType { get; set; }                                                   //Todo Property that is used to create a dynamic label
        public bool GameEnd { get; set; }                                                        //Todo Property that stores whether the game ended or not

        #endregion

        #region GameFunctions


        public void Game_init(int player_id)                                                     //Todo Method to initialise the game
        { 
            XWin_Count = OWin_Count = 0;                                                         //Todo Set win count to 0

            if (player_id == 1) PlayerSymbol = Symbols.X;                                        //Todo Set player symbol
            else PlayerSymbol = Symbols.O;

            PlayerType = player_id == 1 ? "You" : "Opponent";                                    //Todo Set dynamic label party

            New_Game();                                                                          //Todo Start a new game
            
        }

        public void New_Game()                                                                   //Todo Method used to refresh the game table
        {
            GameEnd = false;                                                                     //Todo Set the GameEnd property to false

            Table = new List<IList<string>>()                                                    //Todo Create a new clean table
            {
                new List<string> { "", "", "" },
                new List<string> { "", "", "" },
                new List<string> { "", "", "" }
            };

            TurnCount = 0;                                                                       //Todo Set the turn count to 0
                
        }

        
        public void Check_EndGame(int row, int col, Symbols playerSymbol)                        //Todo Method for checking for any win condition
        {

            if (Table[row][0] == Table[row][1] && Table[row][0] == Table[row][2]) GameEnd = true;//Todo Check for a row, column or diagonal and mark the winning symbol
            else if (Table[0][col] == Table[1][col] && Table[0][col] == Table[2][col]) GameEnd = true;
            else if (row == col && Table[0][0] == Table[1][1] && Table[0][0] == Table[2][2]) GameEnd = true;
            else if ((col == 2 - row || row == 2 - col) && (Table[0][2] == Table[1][1] && Table[0][2] == Table[2][0])) GameEnd = true; 

            if (GameEnd == true)                                                                 //Todo If any player won the add to the count and start a new game
            {
                if (playerSymbol == Symbols.X) XWin_Count++;
                else OWin_Count++;
                New_Game();
            }

            if (TurnCount == 9)                                                                  //Todo Check for a stalemate 
            {
                GameEnd = true;
                New_Game();
            }

        }

        #endregion
    }
}
