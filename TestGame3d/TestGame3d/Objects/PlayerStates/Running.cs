using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tennis01.Input;
using Microsoft.Xna.Framework;
namespace Tennis01.Objects.PlayerStates
{
    class Running:PlayerState
    {
        string currentAnimation = "Animation_2";
        public Running(Player player) : base(player,"Animation_2","run_jari")
        {
            GooL = GooR = true;
        }
        public override void Update(ControllerState controlerState)
        {
            Player.RotationX = (float)Math.PI / 16;
            Vector2 joyStick = controlerState.JoyStick;
            joyStick.Y = -joyStick.Y;

            if(joyStick!=Vector2.Zero)
            {
                if (Player.Position.Y == 0)
                {
                    //加速
                    Player.Accel();
                }
                if (Vector2.Dot(Vector2.Normalize(joyStick), Player.BodyDirection) < (float)Math.Cos(MathHelper.ToRadians(Player.Ability.RotAngle)))
                {
                    Player.Velocity *= Player.Ability.QuickNess;// Player.Velocity * (0.5f * Player.Ability.QuickNess);
                }

                Player.BodyDirection = Vector2.Normalize(joyStick);
            }
            else
            {
                //減速
                Player.Velocity -= 1.0f / 60.0f * (1 + Player.Ability.Deceleration*5);// new Vector3(deceleration * direction.X, 0, deceleration * direction.Y);
                if (Player.Velocity <= 0)
                {
                    //立ち状態へ
                    Player.Velocity = 0;
                    NextState = new Standing(Player);
                }
            }
            GameMain.debugStr["v"] = "" + Player.Velocity;
            //ラケットスイングへ
            if (Player.Ball.Speed.Z * Player.Position.Z > 0)
            {
                //Player.Swing(controlerState);
                //if (Player.Hit(Player.Ball))
                //{
                //    TimeSpan time;
                //    string animation = Player.GetSwingAnimation(controlerState, out time);
                //    NextState = new Swinging(Player, Player.Velocity, controlerState, animation,time);
                //}
                //else if (Player.Diveable())
                //{
                //    int frames = (int)((Player.Position - Player.Ball.Position).Length() / Player.Ability.MaxSpeed);
                //    Vector3 vec = Player.Ball.Position + Player.Ball.Speed*frames/2 - Player.Position;
                //    NextState = new Diving(Player, vec, 60);
                //}
                if (controlerState.HasAnyInput(ControllerState.Inputs.ShotButtons))
                    NextState = new Swinging(Player, Player.Velocity, controlerState);
                else if (controlerState.Button3 == ControlerButtonStates.Pressed && Player.Position.Y == 0)
                {
                    int frames = (int)((Player.Position - Player.Ball.Position).Length() / Player.Ability.MaxSpeed);
                    Vector3 vec = Player.Ball.Position + Player.Ball.Speed * frames / 2 - Player.Position;
                    NextState = new Diving(Player, vec);
                }

            }

            //Playerの状態によってアニメーションを変更
            string animeName = Player.GetRunningAnimation();
            if (currentAnimation != animeName)
            {
                Player.ChangeAnimation(animeName);
                currentAnimation = animeName;
            }
            //else if(controlerState.R == ControlerButtonStates.Pressed)
            //{
            //    Vector2 vec = controlerState.JoyStick;
            //    vec.Y = -vec.Y;
            //    NextState = new Diving(Player, Player.BodyDirection,1.5f,30);
            //}
        }
    }
}
