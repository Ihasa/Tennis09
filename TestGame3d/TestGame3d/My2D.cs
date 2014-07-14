using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace Tennis01
{
    using _2DComponents;
    using Input;
    using Extention;
    class MyCursor : Cursor
    {
        public Controller Controller { get; private set; }
        //ControlerState current, last;
        private MyCursor(Texture2D tex,Color color, Rectangle rect,Vector2 initPosition, Vector2 hotSpot, Controller c, Vector2 scale,int index)
            : base(GameMain.LogoFont,rect,tex,color, initPosition, hotSpot, scale,0.02f,index)
        {
            Controller = c;
            //current = last = new ControlerState();
        }
        public static MyCursor GetStdCursor(int index)
        {
            Color c;
            if(index == 0)
                c = Color.White;
            else{
                c = Color.Blue;
            }
            return new MyCursor(GameMain.Textures["cursor"], c, Scenes.Scene.Game.WindowRect, Vector2.One * (0.5f + index * 0.25f), Vector2.Zero, Scenes.Scene.Controllers[index], Vector2.One * 0.1f, index);
        }
        protected override CursorState GetState(GameTime gameTime)
        {
            //            last = current;
            ControllerState controlerState = Controller.GetState();
            CursorState cursorState = new CursorState();
            Vector2 vec = controlerState.JoyStick;
            vec.Y = -vec.Y;
            cursorState.Offset = vec;// *gameTime.ElapsedGameTime.Milliseconds;
            cursorState.Accepted = controlerState.Button1 == ControlerButtonStates.Pressed;
            return cursorState;
        }
    }
    class SwitchButton : Component2D
    {
        Button[] buttons;
        Button current;
        Logo title;
        public SwitchButton(Rectangle parent, Vector2 position, Vector2 origin, Vector2 scale, Cursor cursor,float margin,string title,Color titleColor,int iniIndex=0, params MenuButtonParams[] selects)
            : base(parent, position, origin, scale, Rotation.Zero, Color.White)
        {
            buttons = new Button[selects.Length];
            margin *= 0.7f;
            Vector2 buttonScale = new Vector2(1.0f / selects.Length /(margin+1),1.0f) * 0.7f;
            for (int i = 0; i < selects.Length; i++)
            {
                Texture2D tex = selects[i].Texture;
                Animation pressed = new Animation("pressed",
                    new AnimationKey(0,new LogoParams(tex,Vector2.Zero,Vector2.One*0.5f,buttonScale,Rotation.Zero,Color.White),RotationWays.ABSOLUTE),
                    new AnimationKey(5,new LogoParams(tex,Vector2.Zero,Vector2.One*0.5f,buttonScale*0.8f,Rotation.Zero,Color.Gray),RotationWays.ABSOLUTE),
                    new AnimationKey(10,new LogoParams(tex,Vector2.Zero,Vector2.One*0.5f,buttonScale,Rotation.Zero,Color.White),RotationWays.ABSOLUTE)
                );
                Animation waiting = new Animation("waiting",
                    new AnimationKey(0,new LogoParams(tex,Vector2.Zero,Vector2.One*0.5f,buttonScale,Rotation.Zero,Color.White),RotationWays.ABSOLUTE),
                    new AnimationKey(60, new LogoParams(tex, Vector2.Zero, Vector2.One * 0.5f, buttonScale, Rotation.Zero, new Color(160,160,160)), RotationWays.ABSOLUTE),
                    new AnimationKey(120, new LogoParams(tex, Vector2.Zero, Vector2.One * 0.5f, buttonScale, Rotation.Zero, Color.White), RotationWays.ABSOLUTE)
                );
                Animation oncursor = new Animation("oncursor",
                    new AnimationKey(0,new LogoParams(tex,Vector2.Zero,Vector2.One*0.5f,buttonScale,Rotation.Zero,Color.White),RotationWays.ABSOLUTE),
                    new AnimationKey(5, new LogoParams(tex, Vector2.Zero, Vector2.One * 0.5f, buttonScale, Rotation.Zero, Color.White), RotationWays.ABSOLUTE)
                );
                buttons[i] = new Button(GameMain.LogoFont, this, Vector2.Zero,new Vector2(1.0f / selects.Length * (i+0.5f)*0.7f+0.3f,0.5f), cursor, pressed, waiting, oncursor);
                buttons[i].Text = selects[i].Name;
                Button b = buttons[i];
                buttons[i].Pressed += () =>
                {
                    changeCurrent(b);
                };
            }
            this.title = new Logo(GameMain.LogoFont, this, new Vector2(0.15f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.3f, 1f), Rotation.Zero, titleColor, null, title,titleColor);
            changeCurrent(buttons[iniIndex < buttons.Length ? iniIndex : 0]);
        }
        private void changeCurrent(Button b)
        {
            current = b;
            current.MultiColor = Color.White;
            current.Enabled = false;
            foreach (Button but in buttons)
            {
                if (current != but)
                {
                    but.MultiColor = new Color(64,64,64);
                    but.Enabled = true;
                }
            }
        }
        public void ChangeCurrent(int index)
        {
            changeCurrent(buttons[index]);
        }
        public string CurrentValue
        {
            get { return current.Text; }
        }
        public override int ComponentWidth
        {
            get { throw new NotImplementedException(); }
        }
        public override int ComponentHeight
        {
            get { throw new NotImplementedException(); }
        }
        protected override void drawContent(SpriteBatch spriteBatch, GameTime gameTime)
        {
            foreach (Button b in buttons)
            {
                b.Draw(gameTime);
            }
            title.Draw(gameTime);
        }
        public override void Update(GameTime gameTime)
        {
            foreach (Button b in buttons)
            {
                b.Update(gameTime);
            }
            title.Update(gameTime);
            //GameMain.debugStr[""] = CurrentValue;
            base.Update(gameTime);
        }
    }
    class TuneButton : Image
    {
        Button button;
        float buttonWidth;
        float amount { get { return button.DefaultPosition.X / (1 - buttonWidth); } set { button.DefaultPosition = new Vector2(value, 0); } }
        float defValue;
        float max, min,step;
        MyCursor cursor;
        Controller controller;
        bool pressed = false;
        Vector2 lastCursor, currentCursor;
        string title;

        public TuneButton(string title,Rectangle parent, Vector2 position, Vector2 origin, Vector2 scale,MyCursor cursor,Texture2D buttonTex,float buttonWidth,float min,float max,float step,float defValue)
            : base(GameMain.Textures["gra"],parent, position, origin, scale, Rotation.Zero, Color.White)
        {
            this.title = title;
            this.buttonWidth = buttonWidth;
            maxX = 1 - buttonWidth;
            Animation pressed = new Animation("pressed",
                new AnimationKey(0,new LogoParams(buttonTex,Vector2.Zero,Vector2.Zero,new Vector2(buttonWidth,1.0f),Rotation.Zero,Color.White),RotationWays.ABSOLUTE),
                new AnimationKey(3, new LogoParams(buttonTex, Vector2.Zero, Vector2.Zero, new Vector2(buttonWidth, 1.0f) * 1.0f, Rotation.Zero, Color.White), RotationWays.ABSOLUTE)
            );
            Animation oncursor = new Animation("oncursor",
                new AnimationKey(0,new LogoParams(buttonTex,Vector2.Zero,Vector2.Zero,new Vector2(buttonWidth,1.0f),Rotation.Zero,Color.White),RotationWays.ABSOLUTE),
                new AnimationKey(10, new LogoParams(buttonTex, Vector2.Zero, Vector2.Zero, new Vector2(buttonWidth, 1.0f) * 1.0f, Rotation.Zero, Color.White), RotationWays.ABSOLUTE)
            );
             Animation waiting = new Animation("waiting",
                new AnimationKey(0, new LogoParams(buttonTex, Vector2.Zero, Vector2.Zero, new Vector2(buttonWidth, 1.0f), Rotation.Zero, Color.White), RotationWays.ABSOLUTE)
            );
            button = new Button(GameMain.LogoFont, this, Vector2.Zero, Vector2.Zero, cursor, pressed, waiting, oncursor);

            this.max = max;
            this.min = min;
            this.step = step;
            this.cursor = cursor;
            CurrentValue = this.defValue = defValue;
            currentCursor = cursor.Position;
            //本当はカーソルを操るコントローラにアクセスできるように
            controller = cursor.Controller;
        }
        public void Reset()
        {
            amount = defValue;
        }
        public float CurrentValue 
        { 
            get
            {
                //float val = MathHelper.Lerp(0, max - min, amount);
                //return min + (int)(val / step) * step;
                return (int)(MathHelper.Lerp(min,max,amount)/step) * step;
            }
            set
            {
                amount = (value - min) / (max - min) * (1 - buttonWidth);
            }
        }
        protected override void drawContent(SpriteBatch spriteBatch, GameTime gameTime)
        {
            base.drawContent(spriteBatch, gameTime);
            button.Draw(gameTime);
        }
        public override void Update(GameTime gameTime)
        {
            lastCursor = currentCursor;
            currentCursor = cursor.Position;
            button.Update(gameTime);
            ControllerState state = controller.GetState();
            //押されたかどうか
            if (state.Button1 == ControlerButtonStates.Pressed && button.DrawArea.Contains(cursor.HotSpotScreen))
            {
                pressed = true;
                button.MultiColor = Color.Gray;
            }
            if (state.Button1 == ControlerButtonStates.Released)
            {
                pressed = false;
                button.MultiColor = Color.White;
                cursor.Enabled = true;
            }

            //押されてたら移動
            if (pressed)
            {
                Vector2 scale = this.Scale;
                if (ParentComponent != null)
                    scale *= this.ParentComponent.Scale;
                button.DefaultPosition += new Vector2(currentCursor.X - lastCursor.X, 0) / scale;
            }

            //行き過ぎでの修正
            if (button.DefaultPosition.X < 0)
            {
                button.DefaultPosition = new Vector2(0, 0);
                cursor.Enabled = false;
            }
            else if (button.DefaultPosition.X > maxX)
            {
                button.DefaultPosition = new Vector2(maxX, 0);
                cursor.Enabled = false;
                //max = button.DefaultPosition.X;
            }
            else if (button.DefaultPosition.X == 0 && state.JoyStick.X > 0)
            {
                cursor.Enabled = true;
            }
            else if (button.DefaultPosition.X == maxX && state.JoyStick.X < 0)
            {
                cursor.Enabled = true;
            }

            //文字はタイトル
            button.Text = title;// +CurrentValue;
            base.Update(gameTime);
        }
        float maxX;
    }
    class MySlideShow : SlideShow
    {
        Controller controller;
        public MySlideShow(Rectangle parentArea, Controller controller, params Texture2D[] images)
            : base(parentArea, 15, images)
        {
            this.controller = controller;
        }
        public override void Update(GameTime gameTime)
        {
            ControllerState state = controller.GetState();
            if (state.JoyStick.X > 0.5f)
            {
                Next();
            }
            else if (state.JoyStick.X < -0.5f)
            {
                Back();
            }

            if (game.Components.Contains(this) && state.HasAnyInput(ControllerState.Inputs.ColorButtons))
            {
                RemoveFromGameComponents(game);
            }
            base.Update(gameTime);
        }
    }
}
