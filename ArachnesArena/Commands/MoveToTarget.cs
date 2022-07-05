using Microsoft.Xna.Framework;
using System;

namespace ArachnesArena
{
    [Serializable]
    class MoveToTarget : Command
    {
        public ECSEntity target;
        public MoveToTarget(ECSEntity target)
        {
            Type = CommandType.MoveToTarget;
            this.target = target;
        }
        public override void execute(ECSEntity actor)
        {
            base.execute(actor);
            //actor.FindComponent<Motor>().MoveTarget(target);
        }

        public override void Update(GameTime gameTime, ECSEntity actor)
        {
            Vector3 Direction = target.transform.Position - actor.transform.Position;
            if (Direction.Length() > 0.25f)
            {
                actor.FindComponent<Motor>().Impulse(Direction, 1f);
            }
            else
            {
                actor.FindComponent<Motor>().Impulse(Vector3.Zero, 0f);
            }
        }
    }
}
