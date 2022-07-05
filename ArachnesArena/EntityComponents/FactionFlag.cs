using Microsoft.Xna.Framework;

namespace ArachnesArena
{
    public enum Faction
    {
        Neutral,
        Blue,
        Red
    }
    class FactionFlag : ECSComponent
    {
        public Faction faction;
        public FactionFlag(ECSEntity attachee, Faction faction = Faction.Neutral)
            : base(attachee)
        {
            this.faction = faction;
        }

        public override void Update(GameTime gameTime)
        {

        }

        public bool IsFaction(Faction fac)
        {
            return faction == fac;
        }

    }
}
