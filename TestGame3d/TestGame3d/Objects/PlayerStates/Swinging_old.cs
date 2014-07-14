using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Extention;
using Tennis01.Input;
namespace Tennis01.Objects.PlayerStates
{
    class Swinging:PlayerState
    {
        int frames;
        float slidingSpeed;
        int swingingTime;//20フレーム...20 / fps秒
        public Swinging(Player player,float speed,ControllerState controlerState,string animationName,TimeSpan startTime)
            : base(player,animationName,"",startTime)//,new TimeSpan(0,0,0,0,(int)(390/120.0f*1000)))
        {
            float penalty;
            swingingTime = 35;
            if (controlerState.Button4 == ControlerButtonStates.Pressed)
            {
                penalty = 0;// 15;
            }
            else
            {
                penalty = 0;// 25;
            }
            if (Player.Ball.Bounds != 0 && Math.Abs(Player.BodyDirection.X) > Math.Abs(Player.BodyDirection.Y)*2)
            {
                swingingTime += (int)(penalty * Player.Velocity);
            }
            Player.Swing(controlerState);
            frames = swingingTime;
            slidingSpeed = speed;
            //base.AnimationName = "Animation_6";
            GooR = true;
            GooL = false;
        }

        public override void Update(Input.ControllerState controlerState)
        {
            if (frames == swingingTime)
            {
                if (slidingSpeed >= 0.8f)
                {
                    Player.Velocity = 1;
                    slidingSpeed = 1;
                }

                Vector2 joyStick = controlerState.JoyStick;
                joyStick.Y = -joyStick.Y;
                if (Vector2.Dot(joyStick, Player.BodyDirection) <= (float)Math.Cos(MathHelper.ToRadians(30)))
                {
                    Player.Velocity = 0;
                }
            }
            else
                Player.Velocity -= 1.0f / swingingTime;// Player.Ability.Deceleration;

            if (Player.Velocity < 0)
                Player.Velocity = 0;
            Player.RotationY = Player.Position.Z > 0 ? new Vector2(0, -1).ToRadians() : new Vector2(0, 1).ToRadians();
            frames--;
            if (frames < 0)
            {
                Player.Velocity = 0;
                NextState = new Running(Player);
            }
            if (Player.Velocity > 0.5f)
                Player.Sliding();
        }
    }
}
