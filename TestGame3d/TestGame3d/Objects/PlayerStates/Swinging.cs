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
        int delay;
        ControllerState state;
        bool disX;
        public Swinging(Player player,float speed,ControllerState controlerState)
            : base(player,"")//,new TimeSpan(0,0,0,0,(int)(390/120.0f*1000)))
        {
            //Player.Swing(controlerState);
            
            delay = Player.Delay;// 5 + (int)(player.Velocity * 5);
            /*if (player.Smashable())
            {
                delay /= 2;
            }
            else if (controlerState.Button2 == ControlerButtonStates.Pressed)
            {
                delay *= 2;
            }*/
            TimeSpan swingStart = new TimeSpan();
            AnimationName = player.GetSwingAnimation(controlerState, ref delay, out swingStart);
            AnimationStartTime = swingStart;
            swingingTime = delay + 30 + (int)(player.Velocity * 20);

            frames = 0;// swingingTime;
            slidingSpeed = speed;
            state = controlerState;
            //base.AnimationName = "Animation_6";
            GooR = true;
            GooL = false;
            disX = player.ForeHandBall();
        }
        Vector3 getColor(Input.ControllerState controllerState)
        {
            if (controllerState.Button1 == ControlerButtonStates.Pressed)
            {
                return new Vector3(255, 96, 0);
            }
            else if (controllerState.Button4 == ControlerButtonStates.Pressed)
            {
                return new Vector3(0, 128, 255);
            }
            else if (controllerState.Button2 == ControlerButtonStates.Pressed)
            {
                return new Vector3(128, 112, 128);
            }
            return Vector3.Zero;
        }
        bool hit = false;
        int threshold = 5;
        public override void Update(Input.ControllerState controlerState)
        {
            Player.RotationX = (float)Math.PI / 8;
            if (Player.OnSmashPoint() && frames <= delay && AnimationName == "Animation_3")
            {
                Vector3 posi = Vector3.Lerp(Player.SwingStartPosition, Player.SmashPoint.EmitPoint, (float)frames / delay);
                posi.Y = Player.Position.Y;
                //posi.Z = Player.Position.Z;
                Player.Position = posi;
                GameMain.debugStr["amo"] = "" + (float)frames / delay;
                Player.Velocity = 0;
            }
            if (!hit && controlerState.Button3 == ControlerButtonStates.Pressed && Player.Diveable() && Player.Position.Y == 0)
            {
                NextState = new Diving(Player, Player.Ball.Position - Player.Position);
            }
            if (frames == 0)
            {
                //if (slidingSpeed >= 0.7f)
                //{
                //    Player.Velocity = 1;
                //    slidingSpeed = 1;
                //}
                //else 
                //    Player.Velocity = 0;

                Vector2 joyStick = controlerState.JoyStick;
                joyStick.Y = -joyStick.Y;
                if (Vector2.Dot(joyStick, Player.BodyDirection) <= (float)Math.Cos(MathHelper.ToRadians(30)))
                {
                    Player.Velocity = 0;
                }
            }
            else
                Player.Velocity -= 1f / swingingTime;

            if (Player.Velocity < 0)
                Player.Velocity = 0;
            else if(Player.Velocity > 0.5f)
                Player.Sliding();

            if (!hit && frames >= delay && frames < delay + threshold && (Player.Ball.Bounds == 0 || disX == Player.ForeHandBall()))
            {
                state.JoyStick = controlerState.JoyStick;
                if(Player.Hit(Player.Ball))
                    hit = true;
                Player.Swing(state);
                GameMain.debugStr["thre"] = ""+(frames == delay);
            }
            else if (frames == (int)(delay * 0.9f))
            {
                Player.PlaySound("swing2");
            }
            if (frames >= delay)
            {
                Player.FireRacket(getColor(state));
            }
            Player.RotationY = Player.Position.Z > 0 ? new Vector2(0, -1).ToRadians() : new Vector2(0, 1).ToRadians();
            frames++;
            if (frames >= swingingTime)
            {
                Player.Velocity = 0;
                NextState = new Running(Player);
            }
            if (Player.Velocity > 0.8f)
                Player.Sliding();
        }
    }
}
