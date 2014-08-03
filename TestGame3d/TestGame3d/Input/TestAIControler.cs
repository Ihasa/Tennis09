using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Tennis01.Objects;
namespace Tennis01.Input
{
    class TestAIControler:Controller
    {
        //int timer = 0;
        Player player;
        Player enemyPlayer;
        Ball ball;
        Vector3 ballVec { get { return Vector3.Normalize(ball.Speed); } }
        int responce;
        int resTimer;
        int tossWait = 90;
        int tossTimer = 0;
        float depth=0.75f;
        Vector2 center;
        public TestAIControler(Player p,Player p2,Ball b,float centerX,float netZ,int res)
        {
            player = p;
            enemyPlayer = p2;
            ball = b;
            responce = res;
            resTimer = 0;
            center = new Vector2(centerX, netZ);
        }
        public override void Update()
        {
            Vector3 v = ball.Position;
            if (player.IsServing && Scenes.Scene.IsEnableControllers)
            {
                tossTimer++;
            }
            resTimer++;
            //if (ballVec.Z > 0)
                //ballVec.Z = -ballVec.Z;
        }
        protected override ControllerState getState()
        {
            ControllerState res = new ControllerState();
            //res.R = ControlerButtonStates.Pressed;
            //移動に関すること
            //ショットに関すること

            float aboutCenter = player.Radius/4;
            res.JoyStick = Vector2.Zero;
            //res.L = ControlerButtonStates.Down;
            if ((player.Position.Z-center.Y)*ball.Speed.Z <= 0)//ショット直後
            {
                resTimer = 0;
                if (player.ShotsInPoint != 0)
                {
                    if (player.Position.X + player.Speed.X > center.X + aboutCenter / 2)
                    {
                        res.JoyStick.X = -1;
                    }
                    else if (player.Position.X + player.Speed.X < center.X - aboutCenter / 2)
                    {
                        res.JoyStick.X = 1;
                    }

                    float centerZ = TennisCourt.CourtLength + center.Y;
                    if (player.Position.Z < 0)
                    {
                        centerZ = -centerZ;
                    }
                    if (player.Position.Z + player.Speed.Z > centerZ + aboutCenter / 2)
                    {
                        res.JoyStick.Y = 1;
                    }
                    else if (player.Position.Z + player.Speed.Z < centerZ - aboutCenter / 2)
                    {
                        res.JoyStick.Y = -1;
                    }
                }
                else
                {
                    
                }
                depth = (float)GameMain.Random.NextDouble() * 0.6f + 0.4f;
            }
            else if(resTimer >= responce || (player.ShotsInPoint == 0))//ショットを打ちに行く
            {
                Vector3 smashVec = (player.SmashPoint.EmitPoint - player.Position);
                if (player.HasSmashPoint && smashVec.Length() / player.Ability.MaxSpeed - player.Ball.GetFrames(player.SmashableHeight) < 30)
                {
                    if (smashVec.Length() < player.HitBounds.Z * 0.25f)
                        res.JoyStick = Vector2.Zero;
                    else 
                        res.JoyStick = Vector2.Normalize(new Vector2(smashVec.X, -smashVec.Z));
                }
                else
                {
                    if (!player.Hit(ball) && ball.Bounds == 1&& (player.DistanceToBall.Z < 1 || ball.Speed.Y / ball.BoundVelocity.Y < -0.5f))
                    {
                        res.JoyStick.X = 1;
                        res.Button3 = ControlerButtonStates.Pressed;
                    }
                    else
                    {
                        float distance = MathHelper.Clamp(Math.Abs(ball.Speed.Length()) * 20, 0, 0.27f * 4);
                        Vector3 target;
                        Vector3 vec;
                        if (ball.Bounds == 0)
                            target = (ball.BoundPoint + ballVec * distance);
                        else
                            target = (ball.Position + ballVec * distance);
                        target.X += player.Position.X < (ball.BoundPoint+ballVec * distance).X ? -player.HitBounds.X*depth*0.5f : player.HitBounds.X * depth*0.5f;
                        target.Y = 0;
                        vec = (target - player.Position);
                        res.JoyStick = Vector2.Normalize(new Vector2(vec.X, -vec.Z));
                        Vector3 vec2 = Vector3.One * player.Radius;

                        //ショット可能位置まできたら止まる
                        if (new HitVolume(player.Position, player.HitBounds.X/16, player.HitBounds.Y, player.HitBounds.Z/4).Hit(new HitVolume(target, ball.Radius)))
                            res.JoyStick = Vector2.Zero;
                        //if (new BoundingBox(player.Position + Vector3.Up * player.Radius - vec2, player.Position + Vector3.Up * player.Radius + vec2).Intersects(new BoundingSphere(ball.BoundPoint + ballVec * distance, ball.Radius)))
                        //{
                        //    res.JoyStick = Vector2.Zero;
                        //}
                    }
                }
            }
            //ショットを打つ
            if(ball.Bounds < 2){
                if ((player.Position.Z - center.Y) * ball.Speed.Z >= 0)
                {
                    int val = GameMain.Random.Next(0, 4);// timer++ % 4;
                    Vector2 dis = new Vector2((ball.Position-player.Position).X,(ball.Position - player.Position).Z);
                    Vector2 spd = new Vector2(ball.Speed.X,ball.Speed.Z);
                    if (player.OnSmashPoint() && player.Ball.GetFrames(player.SmashableHeight) < 35)//スマッシュ
                    {
                        res.Button1 = ControlerButtonStates.Pressed;
                    }
                    else if (dis.Length() < spd.Length() * Player.Delay || player.Hit(ball))//その他
                    {
                        if (player.IsServing)//サーブ
                        {
                            //tossTimer++;
                            GameMain.debugStr["tossTimer"] = "" + tossTimer;
                            if (tossTimer == tossWait)//トスをする
                            {
                                //System.Windows.Forms.MessageBox.Show("toss");
                                res.Button1 = ControlerButtonStates.Pressed;
                            }
                            else
                            {
                                if (ball.Position.Y >= player.HitBounds.Y*0.8f && ball.Speed.Y < 0)//サアブを打つ
                                {
                                    switch (val)
                                    {
                                        case 0:
                                            res.Button1 = ControlerButtonStates.Pressed;
                                            break;
                                        case 1:
                                            res.Button1 = ControlerButtonStates.Pressed;
                                            break;
                                        case 2:
                                            res.Button2 = ControlerButtonStates.Pressed;
                                            break;
                                        case 3:
                                            res.Button4 = ControlerButtonStates.Pressed;
                                            break;
                                    }
                                    //方向設定
                                    res.JoyStick.X = GameMain.Random.Next(0, 2) == 0 ? -1 : 1;
                                    tossTimer = 0;
                                }
                            }
                        }
                        else if (!(player.ShotsInPoint == 0 && ball.Bounds == 0))
                        {
                            res.JoyStick = Vector2.Zero;
                            if (player.Hit(ball))
                            {
                                if (enemyPlayer.Position.X < center.X)
                                {
                                    res.JoyStick.X = 1;
                                }
                                else
                                {
                                    res.JoyStick.X = -1;
                                }
                                if (enemyPlayer.Position.X * enemyPlayer.Speed.X < 0 && GameMain.Random.Next(0, 10) == 0)
                                {
                                    res.JoyStick = -res.JoyStick;
                                }
                            }
                            else
                            {
                                //res.JoyStick = new Vector2((ball.Position - player.Position).X, (ball.Position - player.Position).Z);
                            }

                            if (Math.Abs(enemyPlayer.Position.Z) < TennisCourt.ServiceAreaLength/2)//前にいたらロブ
                            {
                                if (enemyPlayer.Speed.Z * enemyPlayer.Position.Z < 0)
                                {
                                    res.Button1 = ControlerButtonStates.Pressed;
                                    res.R = ControlerButtonStates.Pressed;
                                }
                                else
                                {
                                    res.Button1 = ControlerButtonStates.Pressed;                                    
                                }
                            }
                            else if (ball.Bounds == 0)//ボレーで返す
                            {
                                if(TennisCourt.IsInCourt(ball.BoundPoint))
                                    res.Button1 = ControlerButtonStates.Pressed;
                            }
                            else//ストロークで返す
                            {
                                if (Math.Abs(enemyPlayer.Position.Z) >= TennisCourt.CourtLength * 1.25f)
                                {
                                    res.Button4 = res.R = ControlerButtonStates.Pressed;
                                }
                                else
                                {
                                    int rand = GameMain.Random.Next(0, 3);
                                    if (rand == 0)
                                        res.Button1 = ControlerButtonStates.Pressed;
                                    else if (rand == 1)
                                        res.Button4 = ControlerButtonStates.Pressed;
                                    else
                                        res.Button2 = ControlerButtonStates.Pressed;
                                }
                            }
                        }

                    }
                }
            }

            if (player.Camera.Position.Z < 0)
                res.JoyStick = -res.JoyStick;
            return res;    
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
