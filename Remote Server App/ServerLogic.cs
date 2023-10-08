using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Quic;
using System.Linq.Expressions;
using System.Collections.Concurrent;
using System.Security.Cryptography;

/* Client id based on Clients connected (not a great idea)
 * 
 * 
 * Server Connection Codes:
 * Waiting for client to connect... - No connections made yet
 * Full - Do no accept anymore connections
 * Another Player Needed - One more connection needed and allowed
 * Disconnect - Safe disconnect message to client
 * statusRequest - Client requests server status
 * id_request - Client requests its ID
*/
namespace Remote_Server_App
{
    /// <summary>
    //! The world of concurrency is a world in which I found only terror, despair and doom
    /// Main class that creates a TCP server
    /// </summary>
    public class ServerLogic
    {
        private int clientCount;                                                            //Todo Internal variable for keeping track of the connected clients
        private string serverStatus;                                                        //Todo Internal variable for storing the server status    
        private ConcurrentDictionary<int, ConcurrentQueue<string>> fromClient = new();      //Todo ConcurrentDictonary for storing and queueing client messages
        private ConcurrentDictionary<int, ConcurrentQueue<string>> toClient = new();        //Todo ConcurrentDictonary for storing and queueing server responses
        public string ServerStatus => Volatile.Read(ref serverStatus);                      //Todo Property for reading the server status
                                                                                            //Todo [Volatile.Read] used for concurrency situations
        public int ClientCount => Volatile.Read(ref clientCount);                           //Todo Property for reading the client count

        private TcpListener Listener;                                                       //Todo Property storing the server listener
        public ServerLogic()                                                                //Todo Create the object and set the connection parameters
        {
            Listener = new TcpListener(IPAddress.Any, 1234);
            clientCount = 0;

        }
        public async Task ServerRun()                                                       //Todo Async function that runs the server
        {
            await Task.Yield();                                                             //Todo Push in the background

            Console.WriteLine("Waiting for client to connect...");
            while (true)                                                                    //Todo As long as the application is running
            {
                try
                {
                    if (ClientCount <= 2)                                                   //Todo Check how many players are connected and allow only if player count 
                    {                                                                       //Todo is under 2
                        var client = await ClientConnect();                                 //Todo Await client connection
                        int client_id = ClientCount;                                        //Todo Establish client id base on connection
                        ClientHandleWrite(client, client_id);                               //Todo Handle reading and writing asynchronously
                        ClientHandleRead(client, client_id);
                        foreach (var clnt in toClient) clnt.Value.Enqueue(serverStatus);    //Todo Send status to every client
                        ClientHandleForceDisconnect(client, client_id);                     //Todo Handle force disconnections
                    }
                }
                catch(Exception ex) { Console.WriteLine(ex.ToString()); }
                
            }
        }

        private async Task ClientHandleForceDisconnect(TcpClient client, int client_id)    //Todo Async method for handling force disconneciton
        {
            await Task.Yield();                                                            //Todo Push to background
            bool force_quit = false;
            while (!force_quit)
            {
                try { client.GetStream(); }                                                //Todo As long as the server is able to get the client network stream
                catch                                                                      //Todo the clinet is connected otherwise force disconnection
                {
                    force_quit = true;
                    toClient[client_id].Clear();                                           //Todo Clear every Dictonary slot for that client, decrement count and change server status
                    fromClient[client_id].Clear();
                    Interlocked.Decrement(ref clientCount);
                    if (ClientCount == 1) serverStatus = "Server Status: Another Player Needed";
                    else if (ClientCount == 0) serverStatus = "Server Status: Empty";
                    foreach (var clnt in toClient) clnt.Value.Enqueue(serverStatus);
                    Console.WriteLine($"Client Force Disconnect. Number of players: {ClientCount}");
                }
            }

        }
        
        

        private async Task<TcpClient> ClientConnect()                                     //Todo Async method for accepting client connections
        {
            Listener.Start();                                                             //Todo Start listening for connecitons
            var client = await Listener.AcceptTcpClientAsync();
            Interlocked.Increment(ref clientCount);                                       //Todo After connection was accepted increment count and change server status
            Console.WriteLine($"{((IPEndPoint)client.Client.RemoteEndPoint).Address} has connected at {DateTime.Now}!");
            Console.WriteLine(ClientCount);
            
            if (ClientCount == 2) serverStatus = "Server Status: Full";
            else if (ClientCount == 1) serverStatus = "Server Status: Another Player Needed";
            return client;
        }

        private async Task ClientHandleWrite(TcpClient client, int client_id)             //Todo Async method for handling server writing to client
        {
            await Task.Yield();                                                             

            toClient[client_id] = new ConcurrentQueue<string>();                          //Todo Initialize a new ConcurrentQueue
            using var client_stream = client.GetStream();                                 //Todo Get player stream and create writer object
            using var writer = new BinaryWriter(client_stream);
            bool client_connected = true;

            while(client_connected)                                                       //Todo As long as the client is connected
            {
                if (toClient[client_id].TryDequeue(out string serverMsg))                 //Todo Dequeue message from queue if possible
                {
                    Console.WriteLine($"Writing to client {client_id}: {serverMsg}");
                    switch (serverMsg)                                                    //Todo Act based on the message
                    {
                        case "quit":                                                      //Todo Send disconnection request reply
                            Console.WriteLine($"{((IPEndPoint)client.Client.RemoteEndPoint).Address} has disconnected at {DateTime.Now}!");
                            if (ClientCount == 1) serverStatus = "Server Status: Another Player Needed";
                            else serverStatus = "Server Status: Empty";
                            foreach (var clnt in toClient) clnt.Value.Enqueue(serverStatus);
                            writer.Write("Disconnect");
                            client_connected = false;
                            break;
                        case "statusRequest":
                            Console.WriteLine(ServerStatus);
                            writer.Write(ServerStatus);
                            break;
                        default:
                            writer.Write(serverMsg);
                            break;
                    }

                    writer.Flush();
                }
            }

            toClient[client_id].Clear();                                                  //Todo After client disconnected clear the dictonary slot and decrement client count
            Interlocked.Decrement(ref clientCount);
            Console.WriteLine($"Number of players: {ClientCount}");
        }

        private async Task ClientHandleRead(TcpClient client, int client_id)             //Todo Async method for handling the reading from each client
        {
            await Task.Yield();

            var current_client = client;                                                 //Todo Get client stream and create reader
            using var client_stream = current_client.GetStream();
            using var serverReader = new BinaryReader(client_stream);
            bool client_connect = true;
            fromClient[client_id] = new ConcurrentQueue<string>();

            while (client_connect)                                                       //Todo As long as the client is connected
            {
                fromClient[client_id].Enqueue(serverReader.ReadString());                //Todo Enqueue possible client message
               
                if(fromClient[client_id].TryDequeue(out string? clientMsg))              //Todo If dequeuing was possible act accordingly
                {
                    Console.WriteLine($"Reading from client {client_id}: {clientMsg}");
                    switch (clientMsg)
                    {
                        case "quit":
                            toClient[client_id].Enqueue(clientMsg);
                            client_connect = false;
                            break;
                        case "id_request":
                            toClient[client_id].Enqueue(client_id.ToString());
                            break;
                        case "status":
                            toClient[client_id].Enqueue("statusRequest");
                            break;
                        case null: break;
                        default:                                                       //Todo Client sends a move pack, verify move pack and send to the other client
                            IList<string> tokens = clientMsg.Split(' ');
                            if (tokens.Count > 3) throw new FormatException("Invalid Modification");
                            foreach (var player in toClient)
                                if (player.Key != client_id)
                                {
                                    player.Value.Enqueue(clientMsg);
                                    break;
                                }
                            break;
                    }
                }
            }
            fromClient[client_id].Clear();                                            //Todo After client disconnected clear Dictonary slot

        }
    }
}
