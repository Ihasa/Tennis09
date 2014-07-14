using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Kinect;

namespace Tennis01.Input
{
    using Objects;

    /// <summary>
    /// 腕をジョイスティックに見立てる
    /// </summary>
    class TestKinectControler3:KinectControlerBase
    {
        Player player;
        Ball ball;
        public TestKinectControler3(int kinectNum,Player myPlayer,Ball ball)
            : base(kinectNum)
        {
            player = myPlayer;
            this.ball = ball;
        }
        public override void Update()
        {
            base.Update();
        }
        protected override ControllerState getState()
        {
            float val = 0.1f;
            ControllerState res = new ControllerState();

            if (ActiveJointStates > 0)
            {
                float x = JointStates[0][JointType.HandRight].Position.X - JointStates[0][JointType.ElbowRight].Position.X;
                float z = JointStates[0][JointType.HandRight].Position.Z - JointStates[0][JointType.ElbowRight].Position.Z;
                //左右移動
                if (x > val)//手が肘より右
                {
                    res.JoyStick.X = x;
                }
                else if (x < -val)//手が肘より左
                {
                    res.JoyStick.X = x;
                }

                if (z > val)//手が肘より後ろ
                {
                    res.JoyStick.Y = z;
                }
                else if(z < -val)
                {
                    res.JoyStick.Y = z;
                }
                res.JoyStick = Vector2.Normalize(res.JoyStick);
            }

            if (player.Hit(ball))
            {
                res.Button1 = ControlerButtonStates.Pressed;
            }



            return res;

        }
    }
}
