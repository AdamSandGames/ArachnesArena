using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ArachnesArena
{
    public class ECSEntity : GameComponent // TODO Finish Implementing Methods
    {
        // public Scene
        public List<ECSComponent> components;
        public string name;
        public int tag; // -1 == not assigned
        public Transform transform;
        public bool Alive;
        public ECSEntity(Game1 game, SpriteBatch sb, string name = "Basic Entity", int tag = -1)
            : base(game)
        {
            this.name = name;
            this.tag = tag;
            Alive = true;
            components = new List<ECSComponent>();
        }

        public override void Update(GameTime gameTime)
        {
            //CallComponentsUpdates(gameTime);
            if (this.Alive)
            {
                CallComponentsUpdates(gameTime);
            }
            base.Update(gameTime);
        }

        public void AddComponent(ECSComponent compon)
        {
            components.Add(compon);
            if (compon is Transform)
            {
                transform = (Transform)compon;
            }
        }

        public T FindComponent<T>() where T : ECSComponent
        {
            foreach (ECSComponent compon in components)
            {
                if (compon is T)
                {
                    return (T)compon;
                }
            }
            return null;
        }

        public void RemoveComponent<T>() where T : ECSComponent
        {
            foreach (ECSComponent compon in components)
            {
                if (compon is T)
                {
                    components.Remove(compon);
                }
            }

        }

        private void CallComponentsUpdates(GameTime gameTime)
        {
            foreach (ECSComponent compon in components)
            {
                compon.Update(gameTime);
            }
        }
        public static ECSEntity GetEntityWithTagFromList(int tag, IEnumerable<ECSEntity> collection)
        {
            foreach (ECSEntity ent in collection)
            {
                if (ent.tag == tag)
                {
                    return ent;
                }
            }
            return null;
        }
    }
}
