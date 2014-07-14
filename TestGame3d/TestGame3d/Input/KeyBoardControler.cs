using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace Tennis01.Input
{
    class KeyBoardControler:DeviceControler
    {
        KeyboardState currentState, lastState;
        public KeyBoardControler() : base(
            Keys.Z,
            Keys.C,
            Keys.S,
            Keys.X,
            Keys.Enter,
            Keys.Space,
            Keys.A,
            Keys.LeftShift
        )
        {
            currentState = Keyboard.GetState();
        }
        protected override ControllerState getState()
        {
            ControllerState state = new ControllerState();
            state.SetButtonStates(
                getJoyStick(),
                new bool[]{
                    currentState.IsKeyDown((Keys)Button1),
                    currentState.IsKeyDown((Keys)Button2),
                    currentState.IsKeyDown((Keys)Button3),
                    currentState.IsKeyDown((Keys)Button4),
                    currentState.IsKeyDown((Keys)Pause),
                    currentState.IsKeyDown((Keys)Back),
                    currentState.IsKeyDown((Keys)L),
                    currentState.IsKeyDown((Keys)R)
                },
                new bool[]{
                    lastState.IsKeyDown((Keys)Button1),
                    lastState.IsKeyDown((Keys)Button2),
                    lastState.IsKeyDown((Keys)Button3),
                    lastState.IsKeyDown((Keys)Button4),
                    lastState.IsKeyDown((Keys)Pause),
                    lastState.IsKeyDown((Keys)Back),
                    lastState.IsKeyDown((Keys)L),
                    lastState.IsKeyDown((Keys)R)                    
                }
            );
            return state;
        }
        private Vector2 getJoyStick()
        {
            Vector2 vec = Vector2.Zero;
            if (currentState.IsKeyDown(Keys.Left))
            {
                vec.X = -1;
            }
            else if (currentState.IsKeyDown(Keys.Right))
            {
                vec.X = 1;
            }

            if (currentState.IsKeyDown(Keys.Up))
            {
                vec.Y = 1;
            }
            else if (currentState.IsKeyDown(Keys.Down))
            {
                vec.Y = -1;
            }
            return vec;
        }
        public override void Update()
        {
            lastState = currentState;
            currentState = Keyboard.GetState();
        }
    }
}
