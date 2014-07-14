using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace Tennis01.Input
{
    class XboxControler:DeviceControler
    {
        GamePadState currentGamePadState;
        GamePadState prevGamePadState;
        PlayerIndex playerIndex;

        public XboxControler(PlayerIndex playerIndex):base(
            Buttons.A,
            Buttons.B,
            Buttons.Y,
            Buttons.X,
            Buttons.Start,
            Buttons.Back,
            Buttons.LeftShoulder,
            Buttons.RightShoulder
        )
        {
            this.playerIndex = playerIndex;
            currentGamePadState = GamePad.GetState(playerIndex);
        }

        public override void Update()
        {
            prevGamePadState = currentGamePadState;
            currentGamePadState = GamePad.GetState(playerIndex);
        }
        protected override ControllerState getState()
        {
            ControllerState res = new ControllerState();
            //現在のボタンの状態をboolで表す
            bool[] current = new bool[]{
                currentGamePadState.IsButtonDown((Buttons)Button1),
                currentGamePadState.IsButtonDown((Buttons)Button2),
                currentGamePadState.IsButtonDown((Buttons)Button3),
                currentGamePadState.IsButtonDown((Buttons)Button4),
                currentGamePadState.IsButtonDown((Buttons)Pause),
                currentGamePadState.IsButtonDown((Buttons)Back),
                currentGamePadState.IsButtonDown((Buttons)L),
                currentGamePadState.IsButtonDown((Buttons)R)
            };
            //1フレーム前のボタンの状態をboolで表す
            bool[] prev = new bool[]{
                prevGamePadState.IsButtonDown((Buttons)Button1),
                prevGamePadState.IsButtonDown((Buttons)Button2),
                prevGamePadState.IsButtonDown((Buttons)Button3),
                prevGamePadState.IsButtonDown((Buttons)Button4),
                prevGamePadState.IsButtonDown((Buttons)Pause),
                prevGamePadState.IsButtonDown((Buttons)Back),
                prevGamePadState.IsButtonDown((Buttons)L),
                prevGamePadState.IsButtonDown((Buttons)R)
            };

            //設定して返す
            res.SetButtonStates(currentGamePadState.ThumbSticks.Left, current, prev);
            return res;
        }
    }
}
