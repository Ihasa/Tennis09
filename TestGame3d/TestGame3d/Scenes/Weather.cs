using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VisualEffects;
namespace Tennis01.Objects
{
    using Scenes;
    class Weather : DrawableGameComponent
    {
        /// <summary>
        /// 降ってくるもの
        /// </summary>
        Particle particle;
        /// <summary>
        /// 降ってくるものが発生する場所
        /// </summary>
        Vector3[] emitPoints;
        /// <summary>
        /// この天気の範囲(XZ平面)
        /// </summary>
        Vector2 min, max;
        /// <summary>
        /// 降ってくる高さ
        /// </summary>
        float height;

        Random rand;
        Camera camera;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="camera">使用するカメラ</param>
        /// <param name="texture">テクスチャ</param>
        /// <param name="speed">降ってくるスピード</param>
        /// <param name="force">雨でいうところの降水量</param>
        /// <param name="radScatter">角度のばらつき</param>
        /// <param name="accel">風(加速度)</param>
        /// <param name="size">粒の大きさ</param>
        /// <param name="life">粒の寿命</param>
        /// <param name="weatherRange">降る範囲</param>
        /// <param name="emitHeight">パーティクルが発生する高さ</param>
        public Weather(Camera camera,Texture2D texture,float speed,float force,float radScatter,Vector3 accel,float size,TennisCourt court,float emitHeight,float ground)
            :base(Scene.Game)//new ParticleParams(texture,Vector3.Zero,speed,Vector3.Down,radScatter,accel,size,size,life,1))
        {
            particle = new Particle(new ParticleParams(texture, null, Vector3.Zero, speed, Vector3.Down, radScatter, accel, 0, size, size*2, 1, 0, Vector3.One*255, Vector3.One*255, (int)((emitHeight-ground) / (speed+accel.Y))+60, 1, 0));
            particle.UpdateOrder = this.UpdateOrder + 1;
            //range = new Rectangle((int)(-court.CourtBounds.X / 2), (int)(-court.CourtBounds.Y / 2), (int)court.CourtBounds.X, (int)court.CourtBounds.Y);
            min = -court.CourtBounds / 2;
            max = court.CourtBounds / 2;
            height = emitHeight;
            emitPoints = new Vector3[(int)(court.CourtBounds.X * court.CourtBounds.Y / 20*force)];
            rand = new Random();
            this.camera = camera;
            particle.Script = (p) =>
            {
                float minY = ground + 0.27f*0.001f;
                if (p.Position.Y <= minY)
                {
                    p.Position = new Vector3(p.Position.X, minY, p.Position.Z);
                    p.Speed = Vector3.Zero;
                    p.Normal = Vector3.Normalize(new Vector3(0, 1, 0.000001f));
                    p.InitialTexture = GameMain.Textures["snow"];
                }
            };
        }
        public static Weather Rain(Camera c, TennisCourt tennisCourt, float force)
        {
            return new Weather(c, GameMain.Textures["rain"], 0.1f, force, 0, new Vector3(0, 0, 0), 0.27f * 0.3f, tennisCourt, 4f, 0);
        }
        public static Weather Snow(Camera c, TennisCourt tennisCourt, float force)
        {
            return new Weather(c, GameMain.Textures["snow"], 0.02f, force, 0, new Vector3(0, 0, 0), 0.27f * 0.3f, tennisCourt, 3f, 0);
        }
        public void AddToGameCompo(Scene s)
        {
            s.AddComponents(particle, this);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < emitPoints.Length; i++)
            {
                float minX = min.X;
                float maxX = max.X;
                float minZ = min.Y;
                float maxZ = max.Y;
                emitPoints[i] = new Vector3(MathHelper.Lerp(minX, maxX, (float)rand.NextDouble()), height, MathHelper.Lerp(minZ, maxZ, (float)rand.NextDouble()));
            }

            foreach (Vector3 em in emitPoints)
            {
                particle.EmitPoint = em;
                particle.Emit();
            }
            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            particle.SetView(camera.Position, camera.Target, camera.Up);
            particle.SetProjection(camera.Projection);
            base.Draw(gameTime);
        }
    }
}
