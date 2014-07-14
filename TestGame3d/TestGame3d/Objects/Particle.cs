using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Tennis01.Objects
{
    using Scenes;
    /// <summary>
    /// Particleの特徴を決めるパラメータコレクション
    /// </summary>
    struct ParticleParams
    {
        public Texture2D Texture;
        public Vector3 EmitPoint;
        public float Speed;
        public Vector3 Direction;
        public float Radian;
        public Vector3 Acceleration;
        public float SizeStart;
        public float SizeEnd;
        public int LifeTime;
        public int Adds;
        public float AlphaStart;
        public float AlphaEnd;
        public ParticleParams(Texture2D tex, Vector3 emit, float spd, Vector3 dir, float rad, Vector3 accel, float szStart, float szEnd,int life, int num, float alphaStart=1,float alphaEnd=0)
        {
            Texture = tex;
            EmitPoint = emit;
            Speed = spd;
            Direction = dir;
            Radian = rad;
            Acceleration = accel;
            SizeStart = szStart;
            SizeEnd = szEnd;
            LifeTime = life;
            Adds = num;
            AlphaStart = alphaStart;
            AlphaEnd = alphaEnd;
        }

    }
    /// <summary>
    /// パーティクルの粒たちを管理する
    /// </summary>
    class Particle:DrawableGameComponent
    {
        #region ParticleElementオブジェクトの生成に必要なフィールド
        ParticleParams parameters;
        //Texture2D texture;
        //Vector3 emitPoint;
        //float speed;
        //Vector3 direction;
        //float radian;
        //Vector3 acceleration;
        //float sizeStart;
        //float sizeEnd;
        //int lifeTime;
        //int adds;
        Camera camera;
        Random rand;
        #endregion

        #region その他のフィールド
        List<ParticleElement> elements;
        #endregion

        #region コンストラクタ
        public Particle(Texture2D texName, Camera c,Vector3 emit, float spd, Vector3 dir, float rad, Vector3 accel, float szStart, float szEnd, int life, int num)
            :this(c, new ParticleParams(texName, emit, spd, dir, rad, accel, szStart, szEnd, life, num))
        {
        }
        public Particle(Camera camera, ParticleParams parameters)
            : base(Scenes.Scene.Game)
        {
            this.camera = camera;
            this.parameters = parameters;
            //texture = tex;
            //emitPoint = emit;
            //speed = spd;
            //direction = dir;
            //radian = rad;
            //acceleration = accel;
            //sizeStart = szStart;
            //sizeEnd = szEnd;
            //lifeTime = life;
            //adds = num;
            rand = new Random();
            elements = new List<ParticleElement>();
            //描画順序が最後になるように変更
            DrawOrder = 10;
            base.Initialize();
        }
        #endregion


        #region プロパティ
        //割と頻繁に変更されそうなものたち
        public ParticleParams Parameters { get { return parameters; } set { parameters = value; } }
        public Texture2D Texture { set { parameters.Texture = value; } }
        public Vector3 Direction { get { return parameters.Direction; } set { parameters.Direction = value; } }
        public Vector3 EmitPoint { set { parameters.EmitPoint = value; } }
        public float Speed { get { return parameters.Speed; } set { parameters.Speed = value; } }
        public int ElementsCount { get { return elements.Count; } }
        #endregion

        #region 操作
        /// <summary>
        /// 一回分の粒を噴出する
        /// </summary>
        public void Emit()
        {
            if (parameters.Adds <= 0)
                throw new ArgumentException();

            //directionに直交する単位ベクトルをひとつ求める
            Vector3 dir;
            if (parameters.Direction.X != 0)
            {
                dir = new Vector3((-parameters.Direction.Y - parameters.Direction.Z) / parameters.Direction.X, 1, 1);
            }
            else if (parameters.Direction.Y != 0)
            {
                dir = new Vector3(1, (-parameters.Direction.X - parameters.Direction.Z) / parameters.Direction.Y, 1);
            }
            else if (parameters.Direction.Z != 0)
            {
                dir = new Vector3(1, 1, (-parameters.Direction.X - parameters.Direction.Y) / parameters.Direction.Z);
            }
            else throw new ArgumentException("方向ベクトルが無効です");
            //正規化
            dir = Vector3.Normalize(dir);

            //addsだけ追加
            for (int i = 0; i < parameters.Adds; i++)
            {
                //ばらつき角度
                float rad = (float)(parameters.Radian * rand.NextDouble());

                //方向をランダムにする
                Vector3 dir2 = Vector3.TransformNormal((float)Math.Cos(rad) * parameters.Direction + (float)Math.Sin(rad) * dir, Matrix.CreateFromAxisAngle(parameters.Direction, (float)(rand.NextDouble() * MathHelper.TwoPi)));
                //追加

                elements.Add(new ParticleElement
                    (GraphicsDevice, parameters.Texture, parameters.LifeTime,
                    parameters.EmitPoint, dir2 * parameters.Speed, //new Vector3(dir2.X * parameters.Speed, dir2.Y * parameters.Speed, dir2.Z * parameters.Speed),
                    parameters.Acceleration, parameters.SizeStart, parameters.SizeEnd));
            }
            //Game1.debugStr = "" + elements.Count;
        }
        /// <summary>
        /// すべての粒をクリア
        /// </summary>
        public void Reset()
        {
            elements.Clear();
        }

        /// <summary>
        /// 風でなびく
        /// </summary>
        /// <param name="windDirection"></param>
        public void Blow(Vector3 windDirection)
        {
            foreach (ParticleElement element in elements)
            {
                element.Move(windDirection);
            }
        }
        #endregion

        #region 更新と描画
        public override void Update(GameTime gameTime)
        {
            //粒の追加は外からやる?
            //Emit();


            //粒の状態更新
            foreach (ParticleElement element in elements)
            {
                element.Update(camera.Direction);
            }
            //粒の寿命が終わっていたらリストから消す
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].EndLife)
                {
                    elements.RemoveAt(i);
                }
            }
        }
        
        public override void Draw(GameTime gameTime)
        {
            //elementsが空の時は何もしない
            if (elements.Count == 0)
                return;
            //elements内のすべてをまとめて描画
            List<VertexPositionNormalTexture> vertexList = new List<VertexPositionNormalTexture>();
            //elements内のすべての頂点情報を格納
            foreach (ParticleElement element in elements)
            {
                foreach (VertexPositionNormalTexture vertex in element.Vertices)
                {
                    vertexList.Add(vertex);
                }
            }
            //普通の配列に直す
            VertexPositionNormalTexture[] vertices = vertexList.ToArray();

            // TODO: Add your drawing code here
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            VertexBuffer vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture), vertices.Length, BufferUsage.None);
            vertexBuffer.SetData(vertices);

            IndexBuffer indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, 6, BufferUsage.None);
            indexBuffer.SetData(ParticleElement.indices);

            GraphicsDevice.SetVertexBuffer(vertexBuffer);
            GraphicsDevice.Indices = indexBuffer;
            //深度バッファを変更
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            //プリミティブ描画でTexture2Dを描画
            BasicEffect effect = new BasicEffect(GraphicsDevice);
            effect.TextureEnabled = true;
            effect.View = camera.View;
            effect.Projection = camera.Projection;
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    effect.Alpha = MathHelper.Lerp(Parameters.AlphaStart,Parameters.AlphaEnd,elements[i].Alpha);
                    effect.Texture = elements[i].Texture;
                    pass.Apply();
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, i * 4, 0, vertices.Length, 0, ParticleElement.indices.Length / 3);
                }
            }
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }
        #endregion

        #region 汎用性の高そうなインスタンスを返す静的メソッド
        //particle = new Particle(GraphicsDevice, particleTex, Vector3.Zero, 0.1f, Vector3.Normalize(Vector3.Up),MathHelper.ToRadians(20),new Vector3(0,0,0), 1f,0f,30,1);//炎
        //particle = new Particle(GraphicsDevice, particleTex, Vector3.Zero, 0.1f, Vector3.Normalize(Vector3.Up), MathHelper.ToRadians(10), new Vector3(0, 0, 0), 0.1f, 3f, 120,1);//普通の煙
        //particle = new Particle(GraphicsDevice, particleTex, Vector3.Zero, 0f, Vector3.Normalize(Vector3.Up), 0, new Vector3(0, 0, 0), 1f, 3f, 20,1);//エフェクト単体
        //particle = new Particle(GraphicsDevice, particleTex, Vector3.Zero, 0.4f, Vector3.Normalize(Vector3.Up),MathHelper.ToRadians(15),new Vector3(0,-0.008f,0), 1f,5f,90,1);//噴水を作りたかった
        //particle = new Particle(GraphicsDevice, particleTex, Vector3.Zero, 0.4f, Vector3.Normalize(Vector3.Up), MathHelper.Pi, new Vector3(0, 0, 0), 1f, 1f, 20,2);//火花
        //particle = new Particle(GraphicsDevice, particleTex, Vector3.Zero, 1f, Vector3.Normalize(Vector3.Up), MathHelper.Pi, new Vector3(0, 0, 0), 1f, 10f, 20,180);//爆発
        public static ParticleParams GetFireParticle(Texture2D texture, Camera c,Vector3 position, float size)
        {
            return new ParticleParams(texture, position, 0.1f * size, Vector3.Normalize(Vector3.Up), MathHelper.ToRadians(20), new Vector3(0, 0, 0), 1f * size, 0.01f * size, 30, 1);
        }
        public static ParticleParams GetStandardParticle(Texture2D texture, Camera c, Vector3 position, float size)
        {
            return new ParticleParams(texture, position, 0.1f * size, Vector3.Normalize(Vector3.Up), MathHelper.ToRadians(10), new Vector3(0, 0, 0), 0.1f * size, 3f * size, 120, 1);
        }
        public static ParticleParams GetExplosionParticle(Texture2D texture, Camera c, Vector3 position, float size)
        {
            return new ParticleParams(texture, position, 1f * size, Vector3.Normalize(Vector3.Up), MathHelper.Pi, new Vector3(0, 0, 0), 1f * size, 30f * size, 20, 180);
        }

        #endregion
    }
}
