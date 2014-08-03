using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tennis01.Objects.PlayerStates
{
    class Jumping:PlayerState
    {
        int jumpFrame;
        public Jumping(Player p,int jFrame)
            : base(p, "Animation_6")
        {
            jumpFrame = jFrame;
            p.Jump(jumpFrame);
        }
        public override void Update(Input.ControllerState controlerState)
        {
            if (Player.Speed.Y < 0 && Player.Position.Y <= 0)
            {
                NextState = new Standing(Player);
                //Player.Rotation = Microsoft.Xna.Framework.Vector3.Zero;
            }
            else if (controlerState.Back == Input.ControlerButtonStates.Pressed)
            {
                NextState = new Jumping2(Player,jumpFrame);
            }
        }
    }
}
