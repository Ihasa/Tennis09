using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tennis01.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Extention;
namespace Tennis01.UIComponents
{
    class Button:DrawableGameComponent
    {
        Texture2D texture;
        Rectangle bounds;
        Rectangle newBounds;
        SpriteBatch spriteBatch;
        int generalTimer, time;
        float animationTime = 5;

        SpriteFont font;
        string str;
        bool pressed;
        Cursor cursor;
        //MouseState currentMouseState, lastMouseState;
        //KeyboardState currentKeyState, lastKeyState;
        public event Action Pressed;
        public bool IsEnabled { get; set; }
        public string Name { get { return str; } set { str = value; } }
        public Color OverColor { get; set; }
        public Color MouseOverColor { get; set; }
        public Color TextColor { get; set; }

        public Button(Game game, Texture2D tex,SpriteFont font, Vector2 position,Vector2? size, string str,Cursor c,Color overColor)
            : base(game)
        {
            DrawOrder = c.DrawOrder - 1;
            texture = tex;
            this.font = font;
            cursor = c;
            OverColor = overColor;
            TextColor = new Color(255-overColor.R, 255-overColor.G, 255-overColor.B);
            MouseOverColor = new Color(255-OverColor.R, OverColor.G, OverColor.B);

            if (size == null)
            {
                int wid = (int)(font.MeasureString(str).X / 0.8f);
                int hei = (int)(font.MeasureString(str).Y / 0.8f);
                bounds = new Rectangle((int)position.X, (int)position.Y, wid, hei);
            }
            else
            {
                bounds = new Rectangle((int)position.X, (int)position.Y, (int)(((Vector2)size).X), (int)(((Vector2)size).Y));
            }
            newBounds = bounds;
            this.str = str;
            pressed = false;
            spriteBatch = new SpriteBatch(game.GraphicsDevice);
            if (font.MeasureString(str).X > bounds.Width)
            {
                this.bounds.Width = (int)(font.MeasureString(str).X);
            }
            //if (font.MeasureString(str).Y /** 0.8f*/ > area.Height)
            //{
            //    this.area.Height = (int)(font.MeasureString(str).Y /*/ 0.8f*/);
            //}
            IsEnabled = true;
            generalTimer = 0;
            time = -5;
            //if(shortCuts != null && shortCuts.Length != 0)
            //    shortCutKeys = shortCuts;

            //OverColor = Color.White;
            //MouseOverColor = Color.Yellow;
            //TextColor = Color.Black;
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            float rate;
            float rateMin = 0.8f;
            Color color;
            if (IsEnabled)
            {
                if (!pressed)
                {
                    if (generalTimer - time > animationTime)
                    {
                        rate = 1;
                        if (bounds.Contains(cursor.HotSpot))//currentMouseState.X > bounds.X && currentMouseState.X < bounds.X + bounds.Width && currentMouseState.Y > bounds.Y && currentMouseState.Y < bounds.Y + bounds.Height)
                        {
                            color = MouseOverColor;
                        }
                        else
                        {
                            color = new Color(OverColor.ToVector3() * MathHelper.Lerp(0.4f, 1, rate));
                        }
                    }
                    else
                    {
                        rate = MathHelper.Lerp(rateMin, 1, (generalTimer - time) / animationTime);
                        color = new Color(OverColor.ToVector3() * MathHelper.Lerp(0.4f, 1, rate));
                    }
                }
                else
                {
                    if (generalTimer - time > animationTime)
                        rate = rateMin;
                    else
                        rate = MathHelper.Lerp(1, rateMin, (generalTimer - time) / animationTime);
                    color = new Color(OverColor.ToVector3() * MathHelper.Lerp(1, 0.4f, rate));
                }
            }
            else
            {
                rate = 1;
                color = new Color(0.3f, 0.3f, 0.3f);
            }
            //spriteBatch.Draw(texture, new Rectangle(area.X + (int)(area.Width * (1 - rate) / 2), area.Y + (int)(area.Height * (1 - rate) / 2), (int)(area.Width * rate), (int)(area.Height * rate)), color);
            spriteBatch.Draw(texture, bounds.Scaling(rate), color);
            spriteBatch.DrawString(font, str, new Vector2(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2) - font.MeasureString(str) / 2 * rate, TextColor, 0, Vector2.Zero, rate, SpriteEffects.None, 0);
            spriteBatch.End();
            base.Draw(gameTime);
        }
        public override void Update(GameTime gameTime)
        {
            //lastMouseState = currentMouseState;
            //currentMouseState = Mouse.GetState();
            //lastKeyState = currentKeyState;
            //currentKeyState = Keyboard.GetState();

            if (IsEnabled)
            {
                
                if (cursor.IsAccepted)//currentMouseState.LeftButton == ButtonState.Pressed)
                {
                //    if (lastMouseState.LeftButton == ButtonState.Released)
                //    {
                        if (!pressed &&
                            bounds.Contains(cursor.HotSpot))
                            //currentMouseState.X < bounds.X + bounds.Width && currentMouseState.X > bounds.X &&
                            //currentMouseState.Y < bounds.Y + bounds.Height && currentMouseState.Y > bounds.Y)
                        {
                            pressed = true;
                            registTime();
                            //onPressed();
                        }
                //    }
                }
                else //if (currentMouseState.LeftButton == ButtonState.Released)
                {
                    //if (true/*clicked() || shortCutsPressed()*/)
                    //{
                    //    onPressed();
                    //}
                    if (pressed)
                    {
                        //registTime();
                        if (generalTimer - time == animationTime)
                        {
                            pressed = false;
                            onPressed();
                            registTime();
                        }
                    }
                }
                generalTimer++;
            }
            base.Update(gameTime);
        }
        private void changeSize(float rate)
        {
            newBounds = bounds.Scaling(rate);
            registTime();
        }
        //bool clicked()
        //{
        //    return pressed &&
        //                lastMouseState.LeftButton == ButtonState.Pressed &&
        //                currentMouseState.X < bounds.X + bounds.Width && currentMouseState.X > bounds.X &&
        //                currentMouseState.Y < bounds.Y + bounds.Height && currentMouseState.Y > bounds.Y;
        //}
        //bool shortCutsPressed()
        //{
        //    if (shortCutKeys != null)
        //    {

        //        foreach (Keys key in shortCutKeys)
        //        {
        //            if (currentKeyState.IsKeyUp(key))
        //                return false;
        //        }
        //        bool allSame = true;
        //        foreach (Keys key in shortCutKeys)
        //        {
        //            if (lastKeyState[key] != currentKeyState[key])
        //                allSame = false;
        //        }
        //        if (allSame)
        //            return false;

        //        return true;
        //    }
        //    return false;
        //}
        void onPressed()
        {
            if (Pressed != null)
                Pressed();
        }
        void registTime()
        {
            time = generalTimer;
        }
    }
}
