using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tennis01.Input;
using Microsoft.Xna.Framework;
namespace Tennis01.Objects.PlayerStates
{
    class Standing:PlayerState
    {
        string currentAnimation = "Animation_1";
        public Standing(Player player):base(player,"Animation_1")
        {
            GooL = GooR = true;
        }
        public override void Update(Input.ControllerState controlerState)
        {
            //相手のコートを見るだけ
            Vector2 dir = new Vector2();
            dir.Y = Player.Position.Z > 0 ? -1 : 1;

            if (Player.Ball.Bounds <= 1 && Player.Position.Z * Player.Ball.Speed.Z > 0 && Math.Abs(Player.Ball.BoundPoint.Z) < Math.Abs(Player.Position.Z))
            {
                dir = Player.ForeHandBall() ? new Vector2(1,dir.Y) : new Vector2(-1,0);

                if (Player.Position.Z < 0)
                    dir.X *= -1;
            }
            Player.BodyDirection = Vector2.Normalize(dir);
            

            if (Player.Ball.Speed.Z * Player.Position.Z > 0&&controlerState.HasAnyInput(ControllerState.Inputs.ShotButtons))
            {
                //Player.Swing(controlerState);
                //TimeSpan time;
                //string animeName = Player.GetSwingAnimation(controlerState, out time);
                NextState = new Swinging(Player, 0, controlerState);

                //if (Player.Hit(Player.Ball))
                //{
                //    TimeSpan time;
                //    string animeName = Player.GetSwingAnimation(controlerState, out time);
                //    NextState = new Swinging(Player, 0, controlerState, animeName,time);
                //}
                //else if (Player.Diveable())
                //    NextState = new Diving(Player, Player.Ball.Position + Player.Ball.Speed - Player.Position, 1.5f, 30);
            }
            else if (controlerState.JoyStick.Length() != 0)
            {
                NextState = new Running(Player);
            }
            else if (controlerState.Back == ControlerButtonStates.Pressed)
            {
                NextState = new Jumping(Player,60);
            }
            //アニメーション変更
            string animation = Player.GetStandAnimation();
            if (animation != currentAnimation)
            {
                Player.ChangeAnimation(animation);
                currentAnimation = animation;
            }
        }
    }
}
