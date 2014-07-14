using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Tennis01.UIComponents
{
    struct Rotation
    {
        public float Degree;
        public Vector2 Origin;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="degree">回転角度</param>
        /// <param name="origin">回転の中心座標</param>
        public Rotation(float degree, Vector2 origin)
        {
            Degree = degree;
            Origin = origin;
        }
    }
    struct Scale
    {
        public float X;
        public float Y;
        public Vector2 Origin;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x">横方向への拡大率</param>
        /// <param name="y">縦方向への拡大率</param>
        /// <param name="origin"></param>
        public Scale(float x, float y, Vector2 origin)
        {
            X = x;
            Y = y;
            Origin = origin;
        }
    }
    class Component2D:DrawableGameComponent
    {
        #region フィールド
        /// <summary>
        /// キャンバス(絶対座標)
        /// </summary>
        Rectangle parentArea;
        /// <summary>
        /// 相対座標
        /// </summary>
        Vector2 position;
        /// <summary>
        /// 相対的な大きさ
        /// </summary>
        Vector2 scale;
        /// <summary>
        /// 描画するテクスチャ
        /// </summary>
        Texture2D texture;
        /// <summary>
        /// 上乗せカラー
        /// </summary>
        Color color;
        /// <summary>
        /// 回転
        /// </summary>
        Rotation rotation;
        /// <summary>
        /// 
        /// </summary>
        SpriteBatch spriteBatch;
        #endregion

        #region プロパティ
        /// <summary>
        /// 実際の描画領域
        /// </summary>
        public Rectangle DrawArea
        {
            get
            {
                return new Rectangle((int)(parentArea.X * position.X),(int)(parentArea.Y * position.Y),(int)(parentArea.Width * scale.X),(int)(parentArea.Height * scale.Y));
            }
        }
        #endregion
        public Component2D(Rectangle canbas,Vector2 position,Vector2 scale,Rotation rotation,Color color,Texture2D tex = null)
            : base(Scenes.Scene.Game)
        {
            parentArea = canbas;
            this.position = position;
            this.scale = scale;
            this.texture = tex;
            this.rotation = rotation;
            this.color = color;
            spriteBatch = new SpriteBatch(Scenes.Scene.Game.GraphicsDevice);
        }
        public Component2D(Component2D cmp2D, Vector2 position, Vector2 scale, Rotation rotation, Color color,Texture2D tex = null)
            : this(cmp2D.DrawArea, position, scale, rotation, color,tex)
        {
            DrawOrder = cmp2D.DrawOrder + 1;
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            if (texture != null)
            {
                spriteBatch.Draw(texture, DrawArea, null, color, MathHelper.ToRadians(rotation.Degree), rotation.Origin, SpriteEffects.None, 0);
            }
            base.Draw(gameTime);
        }
    }
}
