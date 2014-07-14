using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Tennis01.Input
{
    /// <summary>
    /// コントローラのボタンの参照先を保持する
    /// </summary>
    abstract class DeviceControler:Controller
    {
        protected Object Button1 { get; private set; }
        protected Object Button2 { get; private set; }
        protected Object Button3 { get; private set; }
        protected Object Button4 { get; private set; }
        protected Object Pause { get; private set; }
        protected Object Back { get; private set; }
        protected Object L { get; private set; }
        protected Object R { get; private set; }
        //protected Scenes.Camera camera;
        /// <summary>
        /// 各ボタンの参照先を決めて初期化する
        /// </summary>
        /// <param name="button1">ショットボタン1</param>
        /// <param name="button2">ショットボタン2</param>
        /// <param name="button3">ショットボタン3</param>
        /// <param name="button4">ショットボタン4</param>
        /// <param name="pauseButton">ポーズボタン</param>
        /// <param name="buttonL">Lボタン</param>
        /// <param name="buttonR">Rボタン</param>
        protected DeviceControler(Object button1, Object button2, Object button3, Object button4, Object pauseButton, Object backButton,Object buttonL, Object buttonR)
        {
            SetButtonReference(button1, button2, button3, button4, pauseButton, backButton,buttonL, buttonR);
            //camera = c;
        }
        //public override ControlerState GetState()
        //{
        //    ControlerState res = getState();
        //    //if (Camera.Position.Z < 0)
        //    //    res.JoyStick = -res.JoyStick;
        //    return res;
        //}
        //protected abstract ControlerState getState();
        /// <summary>
        /// 各ボタンの参照先を決める
        /// </summary>
        /// <param name="button1"></param>
        /// <param name="button2"></param>
        /// <param name="button3"></param>
        /// <param name="button4"></param>
        /// <param name="pauseButton"></param>
        /// <param name="buttonL"></param>
        /// <param name="buttonR"></param>
        protected void SetButtonReference(Object button1, Object button2, Object button3, Object button4, Object pauseButton,Object backButton, Object buttonL, Object buttonR)
        {
            Button1 = button1;
            Button2 = button2;
            Button3 = button3;
            Button4 = button4;
            Pause = pauseButton;
            Back = backButton;
            L = buttonL;
            R = buttonR;
        }
        /// <summary>
        /// 各ボタンの参照先コレクションを返す
        /// </summary>
        /// <returns>各ボタンの参照先が格納されたObjectの配列</returns>
        protected Object[] GetButtonReference()
        {
            return new Object[]{
                Button1,Button2,Button3,Button4,Pause,L,R
            };
        }
    }
}
