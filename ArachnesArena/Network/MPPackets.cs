using Lidgren.Network;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace ArachnesArena
{
    public enum PacketType
    {
        Command,
        Connect,
        Disconnect,
        Map,
        MatchState,
        UnitState
    }

    public interface IPacket
    {
        void SendMessage(NetOutgoingMessage message);
        void ReceiveMessage(NetIncomingMessage message);
    }

    public abstract class Packet : IPacket
    {
        public abstract void ReceiveMessage(NetIncomingMessage message);
        public abstract void SendMessage(NetOutgoingMessage message);
    }
    public class CommandPacket : Packet
    {
        public Command Command { get; set; }
        public int Tag { get; set; }

        public override void SendMessage(NetOutgoingMessage message)
        {
            message.Write((byte)PacketType.Command);
            message.Write((byte)Command.Type);

            switch (Command.Type)
            {
                case CommandType.AttackTarget:
                    message.Write(((AttackTarget)Command).target.tag);
                    break;
                case CommandType.MoveToPoint:
                    message.Write(((MoveToPoint)Command).targetPosition.X);
                    message.Write(((MoveToPoint)Command).targetPosition.Y);
                    message.Write(((MoveToPoint)Command).targetPosition.Z);
                    break;
                case CommandType.MoveToTarget:
                    message.Write(((MoveToTarget)Command).target.tag);
                    break;
                case CommandType.StopCommand:
                    break;
            }
            message.Write(Tag);
        }

        public override void ReceiveMessage(NetIncomingMessage message)
        {
            byte ct = message.ReadByte();
            switch (ct)
            {
                case (byte)CommandType.AttackTarget:
                    int target = message.ReadInt32();
                    Command = new AttackTarget(ECSEntity.GetEntityWithTagFromList(target, Game1.allGameUnits));
                    break;
                case (byte)CommandType.MoveToPoint:
                    float x = message.ReadFloat();
                    float y = message.ReadFloat();
                    float z = message.ReadFloat();
                    var vec = new Vector3(x, y, z);
                    Command = new MoveToPoint(vec);
                    Debug.WriteLine($"Command Packet Move To Point: {vec}");
                    break;
                case (byte)CommandType.MoveToTarget:
                    target = message.ReadInt32();
                    Command = new MoveToTarget(ECSEntity.GetEntityWithTagFromList(target, Game1.allGameUnits));
                    break;
                case (byte)CommandType.StopCommand:
                    Command = new StopCommand();
                    break;
            }
            Tag = message.ReadInt32();
        }
    }

    public class ConnectPacket : Packet
    {
        public string ID { get; set; }

        public override void SendMessage(NetOutgoingMessage message)
        {
            message.Write((byte)PacketType.Connect);
            message.Write(ID);
        }

        public override void ReceiveMessage(NetIncomingMessage message)
        {
            ID = message.ReadString();
        }
    }
    public class DisconnectPacket : Packet
    {
        public string Player { get; set; }

        public override void SendMessage(NetOutgoingMessage message)
        {
            message.Write((byte)PacketType.Disconnect);
            message.Write(Player);
        }

        public override void ReceiveMessage(NetIncomingMessage message)
        {
            Player = message.ReadString();
        }
    }

    public class MapPacket : Packet
    {
        public int[,] elevations { get; set; }
        public float elevStep { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public override void SendMessage(NetOutgoingMessage message)
        {
            message.Write((byte)PacketType.Map);
            message.Write(elevStep);
            message.Write(X);
            message.Write(Y);
            for (int i = 0; i < X; i++)
            {
                for (int j = 0; j < Y; j++)
                {
                    message.Write(elevations[i, j]);
                }
            }
        }

        public override void ReceiveMessage(NetIncomingMessage message)
        {
            elevStep = message.ReadFloat();
            X = message.ReadInt32();
            Y = message.ReadInt32();
            elevations = new int[X, Y];
            for (int i = 0; i < X; i++)
            {
                for (int j = 0; j < Y; j++)
                {
                    elevations[i, j] = message.ReadInt32();
                }
            }
        }
    }

    public class MatchPacket : Packet
    {
        public override void SendMessage(NetOutgoingMessage message)
        {
            message.Write((byte)PacketType.MatchState);

        }
        public override void ReceiveMessage(NetIncomingMessage message)
        {

        }

    }

    //CreateSpider(string name, int tag, Vector3 position, float scale, Faction faction)
    public class UnitPacket : Packet
    {
        public string Name { get; set; }
        public int Tag { get; set; }
        public Vector3 Pos { get; set; }
        public float Scale { get; set; }
        public Faction Faction { get; set; }

        public override void SendMessage(NetOutgoingMessage message)
        {
            message.Write((byte)PacketType.UnitState);
            message.Write(Name);
            message.Write(Tag);
            message.Write(Pos.X);
            message.Write(Pos.Y);
            message.Write(Pos.Z);
            message.Write(Scale);
            message.Write((byte)Faction);

        }
        public override void ReceiveMessage(NetIncomingMessage message)
        {
            Name = message.ReadString();
            Tag = message.ReadInt32();
            Pos = new Vector3(
                message.ReadFloat(),
                message.ReadFloat(),
                message.ReadFloat());
            Scale = message.ReadFloat();
            Faction = (Faction)message.ReadByte();
        }

    }
}
