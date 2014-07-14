using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
namespace Tennis01.Scenes
{
    using Objects;
    class HitChecker : GameComponent
    {
        TennisCourt tennisCourt;

        List<Ball> balls;
        Player[] players;
        public HitChecker(TennisCourt court,params Player[] playerList)
            : base(Scene.Game)
        {
            players = playerList;
            tennisCourt = court;
            balls = new List<Ball>();
        }
        public HitChecker(TennisCourt court,Ball b, params Player[] playerList)
            :base(Scene.Game)
        {
            players = playerList;
            tennisCourt = court;
            balls = new List<Ball>();
            balls.Add(b);
        }
        public HitChecker(TennisCourt court, Ball[] b, params Player[] playerList)
            : base(Scene.Game)
        {
            players = playerList;
            tennisCourt = court;
            balls = new List<Ball>();
            foreach (Ball ball in b)
            {
                balls.Add(ball);
            }
        }
        public void AddBall(params Ball[] balls)
        {
            foreach (Ball b in balls)
            {
                this.balls.Add(b);
            }
        }
        public override void Update(GameTime gameTime)
        {
            //ボールとプレイヤーとテニスコートの壁
            foreach (HitVolume h in tennisCourt.Walls)
            {
                foreach (Ball b in balls)
                {
                    if (b.Hit(h))
                    {
                        b.Position += -b.Speed;
                        if (b.Position.Y < h.Center.Y * 2)
                            b.Speed = 0.5f * Vector3.Reflect(b.Speed, h.Normal);
                        else
                            b.Toss(0.27f);
                        b.Bound(b.Position);
                    }
                }
                foreach (Player p in players)
                {
                    if (p.Hit(h))
                    {
                        p.Position = p.Position + h.Normal * p.Ability.MaxSpeed * p.Velocity;// h.Center;
                        //p.Position += p.Speed - Vector3.Dot(p.Speed, h.Normal) * h.Normal;// Vector3.Reflect(p.Speed, h.Normal);
                    }
                }
            }

            //プレイヤーとネット
            foreach (Player p in players)
            {
                if (Math.Abs(p.Position.Z) < p.HitBounds.Z / 2)
                {
                    if (p.Position.Z > 0)
                    {
                        p.Position = p.Position + Vector3.Backward * p.Ability.MaxSpeed * p.Velocity;// h.Center;
                    }
                    else
                    {
                        p.Position = p.Position + Vector3.Forward * p.Ability.MaxSpeed * p.Velocity;// h.Center;
                    }
                }
            }
            base.Update(gameTime);
        }
    }
}
