using Microsoft.Xna.Framework;
using System;

namespace ArachnesArena
{
    [Serializable]
    class StopCommand : Command
    {
        public StopCommand()
        {
            Type = CommandType.StopCommand;
        }
        public override void execute(ECSEntity actor)
        {
            base.execute(actor);
            actor.FindComponent<Motor>().Stop();
            actor.FindComponent<PlayerController>().ClearCommands();
        }

        public override void Update(GameTime gameTime, ECSEntity actor)
        {

        }
    }
}
