using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Kinect;
namespace Tennis01.Input
{
    /// <summary>
    /// 体の動きで左右移動
    /// </summary>
    class TestKinectControler4:KinectControlerBase
    {
        JointCollection firstFrameJoints;
        float threshold = 0.1f;

        int delayTimer = 0;
        int delay = 40;
        bool takeBacked = false;
        public TestKinectControler4(int kinectNum)
            : base(kinectNum)
        {

        }
        public override void Update()
        {
            //一番最初のJoint情報を保管しておく
            if (ActiveJointStates == 1)
                firstFrameJoints = JointStates[0];

            if (ActiveJointStates > 0 && !takeBacking(0))
            {
                if (delayTimer++ == delay)
                {
                    delayTimer = 0;
                    takeBacked = false;
                }
            }
            base.Update();
        }
        protected override ControllerState getState()
        {
            ControllerState res = new ControllerState();
            if (ActiveJointStates > 0)
            {
                //移動
                float x = JointStates[0][JointType.ShoulderCenter].Position.X - firstFrameJoints[JointType.ShoulderCenter].Position.X;
                //float z = JointStates[0][JointType.ShoulderCenter].Position.Z - firstFrameJoints[JointType.ShoulderCenter].Position.Z;
                GameMain.debugStr["shoulderX"] = "" + x;
                //Game1.debugStr["shoulderZ"] = "" + z;
                //左右
                if (x > threshold)
                {
                    res.JoyStick.X = 1;
                }
                else if (x < -threshold)
                {
                    res.JoyStick.X = -1;
                }

                //ショット
                if (ActiveJointStates > 0)
                {
                    GameMain.debugStr["RightHand.X - RightElbow.X"] = "" + (JointStates[0][JointType.HandRight].Position.X - JointStates[0][JointType.ElbowRight].Position.X);
                    if (!takeBacked)
                    {
                        if (takeBacking(0))
                        {
                            takeBacked = true;
                            GameMain.debugStr["TakeBacked"] = "true";
                        }
                        else
                        {
                            GameMain.debugStr["TakeBacked"] = "false";
                        }
                    }
                    else
                    {
                        GameMain.debugStr["Swing"] = "Not Swinging";
                        //フォロースルーの姿勢によって球種を変える
                        if (robPose(0))
                        {
                            takeBacked = false;
                            res.Button3 = ControlerButtonStates.Pressed;
                            GameMain.debugStr["Swing"] = "RobShot!";
                        }
                        if (topSpinPose(0))
                        {
                            takeBacked = false;
                            res.Button1 = ControlerButtonStates.Pressed;
                            GameMain.debugStr["Swing"] = "TopSpin!";

                        }
                        else if (sliceSpinPose(0))
                        {
                            takeBacked = false;
                            res.Button4 = ControlerButtonStates.Pressed;
                            GameMain.debugStr["Swing"] = "SliceSpin!";
                        }
                    }
                }

            }
            return res;
        }
        public void ResetFirstJoints()
        {
            firstFrameJoints = JointStates[0];
        }

        bool takeBacking(int frame)//普通のテイクバック姿勢
        {
            if (JointStates[frame][JointType.HandRight].Position.X - JointStates[frame][JointType.ElbowRight].Position.X > 0.2f ||
                JointStates[frame][JointType.HandRight].Position.Z > JointStates[frame][JointType.Spine].Position.Z)
                return true;
            return false;
        }
        bool topSpinPose(int frame)//トップスピン...胸より手が上にある
        {
            if (JointStates[frame][JointType.HandRight].Position.X - JointStates[frame][JointType.ElbowRight].Position.X < -0.1f &&
                JointStates[frame][JointType.HandRight].Position.Y >= JointStates[frame][JointType.Spine].Position.Y)
                return true;
            return false;
        }
        bool sliceSpinPose(int frame)//スライススピン...胸より手が下にある
        {
            if (JointStates[frame][JointType.HandRight].Position.X - JointStates[frame][JointType.ElbowRight].Position.X < -0.1f &&
                JointStates[frame][JointType.HandRight].Position.Y < JointStates[frame][JointType.Spine].Position.Y)
                return true;
            return false;
        }
        bool robPose(int frame)//ロブ...引いた時の手のY座標をどこかで保持しておくといいかもしれない。
        {
            return false;
        }
    }
}
