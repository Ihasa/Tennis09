using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace Tennis01.UIComponents
{
    //Pointerのそのフレームにおける動きを決定する
    struct CursorState
    {
        /// <summary>
        /// 前フレームからの相対移動量
        /// </summary>
        public Vector2 Offset;
        /// <summary>
        /// 項目を決定するボタンが押されたかどうか
        /// </summary>
        public bool Accepted;
    }
    //ゲーム画面上の一点を指すアイテム
    abstract class Cursor:DrawableGameComponent
    {
        Texture2D image;
        Rectangle bounds;
        Vector2 hotSpotOffset;
        int width, height;
        int controllerIndex;
        SpriteFont font;
        Scenes.Scene scene;
        public Texture2D Image { get { return image; } set { image = value; } }
        public Vector2 HotSpot 
        { 
            get 
            { 
                return new Vector2(bounds.X + bounds.Width*hotSpotOffset.X,bounds.Y + bounds.Height*hotSpotOffset.Y); 
            }
            set
            {
                bounds = new Rectangle((int)(value.X - bounds.Width * hotSpotOffset.X), (int)(value.Y - bounds.Height * hotSpotOffset.Y), bounds.Width, bounds.Height);
            }
        }
        public Rectangle Bounds { get { return bounds; } set { bounds = value; } }
        public Cursor(Game game,Texture2D texture,Vector2 position,Vector2 size,Vector2 hotSpotOffset,int index,Scenes.Scene s):base(game)
        {
            image = texture;
            bounds = new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
            this.hotSpotOffset = hotSpotOffset;
            width = game.Window.ClientBounds.Width;
            height = game.Window.ClientBounds.Height;
            controllerIndex = index;
            font = Game1.LogoFont;
            scene = s;
        }
        public Cursor(Game game, Texture2D texture, Vector2 position, Vector2 hotSpot,int index,Scenes.Scene s)
            :this(game,texture,position,new Vector2(texture.Width,texture.Height),hotSpot,index,s)
        {
        }
        
        protected abstract CursorState GetState(GameTime gameTime);

        public override void Update(GameTime gameTime)
        {
            control(GetState(gameTime));
            if (IsAccepted && Accepted != null)
            {
                Accepted();
            }
            if (bounds.X < 0)
            {
                bounds.X = 0;
            }
            else if (bounds.X > width - bounds.Width)
            {
                bounds.X = width - bounds.Width;
            }
            if (bounds.Y < 0)
            {
                bounds.Y = 0;
            }
            else if (bounds.Y > height - bounds.Height)
            {
                bounds.Y = height - bounds.Height;
            }
            
            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            //spriteBatch.Draw(image, bounds, Color.White);
            scene.DrawImage(image, bounds, Color.White);
            string str = controllerIndex + "P";
            Vector2 strBounds = font.MeasureString(str);
            scene.DrawString(str, new Vector2(bounds.X + bounds.Width / 2.0f, bounds.Y + bounds.Height / 2.0f) - strBounds / 2,Vector2.Zero, Color.Gray,0.05f);
            //spriteBatch.DrawString(Game1.LogoFont,str,new Vector2(bounds.X + bounds.Width/2.0f,bounds.Y + bounds.Height/2.0f)-strBounds/2,Color.Gray);
            base.Draw(gameTime);
        }
        void control(CursorState state)
        {
            bounds.X += (int)state.Offset.X;
            bounds.Y += (int)state.Offset.Y;
            IsAccepted = state.Accepted;
        }
        public bool IsAccepted { get; private set; }
        public event Action Accepted;
    }
}
