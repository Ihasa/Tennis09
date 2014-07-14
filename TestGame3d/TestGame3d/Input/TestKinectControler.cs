using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Kinect;
using Microsoft.Xna.Framework;
namespace Tennis01.Input
{
    /// <summary>
    /// 右ひじと右手で移動、ショットを行う
    /// </summary>
    class TestKinectControler:KinectControlerBase
    {
        public TestKinectControler(int kinectNum)
            : base(kinectNum)
        {

        }
        public override void Update()
        {
            base.Update();
            //if (ActiveJointStates > 1)
                //Game1.debugStr = "" + (JointStates[0][JointType.HandRight].Position.Z - JointStates[0][JointType.ElbowRight].Position.Z);            
            //    Game1.debugStr = "" + (JointStates[0][JointType.HandRight].Position.X - JointStates[0][JointType.ElbowRight].Position.X);
        }
        protected override ControllerState getState()
        {
            float val = 0.1f;
            ControllerState res = new ControllerState();

            //左右移動
            if (ActiveJointStates > 0)
            {
                if (JointStates[0][JointType.HandRight].Position.X - JointStates[0][JointType.ElbowRight].Position.X > val)//手が肘より右
                {
                    res.JoyStick.X = 1;
                }
                else if (JointStates[0][JointType.HandRight].Position.X - JointStates[0][JointType.ElbowRight].Position.X < -val)//手が肘より左
                {
                    res.JoyStick.X = -1;
                }
            }

            //ショット
            if (ActiveJointStates > 10)
            {
                
                if (JointStates[10][JointType.HandRight].Position.Z - JointStates[0][JointType.HandRight].Position.Z > val)
                {
                    res.Button1 = ControlerButtonStates.Pressed;
                }
            }

            return res;
        }
    }
}
