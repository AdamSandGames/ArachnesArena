using Microsoft.Xna.Framework;

namespace ArachnesArena
{
    class Combat : ECSComponent
    {
        public int Health;
        public int Damage;
        public float Movespeed;
        public float AttacksPerSecond;
        public float attackRate;
        public float AttackRange;

        public Combat(ECSEntity attachee, int HP = 2, int DMG = 1, float MVSPD = 1f, float ATSPD = 1f, float ATRNG = 0.3f)
            : base(attachee)
        {
            Health = HP;
            Damage = DMG;
            Movespeed = MVSPD;
            AttacksPerSecond = ATSPD;
            attackRate = 1 / AttacksPerSecond;
            AttackRange = ATRNG;
        }

        public override void Update(GameTime gameTime)
        {

        }

        public bool Attack(ECSEntity target)
        {
            Combat cb = target.FindComponent<Combat>();
            if (cb == null)
                return false;
            target.FindComponent<Combat>().ReceiveDamage(Damage);
            return true;
        }

        public void ReceiveDamage(int dam)
        {
            Health -= dam;
            if (Health <= 0)
            {
                Kill();
            }
        }
        public void Kill()
        {
            this.parent.Alive = false;
        }


    }
}
