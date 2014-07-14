using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using WiimoteLib;
namespace Tennis01.UIComponents
{
    class WiiCursor:Cursor
    {
        MyWiimote wiimote;
        int width, height;
        public WiiCursor(Game1 game, string textureName,int index,Scenes.Scene scene)
            : base(game, Game1.Textures[textureName], Vector2.One * 10, Vector2.Zero,index,scene)
        {
            wiimote = Game1.Wiimote;
            width = game.Window.ClientBounds.Width;
            height = game.Window.ClientBounds.Height;
        }

        protected override CursorState GetState(Microsoft.Xna.Framework.GameTime gameTime)
        {
            CursorState res = new CursorState();
            //IRState irState = wiimote.WiimoteState.IRState;
            Vector2 irPosi = new Vector2((1-wiimote.IRPosition.X)*(float)width, wiimote.IRPosition.Y*(float)height);
            Game1.debugStr["irPosi"] = wiimote.IRPosition+"";
            //if (irFound(wiimote.WiimoteState))
                HotSpot = irPosi;
            //else
            //    HotSpot = HotSpot;
            res.Accepted = !wiimote.LastButtonState.A && wiimote.CurrentButtonState.A;
            return res;
        }
        bool irFound(WiimoteState wiiState)
        {
            for (int i = 0; i < 4; i++)
            {
                if (wiiState.IRState.IRSensors[i].Found)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
