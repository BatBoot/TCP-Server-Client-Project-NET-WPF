using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Collections.Concurrent;
using System.Threading;
using System.Net.Http;
using System.IO;
using System.Diagnostics;
using System.Numerics;
using System.Windows.Controls;

namespace Tic_Tac_Toe_Multiplayer.Model
{
    /// <summary>
    /// Main class that handles the connection to sercer and the Client-Server communications
    /// </summary>
    public class GameClient 
    {
        #region Client Properties
        public TcpClient Client;                                                                 //Todo Client Property to store client object
        public bool Connected { get; set; }                                                      //Todo Property to check if client is connected to the server
        public string? ServerStatus { get; set; }                                                //Todo Property that stores server status
        public int Get_ID { get; set; } = 0;                                                     //Todo Property that gets ID from server                                              

        public string? Disconnect_Request { get; set; }                                          //Todo Property that stores the disconnection request
        public GameLogic Game { get; set; }                                                      //Todo Property that stores game information
        public CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();  //Todo Cancellation token for safe disconnection

        #endregion

        public async Task PlayerConnect()                                                       //Todo Async method that tries connecting the player and handle TCP method
        { 
            
            Game = new GameLogic();                                                             //Todo Initialise a game obj
            Connected = false;

            var player = new TcpClient();                                                       //Todo Create client to connect to server
            Client = player;
            try
            {
                player.Connect("localhost", 1234);                                              //Todo Try connection to server
                var playerStream = player.GetStream();                                          //Todo Get client stream to handle
                Connected = true;                                                               //Todo Mark the succesful connection
                ClientHandleRead(playerStream, cancellationTokenSource);                        //Todo Handle server reading
                ClientHandlerWrite(playerStream, "id_request");                                 //Todo Request client id
                ClientHandlerWrite(playerStream, "status");                                     //Todo Request server status
            }
            catch { throw; }                                                                    //Todo Catch connection error

            while (!cancellationTokenSource.IsCancellationRequested) await Task.Delay(25);      //Todo Keep client connected until token is cancelled           
        }
                                                                                                

        private async void ClientHandleRead(NetworkStream playerStream, CancellationTokenSource cancellation)
        {                                                                                       //Todo Async method that handles server reading
            await Task.Yield();                                                                 //Todo Push task in the background

            var reader = new BinaryReader(playerStream);                                        //Todo Create reader object
            while(!cancellation.IsCancellationRequested)
            {
                string server_reply = reader.ReadString();
                switch (server_reply)                                                           //Todo Swich based on message and handle accordingly
                {
                    case "Server Status: Full":
                        ServerStatus = server_reply;
                        Debug.WriteLine(ServerStatus);
                        break;
                    case "Server Status: Another Player Needed":
                        ServerStatus = server_reply;
                        Debug.WriteLine(ServerStatus);
                        break;
                    case "Disconnect":
                        cancellationTokenSource.Cancel();
                        break;
                    case not null:                                                             //Todo Switch case handles both id_request and move_package 
                        var move_package = server_reply.Split(' ');                            //Todo based on the amount of tokens inside [separated by ' ']
                        if (move_package.Length == 3)                                          //Todo Update opponent move on game table and check for end game condition
                        {
                            Game.Table[int.Parse(move_package[0])][int.Parse(move_package[1])] = move_package[2];
                            Symbols opponent_symbol = move_package[2] == "X" ? Symbols.X : Symbols.O;
                            Game.TurnCount++;
                            Game.Check_EndGame(int.Parse(move_package[0]), int.Parse(move_package[1]), opponent_symbol);
                        }
                        else if (move_package.Length == 1)                                     //Todo Get the id and initialize the game
                        {
                            Get_ID = int.Parse(server_reply);
                            Game.Game_init(Get_ID);
                        }

                        break;
                }
                
                
            }
        }
        public async Task ClientHandlerWrite(NetworkStream playerStream, string request)       //Todo Async method for writing to the server
        {                                                                                   
            var serverWriter = new BinaryWriter(playerStream);                                 //Todo Create writer object

            switch (request)                                                                   //Todo Swich based on message and handle accordingly
            {
                case "id_request":                                                             //Todo Every command follows TCP method
                    serverWriter.Write(request);                                               //Todo Client Write->Client Read
                    serverWriter.Flush();  
                    break;
                case "status":
                    serverWriter.Write(request);
                    serverWriter.Flush();
                    break;
                case "quit":
                    serverWriter.Write(request);
                    serverWriter.Flush();
                    break;
                case not null:
                    serverWriter.Write(request);
                    serverWriter.Flush();
            
                    break;
            }
            
            
        }
    }
}
