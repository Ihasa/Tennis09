using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Tennis01.Objects
{
    using VisualEffects;
    using Scenes;
    class Target : HittableObject3D
    {
        int deadCount = 0;
        int deadFrames = 1;
        bool dead;
        int scaleCount = 0;
        int scaleFrame = 45;
        int frames = 0;
        Vector3 iniPosition;
        float scale = 1;
        float moveRad = 0.27f;
        public float Scale
        {
            get { return scale; }
            set
            {
                if (scale != value)
                {
                    scale = value;
                    scaleCount = 0;
                }
            }
        }
        public float Amp
        {
            get { return moveRad; }
            set
            {
                moveRad = value;
            }
        }
        public float AmpSpeed { get; set; }
        public bool Active
        { 
            get { return !dead; }
            set
            {
                if (value)
                {
                    if (dead)
                        scaleCount = 0;
                    dead = false;
                    deadCount = 0;
                    Visible = Enabled = true;
                }
                else
                {
                    dead = true;
                }
            }
        }
        public Particle Explode { get; private set; }
        public Ball Ball { get; set; }
        public event Action Hitted;
        public event Action Dead;
        public Target(Model model, Vector3 position,Camera camera,Ball ball,float scale, float amp,float ampSpeed)
            : base(model, camera, HitType.Sphere, position, Vector3.Zero, Vector3.Zero, Matrix.Identity, "ObjectSEs")
        {
            Explode = Particle.CreateFromParFile("./ParFiles/explosion.par");
            Explode.Scale = 0.27f * 3;
            Explode.InitialColor = new Vector3(255, 128, 0);
            Ball = ball;
            Position = iniPosition = position;
            dead = false;
            Scale = scale;
            Amp = amp;
            AmpSpeed = ampSpeed;
        }
        public Target(Model model, Vector3 position, Camera camera, Ball ball,float scale)
            : this(model,  position,camera,ball,scale,0,0)
        {
        }
        public override void Update(GameTime gameTime)
        {
            if (scaleCount++ < scaleFrame)
            {
                this.AnotherTransform = Matrix.CreateScale((float)scaleCount / scaleFrame * Scale);
            }
            if (!dead)
            {
                if (Hit(Ball) && Ball.Speed.Z * Position.Z > 0)
                {
                    //Visible = Enabled = false;
                    //Explode.EmitPoint = Position;
                    //Explode.Emit();
                    //Vector3 direction = Position - Ball.Position;
                    //if (direction.Y < 0)
                    //    direction.Y = -direction.Y;
                    //Speed = 0.27f / 60 * (Vector3.Normalize(direction));
                    dead = true;
                    if (Hitted != null)
                        Hitted();
                }
                //if (frames++ == 360)
                //{
                //    frames = 0;
                //}
                frames++;
                float cosRad = MathHelper.ToRadians(frames);
                position = iniPosition + new Vector3((float)Math.Cos(MathHelper.WrapAngle(cosRad * AmpSpeed)) * Amp,Amp != 0 ? 0 : 0.27f * 0.1f * (float)Math.Cos(MathHelper.WrapAngle(cosRad)),0);
            }
            else
            {
                rotation.Y = MathHelper.Pi * 2 * deadCount / deadFrames;
                if (deadCount++ == deadFrames /*|| deadCount == deadFrames+15*/)
                {
                    //deadCount = 0;
                    Visible = false;
                    Enabled = false;
                    Explode.EmitPoint = Position;
                    Explode.Emit();
                    Speed = Vector3.Zero;
                    rotation.Y = 0;
                    PlaySound("explode");
                    if (Dead != null)
                        Dead();
                }
            }
            this.HitVolume = new HitVolume(Position, 0.27f * Scale * 0.5f);
            Shadow.Scale = 0.27f * scale;
            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            Explode.SetProjection(camera.Projection);
            Explode.SetView(camera.Position, camera.Target, camera.Up);
            base.Draw(gameTime);
        }
        
    }
}
