using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tennis01.Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace Tennis01.Input
{
    class MouseController:Controller
    {
        MouseState current,last;
        Player player;
        Scenes.Camera camera;
        public MouseController(Player p,Scenes.Camera c)
            : base()
        {
            current = new MouseState();
            last = new MouseState();
            player = p;
            camera = c;
        }
        public override void Update()
        {
            last = current;
            current = Mouse.GetState();
        }
        protected override ControllerState getState()
        {
            ControllerState res = new ControllerState();
            Vector2 mouse = new Vector2(current.X,current.Y);
            Vector3 playerScreen = Scenes.Scene.Viewport.Project(player.Position, camera.Projection, camera.View, Matrix.Identity);
            Vector2 playerPosition = new Vector2(playerScreen.X, playerScreen.Y);
            Vector2 joy = mouse - playerPosition;
            GameMain.debugStr["joy"] = joy.ToString();
            if (joy.Length() > 10)
            {
                joy.Y = -joy.Y;
                res.JoyStick = Vector2.Normalize(joy);
            }
            if (current.LeftButton == ButtonState.Pressed && last.LeftButton == ButtonState.Released)
            {
                res.Button1 = ControlerButtonStates.Pressed;
            }
            return res;
        }
    }
}
