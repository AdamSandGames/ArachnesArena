using Lidgren.Network;
using System.Diagnostics;

namespace ArachnesArena
{
    public class MPClient
    {
        public Game1 myGame;
        public bool running = false;
        public string ID;
        public NetClient _client { get; set; }
        public MPClient(Game1 game, int port, string server, string serverName)
        {
            myGame = game;
            var config = new NetPeerConfiguration(serverName);
            config.AutoFlushSendQueue = false;
            //config.Port = port + 1;


            _client = new NetClient(config);
            _client.Start();

            _client.Connect(server, port);
            running = true;

        }

        public void Listen()
        {
            //Debug.WriteLine("Client Listening");
            NetIncomingMessage message;

            while ((message = _client.ReadMessage()) != null)
            {
                Debug.WriteLine("Message recieved from Server: " + message.MessageType + " " + message.LengthBytes + " bytes " + message.DeliveryMethod);

                switch (message.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        byte packetType = message.ReadByte();

                        Packet packet;

                        switch (packetType)
                        {
                            case (byte)PacketType.Connect:
                                packet = new ConnectPacket();
                                packet.ReceiveMessage(message);
                                ExtractLocalPlayerInformation((ConnectPacket)packet);
                                break;

                            case (byte)PacketType.Disconnect:
                                packet = new DisconnectPacket();
                                packet.ReceiveMessage(message);
                                DisconnectPlayer((DisconnectPacket)packet);
                                break;
                            case (byte)PacketType.Map:
                                packet = new MapPacket();
                                packet.ReceiveMessage(message);
                                ExtractMapData((MapPacket)packet);
                                break;
                            case (byte)PacketType.UnitState:
                                packet = new UnitPacket();
                                packet.ReceiveMessage(message);
                                ExtractUnit((UnitPacket)packet);
                                break;
                            case (byte)PacketType.Command:
                                packet = new CommandPacket();
                                packet.ReceiveMessage(message);
                                ProcessCommand((CommandPacket)packet);
                                break;
                            default:
                                Debug.WriteLine("Unhandled Packet Type");
                                break;
                        }
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        Debug.WriteLine("Status Changed " + message.MessageType);
                        break;
                    case NetIncomingMessageType.ConnectionApproval:
                        Debug.WriteLine("Connection Approved");
                        break;
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.ErrorMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.VerboseDebugMessage:
                        string text = message.ReadString();
                        Debug.WriteLine(text);
                        break;
                    case NetIncomingMessageType.Error:
                    case NetIncomingMessageType.UnconnectedData:
                    case NetIncomingMessageType.Receipt:
                    case NetIncomingMessageType.DiscoveryRequest:
                    case NetIncomingMessageType.DiscoveryResponse:
                    case NetIncomingMessageType.NatIntroductionSuccess:
                    case NetIncomingMessageType.ConnectionLatencyUpdated:
                    default:
                        Debug.WriteLine("Unhandled type: " + message.MessageType + " " + message.LengthBytes + " bytes " + message.DeliveryMethod);
                        break;
                }

                _client.Recycle(message);

            }
        }

        public void ExtractLocalPlayerInformation(ConnectPacket packet)
        {
            Debug.WriteLine("Local Id is: " + packet.ID);

            ID = packet.ID;
        }
        public void SendDisconnect()
        {
            Debug.WriteLine("Disconnecting from Server");

            NetOutgoingMessage message = _client.CreateMessage();
            new DisconnectPacket() { Player = ID }.SendMessage(message);
            _client.SendMessage(message, NetDeliveryMethod.ReliableOrdered);
            _client.FlushSendQueue();
        }
        public void ExtractMapData(MapPacket packet)
        {
            Debug.WriteLine("Extracting Map");

            myGame.ReplaceMap(packet.elevStep, packet.elevations);

        }

        public void ExtractUnit(UnitPacket packet)
        {
            Debug.WriteLine("Extracting Unit");

            myGame.CreateSpider(packet.Name, packet.Tag, packet.Pos, packet.Scale, packet.Faction);
        }

        public void SendCommand(Command comm, int tag)
        {
            Debug.WriteLine("Sending Command");

            NetOutgoingMessage message = _client.CreateMessage();
            new CommandPacket() { Command = comm, Tag = tag }.SendMessage(message);
            _client.SendMessage(message, NetDeliveryMethod.ReliableOrdered, 0);
            _client.FlushSendQueue();
        }
        public void ProcessCommand(CommandPacket packet)
        {
            Debug.WriteLine("Processing Net Command");

            myGame.NetCommandIssueByTag(packet.Command, packet.Tag);
        }

        public void DisconnectPlayer(DisconnectPacket packet)
        {
            Debug.WriteLine("Disconnecting...");

            myGame.Exit();
        }
    }
}
