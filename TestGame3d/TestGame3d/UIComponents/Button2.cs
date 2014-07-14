using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Extention;
namespace Tennis01.UIComponents
{
    class Button2 : AnimatableLogo
    {
        Cursor cursor;
        Rectangle validArea { get { return bounds; } }
        static string pressedAnimation = "pressed";
        static string waitingAnimation = "normal";
        public event Action Pressed;
        bool pressed = false;
        public Button2(Texture2D texture,string text, Vector2 position, Cursor cursor,Scenes.Scene scene)
            : base(texture, position, scene)
        {
            base.Text = text;
            cursor.DrawOrder = DrawOrder + 1;
            //validArea = new Rectangle((int)(position.X*winWidth - texture.Width / 2), (int)(position.Y*winHeight - texture.Height / 2), texture.Width, texture.Height);
            this.cursor = cursor;
            float defSize = 0.3f;
            Animation pressed = new Animation(pressedAnimation,
                new AnimationKey(0, new LogoParams(Vector2.Zero, Vector2.One * defSize, defSize*0.5f, Color.White)),
                new AnimationKey(7, new LogoParams(Vector2.Zero, Vector2.One * defSize * 0.6f, defSize*0.6f*0.5f, new Color(128, 128, 128))),
                new AnimationKey(14, new LogoParams(Vector2.Zero, Vector2.One * defSize, defSize*0.5f, Color.White)),
                new AnimationKey(30, new LogoParams(Vector2.Zero, Vector2.One * defSize, defSize*0.5f, Color.White))
                );
            Animation waiting = new Animation(waitingAnimation,
                new AnimationKey(0, new LogoParams(Vector2.Zero, Vector2.One * defSize * (1 + (float)Math.Sin(MathHelper.ToRadians(0))), defSize * 0.5f, Color.White)),
                new AnimationKey(5, new LogoParams(Vector2.Zero, Vector2.One * defSize * (1 + (float)Math.Sin(MathHelper.ToRadians(30))), defSize * 0.5f, Color.White)),
                new AnimationKey(10, new LogoParams(Vector2.Zero, Vector2.One * defSize * (1 + (float)Math.Sin(MathHelper.ToRadians(45))), defSize * 0.5f, Color.White)),
                new AnimationKey(15, new LogoParams(Vector2.Zero, Vector2.One * defSize * (1 + (float)Math.Sin(MathHelper.ToRadians(60))), defSize * 0.5f, Color.White)),
                new AnimationKey(20, new LogoParams(Vector2.Zero, Vector2.One * defSize * (1 + (float)Math.Sin(MathHelper.ToRadians(90))), defSize * 0.5f, Color.White)),
                new AnimationKey(25, new LogoParams(Vector2.Zero, Vector2.One * defSize * (1 + (float)Math.Sin(MathHelper.ToRadians(120))), defSize * 0.5f, Color.White)),
                new AnimationKey(30, new LogoParams(Vector2.Zero, Vector2.One * defSize * (1 + (float)Math.Sin(MathHelper.ToRadians(135))), defSize * 0.5f, Color.White)),
                new AnimationKey(35, new LogoParams(Vector2.Zero, Vector2.One * defSize * (1 + (float)Math.Sin(MathHelper.ToRadians(150))), defSize * 0.5f, Color.White)),
                new AnimationKey(40, new LogoParams(Vector2.Zero, Vector2.One * defSize * (1 + (float)Math.Sin(MathHelper.ToRadians(180))), defSize * 0.5f, Color.White))
            );
            Animation onCursor = new Animation("onCursor",
                new AnimationKey(0, new LogoParams(Vector2.Zero, Vector2.One * defSize, defSize * 0.5f, Color.White)),
                new AnimationKey(20, new LogoParams(new Vector2(0, -0.01f), Vector2.One * defSize * 1.2f, defSize * 0.5f, Color.White))
                );
            AddAnimations(pressed, waiting,onCursor);
            Animate(waitingAnimation,0);
        }
        public Button2(Texture2D texture, string text, Vector2 position, Cursor cursor, Scenes.Scene scene,Animation pressed,Animation waiting,Animation onCursor)
            :base(texture,position,scene,pressed,waiting,onCursor)
        {
            base.Text = text;
            cursor.DrawOrder = DrawOrder + 1;
            //validArea = new Rectangle((int)(position.X * winWidth - texture.Width / 2), (int)(position.Y * winHeight - texture.Height / 2), texture.Width, texture.Height);
        }

        public override void Update(GameTime gameTime)
        {
            if (!pressed)
            {
                if (validArea.Contains(cursor.HotSpot))
                {                    
                    if (cursor.IsAccepted && currentAnimation.Name != pressedAnimation)
                    {
                        Animate(pressedAnimation, 1);
                        pressed = true;
                        //カーソルが動かないように
                        //ここでやる??
                        cursor.Enabled = false;
                    }
                }
            }
            else if (!currentAnimation.IsAnimating)
            {
                //カーソル拘束解除
                cursor.Enabled = true;
                if (Pressed != null)
                {
                    Pressed();
                }
                pressed = false;
                if (currentAnimation.Name != waitingAnimation)
                {
                    Animate(waitingAnimation, 0);
                }
            }
            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            
            base.Draw(gameTime);
        }
    }
}
