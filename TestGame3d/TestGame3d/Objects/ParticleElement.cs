using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Tennis01.Objects
{
    #region ParticleElementクラス
    /// <summary>
    /// パーティクルの一粒を表すクラス
    /// </summary>
    class ParticleElement
    {
        #region フィールド
        /// <summary>
        /// パーティクルに貼るテクスチャ
        /// </summary>
        Texture2D texture;
        /// <summary>
        /// 座標と速度と加速度
        /// </summary>
        Vector3 position;
        Vector3 speed;
        Vector3 acceleration;
        /// <summary>
        /// 最初の大きさ
        /// </summary>
        float sizeStart;
        /// <summary>
        /// 消える直前の大きさ
        /// </summary>
        float sizeEnd;
        /// <summary>
        /// この粒の寿命(フレーム数)
        /// </summary>
        int lifeTime;
        /// <summary>
        /// 生きているフレーム数
        /// </summary>
        int frames;
        #endregion

        #region 静的フィールド
        /// <summary>
        /// 描画に使うGraphicsDevice
        /// </summary>
        public GraphicsDevice GraphicsDevice;
        #endregion


        #region コンストラクタ

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="cameraVector"></param>
        /// <param name="tex"></param>
        /// <param name="iniPosition"></param>
        /// <param name="sizeStart"></param>
        public ParticleElement(GraphicsDevice gDevice, Texture2D tex, int life, Vector3 iniPosition, Vector3 spd, Vector3 accel, float sizeStart, float sizeEnd)
        {
            GraphicsDevice = gDevice;
            texture = tex;
            position = iniPosition;
            speed = spd;
            acceleration = accel;
            this.sizeStart = sizeStart;
            this.sizeEnd = sizeEnd;
            lifeTime = life;
            frames = 0;


            //Effect初期化
            //effect = new BasicEffect(graphicsDevice);
            //effect.Texture = texture;
            //effect.TextureEnabled = true;
            //effect.EnableDefaultLighting();
        }
        #endregion

        #region プロパティ
        /// <summary>
        /// 寿命を迎えたかどうかを取得。
        /// </summary>
        public bool EndLife { get { return frames > lifeTime; } }
        /// <summary>
        /// 描画に必要な4つの点を取得。
        /// </summary>
        public VertexPositionNormalTexture[] Vertices { get; private set; }
        /// <summary>
        /// インデックスバッファを使用して描画する際に必要なインデックスを取得。
        /// </summary>
        public static short[] indices
        {
            get
            {
                return new short[]{
                       0,1,2,
                       2,1,3
                    };
            }
        }
        /// <summary>
        /// 寿命が進んでいる割合を取得
        /// </summary>
        public float Alpha
        {
            get { return (float)frames / lifeTime; }
        }
        /// <summary>
        /// この粒に貼るテクスチャ
        /// </summary>
        public Texture2D Texture { get { return texture; } }

        #endregion
        #region 操作
        
        /// <summary>
        /// distanceだけ移動
        /// </summary>
        /// <param name="distance"></param>
        public void Move(Vector3 distance)
        {
            position += distance;
        }
        #endregion
        #region 更新
        public void Update(Vector3 cameraVector)
        {
            position += speed;
            speed += acceleration;

            setVertices(cameraVector);

            frames++;
        }

        #endregion
        private void setVertices(Vector3 cameraVector)
        {
            float size = MathHelper.Lerp(sizeStart, sizeEnd, Alpha);

            Vector3[] vecs = new Vector3[]{
                new Vector3(-size,size,0),
                new Vector3(size,size,0),
                new Vector3(-size,-size,0),
                new Vector3(size,-size,0)
                };

            //点をカメラに合わせて回転
            //回転角度を求める
            float rx, ry;
            turn(cameraVector, out rx, out ry);
            //回転

            for (int i = 0; i < vecs.Length; i++)
            {
                vecs[i] = Vector3.Transform(vecs[i], Matrix.CreateRotationX(rx) * Matrix.CreateRotationY(ry));
            }

            Vertices = new VertexPositionNormalTexture[]{
                    new VertexPositionNormalTexture(position + vecs[0], -cameraVector, new Vector2(0, 0)),
                    new VertexPositionNormalTexture(position + vecs[1], -cameraVector, new Vector2(1, 0)),
                    new VertexPositionNormalTexture(position + vecs[2], -cameraVector, new Vector2(0, 1)),
                    new VertexPositionNormalTexture(position + vecs[3], -cameraVector, new Vector2(1, 1))
                };
        }
        private void turn(Vector3 cameraVector, out float radX, out float radY)
        {
            Vector2 vec = -new Vector2(cameraVector.X, cameraVector.Z);
            radY = vec.ToRadians();
            radX = (float)Math.Asin(cameraVector.Y / cameraVector.Length());
        }
    }
    #endregion

}
