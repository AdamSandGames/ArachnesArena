
using Microsoft.Xna.Framework;

namespace ArachnesArena
{
    public interface Updatable // implements an alternative to Monogame Game.Components
    {
        void Update(GameTime gameTime);
    }
}
