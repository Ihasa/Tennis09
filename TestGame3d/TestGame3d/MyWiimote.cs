using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WiimoteLib;
using Microsoft.Xna.Framework;
namespace Tennis01
{
    class MyWiimote:IDisposable
    {
        public Wiimote Wiimote;
        List<WiimoteState> states = new List<WiimoteState>();
        public ButtonState CurrentButtonState { get; private set; }
        public ButtonState LastButtonState { get; private set; }
        public Vector3 AccelValue { get; private set; }
        public Vector3 MotionPlusValue { get; private set; }
        public Vector2 IRPosition { get; private set; }
        public bool Connected { get; private set; }
        Object lockObject = new Object();
        public MyWiimote(Game game)
        {
            Wiimote = new Wiimote();
            Connect();
            CurrentButtonState = new ButtonState();
            LastButtonState = new ButtonState();
            AccelValue = new Vector3();
            MotionPlusValue = new Vector3();
        }
        public void Connect()
        {
            if (!Connected)
            {
                try
                {
                    Wiimote.Connect();
                    Wiimote.InitializeMotionPlus();
                    Wiimote.SetReportType(InputReport.IRExtensionAccel, true);
                    Wiimote.SetLEDs(0);
                    Wiimote.WiimoteChanged += (sender, args) =>
                    {
                        if (Connected)
                        {
                            lock (lockObject)
                            {
                                states.Add(args.WiimoteState);
                            }
                        }
                    };
                    Connected = true;
                    if(GameMain.debug)
                        System.Windows.Forms.MessageBox.Show("Wiiリモコンとの接続成功");
                }
                catch
                {
                    if(GameMain.debug)
                        System.Windows.Forms.MessageBox.Show("Wiiリモコンとの接続失敗");
                    Connected = false;
                }
            }
        }
        public void Update()
        {
            if (Connected)
            {
                lock (lockObject)
                {
                    if (states.Count > 0)
                    {
                        AccelValue = new Vector3(
                            states.Average((wmState) => { return wmState.AccelState.Values.X; }),
                            states.Average((wmState) => { return wmState.AccelState.Values.Y; }),
                            states.Average((wmState) => { return wmState.AccelState.Values.Z; })
                        );
                        MotionPlusValue = new Vector3(
                            states.Average((wmState) => { return wmState.MotionPlusState.Values.X; }),
                            states.Average((wmState) => { return wmState.MotionPlusState.Values.Y; }),
                            states.Average((wmState) => { return wmState.MotionPlusState.Values.Z; })
                        );
                        IRPosition = new Vector2(
                            states.Average((wmState) => { return wmState.IRState.Midpoint.X; }),
                            states.Average((wmState) => { return wmState.IRState.Midpoint.Y; })
                        );

                        IRPosition = new Vector2(states[states.Count - 1].IRState.Midpoint.X, states[states.Count - 1].IRState.Midpoint.Y);
                        LastButtonState = CurrentButtonState;
                        CurrentButtonState = states[states.Count - 1].ButtonState;
                        GameMain.debugStr["states.Count"] = "" + states.Count;
                        states.Clear();
                    }
                }
            }
        }
        public bool found(WiimoteState state, int level)
        {
            int founds = 0;
            for (int i = 0; i < 4; i++)
            {
                if (state.IRState.IRSensors[i].Found)
                    founds++;
            }
            if (founds >= level)
                return true;
            return false;
        }
        public void Dispose()
        {
            if(Connected)
                Wiimote.Disconnect();
        }
    }
}
