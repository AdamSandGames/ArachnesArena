using Microsoft.Xna.Framework;
using System;

namespace ArachnesArena
{
    public enum CommandType
    {
        AttackTarget,
        MoveToPoint,
        MoveToTarget,
        StopCommand
    }
    [Serializable]
    public class Command
    {
        public CommandType Type { get; set; }
        //~Command() { }
        public virtual void execute(ECSEntity actor)
        {
            actor.FindComponent<PlayerController>().SingleCommand(this);
        }
        public virtual void Update(GameTime gameTime, ECSEntity actor)
        {

        }

        //public virtual byte[] ToByteArray()
        //{
        //    BinaryFormatter bf = new BinaryFormatter();
        //    using(var ms = new MemoryStream())
        //    {
        //        bf.Serialize(ms, this);
        //        return ms.ToArray();
        //    }
        //}
        //public virtual Command FromByteArray()
        //{

        //}
    }
}
