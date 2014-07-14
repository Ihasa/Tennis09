using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Kinect;
using Tennis01.Objects;
namespace Tennis01.Input
{
    /// <summary>
    /// 右手と右ひじの位置でショットを打ち分ける。移動は自動。
    /// </summary>
    class TestKinectControler2:KinectControlerBase
    {  
        //移動に必要なパラメータ
        Player player;
        Player enemyPlayer;
        Ball ball;
        Vector3 ballVec;

        int delayTimer = 0;
        int delay = 30;

        bool takeBacked;
        public TestKinectControler2(int kinectNum,Player myPlayer,Player enemy,Ball b):base(kinectNum)
        {
            player = myPlayer;
            enemyPlayer = enemy;
            ball = b;

            takeBacked = false;
        }
        public override void Update()
        {
            Vector3 v = ball.Position;
            ballVec = Vector3.Normalize(ball.Speed);

            
            if (ActiveJointStates > 0&&!takeBacking(0))
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

            float aboutCenter = player.Radius;
            res.JoyStick = Vector2.Zero;
            if (player.Position.Z*ball.Speed.Z <= 0)//球を打った直後
            {
                if (player.Position.X+player.Speed.X > aboutCenter/2)
                {
                    res.JoyStick.X = -1;
                }
                else if (player.Position.X+player.Speed.X < -aboutCenter/2)
                {
                    res.JoyStick.X = 1;
                }

                float centerZ = 3.5f;
                if (player.Position.Z < 0)
                {
                    centerZ = -centerZ;
                }
                if (player.Position.Z + player.Speed.Z > centerZ+aboutCenter/2)
                {
                    res.JoyStick.Y = 1;
                }
                else if (player.Position.Z + player.Speed.Z < centerZ-aboutCenter / 2)
                {
                    res.JoyStick.Y = -1;
                }

            }
            else//相手が球を打ってきた直後
            {
                MoveToBall(ref res);
            }

            //Kinectからデータを取って、ショットボタンをいじる
            //Button1...トップスピン...
            //Button2...フラット.......
            //Button3...ロブ...........
            //Button4...スライス.......

            //テイクバックを検出
            if (ActiveJointStates > 20)
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
            
            //if(player.Hit(ball))
            //    res.Button1 = ControlerButtonStates.Pressed;
            return res;    
        }
        bool takeBacking(int frame)//普通のテイクバック姿勢
        {
            if (JointStates[frame][JointType.HandRight].Position.X - JointStates[frame][JointType.ElbowRight].Position.X > 0.2f ||
                JointStates[frame][JointType.ElbowRight].Position.Z > JointStates[frame][JointType.Spine].Position.Z)
                return true;
            return false;
        }
        bool topSpinPose(int frame)//トップスピン...胸より手が上にある
        {
            if (JointStates[frame][JointType.HandRight].Position.X - JointStates[frame][JointType.ElbowRight].Position.X < -0.1f&&
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
        private void MoveToBall(ref ControllerState res)
        {
            float distance = 0.27f*3;
            Vector3 vec;
            if (ball.Bounds == 0)
                vec = (ball.BoundPoint + ballVec * distance) - player.Position;
            else
                vec = (ball.BoundedPoint + ballVec * distance) - player.Position;
            res.JoyStick = new Vector2(vec.X, -vec.Z);
            Vector3 vec2 = Vector3.One * player.Radius;
            if (new BoundingBox(player.Position + Vector3.Up * player.Radius - vec2, player.Position + Vector3.Up * player.Radius + vec2).Intersects(new BoundingSphere(ball.BoundPoint + ballVec * distance, ball.Radius)))
            {
                res.JoyStick = Vector2.Zero;
            }
        }
        private ControllerState MoveToBall1(ControllerState state)
        {
            float d = ball.Position.X + (player.Position.Z - ball.Position.Z) * (ballVec.X / ballVec.Z) - player.Position.X;
            state.JoyStick = Vector2.Zero;
            if (Math.Abs(d) > player.Radius)
            {
                if (d > 0)
                    state.JoyStick.X = 1;
                else state.JoyStick.X = -1;
            }
            return state;
        }

    }
}
