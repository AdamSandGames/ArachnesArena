using Lidgren.Network;
using System.Collections.Generic;
using System.Diagnostics;

namespace ArachnesArena
{
    public class MPServer
    {
        public Game1 myGame;
        public bool running = false;
        private NetServer _server;
        public List<string> players;
        private Dictionary<string, Faction> playerFactions;
        public MPServer(Game1 game, int port, string serverName, int maxConnections = 2)
        {
            myGame = game;
            players = new List<string>();
            playerFactions = new Dictionary<string, Faction>();

            NetPeerConfiguration config = new NetPeerConfiguration(serverName);
            config.MaximumConnections = maxConnections;
            config.Port = port;
            //config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);

            _server = new NetServer(config);
            _server.Start();
            running = true;

        }

        public void Listen()
        {
            //Debug.WriteLine("Server Listening");
            NetIncomingMessage message;

            while ((message = _server.ReadMessage()) != null)
            {
                //Logging.Info("Message recieved");

                // get list of users
                List<NetConnection> all = _server.Connections;


                switch (message.MessageType)
                {
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)message.ReadByte();
                        string reason = message.ReadString();

                        if (status == NetConnectionStatus.Connected)
                        {
                            var player = NetUtility.ToHexString(message.SenderConnection.RemoteUniqueIdentifier);
                            players.Add(player);
                            playerFactions.Add(player, Faction.Red);

                            // send player id
                            SendLocalPlayerPacket(message.SenderConnection, player);

                            SendMapData(message.SenderConnection, myGame.SimpleGameMap);

                            SendAllUnits(message.SenderConnection, Game1.allGameUnits);
                        }
                        break;
                    case NetIncomingMessageType.Data:
                        // get packet type
                        byte type = message.ReadByte();

                        // create packet
                        Packet packet;

                        switch (type)
                        {
                            case (byte)PacketType.Connect:
                                packet = new ConnectPacket();
                                packet.ReceiveMessage(message);
                                //SendPositionPacket(all, (PositionPacket)packet);
                                break;
                            case (byte)PacketType.Disconnect:
                                packet = new DisconnectPacket();
                                packet.ReceiveMessage(message);
                                //SendPlayerDisconnectPacket(all, (DisconnectPacket)packet);
                                break;
                            case (byte)PacketType.Command:
                                packet = new CommandPacket();
                                packet.ReceiveMessage(message);
                                ProcessCommand((CommandPacket)packet);
                                break;
                            case (byte)PacketType.Map:
                            case (byte)PacketType.MatchState:
                            case (byte)PacketType.UnitState:
                            default:
                                Debug.WriteLine("Unhandled Packet Type");
                                break;
                        }
                        break;
                    case NetIncomingMessageType.ConnectionApproval:
                        var data = message.ReadByte();
                        Debug.WriteLine("New connection..." + data.ToString());
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

                _server.Recycle(message);
            }
        }


        public void SendLocalPlayerPacket(NetConnection local, string player)
        {
            Debug.WriteLine("Sending player their user ID: " + player);

            NetOutgoingMessage message = _server.CreateMessage();
            new ConnectPacket() { ID = player }.SendMessage(message);
            _server.SendMessage(message, local, NetDeliveryMethod.ReliableOrdered, 0);
        }


        public void SendPlayerDisconnectPacket(List<NetConnection> all, DisconnectPacket packet)
        {
            Debug.WriteLine("Player Disconnected: " + packet.Player);

            playerFactions.Remove(packet.Player);
            players.Remove(packet.Player);

            NetOutgoingMessage message = _server.CreateMessage();
            packet.SendMessage(message);
            _server.SendMessage(message, all, NetDeliveryMethod.ReliableOrdered, 0);
        }

        public void SendMapData(NetConnection local, SimpleMap map)
        {
            Debug.WriteLine("Sending Map to Player");

            NetOutgoingMessage message = _server.CreateMessage();
            new MapPacket() { elevations = map.elevationField, elevStep = map.levelStep, X = 20, Y = 20 }.SendMessage(message);
            _server.SendMessage(message, local, NetDeliveryMethod.ReliableOrdered, 0);
        }

        public void SendAllUnits(NetConnection local, List<ECSEntity> units)
        {
            Debug.WriteLine("Sending All Units to Player");

            foreach (var unit in units)
            {
                SendUnit(local, unit);
            }
        }

        public void SendUnit(NetConnection local, ECSEntity unit)
        {
            Debug.WriteLine("Sending Unit to Player");

            NetOutgoingMessage message = _server.CreateMessage();
            new UnitPacket()
            {
                Name = unit.name,
                Tag = unit.tag,
                Pos = unit.transform.Position,
                Scale = unit.transform.Scale,
                Faction = unit.FindComponent<FactionFlag>().faction
            }.SendMessage(message);
            _server.SendMessage(message, local, NetDeliveryMethod.ReliableOrdered, 0);
        }

        public void SendCommand(Command comm, int tag)
        {
            Debug.WriteLine("Sending Command to all");

            if (_server.Connections.Count > 0)
            {
                NetOutgoingMessage message = _server.CreateMessage();
                new CommandPacket() { Command = comm, Tag = tag }.SendMessage(message);
                _server.SendMessage(message, _server.Connections, NetDeliveryMethod.ReliableOrdered, 0);
            }
        }
        public void ProcessCommand(CommandPacket packet)
        {
            Debug.WriteLine("Processing Net Command");

            myGame.NetCommandIssueByTag(packet.Command, packet.Tag);
            SendCommand(packet.Command, packet.Tag);
        }

        public void ServerShutdown()
        {
            Debug.WriteLine("Server Shutting Down");

            if (_server.Connections.Count > 0)
            {
                NetOutgoingMessage message = _server.CreateMessage();
                new DisconnectPacket() { Player = "" }.SendMessage(message);
                _server.SendMessage(message, _server.Connections, NetDeliveryMethod.ReliableOrdered, 0);
            }
        }
    }
}
