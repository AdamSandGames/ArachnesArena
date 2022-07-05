using Microsoft.Xna.Framework;
using System;

namespace ArachnesArena
{
    [Serializable]
    class MoveToPoint : Command
    {
        public Vector3 targetPosition;
        public MoveToPoint(Vector3 targetPos)
        {
            Type = CommandType.MoveToPoint;
            targetPosition = targetPos;
        }
        public override void execute(ECSEntity actor)
        {
            base.execute(actor);
            //actor.FindComponent<Motor>().MovePoint(targetPosition);
        }

        public override void Update(GameTime gameTime, ECSEntity actor)
        {
            Vector3 Direction = targetPosition - actor.transform.Position;
            if (Direction.Length() > 0.05f)
            {
                actor.FindComponent<Motor>().Impulse(Direction, 1f);
            }
            else
            {
                actor.FindComponent<Motor>().Impulse(Vector3.Zero, 0f);
                actor.FindComponent<PlayerController>().NextCommand();
            }
        }
    }
}
