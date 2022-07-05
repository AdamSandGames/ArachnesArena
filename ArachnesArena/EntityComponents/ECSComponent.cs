using Microsoft.Xna.Framework;

namespace ArachnesArena
{
    public abstract class ECSComponent // TODO Finish Implementing Methods
    {
        public ECSEntity parent;
        public ECSComponent(ECSEntity attachee)
        {
            this.parent = attachee;
        }

        public abstract void Update(GameTime gameTime);
    }
}
