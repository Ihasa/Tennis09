using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tennis01.Scenes
{
    class ScenePlayTutorial:Scene
    {
        ScenePlaying scene;
        bool update;
        bool end;
        public ScenePlayTutorial(ScenePlaying s,bool update)
            : base(s.Camera,false,"")
        {
            end = false;
            scene = s;
            this.update = update;
        }
        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            scene.Draw(gameTime);

            base.Draw(gameTime);
        }
        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if(update)
                scene.Update(gameTime);
            base.Update(gameTime);
        }
        public override Scene NextScene
        {
            get 
            {
                if(end)
                    return scene;
                return null;
            }
        }
    }
}
