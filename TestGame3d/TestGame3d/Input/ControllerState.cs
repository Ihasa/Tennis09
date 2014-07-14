using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
namespace Tennis01.Input
{
    enum ControlerButtonStates{
        Up,//押されていない
        Pressed,//押された瞬間
        Down,//押されている
        Released//離れた瞬間
    }
    struct ControllerState
    {
        public Vector2 JoyStick;
        public ControlerButtonStates Button1;
        public ControlerButtonStates Button2;
        public ControlerButtonStates Button3;
        public ControlerButtonStates Button4;
        public ControlerButtonStates Pause;
        public ControlerButtonStates Back;
        public ControlerButtonStates L;
        public ControlerButtonStates R;

        public ControllerState(
            Vector2 joyStick,
            ControlerButtonStates button1,
            ControlerButtonStates button2,
            ControlerButtonStates button3,
            ControlerButtonStates button4,
            ControlerButtonStates pause,
            ControlerButtonStates back,
            ControlerButtonStates l,
            ControlerButtonStates r)
        {
            JoyStick = joyStick;
            Button1 = button1;
            Button2 = button2;
            Button3 = button3;
            Button4 = button4;
            Pause = pause;
            Back = back;
            L = l;
            R = r;
        }

        /// <summary>
        /// 何も入力がない状態を表す定数
        /// </summary>
        public static ControllerState NoInput { get { return new ControllerState(); } }

        public enum Inputs
        {
            AllButtons,ColorButtons,FrontButtons,ShotButtons
        }
        public bool HasAnyInput(Inputs input)
        {
            if (input == Inputs.FrontButtons)
            {
                return Button1 == ControlerButtonStates.Pressed ||
                    Button2 == ControlerButtonStates.Pressed ||
                    Button3 == ControlerButtonStates.Pressed ||
                    Button4 == ControlerButtonStates.Pressed ||
                    Pause == ControlerButtonStates.Pressed;
            }
            else if (input == Inputs.ColorButtons)
            {
                return Button1 == ControlerButtonStates.Pressed ||
                    Button2 == ControlerButtonStates.Pressed ||
                    Button3 == ControlerButtonStates.Pressed ||
                    Button4 == ControlerButtonStates.Pressed;
            }
            else if (input == Inputs.AllButtons)
            {
                return Button1 == ControlerButtonStates.Pressed ||
                    Button2 == ControlerButtonStates.Pressed ||
                    Button3 == ControlerButtonStates.Pressed ||
                    Button4 == ControlerButtonStates.Pressed ||
                    Pause == ControlerButtonStates.Pressed ||
                    Back == ControlerButtonStates.Pressed ||
                    L == ControlerButtonStates.Pressed ||
                    R == ControlerButtonStates.Pressed;
            }
            else if (input == Inputs.ShotButtons)
            {
                return Button1 == ControlerButtonStates.Pressed ||
                    Button2 == ControlerButtonStates.Pressed ||
                    //Button3 == ControlerButtonStates.Pressed ||
                    Button4 == ControlerButtonStates.Pressed;
            }
            return false;
        }
        
        /// <summary>
        /// ジョイスティックの状態を表すVector2,現在のボタンの状態,1フレーム前のボタンの状態から、このオブジェクトの状態を決定する
        /// </summary>
        /// <param name="joyStick">ジョイスティックの状態を表すVector2</param>
        /// <param name="currentButtonBools">現在のボタンの状態を列挙した配列。サイズは8でなければならない</param>
        /// <param name="prevButtonBools">1フレーム前のボタンの状態を列挙した配列。サイズは8でなければならない</param>
        public void SetButtonStates(Vector2 joyStick,bool[] currentButtonBools,bool[] prevButtonBools)
            //bool button1, bool button2, bool button3, bool button4, bool pause, bool buttonL, bool buttonR,
            //bool button1p,bool button2p,bool button3p,bool button4p,bool pausep,bool buttonLp,bool buttonRp)
        {
            if (currentButtonBools.Length != 8 || prevButtonBools.Length != 8 || currentButtonBools.Length != prevButtonBools.Length)
                throw new ArgumentException();

            //JoyStickの状態を反映
            JoyStick = joyStick;

            //繰返しで参照する用の配列を作る
            ControlerButtonStates[] buttons = new ControlerButtonStates[8];

            //現在の状態と1フレーム前の状態によってボタンフラグを変更
            for (int i = 0; i < buttons.Length; i++)
            {
                //全4通り。
                if (currentButtonBools[i])
                {
                    if (prevButtonBools[i])
                    {
                        buttons[i] = ControlerButtonStates.Down;
                    }
                    else
                    {
                        buttons[i] = ControlerButtonStates.Pressed;
                    }
                }
                else
                {
                    if (prevButtonBools[i])
                    {
                        buttons[i] = ControlerButtonStates.Released;
                    }
                    else
                    {
                        buttons[i] = ControlerButtonStates.Up;
                    }
                }
            }
            //メンバに設定してやる
            Button1 = buttons[0];
            Button2 = buttons[1];
            Button3 = buttons[2];
            Button4 = buttons[3];
            Pause = buttons[4];
            Back = buttons[5];
            L = buttons[6];
            R = buttons[7];

#region 非常にめんどい方法
            //if (button1) Button1 = ControlerButtonStates.Down; else Button1 = ControlerButtonStates.Up;

            //if (button2) Button1 = ControlerButtonStates.Down; else Button2 = ControlerButtonStates.Up;
            
            //if (button3) Button1 = ControlerButtonStates.Down; else Button3 = ControlerButtonStates.Up;
            
            //if (button4) Button1 = ControlerButtonStates.Down; else Button4 = ControlerButtonStates.Up;
            
            //if (pause) Button1 = ControlerButtonStates.Down; else Pause = ControlerButtonStates.Up;
            
            //if (buttonL) Button1 = ControlerButtonStates.Down; else L = ControlerButtonStates.Up;
            
            //if (buttonR) Button1 = ControlerButtonStates.Down; else R = ControlerButtonStates.Up;

            ////修正
            ////まだしてません 06/12

            //if (button1p) Button1 = ControlerButtonStates.Down; else Button1 = ControlerButtonStates.Up;

            //if (button2p) Button1 = ControlerButtonStates.Down; else Button2 = ControlerButtonStates.Up;

            //if (button3p) Button1 = ControlerButtonStates.Down; else Button3 = ControlerButtonStates.Up;

            //if (button4p) Button1 = ControlerButtonStates.Down; else Button4 = ControlerButtonStates.Up;

            //if (pausep) Button1 = ControlerButtonStates.Down; else Pause = ControlerButtonStates.Up;

            //if (buttonLp) Button1 = ControlerButtonStates.Down; else L = ControlerButtonStates.Up;

            //if (buttonRp) Button1 = ControlerButtonStates.Down; else R = ControlerButtonStates.Up;
#endregion

        }

    }
}
