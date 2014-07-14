using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using WiimoteLib;
namespace Tennis01.Input
{
    class WiimoteController:Controller
    {
        protected MyWiimote wiimote;
        protected ButtonState currentWiiButtonState, lastWiiButtonState;
        protected ClassicControllerState currentClassicState, lastClassicState;
        protected NunchukState currentNunchuk, lastNunchuk;
        Objects.Player player;
        public WiimoteController(MyWiimote wm,Objects.Player p,InputReport inputReport)
            : base()
        {
            player = p;
            wiimote = wm;
            //Wiimote.SetLEDs(false, false, false, false);
            currentWiiButtonState = new ButtonState();
            lastWiiButtonState = new ButtonState();
            currentClassicState = new ClassicControllerState();
            lastClassicState = new ClassicControllerState();
            currentNunchuk = new NunchukState();
            lastNunchuk = new NunchukState();
            //wiimote.Wiimote.WiimoteExtensionChanged += (o, e) =>
            //{
            //    if (e.Inserted)
            //    {
            //        wiimote.Wiimote.SetReportType(InputReport.IRExtensionAccel,true);
            //    }
            //    else
            //    {
            //        wiimote.SetReportType(InputReport.IRAccel,true);
            //    }
            //};
        }
        protected override ControllerState getState()
        {
            ControllerState res = new ControllerState();
            //if (currentState.ExtensionType == lastState.ExtensionType)
            //{
                switch (wiimote.Wiimote.WiimoteState.ExtensionType)
                {
                    case ExtensionType.MotionPlus://メインモード
                        break;
                    case ExtensionType.Nunchuk://リモコンを振ってショット、ヌンチャクのスティックで移動
                        Vector2 joy = new Vector2(wiimote.Wiimote.WiimoteState.NunchukState.Joystick.X, wiimote.Wiimote.WiimoteState.NunchukState.Joystick.Y);
                        //if (Camera.Position.Z < 0)
                        //    joy = -joy;
                        if (joy.Length() > 0.1f)
                            joy = Vector2.Normalize(joy);
                        else
                            joy = Vector2.Zero;
#region リモコンを振って操作する場合
                        AccelState accelState = wiimote.Wiimote.WiimoteState.AccelState;
                        if (accelState.Values.X > 1.0f)
                        {
                            if (accelState.Values.Z > 0)
                            {
                                res.Button1 = ControlerButtonStates.Pressed;
                            }
                            else
                            {
                                res.Button4 = ControlerButtonStates.Pressed;
                            }
                        }
                        if (player.IsServing)
                        {
                            if (currentNunchuk.C || currentNunchuk.Z)
                            {
                                res.Button1 = ControlerButtonStates.Pressed;
                            }
                        }
                        res.JoyStick = joy;
#endregion
#region ボタン使用の場合
                        //ボタンで代用
                        //res.SetButtonStates(joy,
                        //    new bool[]{
                        //        currentWiiButtonState.A,
                        //        currentWiiButtonState.Minus,
                        //        currentWiiButtonState.Plus,
                        //        currentWiiButtonState.B,
                        //        currentWiiButtonState.Home,
                        //        currentWiiButtonState.Up,
                        //        currentNunchuk.C,
                        //        currentNunchuk.Z
                        //    },
                        //    new bool[]{
                        //        lastWiiButtonState.A,
                        //        lastWiiButtonState.Minus,
                        //        lastWiiButtonState.Plus,
                        //        lastWiiButtonState.B,
                        //        lastWiiButtonState.Home,
                        //        lastWiiButtonState.Up,
                        //        lastNunchuk.C,
                        //        lastNunchuk.Z
                        //    });
#endregion
                        break;
                    case ExtensionType.ClassicController:
                    //case ExtensionType.ClassicControllerPro://1.8でも使えない

                        Vector2 joyStick = new Vector2(currentClassicState.JoystickL.X, currentClassicState.JoystickL.Y);
                        joyStick = Vector2.Normalize(joyStick);
                        //if (Camera.Position.Z < 0)
                        //    joyStick = -joyStick;
                        res.SetButtonStates(joyStick,
                            new bool[]{
                            currentClassicState.ButtonState.B,currentClassicState.ButtonState.A,currentClassicState.ButtonState.X,currentClassicState.ButtonState.Y,
                            currentClassicState.ButtonState.Plus,currentClassicState.ButtonState.Minus,currentClassicState.ButtonState.TriggerL,currentClassicState.ButtonState.TriggerR
                        },
                            new bool[]{
                            lastClassicState.ButtonState.B,lastClassicState.ButtonState.A,lastClassicState.ButtonState.X,lastClassicState.ButtonState.Y,
                            lastClassicState.ButtonState.Plus,lastClassicState.ButtonState.Minus,lastClassicState.ButtonState.TriggerL,lastClassicState.ButtonState.TriggerR
                        }
                        );
                        break;
                //}
            }
            return res;
        }
        public override void Update()
        {
            //リモコンのボタンの状態を更新
            lastWiiButtonState = currentWiiButtonState;
            currentWiiButtonState = wiimote.Wiimote.WiimoteState.ButtonState;
            //クラシックコントローラのボタンの状態を更新
            lastClassicState = currentClassicState;
            currentClassicState = wiimote.Wiimote.WiimoteState.ClassicControllerState;
            //ヌンチャクの状態を更新
            lastNunchuk = currentNunchuk;
            currentNunchuk = wiimote.Wiimote.WiimoteState.NunchukState;
            GameMain.debugStr["wiiExtension"] = wiimote.Wiimote.WiimoteState.ExtensionType.ToString();
        }
    }
}
