using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tennis01.Objects.PlayerStates
{
    class Jumping2:Jumping
    {
        public Jumping2(Player p,int jFrame)
            : base(p,jFrame)
        {
            AnimationName = "Animation_10";
        }
        public override void Update(Input.ControllerState controlerState)
        {
            Player.Rotation += new Microsoft.Xna.Framework.Vector3((float)Math.PI/12,0,0);
            base.Update(controlerState);
        }
    }
}
