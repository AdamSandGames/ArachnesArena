using Microsoft.Xna.Framework;
using System;

namespace ArachnesArena
{
    [Serializable]
    class AttackTarget : Command
    {
        public ECSEntity target;
        private float attackCooldown = 1f;
        private float lastAttack;
        public bool CanAttack = true;
        private float range = 0f;
        public AttackTarget(ECSEntity target)
        {
            Type = CommandType.AttackTarget;
            this.target = target;
            lastAttack = 0f;
        }
        public override void execute(ECSEntity actor)
        {
            base.execute(actor);
            attackCooldown = actor.FindComponent<Combat>().attackRate;
            range = actor.FindComponent<Combat>().AttackRange;
            //actor.FindComponent<Motor>().MoveTarget(target);
        }

        public override void Update(GameTime gameTime, ECSEntity actor)
        {
            Vector3 Direction = target.transform.Position - actor.transform.Position;
            if (Direction.Length() > range)
            {
                actor.FindComponent<Motor>().Impulse(Direction, 1f);
            }
            else
            {
                actor.FindComponent<Motor>().Impulse(Vector3.Zero, 0f);
                if (lastAttack + attackCooldown <= gameTime.TotalGameTime.TotalSeconds)
                {
                    CanAttack = true;
                }
                if (CanAttack)
                {
                    //Attack
                    if (actor.FindComponent<Combat>().Attack(target))
                    {
                        lastAttack = (float)gameTime.TotalGameTime.TotalSeconds;
                        CanAttack = false;
                    }
                }
            }
        }
    }
}
