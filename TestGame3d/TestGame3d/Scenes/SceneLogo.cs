using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using _2DComponents;
namespace Tennis01.Scenes
{
    class SceneLogo : Scene
    {
        public SceneLogo()
            : base(new Camera(Vector3.Zero, Vector3.Zero, 45, Game.GraphicsDevice.Viewport), true, "")
        {
            AnimatableLogo logo = new AnimatableLogo(GameMain.LogoFont, Game.WindowRect, Vector2.One * 0.5f, Vector2.One * 0.5f);
            AddComponents(logo);
            logo.EndAnimating += () =>
            {
                nextScene = new SceneTitle();
            };
            
        }
        Scene nextScene = null;
        public override Scene NextScene
        {
            get { return nextScene; }
        }
    }
}
