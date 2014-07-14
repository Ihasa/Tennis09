using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
namespace Tennis01.Objects.PlayerStates
{
    class Helicopter:PlayerState
    {
        int rot;
        Vector3 accel;
        Vector3 speed;
        public Helicopter(Player player)
            : base(player, "Animation_10")
        {
            rot = 0;
            speed = Vector3.Zero;
            accel = Vector3.Zero;
        }
        public override void Update(Input.ControllerState controlerState)
        {
            rot++;
            //Player.Rotation.Y = MathHelper.ToRadians(rot * 30);
            //Player.Position += speed;
            speed += accel;

            //ジョイスティックで加速度を操作
        }
    }
}
