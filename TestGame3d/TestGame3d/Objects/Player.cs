using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Tennis01.Input;
using Extention;
using VisualEffects;
namespace Tennis01.Objects
{
    using Scenes;
    using PlayerStates;

    enum Shots
    {
        TOP,
        SLICE,
        LOB,
        DROP,
        VOLLEY,
        SMASH,
        SERVE
    }
    class Player:Human,IControleable
    {
        #region フィールド
        /// <summary>
        /// 現在のスピード
        /// </summary>
        float v;
        /// <summary>
        /// 現在どんな行動をしているか
        /// </summary>
        PlayerState playerState;
        /// <summary>
        /// この選手の能力
        /// </summary>
        PlayerAbility ability;
        /// <summary>
        /// 打つべきボール
        /// </summary>
        Ball ball;
        /// <summary>
        /// いろいろ使う乱数
        /// </summary>
        Random rand;
        /// <summary>
        /// 持ってるラケットの長さ
        /// </summary>
        float racketLength;
        //あたり判定の大きさ
        Vector3 hitBounds;
        /// <summary>
        /// 煙類
        /// </summary>
        Particle sliding;
        Particle shotMark;
        Particle smashPoint;
        Particle smashPoint2;
        Particle shotPoint;
        Particle swing;
        //ショット用パラメータ
        const float angleY = 20;//打球角度
        static float spinSpeed = 80;//通常の打球速度
        public static float Rate = 1.0f;
        public static int Delay = 7;
        const float sliceSpeed = 65;//スライスの速度
        const float dropDepth = 0.3f;//ドロップの精度(基準)
        const float dropFrames = 50; //ドロップは最短何フレームで到着するか
        const float flatServeVmax = 170;//フラットサーブの最高速度(基準
        const float spinServeVmax = 130;//スピンサーブの
        const float sliceServeVmax = 110;//スライスサーブの
        #endregion
        #region イベント
        public event Action<Shots> ShottingBall;
        void onShotting(Shots shots)
        {
            if(ShottingBall != null)
                ShottingBall(shots);
        }
        #endregion
        #region コンストラクタ
        public Player(Model model,Camera c, Vector3 position,PlayerAbility ability, Vector2 direction,string soundBankName,string racketName,Ball ball,int controlerIndex)
        :base(model,c,position,Vector2.Zero,direction,null,null,soundBankName,racketName)
        {
            this.ability = ability;
            this.ball = ball;
            this.Index = controlerIndex;

            playerState = new Standing(this);
            StartAnimation(playerState.AnimationName, playerState.AnimationStartTime, playerState.AnimationStartTime2);
            v = 0;
            rand = new Random();
            //衝突判定(ショット判定)初期化。キャラのジョイント情報から初期化
            Matrix[] bones = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(bones);
            float shoulderX = bones[32].Translation.X;
            float shoulderY = bones[32].Translation.Y;
            float armLength = bones[34].Translation.X;

            racketLength = GameMain.Models[racketName].Meshes[0].BoundingSphere.Radius * 2;

            hitBounds.X = (armLength + racketLength) * 2.0f;
            hitBounds.Y = (shoulderY + armLength + racketLength);
            hitBounds.Z = racketLength * 5;

            GameMain.debugStr["playerHit"] = "" + new Vector3(hitBounds.X/0.27f, hitBounds.Y/0.27f, hitBounds.Z/0.27f) + "m";
            HitVolume = new HitVolume(position,hitBounds.X,hitBounds.Y,hitBounds.Z);
            sliding = Particle.CreateFromParFile("ParFiles/sand2.par");
            sliding.Scale = 0.27f * 0.6f;
            sliding.Script = (p) =>
            {
                float minY = 0.0001f;
                if (p.Speed.Y < 0 && p.Position.Y <= minY)
                {
                    p.Position = new Vector3(p.Position.X, minY, p.Position.Z);
                    p.Speed = Vector3.Zero;
                    p.Normal = Vector3.Normalize(new Vector3(0, 1, 0.0000001f));
                }
            };

            shotMark = Particle.CreateFromParFile("ParFiles/shotMark.par");
            shotMark.Normal = Vector3.Normalize(new Vector3(0, 1, 0.00001f));
            //shotMark.LifeTime = 3;
            shotMark.Scale = hitBounds.Z;

            shotPoint = Particle.CreateFromParFile("ParFiles/shotPoint.par");
            shotPoint.Scale = 0.27f * 0.5f;
            shotPoint.Normal = new Vector3(0, 1, 0.0000001f);
            //shotPoint.FinalColor = shotPoint.InitialColor = new Vector3(0,0,255);
            

            smashPoint = Particle.CreateFromParFile("ParFiles/shadow.par");
            smashPoint.Scale = ModelRadius*2;
            smashPoint.InitialTexture = GameMain.Textures["circle2"];
            smashPoint.FinalColor = new Vector3(255, 255, 0);
            smashPoint.LifeTime = 1;
            smashPoint2 = new Particle(smashPoint.Parameters);
            smashPoint2.InitialTexture = GameMain.Textures["circle"];
            smashPoint2.FinalColor = new Vector3(255, 255, 0);
            //smashPoint2.LifeTime = 3;
            smashPoint.Normal = smashPoint2.Normal = Vector3.Normalize(new Vector3(0, 1, 0.00001f));

            swing = Particle.CreateFromParFile("ParFiles/swing.par");
            swing.Scale = racketLength/2;
        }
        #endregion

        //ボールを打つテスト
        //float spin = 30;
        #region プロパティ
        protected override Vector3 Center
        {
            get
            {
                return position + Vector3.Up * hitBounds.Y / 2;
            }
        }
        /// <summary>
        /// この選手の能力
        /// </summary>
        public PlayerAbility Ability { get { return ability; } }
        /// <summary>
        /// 走っている速さ
        /// </summary>
        public float Velocity { get { return v; } set { v = value; } }
        public Vector2 BodyDirection { get { return bodyDirection; } set { bodyDirection = value; } }
        public Vector3 Rotation { get { return rotation; } set { rotation = value; } }
        public float RotationY { get { return Rotation.Y; } set { rotation.Y = value; } }
        public float RotationX { set { rotation.X = value; } }
        public Vector3 DistanceToBall
        {
            get
            {
                Vector3 res = new Vector3();
                res.X = (ball.Position.X - position.X) / (hitBounds.X/2);
                res.Z = (position.Z - ball.Position.Z) / (hitBounds.Z / 2);
                if (Position.Z < 0)
                {
                    res = -res;
                }
                res.Y = ball.Position.Y / hitBounds.Y;
                return res;
            }
        }

        public Matrix[] Bones
        {
            get
            {
                Matrix[] bones = new Matrix[Model.Bones.Count];
                Model.CopyAbsoluteBoneTransformsTo(bones);
                return bones;
            }
        }
        public Ball Ball { get { return ball; } set { ball = value; } }
        /// <summary>
        /// サーブ中かどうか
        /// </summary>
        public bool IsServing { get { return playerState is ServeStateBase; } }
        /// <summary>
        /// 何バウンドでショットをしたか
        /// </summary>
        public int Bounds { get; private set; }
        //使うことになるジョイントの位置など
        public Vector3 HeadPosition { get { return Bones[7].Translation + position; } }
        public float ShoulderY { get { return Bones[32].Translation.Y; } }
        public float ShoulderWidth { get { return Bones[32].Translation.X * 2; } }
        public Matrix HandTransformR { get { return animationPlayer.GetWorldTransforms()[12]; } }
        public Matrix HandTransformL { get { return animationPlayer.GetWorldTransforms()[32]; } }
        public float ArmLength { get { return Bones[34].Translation.X; } }
        public Particle SlideEffect { get { return sliding; } }
        public Particle ShotMark { get { return shotMark; } }
        public Particle SmashPoint { get { return smashPoint; } }
        public Particle SmashPoint2 { get { return smashPoint2; } }
        public Vector3 HitBounds { get { return hitBounds; } }
        /// <summary>
        /// スマッシュ可能な球の高さ
        /// </summary>
        public float SmashableHeight { get { return ShoulderY + ArmLength; } }
        /// <summary>
        /// スマッシュ可能な球の深さ
        /// </summary>
        public float SmashableDepth { get { return TennisCourt.CourtLength * 0.8f; } }
        /// <summary>
        /// スマッシュ可能な球の速度
        /// </summary>
        public float SmashableSpeed { get { return 55*1000 * 0.27f / 60 / 3600; } }
        /// <summary>
        /// ポイント内でのショット数
        /// </summary>
        public int ShotsInPoint { get; private set; }
        /// <summary>
        /// 総ショット数
        /// </summary>
        public int TotalShots { get; private set; }
        /// <summary>
        /// 自分のコート内にスマッシュポイントがあるか
        /// </summary>
        public bool HasSmashPoint
        {
            get
            {
                Vector3 point = ball.GetPoint((SmashableHeight + hitBounds.Y) / 2);
                //スマッシュ可能な高さ(最低点)に達するまでの時間
                int framesMax = ball.GetFrames(SmashableHeight) -(int)(Delay*1.5f);
                return point != Vector3.Zero && Math.Abs(point.Z) > hitBounds.Z/2 && ball.Bounds == 0 && framesMax > 0 && position.Z * ball.Speed.Z > 0 && point.Z * ball.Speed.Z > 0
                && new Vector2(ball.Speed.X, ball.Speed.Z).Length() < SmashableSpeed;
            }
        }
        public Vector3 SwingStartPosition { get; private set; }
        #endregion

        #region 判定等
        public bool Diveable()
        {
            Vector3 vec = position - ball.Position;
            return new Vector2(vec.X, vec.Z).Length() < hitBounds.X * 2f;
        }
        //public void SetFirstPosition(CourtSide side)
        //{
        //    if (IsServer)
        //    {
        //        SetToServerPosition(side);
        //    }
        //    else
        //    {
        //        SetToReceiverPosition(side);
        //    }
        //    //
        //    ShotsInPoint = 0;
        //}
        #endregion

        #region 操作
        public void Toss()
        {
            ball.Toss(hitBounds.Y - ball.Position.Y+0.1f);
        }
        public void SetToServerPosition(CourtSide side,TennisEvents events)
        {
            float x = 0;
            if (events == TennisEvents.Singles)
                x = ShoulderWidth * 3;
            else if (events == TennisEvents.Doubles)
                x = TennisCourt.SinglesWidth / 2 - ShoulderWidth * 2;
            float z = TennisCourt.CourtLength + ShoulderWidth;
            if (side == CourtSide.Deuse)
            {
                if (position.Z < 0)
                {
                    position = new Vector3(-x, 0, -z);
                }
                else
                {
                    position = new Vector3(x, 0, z);
                }
            }
            else
            {
                if (position.Z < 0)
                {
                    position = new Vector3(x, 0, -z);
                }
                else
                {
                    position = new Vector3(-x, 0, z);
                }
            }
            initPoint();
            readyToServe(side);
        }
        void readyToServe(CourtSide side)
        {
            changeState(new ServeReady(this, side));
        }
        public void SetToReceiverPosition(CourtSide side)
        {
            float x = TennisCourt.SinglesWidth / 2 - 0.27f;
            float z = TennisCourt.CourtLength - 0.27f;
            if (side == CourtSide.Deuse)
            {
                if (position.Z < 0)
                {
                    position = new Vector3(-x, 0, -z);
                }
                else
                {
                    position = new Vector3(x, 0, z);
                }
            }
            else
            {
                if (position.Z < 0)
                {
                    position = new Vector3(x, 0, -z);
                }
                else
                {
                    position = new Vector3(-x, 0, z);
                }
            }
            changeState(new Standing(this));
            initPoint();
        }
        public void AddToGameComponents(Scene scene,bool assist)
        {
            scene.AddComponents(Belonging, SlideEffect, SmashPoint2, SmashPoint,swing);
            if (assist)
            {
                scene.AddComponents(ShotMark,shotPoint);
            }
        }
        void initPoint()
        {
            ShotsInPoint = 0;
            stop();

        }
        public void Sliding()
        {
            if (Position.Y == 0)
            {
                sliding.Direction = Vector3.Normalize(-this.Speed + Vector3.Up);
                sliding.EmitPoint = position;
                sliding.Emit();
                //vibrationXController(1);
            }
        }
        public void Accel()
        {
            Velocity += 0.0166f + 0.0166f * 2.0f * Ability.Acceleration;
            if (Velocity > 1)
                Velocity = 1;
        }
        public void FireRacket(Vector3 color)
        {
            swing.EmitPoint = (Matrix.CreateTranslation(new Vector3(0, 0, ArmLength + racketLength)) * HandMatrix()).Translation;
            //swing.InitialVelocity = 0;
            //sliding.InitialTexture = GameMain.Textures["smoke"];
            swing.InitialColor = swing.FinalColor = color;
            swing.Emit();
        }

        int currentDelay = 0;
        int maxDelay = 22;
        public bool OnSmashPoint()
        {
            return HasSmashPoint && Hit(new HitVolume(smashPoint.EmitPoint+Vector3.Up * 0.27f,smashPoint.Scale/4,0.27f*2,smashPoint.Scale/4));
        }
        /// <summary>
        /// 現在の状況に最適なラケットスイングのアニメーション名を取得
        /// </summary>
        /// <returns></returns>
        public string GetSwingAnimation(ControllerState controlerState,ref int delay, out TimeSpan start)
        {
            SwingStartPosition = Position;
            int startMillis = 0;
            string res = "Animation_7";
            int frames = Ball.GetFrames(0);
            //int smashFrames = Ball.GetFrames(SmashableHeight);
            if (controlerState.Button2 == ControlerButtonStates.Pressed)
            {
                delay = (int)(delay * 1.5f);
            }
            
            int bounds = Ball.Bounds >= 1 || frames < delay ? 1 : 0;
            
            if (bounds == 0)//ボレー
            {
                if(OnSmashPoint())
                {
                    //スマッシュのモーション
                    res = "Animation_3";
                    delay = (int)(MathHelper.Clamp(ball.GetFrames((SmashableHeight+HitBounds.Y)/2), Delay, maxDelay));
                    startMillis = 444;

                    //if (Smashable() && Ball.GetFrames(SmashableHeight) > delay)//スマッシュ
                    //{

                    //}
                    //else //スイングボレーはボツ
                    //{
                    //    if (!ForeHandBall())
                    //    {
                    //        res = "Animation_21";
                    //        startMillis = 1492;// start = new TimeSpan(0, 0, 0, 0, (int)(1492 / 120.0f * 1000));
                    //    }
                    //    else
                    //    {
                    //        res = "Animation_20";
                    //        startMillis = 1500;// = new TimeSpan(0, 0, 0, 0, (int)(1500 / 120.0f * 1000));
                    //    }
                    //}
                }
                else//普通のボレー
                {
                    if (!ForeHandBall())//バック
                    {
                        res = "Animation_22";
                        startMillis = 1570;// = new TimeSpan(0, 0, 0, 0, (int)(1570 / 120.0f * 1000));
                    }
                    else
                    {
                        res = "Animation_22";
                        startMillis = 690;// = new TimeSpan(0, 0, 0, 0, (int)(690 / 120.0f * 1000));
                    }
                    delay /= 2;
                }
            }
            else //ストローク
            {
                if (!ForeHandBall())//バック
                {
                    res = "Animation_21";
                    if (controlerState.Button4 == ControlerButtonStates.Pressed)//バックスライス
                    {
                        startMillis = getSwingStart(444,642,642);//825);
                        //start = new TimeSpan(0, 0, 0, 0, (int)(630 / 120.0f * 1000));
                    }
                    else
                    {
                        startMillis = getSwingStart(1082,1290,1502);

                        //start = new TimeSpan(0, 0, 0, 0, (int)(1278 / 120.0f * 1000));//ふつうにバック
                    }
                    //start = new TimeSpan(0, 0, 0, 0, (int)(startMillis / 120.0f * 1000));
                }
                else//フォア
                {
                    res = "Animation_20";
                    if (controlerState.Button4 == ControlerButtonStates.Pressed)//フォアスライス
                    {
                        res = "Animation_7";
                        startMillis = 1061;
                        //res = "Animation_22";
                        //start = new TimeSpan(0, 0, 0, 0, (int)(690 / 120.0f * 1000));
                    }
                    else
                    {
                        //float startMillis = 1275;
                        //if (ball.Position.Y >= ShoulderY)
                        //{
                        //    startMillis = 1500;
                        //}
                        //else if (ball.Position.Y <= ShoulderY / 2)
                        //{
                        //    startMillis = 1058;
                        //}
                        startMillis = getSwingStart(1069, 1290, 1514);
                    }
                }
            }
            if (Ball.Bounds < 2 && Ball.GetFrames(HitBounds.Y) > delay)
            {
                Jump(delay*2);
                //Jump(MathHelper.Clamp(Ball.Position.Y - HitBounds.Y, 0, 0.27f));
            }
            startMillis -= delay * 2;
            if (startMillis < 0)
                startMillis = 0;
            start = new TimeSpan(0, 0, 0, 0, (int)(startMillis / 120.0f * 1000));
            currentDelay = delay;
            return res;
        }
        int getSwingStart(int low, int middle,int high)
        {
            if (ball.Position.Y >= ShoulderY)
            {
                return high;
            }
            else if (ball.Position.Y <= ShoulderY / 2)
            {
                //Jump(0.27f * 0.2f);
                return low;
            }
            return middle;
        }
        public string GetRunningAnimation()
        {
            if (ball.Speed.Z * position.Z > 0 && ball.Bounds < 2)//こっちに球が向かってくる
            {
                //if (HasSmashPoint)
                //{
                //}
                if (ForeHandBall())
                {
                    return "Animation_16";
                }
                else
                {
                    return "Animation_17";
                }
            }
            //デフォルト
            return "Animation_2";
        }

        public bool ForeHandBall()
        {
            Ray ray = Ball.Ray;
            if (position.Z >= 0)
            {
                return ((ray.Position + ray.Direction * (position.Z - ball.Position.Z)).X > position.X);
            }
            else
            {
                return ((ray.Position + ray.Direction * (-position.Z + ball.Position.Z)).X < position.X);

            }
        }
        public string GetStandAnimation()
        {
            if (ball.Speed.Z * position.Z > 0 && ball.Bounds < 2)//こっちに球が向かってくる
            {
                if (ForeHandBall())
                {
                    return "Animation_18";
                }
                else
                {                    
                    return "Animation_19";
                }
            }
            //デフォルト
            return "Animation_1";
        }
        #endregion

        #region インターフェイスの実装
        /// <summary>
        /// コントローラによって、このオブジェクトを操作する
        /// </summary>
        /// <param name="controlerState">このオブジェクトを操作するコントローラ</param>
        public void Control(ControllerState controlerState)
        {
            if (Camera.Position.Z < 0)
                controlerState.JoyStick = -controlerState.JoyStick;
            playerState.Update(controlerState);
            if (playerState.NextState != null)
            {
                changeState(playerState.NextState);
            }
            if (playerState.GooL)
            {
                gooL = true;
            }
            else gooL = false;
            if (playerState.GooR)
            {
                gooR = true;
            }
            else gooR = false;
            if (controlerState.L == ControlerButtonStates.Down)
            {
                shotMark.Visible = shotPoint.Visible = true;
            }
            else
            {
                shotMark.Visible = shotPoint.Visible = false;
            }
            #region 
            ////走行中
            //if (controlerState.JoyStick != Vector2.Zero)//移動の入力あり
            //{

            //    //加速
            //    v += ability.Acceleration;
            //    if (v > 1)
            //        v = 1;

            //    if (speed.Length() == 0)
            //    {
            //        StartAnimation("Animation_2", new TimeSpan(0, 0, 0, 0, 100));
            //        PlaySound("catJump");
            //    }

            //    if (Vector2.Dot(Vector2.Normalize(joyStick), bodyDirection) < (float)Math.Cos(MathHelper.ToRadians(ability.RotAngle)))
            //    {
            //        v = v * ability.QuickNess;
            //    }

            //    ////direction = thumbDeg;
            //    bodyDirection = Vector2.Normalize(joyStick);
            //    //new Running().Update(controlerState, ability, ref v, ref bodyDirection);

            //}
            //else if (v != 0)//入力なし
            //{
            //    //減速
            //    v -= ability.Deceleration;// new Vector3(deceleration * direction.X, 0, deceleration * direction.Y);
            //    if (v <= 0)
            //    {
            //        //立ち状態へ
            //        v = 0;
            //        StartAnimation("Animation_3");
            //        StopSound("catJump");

            //        //相手のほうを見る
            //        this.SetPosture(position, Vector2.Normalize(new Vector2(0, -position.Z)));
            //    }
            //}
            #endregion


            //自分の上にリセット
            //if (controlerState.Pause == ControlerButtonStates.Pressed)
            //{
            //    ball.Init(Center);
            //    Toss();
            //}
            //ジャンプ
            //if (controlerState.R == ControlerButtonStates.Pressed)
            //{
            //    Jump(0.27f);
            //    //Smash(controlerState.JoyStick,TennisCourt.CourtLength / 1.5f);
            //    //Serve(controlerState.JoyStick,3,1.5f,CourtSide.Deuse,spin);
            //}
        }
        private void changeState(PlayerState nextState)
        {
            //サウンド変える
            
            if(playerState.SoundName != "")
                StopSound(playerState.SoundName);
            if(nextState.SoundName != "")
                PlaySound(nextState.SoundName);
            //アニメーション変える
            StartAnimation(nextState.AnimationName, nextState.AnimationStartTime, nextState.AnimationStartTime2);
            //新状態へ
            playerState = nextState;
            Rotation = new Vector3(0,Rotation.Y,0);
        }
        public void Play()
        {
            changeState(new Standing(this));
        }
        public void Won()
        {
            changeState(new Won(this));
        }
        public void Lost()
        {
            changeState(new Lost(this));
        }
        public void ChangeAnimation(string animationName)
        {
            StartAnimation(animationName);
        }

        Vector3 shotPointEmit = Vector3.Zero;
        /// <summary>
        /// ラケットを振る
        /// </summary>
        /// <param name="controlerState"></param>
        public void Swing(ControllerState controlerState)
        {
            Vector2 joyStick = new Vector2(controlerState.JoyStick.X, -controlerState.JoyStick.Y);
            float distanceFactor = (float)(Math.Abs(position.Z) - hitBounds.Z/2) / TennisCourt.CourtLength;

            //float rate = 1;
            if (ball != null && this.Hit(ball))
            {
                shotPointEmit = DistanceToBall*HitBounds/2;
                shotPointEmit.Y = 0.0027f;
                shotPointEmit.Z *= -1;
                if (Position.Z < 0)
                {
                    shotPointEmit.X *= -1;
                    shotPointEmit.Z *= -1;
                }
                Vector3 posi = new Vector3(Position.X, 0, Position.Z);
                shotPoint.EmitPoint = posi + shotPointEmit;
                shotPoint.Reset();
                shotPoint.Emit();

                Func<float, float, float> getDepth = (minDepth, maxDepth) =>
                {
                    return TennisCourt.CourtLength * MathHelper.SmoothStep(minDepth, maxDepth, Math.Abs(DistanceToBall.X));
                    //return TennisCourt.CourtLength * (minDepth + Math.Abs(DistanceToBall.X) * (maxDepth - minDepth));
                };
                //float depth = TennisCourt.CourtLength * (0.5f + Math.Abs(DistanceToBall.X)*0.49f);// (0.7f + (float)GameMain.Random.NextDouble() * 0.2f);// *(float)(new Random().NextDouble() * 0.5f + 0.5f);

                Vector3 color;
                //ショットの性質を決定する係数

                float heightFactor = 1.0f - (ball.Position.Y / this.hitBounds.Y);
                bool rButtonIsDown = controlerState.R == ControlerButtonStates.Down || controlerState.R == ControlerButtonStates.Pressed;
                //重み
                float w1 = 1f;
                float w2 = 0f;
                //重みと係数に基づいて計算
                float factor = (w1*distanceFactor + w2 * heightFactor) / (w1+w2);
                if (controlerState.Button1 == ControlerButtonStates.Pressed)//通常ショット(ストローク、ボレー、スマッシュ)
                {
                    //if (smashable())
                    //    Smash(joyStick,MathHelper.Clamp(Math.Abs(position.Z)*1.25f,TennisCourt.CourtLength/1.25f,TennisCourt.CourtLength),150);
                    //else
                    //{
                    color = new Vector3(0, 192, 128);
                    //ball.SetFireColors(color, null);
                    //ball.SetShotEffectColor(color);
                    if (rButtonIsDown)
                    {
                        lobShot(joyStick, TennisCourt.CourtLength * (0.9f + 0.1f * Math.Abs(DistanceToBall.X)), distanceFactor);
                    }
                    else if (Smashable())
                    {
                        Smash(joyStick, TennisCourt.CourtLength * 0.75f);
                    }
                    //else if (Smashable())
                    //{
                    //    //if (rButtonIsDown)
                    //    //depth /= 2;
                    //    Smash(joyStick, TennisCourt.CourtLength * 0.75f, (90 + 50 * ability.ServePower).ResizeVelocity() * 1000*Rate);
                    //    //ShotBall(joyStick, ball.GetMinAngle(depth)/1.5f, depth, 0.001f);
                    //    //color = new Vector3(255, 128, 0);
                    //    //ball.SetFireColors(color, null);
                    //    //ball.SetShotEffectColor(color);
                    //}
                    else if (ball.Bounds != 0)
                    {
                        //つなぎストローク
                        //ShotBall(joyStick, /*ball.GetMinAngle(depth)/10 + 10 + spin / 3*/ball.GetSafetyAngle(height, depth), depth, spin * rate);
                        spinShot(joyStick, getDepth(0.5f, 0.99f), distanceFactor, rButtonIsDown);
                    }
                    else
                    {
                        //ボレー
                        highVolley(joyStick, getDepth(0.6f, 0.99f), distanceFactor, rButtonIsDown);
                        //ShotBall(joyStick, ball.GetSafetyAngle(height, depth), depth, -15);
                    }
                        //}//ball.SetFireTexture(Game1.Textures["fireG"]);
                   
                    //ball.ExplodeLittle();
                }
                else if (controlerState.Button2 == ControlerButtonStates.Pressed)//強打
                {
                    if (ball.Bounds == 0)
                    {
                        if (Smashable())
                        {
                            Smash(joyStick, TennisCourt.CourtLength * 0.55f);
                        }
                        else
                        {
                            highVolley(joyStick, getDepth(0.5f, 0.75f), distanceFactor, false);
                        }
                        //else if(Position.Y == 0)
                        //{
                        //    swingVolley(joyStick);
                        //}
                        //else
                        //    highVolley(joyStick, getDepth(0.6f,0.99f), distanceFactor, rButtonIsDown);
                    }
                    else
                    {
                        flatShot(joyStick, getDepth(0.5f,1.0f));
                    }
                }
                else if (controlerState.Button3 == ControlerButtonStates.Pressed)//ロブ
                {
                    color = new Vector3(255, 255, 0);
                    //ball.SetFireColors(color, null);
                    //ball.SetShotEffectColor(color);
                    lobShot(joyStick, TennisCourt.CourtLength*0.8f,distanceFactor);
                    //ShotBall2(joyStick, ball.GetSafetyAngle(TennisCourt.NetHeight * 8, depth), depth, 40000.0f*0.27f / 3600 / 60);
                    //ball.ShotByVelocityAndDistance(new Vector2(0, ball.GetSafetyAngle(TennisCourt.NetHeight*3,depth)),20000.0f / 3600 / 60.0f,depth);
                    //ShotBall(joyStick, 45, TennisCourt.CourtLength, spin*rate);//, new Vector2(0, 6));//ロブ
                    //ShotBall(joyStick, ball.GetSafetyAngle(TennisCourt.NetHeight*4,depth/3), depth/3,-spin*rate);//,new Vector2(0,6));//ドロップ
                    //ShotBall(joyStick, 20, depth / 2, spin);
                    //ShotBall(joyStick, 45, depth / 2, -spin);
                    
                    //ball.SetFireTexture(Game1.Textures["fireY"]);
                    
                    //ball.SetFireColors(new Vector3(255,255,0),null);
                    //ball.SetShotEffectColor(color);
                    //ball.ExplodeLittle();

                }
                else if (controlerState.Button4 == ControlerButtonStates.Pressed)//スライス系
                {
                    color = new Vector3(0, 255, 255);
                    //ball.SetFireColors(color, null);
                    //ball.SetShotEffectColor(color);
                    //dropShot(joyStick, dropDepth);
                    if (Smashable() && !rButtonIsDown)
                    {
                        Smash(joyStick, TennisCourt.CourtLength * 0.95f);
                    }
                    else
                        sliceShot(joyStick, getDepth(0.5f,0.98f), distanceFactor, rButtonIsDown);
                    
                    //ShotBall(joyStick, ball.GetSafetyAngle(MathHelper.Lerp(TennisCourt.NetHeight, TennisCourt.NetHeight * 4, factor), depth), depth, MathHelper.Lerp(-10, 10, ball.Position.Y / hitBounds.Y));//ボレー
                    //ShotBall(joyStick, ball.GetSafetyAngle(TennisCourt.NetHeight * 4, depth / 3), depth / 3, -spin * rate);//,new Vector2(0,6));//ドロップ
                    //float height = MathHelper.Lerp(TennisCourt.NetHeight, TennisCourt.NetHeight * 3f, factor);
                    //ShotBall(joyStick, ball.GetSafetyAngle(height,depth) /*+ 10 + (30 - spin) / 2*/, depth,-spin*rate);
                    //System.Windows.Forms.MessageBox.Show(ball.GetSafetyAngle(2f, depth) + "");
                    //ball.SetFireTexture(Game1.Textures["fireB"]);
                    
                    //ball.SetFireColors(new Vector3(0, 255, 255), null);
                    //ball.SetShotEffectColor(color);
                    //ball.ExplodeLittle();
                }
                else
                {
                    ShotBall(controlerState.JoyStick, 45, TennisCourt.CourtLength * (Velocity * 0.3f + 0.35f), new Vector3(224, 224, 224), -10, angleLimitR(0), angleLimitL(0));
                    
                    ball.ExplodeLittle();
                }
            }
        }
        public bool Smashable()
        {
            if (ball.Bounds == 0 && ball.Position.Y > SmashableHeight //球が十分に高いところにあり、
                //Math.Abs(ball.Position.Z) < SmashableDepth && //プレイヤーがある程度前にいて、
                //new Vector2(ball.Speed.X, ball.Speed.Z).Length() <= SmashableSpeed / (ball.Bounds+1)//球のXZ速度が55km/h以下である
                //&& this.HasSmashPoint
                )
                return true;
            return false;
        }
        float angleLimitL(float risk) 
        {
            float res = angleLimit();
            if (Position.X < 0)
            {
                float val = MathHelper.Clamp(1- (-Position.X * 4 / TennisCourt.SinglesWidth), 0.25f, 1);
                res *= val;
                risk /= val;
            }
            //if (position.X > -TennisCourt.SinglesWidth * 0.5f * 0.25f)
            //{
            //    res = angleLimit();
            //}else
            //    res = angleLimit() * 0.75f;

            float disZ = DistanceToBall.Z;// MathHelper.SmoothStep(-1, 1, (DistanceToBall.Z + 1) / 2);
            float offset = DistanceToBall.X >= 0 ? disZ * res : disZ * -res / ability.BackHand;
            offset *= (1 + risk);
            if (Position.Z > 0)
            {
                res -= offset;
            }
            else
            {
                res += offset;
            }
            return res;
            
        }

        private float angleLimit()
        {
            float f1 = Math.Abs(position.X)*2 / TennisCourt.SinglesWidth;
            float f2 = 1 - (Math.Abs(position.Z) - TennisCourt.CourtLength / 2) / (TennisCourt.CourtLength / 2);
            if (f2 > 1)
                f2 = 1;
            if (f2 < 0)
                f2 = 0;
            //return MathHelper.Lerp(0.5f, 0, f2);
            //return MathHelper.Clamp(MathHelper.Lerp(0.5f, 0.25f*(1-ability.Nicety), f1),0,0.5f);
            //return MathHelper.SmoothStep(0.5f*(1-ability.Nicety),0.5f,(Math.Abs(Position.Z) - HitBounds.Z/2) / TennisCourt.CourtLength);
            return (1-ability.Nicety*0.5f);
        }
        float angleLimitR(float risk)
        {
            float res = angleLimit();
            if (Position.X > 0)
            {
                float val = MathHelper.Clamp(1 - (Position.X * 4 / TennisCourt.SinglesWidth), 0.25f, 1);
                res *= val;
                risk /= val;
            }
            //if (position.X < TennisCourt.SinglesWidth * 0.5f * 0.25f)
            //{
            //    res = angleLimit();
            //}else 
            //    res = angleLimit() * 0.75f;
            float disZ = DistanceToBall.Z;// MathHelper.SmoothStep(-1, 1, (DistanceToBall.Z + 1) / 2);
            float offset = DistanceToBall.X >= 0 ? disZ * -res : disZ * res / ability.BackHand;
            offset *= (1 + risk);
            if (Position.Z > 0)
            {
                res -= offset;
            }
            else
            {
                res += offset;
            }

            return res;
        }
        
        void spinShot(Vector2 joyStick,float distanceFromNet,float distanceFactor,bool rDown)
        {
            float velocity = spinSpeed * Rate * 1000 * 0.27f / 3600 / 60;
            float spin = ability.TopSpin * Rate*Rate;
            
            //velocity *= decByPliZ(1.0f*Rate*Rate);

            //spin *= MathHelper.Lerp(1.2f, 0.4f, DistanceToBall.Y);
            //if (ball.Position.Y >= ShoulderY)
            //    spin *= 0.75f;
            //else if (ball.Position.Y <= ShoulderY)
            //    spin *= 1.25f;
            //spin *= MathHelper.Lerp(1.2f, 1.0f, Math.Abs(DistanceToBall.X));

            //spin *= DistanceToBall.Z > 0 ? 1 : MathHelper.Lerp(1.0f, 0.2f, -DistanceToBall.Z);
            velocity *= decByPliY(1.0f*Rate*Rate);
            if (Math.Abs(DistanceToBall.X) < 0.25f)
            {
                velocity *= 0.75f + 0.1f * ability.PliabilityX;
            }
            //velocity *= decByHeight(0.95f,1.0f + 0.15f*Math.Abs(DistanceToBall.X) + 0.15f*MathHelper.Clamp(DistanceToBall.Z,0,1));
            float pow = ability.Power * 0.35f;
            if (DistanceToBall.X < 0)
                pow *= ability.BackHand;
            velocity *= decByHeight(0.95f,1.0f + pow);

            //float height = MathHelper.Lerp(TennisCourt.NetHeight * 1.5f, TennisCourt.NetHeight * 6,distanceFactor);
            //float y = ball.GetSafetyAngle(height, distanceFromNet,velocity); //angleY;
            //if (ball.Position.Y < TennisCourt.NetHeight)//ネットより低いと弱体化
            //{
            //    float factor = ball.Position.Y / TennisCourt.NetHeight;
            //    //y *= MathHelper.Lerp(1.0f/negativeFactor,1,factor);
            //    velocity *= MathHelper.Lerp(ability.PliabilityY, 1, factor);
            //}
            //else if (ball.Position.Y > ShoulderY)
            //{
            //    float factor = MathHelper.Lerp(1, 1.1f, ball.Position.Y / ShoulderY);
            //    velocity *= factor;
            //}
            //else
            //{
            //    y = MathHelper.Lerp(angleY,ball.GetMinAngle(distanceFromNet), (ball.Position.Y) / hitBounds.Y);
            //}

            //if (rDown)
            //{
            //    //アングルショット
            //    //角度
            //    //速度が問題
            //    distanceFromNet = TennisCourt.ServiceAreaLength;
            //    //float factor1 = MathHelper.Clamp(Math.Abs(position.X) / TennisCourt.SinglesWidth,0,1);
            //    //float factor2 = MathHelper.Clamp((1-distanceFactor),0,1);
            //    //float factor = (factor1 * 0.3f + factor2 * 0.7f);
            //    //joyStick.X *= MathHelper.Lerp(0.4f, 1, factor);
            //    //y *= 1.1f;
            //    velocity *= decByPliY(1.0f*Rate);
            //}
            ShotBall3(joyStick, distanceFromNet, velocity, new Vector3(0,192,128), spin,true,ability.MaxAngleY,angleLimitR(0.125f),angleLimitL(0.125f));
            onShotting(Shots.TOP);
            //ShotBall2(joyStick, y, distanceFromNet, velocity,new Vector3(0,192,128),angleLimitR,angleLimitL);
            //遅れると動き出しが遅く
            delay(1.0f);
        }
        bool canSwingVolley()
        {
            if (ball.Bounds == 0 && ball.Speed.Y < 0 && ball.Position.Y >= HeadPosition.Y 
                && ball.Speed.Length() <= SmashableSpeed )
                return true;
            return false;
        }
        private void flatShot(Vector2 joyStick, float depth)
        {   
            float velocity = spinSpeed * 1000.0f.ResizeVelocity() * Rate * 0.95f;
            float height = Math.Abs(ball.Speed.Y / ball.BoundVelocity.Y)*Rate;
            if (ball.Speed.Y < 0)
                height *= 0.5f;
            velocity *= 0.4f + (1 - height) * 0.6f + 0.3f * ability.Power * (DistanceToBall.X < 0 ? ability.BackHand : 1);
            //velocity *= Math.Abs(ball.Speed.Y / ball.BoundVelocity.Y) <= 0.5f ? 1 + 0.1f * (ability.Power) : 0.75f;
            if (ball.Position.Y >= ShoulderY)
            {
                velocity *= 1.1f;
            }
            else if (ball.Position.Y <= ShoulderY / 2)
            {
                velocity *= 0.95f;
            }
            
            float spin = ability.TopSpin * 0.1f * Rate * Rate;
            //depth *= MathHelper.Lerp(0.8f, 1.2f, Math.Abs(DistanceToBall.X));
            ShotBall3(joyStick,depth,velocity,Vector3.Zero,spin,true,45,angleLimitR(0.5f),angleLimitL(0.5f));
            //float height = TennisCourt.NetHeight * (0.5f + Math.Abs(DistanceToBall.X) * 1.0f + Math.Abs(ball.Speed.Y / ball.BoundVelocity.Y) * 1.5f);
            //ShotBall4(joyStick, depth, height, velocity, new Vector3(255, 128, 0), ability.TopSpin * 0.75f, angleLimitR(2.0f), angleLimitL(2.0f));
        }
        private void swingVolley(Vector2 joyStick)
        {
            float spin = ability.TopSpin * Rate * Rate;
            float rate = MathHelper.Clamp(MathHelper.Lerp(0.6f, 1.1f, ball.Speed.Y / new Vector2(ball.Speed.X, ball.Speed.Z).Length() * 0.5f), 0.6f, 1.1f);
            float velocity = spinSpeed * 1000.0f.ResizeVelocity() * Rate * rate;
            if (ball.Position.Y < ShoulderY)
            {
                velocity *= 0.6f;
                spin *= 0.5f;
            }
            ShotBall3(joyStick, TennisCourt.CourtLength * 0.6f, velocity, Vector3.Zero, spin, true, ability.MaxAngleY, 0.3f, 0.3f);
        }
        private float decByHeight(float min, float max)
        {
            return MathHelper.Lerp(min, max, ball.Position.Y / HitBounds.Y * Math.Abs(Position.Z) / TennisCourt.CourtLength);
        }

        private float decByPliZ(float val)
        {
            //if(DistanceToBall.Z < 0)
            //    return MathHelper.Lerp(1, 0.3f + 0.2f * ability.PliabilityZ, -DistanceToBall.Z / hitBounds.Z * val);
            return 1;
        }

        private float decByPliY(float val)
        {
            if (ball.Speed.Y > 0)
                return MathHelper.Lerp(1, 0.55f + 0.3f * ability.PliabilityY, ball.Speed.Y / ball.BoundVelocity.Y * val);
            return 1;
        }
        void sliceShot(Vector2 joyStick, float distanceFromNet, float distanceFactor, bool rDown)
        {
            if (rDown)
            {
                dropShot(joyStick, dropDepth * TennisCourt.CourtLength, distanceFactor);
            }
            else if (ball.Bounds == 0)
            {
                lowVolley(joyStick, distanceFromNet, distanceFactor, rDown);
            }
            else
            {
                if (distanceFactor > 1)
                    distanceFactor = 1;
                float velocity = sliceSpeed * 1000 * 0.27f / 3600 / 60 * (1 + (Rate - 1) * 0.25f) * MathHelper.Clamp(MathHelper.Lerp(0.75f, 1.0f, distanceFactor), 0.75f, 1.0f);
                velocity *= decByPliY(0.6f*Rate*Rate);
                //velocity *= decByPliZ(0.4f);
                velocity *= decByHeight(0.95f, 1.05f);
                velocity *= MathHelper.SmoothStep(0.5f + 0.5f * ability.PliabilityX, 1, Math.Abs(DistanceToBall.X));
                //float height = MathHelper.Lerp(TennisCourt.NetHeight * 1.25f, TennisCourt.NetHeight * 1.5f, distanceFactor) / ability.Slice;
                //ShotBall4(joyStick, distanceFromNet, TennisCourt.NetHeight * 1.3f, velocity, new Vector3(0, 255, 255), -ability.SliceSpin, angleLimitR, angleLimitL);
                float spin = -ability.SliceSpin;
                float maxAngle = ability.MaxAngleY * (2-decByPliY(1.0f));
                //spin *= decByPliY(2.0f);
                ShotBall3(joyStick, distanceFromNet, velocity, new Vector3(0, 255, 255),spin,true, maxAngle,angleLimitR(0.0f), angleLimitL(0.0f));
                //ShotBall2(joyStick, ball.GetSafetyAngle(height, distanceFromNet,velocity), distanceFromNet, velocity,new Vector3(0,255,255),angleLimitR,angleLimitL);
                onShotting(Shots.SLICE);
                delay(0.5f);
            }
        }
        private void delay(float val)
        {
            //if (DistanceToBall.Z < 0)
            //{
            //    Jump((int)((1.0f - ability.QuickNess) * 10 * val + 25));
            //}
        }
        void lobShot(Vector2 joyStick, float distanceFromNet,float distanceFactor)
        {
            //前に行くと弱くなる
            float velocity = SmashableSpeed * 0.7f + SmashableSpeed * 0.25f * ability.LobPower;

            if (velocity > SmashableSpeed)
            {
                velocity = SmashableSpeed;
                //
                //System.Windows.Forms.MessageBox.Show("Big");
            }
            float angle = 42;// angleY * 2.5f;
            float spin = (ability.TopSpin) * Rate;
            //ロブボレーはスピンが少ない
            if (ball.Bounds == 0)
                spin *= 0.8f;
            ShotBall(joyStick, angle, distanceFromNet, new Vector3(255, 255, 0), spin, angleLimitR(0), angleLimitL(0));
            onShotting(Shots.LOB);
            //ShotBall2(joyStick, angle, distanceFromNet, velocity,new Vector3(255,255,0),angleLimitR,angleLimitL);
            //ShotBall3(joyStick, distanceFromNet, velocity * 1.414f, new Vector3(255, 255, 0), spin, true,45,angleLimitR,angleLimitL);
            //ShotBall4(joyStick, distanceFromNet, TennisCourt.NetHeight * 5, velocity, new Vector3(255, 255, 0), spin, angleLimitR, angleLimitL);
        }
        void dropShot(Vector2 joyStick, float distanceFromNet,float distanceFactor)
        {
            //distanceFromNet *= MathHelper.Lerp(1, 1.2f, distanceFactor) * MathHelper.Lerp(1.5f, 1, ability.Drop);
            float velocity = /*MathHelper.Lerp(1, 0.6f, distanceFactor) * */ 35000.0f.ResizeVelocity();//(Math.Abs(ball.Position.Z) + distanceFromNet) / dropFrames;
            distanceFactor = Math.Abs(ball.Position.Z) / TennisCourt.CourtLength;            
            //ShotBall(joyStick, angle, distanceFromNet, -30);
            //float velocity = 20*1000*0.27f/3600/60;
            //前で打った方が早く到着
            //ShotBall4(joyStick, distanceFromNet, TennisCourt.NetHeight * 1.5f, velocity, new Vector3(128, 255, 255), -ability.SliceSpin, angleLimitR, angleLimitL);
            //ShotBall3(joyStick, distanceFromNet, velocity, new Vector3(188, 255, 255),-ability.SliceSpin,false,45, angleLimitR, angleLimitL);
            //ShotBall2(joyStick, angle, distanceFromNet, velocity,new Vector3(128,255,255),angleLimitR,angleLimitL);
            float height = MathHelper.Lerp(1.25f, 5.0f, distanceFactor);
            if (ball.Bounds == 0)
            {
                height *= 1.5f;
            }
            else
            {
                height /= decByPliY(2f);
            }
            float angle = MathHelper.ToDegrees((float)Math.Atan(MathHelper.Clamp(TennisCourt.NetHeight*height-ball.Position.Y,0,TennisCourt.NetHeight*height) / Math.Abs(Position.Z)));
            
            float spin = -45 + ability.Drop * 20;
            //ShotBall4(joyStick, distanceFromNet, TennisCourt.NetHeight*2, velocity, Vector3.Zero, -ability.SliceSpin, angleLimitR, angleLimitL);
            ShotBall(joyStick, angle, distanceFromNet, Vector3.Zero,spin, 0.4f, 0.4f);
            onShotting(Shots.DROP);
        }
        void highVolley(Vector2 joyStick, float distanceFromNet,float distanceFactor,bool rDown)
        {
            //打点が低いと弱い
            //後ろにいても弱い
            float heightFactor = MathHelper.Clamp(ball.Position.Y / ShoulderY,0,1);
            float velocity = ability.VolleyPower * 1000 * 0.27f / 3600 / 60;
            //velocity *= MathHelper.Lerp(0.8f, 1, heightFactor);
            //velocity *= MathHelper.Lerp(1, 0.5f, distanceFactor);

            velocity *= MathHelper.Lerp(0.7f, 1.2f, DistanceToBall.Y);

            //float height = MathHelper.Lerp(TennisCourt.NetHeight * 1.25f, TennisCourt.NetHeight * 3f, distanceFactor);
            //height *= MathHelper.Lerp(2f, 1f, heightFactor);
            //ShotBall2(joyStick, 15, distanceFromNet, velocity, Vector3.Zero, angleLimitR, angleLimitL);
            float risk = distanceFactor;
            ShotBall3(joyStick, distanceFromNet, velocity, new Vector3(128, 255, 192),-ability.SliceSpin*0.5f, true,ability.MaxAngleY,angleLimitR(risk), angleLimitL(risk));
            //ShotBall(joyStick, 45, TennisCourt.CourtLength * 0.98f, new Vector3(128, 255, 192), -400f, angleLimitR(risk), angleLimitL(risk));//白鯨
            //ShotBall(joyStick, 45, TennisCourt.CourtLength * 0.98f, new Vector3(128, 255, 192), 300f, angleLimitR(risk), angleLimitL(risk));//スーパーロブ
            //ShotBall(joyStick, 5, TennisCourt.CourtLength * 0.98f, new Vector3(128, 255, 192), -300f, angleLimitR(risk), angleLimitL(risk));//戻るスライス
            //ShotBall(joyStick, 30, TennisCourt.CourtLength * 0.8f, new Vector3(128, 255, 192), 300, angleLimitR(risk), angleLimitL(risk));

            onShotting(Shots.VOLLEY);
            //ShotBall2(joyStick, ball.GetSafetyAngle(height,distanceFromNet,velocity), distanceFromNet, velocity,new Vector3(128,255,192),angleLimitR,angleLimitL);
        }
        void lowVolley(Vector2 joyStick, float distanceFromNet, float distanceFactor, bool rDown)
        {
            float heightFactor = MathHelper.Clamp(ball.Position.Y / ShoulderY, 0, 1);
            float velocity = ability.VolleyPower * 1000 * 0.27f / 3600 / 60 * 0.9f;
            //velocity *= MathHelper.Lerp(0.8f, 1, heightFactor);
            //velocity *= MathHelper.Lerp(1, 0.8f, distanceFactor);
            //ShotBall2(joyStick, 30, distanceFromNet, velocity, Vector3.Zero, angleLimitR, angleLimitL);
            float risk = distanceFactor/2;
            ShotBall3(joyStick, distanceFromNet, velocity, new Vector3(128, 255, 192), -ability.SliceSpin*0.75f, true,ability.MaxAngleY, angleLimitR(risk), angleLimitL(risk));
            //ShotBall4(joyStick, distanceFromNet, TennisCourt.NetHeight * 2f, velocity, Vector3.Zero, -ability.SliceSpin / 3, angleLimitR, angleLimitL);
            onShotting(Shots.VOLLEY);
        }

        void stop()
        {
            Velocity = 0;
            bodyDirection = Vector2.Normalize(new Vector2(0, -position.Z));
        }
        private float GetFactor(float distanceWeight, float heightWeight)
        {
            float distanceFactor = (float)(Math.Abs(position.Z) - Radius) / TennisCourt.CourtLength;
            float heightFactor = 1.0f - (ball.Position.Y / this.hitBounds.Y);

            //重みと係数に基づいて計算
            return (distanceWeight * distanceFactor + heightWeight * heightFactor) / (distanceWeight + heightWeight);
        }
        /// <summary>
        /// ボールを打ち出す
        /// </summary>
        /// <param name="joyStick">ジョイスティックのベクトル</param>
        /// <param name="angleY"></param>
        /// <param name="distanceFromNet"></param>
        /// <param name="spin"></param>
        /// <param name="angleScatter">角度のばらつき(-1～1)</param>
        public void ShotBall(Vector2 joyStick,float angleY,float distanceFromNet,Vector3 color,float spin,float rightLimit,float leftLimit,float? maxVelocity=null,Vector2? angleScatter = null,Vector2? angleScatterCenter = null)
        {
            //ボールのバウンド数を登録
            Bounds = ball.Bounds;
            float vel = ball.Speed.Length();
            Vector2 scatterCenter = angleScatterCenter ?? Vector2.Zero;
            if (Math.Abs(scatterCenter.X) > 1 || Math.Abs(scatterCenter.Y) > 1)
            {
                throw new ArgumentException("-1～1の値しか受け付けません");
            }

            float angleX = getAngleX(joyStick, distanceFromNet,rightLimit,leftLimit);

            Vector2 angles = new Vector2(MathHelper.ToDegrees(angleX), angleY);
            float v0 = ball.GetShotVelocity(angles, distanceFromNet+Math.Abs(ball.Position.Z), spin);
            if (maxVelocity != null && v0 > maxVelocity)
                v0 = (float)maxVelocity;
            //能力によって打てる角度を制限
            
            //角度にばらつきを付けてから打ち出す
            Vector2 scatter = angleScatter ?? Vector2.Zero;
            angles += new Vector2((float)(rand.NextDouble()-0.5f+scatterCenter.X/2) * scatter.X, (float)(rand.NextDouble()-0.5f+scatterCenter.Y/2) * scatter.Y);
            ball.ShotByVelocity(angles, v0, spin);
            shotEvents(vel/v0,color);
        }

        /// <summary>
        /// 速度を指定して打ち出す
        /// </summary>
        /// <param name="joyStick"></param>
        /// <param name="angleY"></param>
        /// <param name="distanceFromNet"></param>
        /// <param name="velocity"></param>
        /// <param name="angleScatter"></param>
        /// <param name="angleScatterCenter"></param>
        public void ShotBall2(Vector2 joyStick, float angleY, float distanceFromNet, float velocity, Vector3 color,float rightLimit = 0,float leftLimit = 0,Vector2? angleScatter = null, Vector2? angleScatterCenter = null)
        {
            //ボールのバウンド数を登録
            Bounds = ball.Bounds;
            float vel = ball.Speed.Length();
            Vector2 scatterCenter = angleScatterCenter ?? Vector2.Zero;
            if (Math.Abs(scatterCenter.X) > 1 || Math.Abs(scatterCenter.Y) > 1)
            {
                throw new ArgumentException("-1～1の値しか受け付けません");
            }

            float angleX = getAngleX(joyStick, distanceFromNet,rightLimit,leftLimit);

            Vector2 angles = new Vector2(MathHelper.ToDegrees(angleX), angleY);
            //float v0 = ball.GetShotVelocity(angles, distanceFromNet + Math.Abs(ball.Position.Z), spin);
            //能力によって打てる角度を制限

            //角度にばらつきを付けてから打ち出す
            Vector2 scatter = angleScatter ?? Vector2.Zero;
            angles += new Vector2((float)(rand.NextDouble() - 0.5f + scatterCenter.X / 2) * scatter.X, (float)(rand.NextDouble() - 0.5f + scatterCenter.Y / 2) * scatter.Y);
            //xz速度がvelocity になるように調整してから打ち出す
            //単にCosでわるだけでは×か?
            ball.ShotByVelocityAndDistance(angles, (float)(velocity / Math.Cos(MathHelper.ToRadians(angles.Y))), distanceFromNet);
            //ball.ShotByVelocity(angles, v0, spin);
            shotEvents(vel/velocity, color);
        }

        private void shotEvents(float velocity,Vector3 color)
        {
            //ボールの弾道色を設定
            ball.SetFireColors(color, null);
            //ball.SetShotEffectColor(color);
            PlaySound("shot");
            //ショット数+1
            ShotsInPoint++;
            TotalShots++;
            //ボール爆発
            ball.ExplodeLittle();
            //このインスタンスが最後に打ったプレイヤー
            ball.LastHitter = this;
            vibrationXController(MathHelper.Clamp(0.6f + Math.Abs(velocity) * 0.4f,0,1));
        }
        public void ShotBall3(Vector2 joyStick,float distanceFromNet,float velocity,Vector3 color,float spin,bool low,float maxAngle,float rightLimit, float leftLimit,Vector2? angleScatter=null,Vector2? angleScatterCenter = null)
        {
            //ボールのバウンド数を登録
            Bounds = ball.Bounds;
            float vel = ball.Speed.Length();
            Vector2 scatterCenter = angleScatterCenter ?? Vector2.Zero;
            if (Math.Abs(scatterCenter.X) > 1 || Math.Abs(scatterCenter.Y) > 1)
            {
                throw new ArgumentException("-1～1の値しか受け付けません");
            }

            float angleX = MathHelper.ToDegrees(getAngleX(joyStick, distanceFromNet, rightLimit, leftLimit));

            //Vector2 angles = new Vector2(MathHelper.ToDegrees(angleX), angleY);
            ////float v0 = ball.GetShotVelocity(angles, distanceFromNet + Math.Abs(ball.Position.Z), spin);
            ////能力によって打てる角度を制限

            ////角度にばらつきを付けてから打ち出す
            //Vector2 scatter = angleScatter ?? Vector2.Zero;
            //angles += new Vector2((float)(rand.NextDouble() - 0.5f + scatterCenter.X / 2) * scatter.X, (float)(rand.NextDouble() - 0.5f + scatterCenter.Y / 2) * scatter.Y);
            ball.ShotWithoutAngleY(angleX, velocity, distanceFromNet, spin,low ? ShotAngles.Low : ShotAngles.High,maxAngle);
            //ball.ShotByVelocity(angles, v0, spin);
            shotEvents(vel/velocity, color);
        }
        public void ShotBall4(Vector2 joyStick, float distanceFromNet,float height, float velocity, Vector3 color, float spin, float rightLimit = 0, float leftLimit = 0)
        {
            Bounds = ball.Bounds;
            float vel = ball.Speed.Length();
            float angleX = MathHelper.ToDegrees(getAngleX(joyStick, distanceFromNet, rightLimit, leftLimit));
            ball.ShotByNetHeight(angleX, velocity, height, spin);
            shotEvents(vel/velocity, color);
        }
        bool vib = false;
        static int vibing = 10;
        int vibFrames = vibing;
        /// <summary>
        /// Xboxコントローラを振動させる
        /// </summary>
        /// <param name="velocity"></param>
        void vibrationXController(float vib)
        {
            //float value = MathHelper.Clamp((velocity / 0.27f * 60 * 3600 / 1000.0f) / 100.0f, 0, 1);
            //vibrationXController(value, value);
            vibrationXController(vib,vib);
        }
        void vibrationXController(float left, float right)
        {
            //コントローラを振動(Xboxコントローラのみ)
            if (Scene.Controllers[Index] is XboxControler && !vib)
            {
                GamePad.SetVibration((PlayerIndex)Index,left, right);
                vibFrames = vibing;
                vib = true;
            }
        }
        /// <summary>
        /// ラジアンで帰ってきます
        /// </summary>
        /// <param name="joyStick"></param>
        /// <param name="distanceFromNet"></param>
        /// <param name="rightLimit"></param>
        /// <param name="leftLimit"></param>
        /// <returns></returns>
        private float getAngleX(Vector2 joyStick, float distanceFromNet, float rightLimit,float leftLimit)
        {
            //コート手前か奥かで狙うところが違う
            //飛距離によって調整
            Vector2 right, left;
            if (ball.Position.Z > 0)
            {
                right = TennisCourt.RightUp;
                left = TennisCourt.LeftUp;
                right.Y = left.Y = -distanceFromNet;
            }
            else if (ball.Position.Z < 0)
            {
                right = TennisCourt.RightBelow;
                left = TennisCourt.LeftBelow;
                right.Y = left.Y = distanceFromNet;
            }
            else throw new Exception("エラー。Player.csを参照。");

            if (rightLimit < 0 || leftLimit < 0)
            {
                //throw new ArgumentException();
            }
            right.X *= (1-rightLimit);
            left.X *= (1-leftLimit);

            //角度をつける能力で狙える範囲を変更
            //right.X *= ability.Angle;
            //left.X *= ability.Angle;

            //joyStickの左右入力値によってショットの角度を決定。
            float maxAngleR, maxAngleL;
            maxAngleR = getAngle(right);// (float)Math.Atan((ball.Position.X - right.X) / (ball.Position.Z - right.Y));
            maxAngleL = getAngle(left);// (float)Math.Atan((ball.Position.X - left.X) / (ball.Position.Z - left.Y));
            //MathHelper.Lerpでつかうため、0~1の範囲にする
            float x = joyStick.X / 2.0f + 0.5f;
            //角度の決定
            float angleX = MathHelper.Lerp(maxAngleL, maxAngleR, x);
            return angleX;
        }

        /// <summary>
        /// 指定した点とボールの座標をつないだ線の角度を取得
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        float getAngle(Vector2 point)
        {
            return (float)Math.Atan((ball.Position.X - point.X) / (ball.Position.Z - point.Y));
        }

        /// <summary>
        /// サーブを打つ
        /// </summary>
        /// <param name="joyStick">ジョイスティックの入力</param>
        /// <param name="shotAngleY">射出角度(上下)</param>
        /// <param name="velocity">速度(km/h)</param>
        /// <param name="distanceFromNet">着地地点</param>
        /// <param name="side">デュースサイドかアドサイドか</param>
        /// <param name="spin">スピン量(縦)</param>
        public void Serve(ControllerState controlerState,CourtSide side)
        {
            Vector2 joyStick = controlerState.JoyStick;

            float depth = TennisCourt.ServiceAreaLength*0.9f;
            //Vector2 right=Vector2.Zero, left=Vector2.Zero;
            //if (ball.Position.Z > 0)
            //{
            //    right = TennisCourt.RightUp;
            //    left = TennisCourt.LeftUp;
            //    right.Y = left.Y = -depth;

            //    if (side == CourtSide.Deuse)
            //    {
            //        right.X = 0;
            //    }
            //    else
            //    {
            //        left.X = 0;
            //    }
            //}
            //else if (ball.Position.Z < 0)
            //{
            //    right = TennisCourt.RightBelow;
            //    left = TennisCourt.LeftBelow;
            //    right.Y = left.Y = depth;

            //    if (side == CourtSide.Deuse)
            //    {
            //        left.X = 0;
            //    }
            //    else
            //    {
            //        right.X = 0;
            //    }
            //}

            //float maxAngle = MathHelper.ToDegrees(getAngle(right));
            //float minAngle = MathHelper.ToDegrees(getAngle(left));

            float x = (joyStick.X + 1) / 2.0f;
            //float v = velocity * 1000 / 3600 / 60 * 0.27f;
            //ボタンによって打ち分け
            float speed = (0.2f + ability.ServePower * 0.8f)*Rate * 1000 * 0.27f / 3600 / 60;
            float limit = 0.01f;
            float rightLimit = side == CourtSide.Deuse ? 1+limit : limit;
            float leftLimit = side == CourtSide.Deuse ? limit : 1+limit;
            if (position.Z < 0)
            {
                float buf = rightLimit;
                rightLimit = leftLimit;
                leftLimit = buf;
            }
            if (controlerState.Button1 == ControlerButtonStates.Pressed)//スピンサアブ
            {
                //普通のサーブ
                //落ちはじめが強い
                //失敗するときはオーバーが多い
                //左に打ちにくい
                if (position.Z > 0)
                {
                    leftLimit += 0.1f;
                }
                else
                {
                    rightLimit += 0.1f;
                }
                //float angle = ball.GetSafetyAngle(HeadPosition.Y*3,depth,speed);
                //angle *= MathHelper.Lerp(1f, 0.5f, ability.ServePower);
                //低いとスピン量増加＋速度低下
                float spinFactor = 1 + (1-ball.Position.Y / hitBounds.Y);
                speed *= spinServeVmax;
                if (ball.Speed.Y > 0)
                    spinFactor = 1.0f / spinFactor;
                else
                    speed /= Math.Abs(spinFactor);

                depth /= (Math.Abs(spinFactor)*1.0f);
                ShotBall3(joyStick, depth, speed, new Vector3(255, 128, 0), ability.TopSpin * spinFactor,true,40,rightLimit,leftLimit);
                //ShotBall3(joyStick, depth, speed,new Vector3(0, 192, 128), spin*spinFactor,true,rightLimit,leftLimit);
                //ShotBall2(joyStick, angle, depth, speed,new Vector3(0,192,128),rightLimit,leftLimit);
            }
            else if (controlerState.Button2 == ControlerButtonStates.Pressed)//フラツトサアブ
            {
                //速いサーブ
                //失敗しやすい
                speed *= flatServeVmax;
                //depth *= MathHelper.Lerp(0.5f, 1.5f, (float)GameMain.Random.NextDouble());
                rightLimit += 0.05f;
                leftLimit += 0.05f;
                speed *= MathHelper.SmoothStep(0.25f, 1.0f, ball.Position.Y / HitBounds.Y);
                //ShotBall4(joyStick, depth, TennisCourt.NetHeight * 1.5f, speed, new Vector3(255, 128, 0), 1, rightLimit, leftLimit);
                ShotBall3(joyStick, depth, speed, new Vector3(255, 128, 0), -0.00001f, true,ability.MaxAngleY,rightLimit, leftLimit);
                //ShotBall2(joyStick, ball.GetMinAngle(depth)*0.7f, depth, speed,new Vector3(255,128,0),rightLimit,leftLimit);
            }
            else if (controlerState.Button4 == ControlerButtonStates.Pressed)//スライスサアブ
            {
                //遅い
                //失敗するときはネットが多い
                //右にうちにくい
                if(position.Z > 0)
                {
                    rightLimit += 0.1f;
                }
                else
                {
                    leftLimit += 0.1f;
                }
                speed *= sliceServeVmax;
                float hei = MathHelper.Lerp(0.5f, 1.0f, ball.Position.Y / HitBounds.Y);
                depth *= hei;
                speed *= hei;
                ShotBall3(joyStick, depth, speed, new Vector3(0, 255, 255), -ability.SliceSpin, true,ability.MaxAngleY,rightLimit, leftLimit);
                //ShotBall2(joyStick, angle, depth, speed,new Vector3(0,255,255),rightLimit,leftLimit);
            }
            onShotting(Shots.SERVE);
            //ball.CalcBuoyancy(v, spin);
            //ball.ShotByVelocity(new Vector2(MathHelper.Lerp(minAngle, maxAngle, x), shotAngleY), v, spin);
            //ball.Shot(new Vector2(MathHelper.Lerp(minAngle, maxAngle, x), shotAngleY), distanceFromNet, spin);
        }
        /// <summary>
        /// 決め球
        /// </summary>
        /// <param name="joyStick">ジョイスティックの価</param>
        /// <param name="distanceFromNet">ネットからの距離</param>
        /// <param name="velocity">速度(km/h)</param>
        public void Smash(Vector2 joyStick,float distanceFromNet)
        {
            //float l = (distanceFromNet + Math.Abs(ball.Position.Z));
            //if (l == 0)
            //    return;
            //float angleY = MathHelper.ToDegrees((float)Math.Atan(-ball.Position.Y / l));
            float spin = MathHelper.Lerp(-10, 3, (float)currentDelay / maxDelay);
            float velocity = (50 + 70 * (float)currentDelay / maxDelay + 20 * ability.ServePower).ResizeVelocity() * 1000 * Rate;
            //ball.ShotByVelocity(new Vector2(MathHelper.ToDegrees(getAngleX(joyStick,distanceFromNet)), angleY), velocity*1000 * 0.27f / 3600 / 60,0.001f);
            //ShotBall(joyStick,angleY,distanceFromNet);
            //ShotBall2(joyStick, ball.GetMinAngle(distanceFromNet + Math.Abs(ball.Position.Z)), distanceFromNet,velocity,new Vector3(255,128,0));
            ShotBall3(joyStick, distanceFromNet, velocity*Rate, new Vector3(255, 128, 0), spin,true,ability.MaxAngleY,0.25f,0.25f);
            //Jump(40);
            onShotting(Shots.SMASH);
        }
        //void smash2(Vector2 joyStick, float distanceFromNet,float velocity)
        //{
        //    ShotBall4(joyStick, distanceFromNet, TennisCourt.NetHeight * 1.5f, velocity, Vector3.Zero, 0.00001f, 0.3f, 0.3f);
        //    //Jump(40);
        //    onShotting(Shots.SMASH);
        //}
        /// <summary>
        /// 指定した高さだけジャンプする
        /// </summary>
        /// <param name="height">高さ</param>
        public void Jump(float height)
        {
            if (position.Y < 0)
                position.Y = 0;
            speed.Y = (float)Math.Sqrt(2 * Ball.Gravity * height);
        }
        public void Jump(int frames)
        {
            if (position.Y < 0)
                position.Y = 0;
            speed.Y = Ball.Gravity * frames / 2;
        }
        /// <summary>
        /// これを操作するコントローラのインデックス
        /// </summary>
        public int Index { get; set; }
        #endregion

        #region 更新
        public override void Update(GameTime gameTime)
        {
            //コントローラによる操作...TennisPlayer
            //controler.Update();
            Control(Scene.Controllers[Index].GetState());
            //↑外部で行うべきでは？

            //if (spin > 0 && GameMain.gamePadStates[0][0].DPad.Down == ButtonState.Pressed)
            //{
            //    spin--;
            //}
            //else if (spin < 100 && GameMain.gamePadStates[0][0].DPad.Up == ButtonState.Pressed)
            //{
            //    spin++;
            //}
            //GameMain.debugStr["spin"] = spin + "rot/sec";
            //Game1.debugStr = "spin = " + spin;
            foreach (ParticleElement ele in shotPoint.Elements)
            {
                Vector3 posi = new Vector3(Position.X, 0, Position.Z);
                ele.Position = posi + shotPointEmit;
            }

            //進行方向から速度を設定
            Func<float, float, float, float> interp = (v1, v2, amount) =>
            {
                return MathHelper.Lerp(v1, v2, amount);
                //return MathHelper.Lerp(v1, v2, amount);
            };
            float radian = bodyDirection.ToRadians();
            speed = new Vector3(interp(0, ability.MaxSpeed, Velocity) * (float)(Math.Sin(radian)), speed.Y, interp(0, ability.MaxSpeed, Velocity) * (float)(Math.Cos(radian)));
            
            base.Update(gameTime);

            if (position.Y < 0)
            {
                position.Y = 0;
                speed.Y = 0;
            }
            speed.Y -= Ball.Gravity;
            //ショットエリアの表示
            shotMark.FinalColor = new Vector3(255, 255, 255);

            if (Hit(ball) && ball.Speed.Z * position.Z > 0)
            {
                if (GameMain.TotalFrames % 4 > 2)
                    shotMark.FinalColor = new Vector3(255, 0, 0);
            }
            //shotMark.Reset();
            
            Vector3 posiXZ = new Vector3(position.X, 0, position.Z);
            shotMark.EmitPoint = posiXZ + new Vector3(-hitBounds.X / 2, 0.001f, 0);
            shotMark.Rotation = 0;
            shotMark.Emit();
            shotMark.EmitPoint = posiXZ + new Vector3(hitBounds.X / 2, 0.001f, 0);
            shotMark.Rotation = 180;
            shotMark.Emit();
            GameMain.debugStr["shotMark"] = "" + shotMark.Elements.Count;


            //スマッシュポイントの表示
            //smashPoint.Reset();
            //smashPoint2.Reset();
            Vector3 point = ball.GetPoint((SmashableHeight+hitBounds.Y) / 2);

            //スマッシュ可能な高さ(最低点)に達するまでの時間
            int framesMax = ball.GetFrames(SmashableHeight) - (int)(Delay*1.5f);
            //スマッシュ可能な高さ(最高点)に達するまでの時間
            int framesMin = ball.GetFrames(hitBounds.Y) - (int)(Delay*1.5f);
            //if (point != Vector3.Zero && ball.Bounds == 0 && framesMax > 0 && position.Z * ball.Speed.Z > 0 && point.Z * ball.Speed.Z > 0
            //    && new Vector2(ball.Speed.X,ball.Speed.Z).Length() < SmashableSpeed)
            if(HasSmashPoint)
            {
                smashPoint.EmitPoint = smashPoint2.EmitPoint = point + new Vector3(0, 0.0027f, 0);

                //タイミング取る用の円
                smashPoint2.Scale = smashPoint.Scale;
                float val = 1;
                float sec = 1;
                for (int i = 0; i < 2; i++)
                {
                    val *= framesMax;
                    sec *= 60.0f;
                }
                smashPoint2.Scale *= MathHelper.Lerp(1, 2, MathHelper.Clamp(val, 0, sec) / sec);
                //打てるタイミングで色を変える
                Vector3 yet = new Vector3(0,255,255);
                Vector3 yes = new Vector3(255, 0, 0);
                if (framesMin < 0)
                {
                    smashPoint.FinalColor = smashPoint2.FinalColor = smashPoint.InitialColor = smashPoint2.InitialColor = yes;
                }
                else
                    smashPoint.FinalColor = smashPoint2.FinalColor = smashPoint.InitialColor = smashPoint2.InitialColor = Vector3.Lerp(yes, yet, MathHelper.Clamp(val, 0, sec) / sec);
                // *MathHelper.Lerp(1, 3, MathHelper.Clamp(framesMax, 0, 120) / 120.0f);
                //smashPoint2.Rotation = frames2;
                smashPoint2.Emit();
                //中心の円
                smashPoint.Rotation = -framesMax*3;
                smashPoint.Emit();
            }
            GameMain.debugStr["ball.GetFrames(SmashableHeight)"] = "" + ball.GetFrames(SmashableHeight);

            //振動に関する処理
            if (vib && vibFrames-- <= 0)
            {
                GamePad.SetVibration((PlayerIndex)Index, 0, 0);
                vibFrames = vibing;
                vib = false;
            }
        }
        public override void Draw(GameTime gameTime)
        {
            sliding.SetView(camera.Position, camera.Target, camera.Up);
            sliding.SetProjection(camera.Projection);
            smashPoint.SetView(camera.Position, camera.Target, camera.Up);
            smashPoint.SetProjection(camera.Projection);
            smashPoint2.SetView(camera.Position, camera.Target, camera.Up);
            smashPoint2.SetProjection(camera.Projection);
            shotMark.SetView(camera.Position, camera.Target, camera.Up);
            shotMark.SetProjection(camera.Projection);
            shotPoint.SetView(camera.Position, camera.Target, camera.Up);
            shotPoint.SetProjection(camera.Projection);
            swing.SetView(camera.Position, camera.Target, camera.Up);
            swing.SetProjection(camera.Projection);
            base.Draw(gameTime);
        }
        
        #endregion
    }
}
