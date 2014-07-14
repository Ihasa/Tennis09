using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Extention;
namespace Tennis01.Objects
{
    using Scenes;
    using VisualEffects;
    class TennisMachine:Object3D
    {
        float moveCount = 0;
        int shotFrames = 0;
        int shotCount = 0;
        Action shot = () => { };
        float spd;
        Vector3 target;
        Vector3 lastPosition;
        public event Action Shot;
        public Ball Ball { get; set; }
        public Particle Smoke { get; private set; }
        public Particle Smoke2 { get; private set; }
        public TennisMachine(Model model, Camera c, Vector3 position,Ball b)
            : base(model, c,position,Vector3.Zero,new Vector3(0,position.Z < 0 ? 0 : MathHelper.Pi,0), Matrix.Identity, "ObjectSEs")
        {
            Ball = b;
            target = lastPosition = position;
            Smoke = Particle.CreateFromParFile("./ParFiles/fire.par");
            Smoke.Direction = new Vector3(0, 0, position.Z < 0 ? 1 : -1);
            Smoke.Scale = 0.0f;
            Smoke.Visible = true;

            Smoke2 = Particle.CreateFromParFile("./ParFiles/tankSmoke2.par");
            Smoke2.Direction = new Vector3(0, 0, position.Z < 0 ? 1 : -1);
            Smoke2.Scale = 0.27f*3;
            Smoke2.Visible = true;
        }
        public void shotBall(float angleX, float velocity, float distanceFromNet, float height, float spin,int delay)
        {
            init(height, delay);
            shot = () => {
                Ball.Init(Position + Vector3.Up * height);
                Ball.ShotWithoutAngleY(angleX, velocity, distanceFromNet, spin, ShotAngles.Low,45);
                PlaySound("oneShot");
            };
            if (Shot != null)
                Shot();
        }
        public void shotBall(Vector2 angles, float distanceFromNet, float height, float spin,int delay)
        {
            init(height, delay);
            shot = () => {
                Ball.Init(Position + Vector3.Up * height);
                Ball.ShotByDistance(angles, Math.Abs(Position.Z) + distanceFromNet, spin);
                PlaySound("oneShot");
            };
            if (Shot != null)
                Shot();
        }
        public void ShotByVelocity(Vector3 target, float velocity,float height, float spin, int delay)
        {
            Ball.Init(Position + Vector3.Up * height);
            float distance = target.Z - Ball.Position.Z;
            float angleX = MathHelper.ToDegrees((float)Math.Atan((target.X - Ball.Position.X) / distance));
            shotBall(angleX, velocity, Math.Abs(target.Z),height, spin, delay);
        }
        public void ShotByAngle(Vector3 target, float angleY, float height, float spin, int delay)
        {
            Ball.Init(Position + Vector3.Up * height);
            float distance = Math.Abs(target.Z - Ball.Position.Z);
            float angleX = MathHelper.ToDegrees((float)Math.Atan((target.X - Ball.Position.X) / distance));
            shotBall(new Vector2(angleX,angleY),Math.Abs(target.Z), height, spin, delay);
        }
        public void Drop(float side,float height, int delay)
        {
            ShotByAngle(new Vector3(TennisCourt.SinglesWidth * 0.5f * side, 0, TennisCourt.CourtLength * 0.3f), 30, height, -30, delay);
        }
        public void Lob(float side, float height, int delay)
        {
            ShotByAngle(new Vector3(TennisCourt.SinglesWidth * 0.5f * side, 0, TennisCourt.CourtLength * 0.8f + 0.2f * (float)GameMain.Random.NextDouble()), 50, height, 15, delay);
        }
        public void ChanceBall(float side, float height, int delay)
        {
            ShotByAngle(new Vector3(TennisCourt.SinglesWidth * 0.5f * side, 0, TennisCourt.CourtLength * 0.4f), 45, height, 0.01f, delay);
        }
        public void ChanceBall2(float side, float height, int delay)
        {
            ShotByAngle(new Vector3(TennisCourt.SinglesWidth * 0.5f * side, 0, TennisCourt.CourtLength * ((float)GameMain.Random.NextDouble() * 0.4f + 0.2f)), 30, height, -0.0001f, delay);
        }
        public void TopSpin(float side, float height, int delay)
        {
            ShotByAngle(new Vector3(TennisCourt.SinglesWidth * 0.5f * side, 0, (float)(GameMain.Random.NextDouble() * 0.3f + 0.6f)* TennisCourt.CourtLength), 25, height, 50, delay);
        }
        public void SliceSpin(float side, float height, int delay)
        {
            ShotByVelocity(new Vector3(TennisCourt.SinglesWidth * 0.5f * side, 0, (float)(GameMain.Random.NextDouble() * 0.3f + 0.6f)* TennisCourt.CourtLength), 52000.0f.ResizeVelocity(), height, -40, delay);
        }
        public void SpeedBall(float side, float height, int delay)
        {
            ShotByVelocity(new Vector3(TennisCourt.SinglesWidth * 0.5f * side, 0, TennisCourt.CourtLength * 0.8f), 75000.0f.ResizeVelocity(), height, 10, delay);
        }
        public void SlowBall(float side, float height, int delay)
        {
            ShotByVelocity(new Vector3(TennisCourt.SinglesWidth * 0.25f * side, 0, TennisCourt.CourtLength * 0.7f), 50000.0f.ResizeVelocity(), height, -15, delay);            
        }
        public void SnipeBall(Player player, float height, int delay)
        {
            Vector3 target = player.Position.Z > TennisCourt.ServiceAreaLength * 0.5f ? player.Position * 0.7f : player.Position * 1.5f;
            ShotByAngle(player.Position * 0.7f, 30, 0, 15, delay);
        }
        //public void RandomBall(float side, float height, int delay)
        //{
        //    ShotByVelocity(
        //        new Vector3(TennisCourt.SinglesWidth * 0.5f * side, 0, TennisCourt.CourtLength * MathHelper.Clamp((float)GameMain.Random.NextDouble(), 0.4f, 1)),
        //        80000.0f.ResizeVelocity() * MathHelper.Clamp((float)GameMain.Random.NextDouble(), 0.6f, 1),
        //        height,
        //        GameMain.Random.Next(-50, 50),
        //        delay
        //    );
        //}
        //public void ForBeginner(float height, int delay)
        //{
        //    ShotByAngle(new Vector3(-player.Position.X,0,player.Position.Z) * 0.7f, 25, height, 5, delay);
        //}
        //public void Running(float height, int delay)
        //{
        //    ShotByAngle(new Vector3(player.Position.X > 0 ? 0.7f : -0.7f, 0, TennisCourt.CourtLength * 0.7f), 25, height, 25, delay);
        //}
        private void init(float height, int delay)
        {
            Ball.Enabled = Ball.Visible = false;
            Ball.Init(Position + Vector3.Up * height);
            shotFrames = delay;
            shotCount = 0;
        }

        public void Move(Vector3 newPosition,int frames)
        {
            target = newPosition;
            lastPosition = position;
            spd = 1.0f / frames;
            moveCount = 0;
        }
        public void Cancel()
        {
            shot = null;
        }
        public override void Update(GameTime gameTime)
        {
            if (shot != null && shotCount < shotFrames)//発射
            {
                shotCount++;
                if (shotCount == shotFrames)
                {
                    shot();
                    Ball.Visible = Ball.Enabled = true;
                    Smoke2.EmitPoint = Position;
                    //Smoke2.InitialColor = Ball.Fire.InitialColor;
                    Smoke2.Emit();
                    //Ball.ExplodeLittle();
                }
                else
                {
                    Smoke.EmitPoint = Position;
                    Smoke.Scale = MathHelper.Lerp(0.27f * 0.1f, 0.27f, (float)shotCount / shotFrames);
                    Smoke.Emit();
                }
            }
            else if (position != target)//移動
            {
                position = Vector3.SmoothStep(lastPosition, target, moveCount);
                moveCount += spd;
                if (moveCount >= 1)
                {
                    position = target;
                }
            }
            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            Smoke.SetProjection(camera.Projection);
            Smoke.SetView(camera.Position, camera.Target, camera.Up);

            Smoke2.SetProjection(camera.Projection);
            Smoke2.SetView(camera.Position, camera.Target, camera.Up);
            base.Draw(gameTime);
        }
    }
}
