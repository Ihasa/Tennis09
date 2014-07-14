using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
namespace Tennis01.Scenes
{
    using UIComponents;
    class SceneTransition:Scene
    {
        Scene current, next;
        Scene updateScene;
        int frames = 0;
        int nextFrames = 180;
        AnimatableLogo logo;

        public SceneTransition(Scene currentScene, Scene nextScene)
            : base(currentScene.Camera, false, "")
        {
            current = currentScene;
            next = nextScene;
            updateScene = current;
            logo = new AnimatableLogo(Game1.Textures["alpaca"], Vector2.One * 0.5f, this,
            new Animation("animation",
                new AnimationKey(0, new LogoParams(Vector2.Zero, Vector2.Zero, Color.White)),
                new AnimationKey(nextFrames / 3, new LogoParams(Vector2.Zero, Vector2.One * 20, Color.White)),
                new AnimationKey(nextFrames / 3 * 2, new LogoParams(Vector2.Zero, Vector2.One * 20, Color.White)),
                new AnimationKey(nextFrames, new LogoParams(Vector2.Zero, Vector2.Zero, Color.White))
                )
            );
            logo.Visible = true;
            AddComponents(logo);
            logo.Animate("animation",1);
        }
        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            updateScene.Draw(gameTime);
            if (frames > nextFrames / 2)
            {
                camera = next.Camera;
                next.Draw(gameTime);
            }
            base.Draw(gameTime);
        }

        public override Scene NextScene
        {
            get { if (frames++ > nextFrames) return next; return null; }
        }
    }
}
