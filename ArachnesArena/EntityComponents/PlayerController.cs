using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ArachnesArena
{
    class PlayerController : ECSComponent
    {
        private Queue<Command> commandQueue;
        public PlayerController(ECSEntity attachee)
            : base(attachee)
        {
            commandQueue = new Queue<Command>();
        }

        public override void Update(GameTime gameTime)
        {
            if (commandQueue.Count != 0)
                commandQueue.Peek().Update(gameTime, parent);
            else
                IdleUpdate();
        }

        private void IdleUpdate()
        {
            foreach (var unit in Game1.allGameUnits)
            {
                if (parent.transform.DistanceTo(unit) < 1f && !parent.FindComponent<FactionFlag>().IsFaction(unit.FindComponent<FactionFlag>().faction))
                {
                    ((Game1)parent.Game).IssueCommand(new AttackTarget(unit), parent);
                }
            }
        }
        public void ClearCommands()
        {
            commandQueue.Clear();
        }

        public void QueueCommand(Command command)
        {
            commandQueue.Enqueue(command);
        }

        public void SingleCommand(Command command)
        {
            commandQueue.Clear();
            commandQueue.Enqueue(command);
        }
        public void NextCommand()
        {
            commandQueue.Dequeue();
        }
    }
}
