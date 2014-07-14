using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
namespace Tennis01.Objects.PlayerStates
{
    class Spinning:PlayerState
    {
        int frames=90;
        public Spinning(Player player)
            : base(player, "Animation_1")
        {

        }
        public override void Update(Input.ControllerState controlerState)
        {
            Player.Velocity = 0;
            //Player.Rotation.Y = MathHelper.ToRadians(frames * 30);
            if (frames-- < 0)
                NextState = new Standing(Player);
        }
    }
}
