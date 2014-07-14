using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Tennis01
{
    class Drawer2D
    {
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;
        List<Tuple<Texture2D, Rectangle,Color>> textureList;
        List<Tuple<string, Vector2,Color,float>> stringList;
        GraphicsDevice graphicsDevice;
        int width, height;
        public Drawer2D(GraphicsDevice graphicsDevice,SpriteFont spriteFont)
        {
            this.graphicsDevice = graphicsDevice;
            width = graphicsDevice.Viewport.Width;
            height = graphicsDevice.Viewport.Height;
            spriteBatch = new SpriteBatch(graphicsDevice);
            this.spriteFont = spriteFont;
            textureList = new List<Tuple<Texture2D, Rectangle,Color>>();
            stringList = new List<Tuple<string, Vector2,Color,float>>();
        }
        public Drawer2D(GraphicsDevice graphicsDevice)
            : this(graphicsDevice, null){}


        public void AddTexture(Texture2D texture, Vector2 position, Vector2 clip,float scaleX, float scaleY,Color color)
        {
            //画面に合わせてscaleX,Yを変える
            //1,1で画面いっぱい
            scaleX = (float)width / texture.Width * scaleX;
            scaleY = (float)height / texture.Height * scaleY;
            Vector2 texBounds = new Vector2(texture.Width * scaleX *clip.X,texture.Height * scaleY * clip.Y);
            textureList.Add(new Tuple<Texture2D, Rectangle, Color>(texture, new Rectangle((int)(width*position.X-texBounds.X),(int)(height*position.Y-texBounds.Y), (int)(texture.Width * scaleX), (int)(texture.Height * scaleY)),color));
        }
        public void AddTexture(Texture2D texture, Vector2 position,Vector2 clip,Color color)
        {
            AddTexture(texture, position,clip, 1,1,color);
        }
        public void AddTexture(Texture2D texture, Vector2 position, Vector2 clip,float scale,Color color)
        {
            AddTexture(texture, position,clip, scale, scale,color);
        }
        public void AddTexture(Texture2D texture, Rectangle rect,Color color)
        {
            textureList.Add(new Tuple<Texture2D, Rectangle,Color>(texture, rect,color));
        }
        public void AddString(string str, Vector2 position, Vector2 clip, Color color, float scale)
        {
            scale = (float)height / spriteFont.MeasureString(str).Y * scale;
            Vector2 strBounds = spriteFont.MeasureString(str) * scale;
            strBounds.X *= clip.X;
            strBounds.Y *= clip.Y;
            Vector2 strPosi = new Vector2(width * position.X - strBounds.X, height * position.Y - strBounds.Y);
            stringList.Add(new Tuple<string, Vector2, Color, float>(str,strPosi, color, scale));
        }
        //実際の2D描画
        public void Draw()
        {
            spriteBatch.Begin();
            foreach (Tuple<Texture2D, Rectangle,Color> pair in textureList)
            {
                spriteBatch.Draw(pair.Item1, pair.Item2, pair.Item3);
            }
            foreach (Tuple<string, Vector2,Color,float> pair in stringList)
            {
                if (spriteFont == null)
                    throw new Exception("文字列を描画する場合は、コンストラクタでフォントを指定してください");
                spriteBatch.DrawString(spriteFont, pair.Item1, pair.Item2,pair.Item3,0,Vector2.Zero,pair.Item4,SpriteEffects.None,0);
            }
            spriteBatch.End();

            //// Zバッファを有効にする?
            graphicsDevice.DepthStencilState = DepthStencilState.Default;


            textureList.Clear();
            stringList.Clear();
        }
    }
}
