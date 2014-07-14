using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tennis01.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace Tennis01.UIComponents
{
    class KeyCursor:Cursor
    {
        KeyboardState current, last;
        public KeyCursor(Game game, Texture2D texture, Vector2 position, Vector2 hotSpot,int index,Scenes.Scene scene)
            : base(game, texture, position, hotSpot,index,scene)
        {
            current = new KeyboardState();
            last = new KeyboardState();
        }
        protected override CursorState GetState(GameTime gameTime)
        {
            CursorState state = new CursorState();
            float offsetY = 0;
            float offsetX = 0;
            if (current.IsKeyDown(Keys.Up))
                offsetY = -5;
            else if (current.IsKeyDown(Keys.Down))
                offsetY = 5;
            if (current.IsKeyDown(Keys.Left))
                offsetX = -5;
            else if (current.IsKeyDown(Keys.Right))
                offsetX = 5;
            state.Offset = new Vector2(offsetX, offsetY);
            state.Accepted = current.IsKeyDown(Keys.Enter) && last.IsKeyUp(Keys.Enter);
            return state;
        }
        public override void Update(GameTime gameTime)
        {
            last = current;
            current = Keyboard.GetState();
            base.Update(gameTime);
        }
    }

    class XBoxCursor : Cursor
    {
        GamePadState current, last;
        public XBoxCursor(Game game, Texture2D texture, Vector2 position, Vector2 hotSpot,int index,Scenes.Scene scene)
            : base(game, texture, position, hotSpot,index,scene)
        {
            current = new GamePadState();
            last = new GamePadState();
        }
        protected override CursorState GetState(GameTime gameTime)
        {
            CursorState state = new CursorState();
            

            //if (current.IsKeyDown(Keys.Up))
            //    offsetY = -5;
            //else if (current.IsKeyDown(Keys.Down))
            //    offsetY = 5;
            //if (current.IsKeyDown(Keys.Left))
            //    offsetX = -5;
            //else if (current.IsKeyDown(Keys.Right))
            //    offsetX = 5;
            Vector2 direction = current.ThumbSticks.Left;
            direction.Y = -direction.Y;
            state.Offset = direction * 10;
            //System.Windows.Forms.MessageBox.Show("" + state.Offset);
            state.Accepted = current.IsButtonDown(Buttons.A) && last.IsButtonUp(Buttons.A);
            return state;
        }
        public override void Update(GameTime gameTime)
        {
            last = current;
            current = GamePad.GetState(PlayerIndex.One);
            base.Update(gameTime);
        }
    }
    class MyCursor : Cursor
    {
        Controller controler;
        //ControlerState current, last;
        public MyCursor(Game game, Texture2D tex, Vector2 initPosition, Vector2 hotSpot, Controller c,int index,Scenes.Scene scene)
            : base(game, tex, initPosition, hotSpot,index,scene)
        {
            controler = c;
            //current = last = new ControlerState();
        }
        
        protected override CursorState GetState(GameTime gameTime)
        {
//            last = current;
            ControllerState controlerState = controler.GetState();
            CursorState cursorState = new CursorState();
            Vector2 vec = controlerState.JoyStick;
            vec.Y = -vec.Y;
            cursorState.Offset = vec * gameTime.ElapsedGameTime.Milliseconds;
            cursorState.Accepted = controlerState.Button1 == ControlerButtonStates.Pressed;
            return cursorState;
        }
    }
}
