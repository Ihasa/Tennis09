using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
namespace Tennis01.Scenes
{
    class ScenePause:Scene
    {
        Scene scene;
        GameComponent[] components;
        public ScenePause(Scene s)
            : base(s.Camera,false,"")
        {
            scene = s;
            components = new GameComponent[Game.Components.Count];
            Game.Components.CopyTo(components, 0);
            foreach (GameComponent c in Game.Components)
            {
                c.Enabled = false;
            }
        }
        public override Scene NextScene
        {
            get 
            {
                if (Controllers[0].GetState().Pause == Input.ControlerButtonStates.Pressed)
                {
                    foreach (GameComponent c in Game.Components)
                    {
                        c.Enabled = true;
                    }
                    return scene;
                }
                return null;
            }
        }
    }
}
