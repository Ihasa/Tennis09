using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Extention;
using Microsoft.Xna.Framework;
namespace Tennis01.Objects.PlayerStates
{
    class Diving:PlayerState
    {
        int frames;
        int maxFrames;
        bool hit;
        public Diving(Player p, Vector3 direction)
            : base(p, "Animation_9", "",null,new TimeSpan(0,0,0,0,(int)(239 / 120.0f * 1000)))
        {
            frames = maxFrames = 120;
            hit = false;
            Player.BodyDirection = new Vector2(direction.X,direction.Z);
            //Player.Jump((int)(maxFrames * Player.Ability.Acceleration));
            vel = 1.5f;
            jump = 30;
            //Player.Velocity /= 2;
            GooL = false;
            GooR = true;
            Player.TargetObject = null;
        }
        int valid = 60;
        int delay = 5;
        float vel;
        int jump;
        public override void Update(Input.ControllerState controlerState)
        {
            frames--;
            if (frames == maxFrames - delay)
            {
                Player.Velocity = vel;
                Player.Jump(jump);
                Player.Sliding();
            }
            else if (frames > maxFrames - delay)//|| (!hit && Player.Position.Y > 0))
            {
                Vector3 vec = (Player.Ball.Position + Player.Ball.Speed * 25 - Player.Position);
                Player.BodyDirection = Vector2.Normalize(new Vector2(vec.X, vec.Z));
            }
            else if (Player.Position.Y == 0)
            {
                if (Player.Velocity > 0)
                    Player.Sliding();
                Player.Velocity -= vel / (valid-30);
                if (Player.Velocity < 0)
                    Player.Velocity = 0;
                    //Player.Velocity *= 0.9f;
                    //Player.Velocity *= 0.95f;
                
            }
            
            if (!hit && frames <= maxFrames - delay && frames >= maxFrames - valid && Player.Hit(Player.Ball) && Player.Ball.Position.Z * Player.Position.Z > 0 && Player.Ball.Bounds < 2 && Player.Ball.Position.Y <= Player.HitBounds.Y * 0.9f)
            {
                //NextState = new Swinging(Player, Player.Velocity, new Input.ControllerState());
                if (controlerState.Button1 == Input.ControlerButtonStates.Down || controlerState.Button1 == Input.ControlerButtonStates.Pressed)
                {
                    Player.ShotBall(controlerState.JoyStick, 35, TennisCourt.CourtLength * (Math.Abs(Player.DistanceToBall.X) * 0.3f + 0.4f), new Vector3(224, 224, 224), 10, 0.1f, 0.1f);
                }
                else if (controlerState.Button4 == Input.ControlerButtonStates.Down || controlerState.Button4 == Input.ControlerButtonStates.Pressed)
                {
                    Player.ShotBall(controlerState.JoyStick, 30, TennisCourt.CourtLength * (Math.Abs(Player.DistanceToBall.X) * 0.3f + 0.5f), new Vector3(224, 224, 224), -20, 0.1f, 0.1f);
                }else
                    Player.ShotBall(controlerState.JoyStick, 40, TennisCourt.CourtLength * (Math.Abs(Player.DistanceToBall.X) * 0.6f + 0.3f), new Vector3(224, 224, 224), -5, 0.1f, 0.1f);
                Player.Ball.ExplodeLittle();
                //Player.Swing(controlerState);
                hit = true;
                Player.TargetObject = Player.Ball;
                //NextState = new Swinging(Player, 2,controlerState);
                //Player.Velocity = 0;
                //NextState = new Standing(Player);
            }
            else if (frames < 0)
            {
                Player.Velocity = 0;
                NextState = new Standing(Player);                
            }
        }
    }
}
