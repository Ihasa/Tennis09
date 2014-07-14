using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Tennis01.Input;
namespace Tennis01.Objects
{
    class TestObject:HittableObject3D,IControleable
    {

        #region TennisPlayerクラスで実装
        //現在のスピード(0～1)
        float v;
        PlayerAbility ability;
        //打つべきボール
        public Ball ball{private get;set;}
        //このオブジェクトのコントローラ
        GamePadState controler;
        #endregion


        #region Humanクラスで実装
        //首をボールに向かせるテスト
        float neckRadY, neckRadX;
        //見つめるもの
        public Object3D targetObject { private get; set; }
        //向いている方向
        protected Vector2 direction;
        //背の高さ
        float height;
        //持ち物
        public HittableObject3D belonging { get; protected set; }
        #endregion


        public TestObject(Model m,Scenes.Camera c, Vector3? pos=null, Vector3? spd=null, Vector3? rot=null)
            : base(m, c,null,pos, spd, rot,null,"ObjectSEs")
        {
            #region Humanクラスで実装
            //通常状態のアニメーションを開始
            StartAnimation("Animation_1");
            //direction = new Vector2(0, 1);
            direction = new Vector2(0,1);

            neckRadY = 0;
            neckRadX = 0;

            //持ち物のモデル名を指定する必要がある
           // belonging = new Object3D(Game1.models["racket"]);
            height = Radius * 2;
            if (belonging != null)
                Radius = Radius + belonging.Radius;
                //boundingSphere = new BoundingSphere(position, boundingSphere.Radius + belonging.boundingSphere.Radius);

            #endregion

            #region Playerクラスで実装
            ability = new PlayerAbility(0.03f, 0.03f, 0.02f, 60, -3f);
            //ability.Acceleration = 0.03f;
            //ability.Deceleration = 0.03f;
            //ability.MaxSpeed = 0.02f;
            //ability.RotAngle = 135;
            //ability.QuickNess = 0.1f;
            v = 0;
            #endregion
        }

        /// <summary>
        /// TennisPlayer
        /// </summary>
        /// <param name="controler"></param>
        void shotBall(GamePadState controler)
        {
            #region テスト用
            if (ball != null)
            {
                if (controler.IsButtonDown(Buttons.A))
                {
                    ball.ShotByDistance(new Vector2(0, 0), 5);
                }
                if (controler.IsButtonDown(Buttons.B))
                {
                    ball.ShotByDistance(new Vector2(0, 30), 5);
                }
                if (controler.IsButtonDown(Buttons.X))
                {
                    ball.Init(new Vector3(0, 4, 4));
                }
                if (controler.IsButtonDown(Buttons.Y))
                {
                    ball.Toss(60);
                }
            }
            #endregion
        }

        public void SetPosture(Vector3 position, Vector2 direction)
        {
            this.position = position;
            this.direction = direction;
//            float deg = direction.ToDegrees();
//            this.direction = deg;
        }
        public int Index { get; set; }
        /// <summary>
        /// IControleable...TennisPlayer
        /// </summary>
        /// <param name="controler"></param>
        public void Control(ControlerState controler)
        {
            //if (controler.ThumbSticks.Left != Vector2.Zero)//移動の入力あり
            //{
            //    if (speed.Length() == 0)
            //    {
            //        StartAnimation("Animation_2");
            //        PlaySound("catJump");
            //    }
            //    Vector2 thumb = controler.ThumbSticks.Left;
            //    //カメラの都合でZ方向を逆にしています
            //    thumb.Y = -thumb.Y;

            //    //float thumbDeg = thumb.ToDegrees();// MathHelper.ToDegrees((float)Math.Acos(thumb.Y / thumb.Length()));
            //    //if (thumb.X < 0)
            //    //    thumbDeg = -thumbDeg;

            //    #region 一気には回転できない方式(ボツ)
            //    /*if (degree > thumbDeg)
            //    {
            //        if (degree - thumbDeg > 180)
            //        {
            //            degree += rotAngle;

            //            if (degree > thumbDeg)
            //                degree = thumbDeg;
            //        }
            //        else
            //        {
            //            degree -= rotAngle;
            //            if (degree < thumbDeg)
            //                degree = thumbDeg;
            //        }
            //    }
            //    else if (degree != thumbDeg)
            //    {
            //        if (thumbDeg - degree > 180)
            //        {
            //            degree -= rotAngle;
            //            if (degree < thumbDeg)
            //                degree = thumbDeg;
            //        }
            //        else
            //        {
            //            degree += rotAngle;
            //            if (degree > thumbDeg)
            //                degree = thumbDeg;
            //        }
            //    }*/
            //    #endregion

            //    //rotAngle以上の開きがあったら減速方式
            //    //if (Math.Abs(thumbDeg - direction) > ability.RotAngle / 2 && Math.Abs(thumbDeg - direction) < 360 - ability.RotAngle / 2)
            //    //    v *= ability.QuickNess;
            //    if (Vector2.Dot(Vector2.Normalize(thumb),direction) < (float)Math.Cos(MathHelper.ToRadians(ability.RotAngle)))
            //    {
            //        v *= ability.QuickNess;
            //    }

            //    //direction = thumbDeg;
            //    direction = Vector2.Normalize(thumb);

            //    v += ability.Acceleration;
            //    if (v > 1)
            //        v = 1;
            //}
            //else if (v != 0)//入力なし
            //{
            //    v -= ability.Deceleration;// new Vector3(deceleration * direction.X, 0, deceleration * direction.Y);
            //    if (v <= 0)
            //    {
            //        v = 0;
            //        StartAnimation("Animation_3");
            //        StopSound("catJump");
            //    }
            //}
            //shotBall(controler);
        }

        public override void Update(GameTime gameTime)
        {
            //コントローラによる操作...TennisPlayer
            //本当は外からやる?
            controler = Game1.gamePadStates[0];
            //Control(controler);
            #region 移動の実装法1(ボツ)
            //if (controler.ThumbSticks.Left != Vector2.Zero)//移動の入力あり
            //{
            //    if (speed == Vector3.Zero)
            //    {
            //        StartAnimation("Animation_2", new TimeSpan(0, 0, 0, 0, 70));
            //    }
            //    direction = Vector2.Normalize(controler.ThumbSticks.Left);
            //    direction.Y = -direction.Y;
            //    speed += new Vector3(acceleration * direction.X, 0, acceleration * direction.Y);
            //    if (speed.Length() > maxSpeed)
            //        speed = new Vector3(maxSpeed*direction.X,0,maxSpeed*direction.Y);
            //}
            //else if(speed != Vector3.Zero)//入力なし
            //{
            //    speed -= new Vector3(deceleration*direction.X,0,deceleration*direction.Y);
            //    if (speed.Length() < deceleration)
            //    {
            //        speed = Vector3.Zero;
            //        StartAnimation("Animation_3");
            //    }
            //}
            #endregion
            
            #region 移動の実装法２(ボツ)
            //if (controler.ThumbSticks.Left != Vector2.Zero)//移動の入力あり
            //{
            //    if (v == 0)
            //    {
            //        StartAnimation("Animation_2", new TimeSpan(0, 0, 0, 0,70));
            //    }
            //    Vector2 thumb = controler.ThumbSticks.Left;
            //    direction = Vector2.Normalize(thumb);//Vector2.Normalize(controler.ThumbSticks.Left);
            //    direction.Y = -direction.Y;
            //    v += acceleration;
            //    if (v > maxSpeed)
            //        v = maxSpeed;// new Vector3(maxSpeed * direction.X, 0, maxSpeed * direction.Y);

            //    speed = new Vector3(v * direction.X, 0, v * direction.Y);

            //}
            //else if (v != 0)//入力なし
            //{
            //    v -= deceleration;// new Vector3(deceleration * direction.X, 0, deceleration * direction.Y);
            //    if (speed.Length() < deceleration)
            //    {
            //        v = 0;
            //        StartAnimation("Animation_3");
            //    }
            //    speed = new Vector3(v * direction.X, 0, v * direction.Y);
            //}
            #endregion
            #region Humanクラスで実装
            //角度と速度から、x,y,z速度を決定
            float radian = direction.ToRadians();
            //float radian = MathHelper.ToRadians(direction);
            speed = new Vector3(MathHelper.Lerp(0, ability.MaxSpeed, v) * (float)(Math.Sin(radian)), 0, MathHelper.Lerp(0, ability.MaxSpeed, v) * (float)(Math.Cos(radian)));
            #region 向いている方向をVector2で表す場合(たぶんボツ)
            //float radian = (float)Math.Acos(direction.Y/direction.Length());
            //float radian = (float)MathHelper.ToRadians(degree);
            //if (direction.X < 0)
            //    radian = -radian;
            //speed = new Vector3((float)(v * Math.Sin(radian)), 0, (float)(v * Math.Cos(radian)));
            #endregion

            turn(radian);
            
            //首をターゲットの方向へ回す
            turnNeck(radian);
            #endregion
            base.Update(gameTime);            
        }

        private void turnNeck(float radian)
        {
            if (targetObject != null)
            {
                //自分から見てターゲットがある方向(Y軸回転)
                Vector2 vec = new Vector2(targetObject.Position.X - position.X, targetObject.Position.Z - position.Z);
                float rad;
                if (vec.Length() != 0)
                {
                    //ベクトルから角度を求める
                    //0~πでしか返ってこないので、x方向によっては補正
                    //rad = (float)Math.Acos(vec.X / vec.Length());
                    //if (vec.Y < 0)
                    //    rad = -rad;
                    rad = vec.ToRadians();
                    //首の角度=ボールの方向-体の向いている方向
                    neckRadY = rad - radian;
                }
                //π/2以下だったら、X軸でも回転
                if (Math.Abs(neckRadY) < Math.PI / 2)
                {
                    //自分から見てボールがある方向(X軸回転)
                    Vector3 vec3 = targetObject.Position - position;
                    vec3.Y -= height;
                    if (vec3.Length() != 0)
                    {
                        rad = (float)Math.Asin(vec3.Y / vec3.Length());

                        neckRadX = rad;
                    }
                }
                else
                {
                    neckRadX = neckRadY = 0;
                }

                //

                /*if (vec.Length() != 0)
                {
                    //首の角度 = vecとdirectionがなす角
                    neckRad = (float)Math.Acos((vec.X * direction.X + vec.Y * direction.Y) / (vec.Length() * direction.Length()));           
                }*/
            }
        }

        /// <summary>
        /// モデルを自然に進行方向へ回転させる
        /// </summary>
        /// <param name="radian"></param>
        /// <returns></returns>
        private void turn(float radian)
        {
            //角度を-π~πに直す
            radian = MathHelper.WrapAngle(radian);
            rotation.Y = MathHelper.WrapAngle(rotation.Y);
            //一定角度ずつ回転

            if (rotation.Y > radian)
            {
                if (rotation.Y - radian > Math.PI)
                {
                    rotation.Y += MathHelper.ToRadians(10);
                }
                else
                {
                    rotation.Y -= MathHelper.ToRadians(10);
                }
            }
            else if (rotation.Y != radian)
            {
                if (radian - rotation.Y > Math.PI)
                {
                    rotation.Y -= MathHelper.ToRadians(10);
                }
                else
                {
                    rotation.Y += MathHelper.ToRadians(10);
                }
            }
            if (Math.Abs(rotation.Y - radian) <= MathHelper.ToRadians(10))
                rotation.Y = radian;
        }


        
        /// <summary>
        /// このオブジェクトの速度を設定し、速度方向に体を向けさせる。
        /// </summary>
        /// <param name="spd">新しく設定したい速度</param>
        public void SetSpeed(Vector3 spd)
        {

        }
 

        /// <summary>
        /// override...Human
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="view"></param>
        /// <param name="projection"></param>
        protected override void DrawModel(GameTime gameTime, Matrix view, Matrix projection)
        {
            TransformBones(3, Matrix.CreateRotationX(-neckRadX) * Matrix.CreateRotationY(neckRadY));
            base.DrawModel(gameTime, view, projection);
            if (belonging != null)
            {
                Matrix[] m = new Matrix[model.Bones.Count];
                model.CopyAbsoluteBoneTransformsTo(m);
                belonging.AnotherTransform = animationPlayer.GetWorldTransforms()[12] * Matrix.CreateRotationY(rotation.Y) * Matrix.CreateTranslation(position);
                //belonging.DrawModel(gameTime, view, projection);
            }
        }
    }
}
