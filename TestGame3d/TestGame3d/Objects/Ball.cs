using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VisualEffects;
using Extention;

namespace Tennis01.Objects
{
    using Scenes;
    enum ShotAngles
    {
        Low,
        High
    }
    class Ball:HittableObject3D
    {
        #region フィールド
        //0.27 = 1mの世界である
        //60 = 1秒の世界である
        /// <summary>
        /// ボールに働く揚力
        /// </summary>
        float buoyancy;
        /// <summary>
        /// ボールに働く抗力
        /// </summary>
        //float drag;
        /// <summary>
        /// 現在の回転速度(回転数/秒)
        /// </summary>
        float spin;
        /// <summary>
        /// ボールに働く重力加速度(一定)
        /// </summary>
        public static readonly float Gravity = 0.27f*9.80665f/3600;
        /// <summary>
        /// ボールへの加速度
        /// </summary>
        Vector3 accel;
        /// <summary>
        /// バウンド係数
        /// </summary>
        Vector2 boundFactor;
        /// <summary>
        /// 現在のバウンド回数
        /// </summary>
        int bounds;
        /// <summary>
        /// 弾道のエフェクト
        /// </summary>
        Particle fire=null;
        /// <summary>
        /// ショットされた時のエフェクト
        /// </summary>
        Particle shotEffect;
        /// <summary>
        /// バウンドした時のエフェクト
        /// </summary>
        Particle boundEffect;
        /// <summary>
        /// 影
        /// </summary>
//        Particle shadow;
        /// <summary>
        /// 着弾予想地点
        /// </summary>
        Particle boundPoint;
        /// <summary>
        /// 着弾予想などに必要な変数
        /// </summary>
        Vector2 lastShotAngle;
        float v0;
        Vector3 lastPosition;

        #endregion
        Vector3 iniPosition;
        TennisCourt tennisCourt;
        public Ball Copy(Camera c)
        {
            return new Ball(Model, c, iniPosition, Vector3.Zero, tennisCourt);
        }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="m">ボールのモデル</param>
        /// <param name="position">初期位置</param>
        /// <param name="spd">初期速度</param>
        /// <param name="boundFactor">バウンド係数(進行方向、Y軸方向)</param>
        public Ball(Model m, Camera c,Vector3 position, Vector3 spd,TennisCourt tennisCourt)
            : base(m, c,null,position, spd, Vector3.Zero,null,"ObjectSEs")
        {
            this.boundFactor = tennisCourt.BoundFactor;
            iniPosition = position;
            this.tennisCourt = tennisCourt;
            //tennisCourt = court;
            fire = Particle.CreateFromParFile("./ParFiles/ballLoc.par");// new Particle(c, "fireB", position, Vector3.Up, 0.1f, 40);
            boundEffect = Particle.CreateFromParFile("./ParFiles/sand.par");// new Particle(c, new ParticleParams(Game1.Textures["sand"], Vector3.Zero, 0.01f, Vector3.Up, MathHelper.PiOver4, new Vector3(0, -Ball.Gravity, 0), 0.27f / 50, 0.27f / 50, 30, 50));
            shotEffect = Particle.CreateFromParFile("./ParFiles/shotEffect.par");
            //shadow = Particle.CreateFromParFile("./ParFiles/shadow.par");
            boundPoint = Particle.CreateFromParFile("./ParFiles/shadow.par");
            //smashPoint = new Particle(boundPoint.Parameters);

            fire.Scale = Radius*4;

            boundEffect.Scale = 0.27f * 0.5f;
            boundEffect.Script = (p) =>
            {
                float minY = 0.001f;
                if (p.Position.Y <= minY)
                {
                    p.Position = new Vector3(p.Position.X, minY, p.Position.Z);
                    p.Speed = Vector3.Zero;
                    //p.Normal = Vector3.Normalize(new Vector3(0, 1, 0.000000001f));
                }
            };
            //boundEffect.InitialColor = boundEffect.FinalColor = new Vector3(128, 128, 0);

            shotEffect.Scale = 0.27f;

            //shadow.FinalColor = Vector3.Zero;
            //shadow.Scale = Radius * 4;
            //shadow.Normal = Vector3.Normalize(new Vector3(0, 1, 0.001f));

            boundPoint.Scale = Radius * 16;
            boundPoint.FinalColor = new Vector3(255, 0, 0);
            boundPoint.Normal = Vector3.Normalize(new Vector3(0, 1, 0.001f));

            //smashPoint.Scale = Radius * 8;
            //smashPoint.FinalColor = new Vector3(255, 255, 0);
            //smashPoint.Normal = Vector3.Normalize(new Vector3(0, 1, 0.001f));

            initShadow(Radius * 4);
            //float size = 0.2f;
            //float height = 10;
            //particle = new Particle(Game1.Textures["fire"], c, position,0.2f*size, Vector3.Up, MathHelper.ToRadians(10), Vector3.Zero, 1*size, 0.02f*size, (int)height, 1);
            ReachedToNet = true;
            IsNet = false;
        }

        /// <summary>
        /// ボールを指定位置に移動してから初期化
        /// </summary>
        /// <param name="position"></param>
        public void Init(Vector3 pos,Vector3? spd = null)
        {
            position = pos;
            speed = spd ?? Vector3.Zero;
            bounds = 0;
            spin = 0;
            buoyancy = 0;
            accel = new Vector3(0, Gravity+buoyancy,0);
            v0 = 0;
            lastPosition = Vector3.Zero;
            lastShotAngle = Vector2.Zero;
            BoundedPoint = Vector3.Zero;
            BoundPoint = Vector3.Zero;
            LastHitter = null;
            IsNet = false;
            ReachedToNet = true;
            TouchedNet = false;
            fire.Reset();
            //drag = 0;
        }
        public void AddToComponents(Scene s,bool assist)
        {
            s.AddObjects(this);
            s.AddComponents(BoundEffect,ShotEffect);
            if (assist)
            {
                s.AddComponents(BoundPointEffect,Fire);
            }
        }
        /// <summary>
        /// ボールに、その位置から指定した高さだけ上がるようにY速度を与える
        /// </summary>
        /// <param name="height">投げ上げる高さ</param>
        public void Toss(float height)
        {
            if (position.Y < Radius)
                position.Y = Radius;
            speed.Y = (float)Math.Sqrt(2 * Gravity * height);
        }

        /// <summary>
        /// ボールが上がって、framesフレーム後に元の高さに戻るようにY速度を与える
        /// </summary>
        /// <param name="frames">元の高さに戻るまでの時間</param>
        public void Toss(int frames)
        {
            if (position.Y < Radius)
                position.Y = Radius;
            speed.Y = Gravity * frames / 2;
        }

        #region ショット
        /// <summary>
        /// 球を指定した角度と飛距離と重力で打ち出す
        /// </summary>
        /// <param name="shotAngle">打ち出す角度(Degree)。Xは横方向、Yは縦方向</param>
        /// <param name="distance">Z軸上の飛距離</param>
        /// <param name="spin">球の回転速度(回転数/秒)</param>
        public void ShotByDistance(Vector2 shotAngle, float distance,float spin = 0)
        {
            //重力、揚力を登録
            //this.Gravity = Gravity;
            this.spin = spin;
            if (Gravity <= 0)
            {
                throw new ArgumentException("重力以上の揚力を設定することはできません");
            }
            bounds = 0;
            //座標の補正
            if (position.Y < Radius)
                position.Y = Radius;


            float v0 = GetShotVelocity(shotAngle, distance, spin);
            ShotByVelocity(shotAngle, v0, spin);

            //float t = 2 * v0 / Gravity;
            //v0 = (float)Math.Sqrt(buf1 / buf2);
            //v0 += t*drag;

            ////速度の決定Y
            //speed.Y = (float)(v0 * Math.Sin(shotAngle.Y));

            ////速度の決定Z
            //speed.Z = (float)(v0 * Math.Cos(shotAngle.Y));
            //if (position.Z > 0)
            //{
            //    speed.Z = -speed.Z;
            //}
            //speed.X = (float)(speed.Z * Math.Tan(shotAngle.X));


            //着弾予定の座標を格納
            float d = distance;
            if (position.Z > 0)
                d = -d;
            BoundPoint = new Vector3((float)(position.X + Math.Tan(MathHelper.ToRadians(shotAngle.X)) * d),0, (float)(position.Z + d));
        }

        /// <summary>
        /// ネットからの飛距離を指定して打ち出す
        /// </summary>
        /// <param name="shotAngle"></param>
        /// <param name="distanceFromNet"></param>
        /// <param name="spin"></param>
        public void Shot(Vector2 shotAngle, float distanceFromNet, float spin = 0)
        {
            ShotByDistance(shotAngle, distanceFromNet + Math.Abs(position.Z),spin);
        }


        /// <summary>
        /// 球を指定した角度と初速と重力で打ち出す
        /// </summary>
        /// <param name="shotAngle">打ち出す角度(Degree)。Xは横方向、Yは縦方向</param>
        /// <param name="velocity">初速度</param>
        /// <param name="spin">球に働く揚力</param>
        public void ShotByVelocity(Vector2 shotAngle, float velocity, float spin = 0)
        {
            //重力、揚力を登録
            //this.Gravity = Gravity;
            this.spin = spin;
            if (Gravity <= 0)
            {
                throw new ArgumentException("重力以上の揚力を設定することはできません");
            }
            bounds = 0;

            //座標の補正
            if (position.Y < Radius)
                position.Y = Radius;

            //角度をラジアンに変換
            shotAngle.X = MathHelper.ToRadians(shotAngle.X);
            shotAngle.Y = MathHelper.ToRadians(shotAngle.Y);
            //速度の決定
            speed.Y = (float)(velocity * Math.Sin(shotAngle.Y));
            speed.Z = (float)(velocity * Math.Cos(shotAngle.Y));
            if (position.Z > 0)
                speed.Z = -speed.Z;
            speed.X = (float)(speed.Z * Math.Tan(shotAngle.X));

            //ショット角度、初速、座標を登録
            setParams(shotAngle, velocity, position);

            float d;
            float A = (float)(2 * velocity * velocity * Math.Cos(shotAngle.Y) * Math.Cos(shotAngle.Y));
            d = (float)(A*Math.Tan(shotAngle.Y)+Math.Sqrt(A*A*Math.Tan(shotAngle.Y)*Math.Tan(shotAngle.Y)+4*(Gravity+buoyancy)*A*position.Y)) / (2*(Gravity+buoyancy));
            if (position.Z > 0)
                d = -d;
            BoundPoint = new Vector3((float)(position.X + Math.Tan(shotAngle.X) * d), 0, (float)(position.Z + d));
        }

        public void ShotByVelocityAndDistance(Vector2 shotAngle, float velocity,float distanceFromNet)
        {
            float angleY = MathHelper.ToRadians(shotAngle.Y);
            float d = distanceFromNet + Math.Abs(position.Z);
            if (d == 0)
                throw new ArgumentException();
            double g = 2*velocity*velocity*Math.Cos(angleY)*Math.Cos(angleY)*(Math.Tan(angleY)*d + position.Y) / (d*d);
            if (g <= 0)
                throw new Exception();
            buoyancy = (float)g - Gravity;
            //スピンを自動計算(追加)
            calcSpin(velocity);
            ShotByVelocity(shotAngle, velocity, this.spin);
        }
        public void ShotWithoutAngleY(float shotAngleX,float velocity,float distanceFromNet,float spin,ShotAngles angle,float maxAngle)
        {
            if (distanceFromNet == 0)
                throw new ArgumentException();
            //スピンによる浮力計算
            calcBuoyancy(velocity, spin);
            //射出角度決定
            float? angleY = getShotAngleY(velocity, distanceFromNet, 0,angle == ShotAngles.Low);
            if (angleY == null || (float)angleY > maxAngle)
            {
                angleY = maxAngle;
            }
            Vector2 shotAngle = new Vector2(shotAngleX, (float)angleY);
            ShotByVelocity(shotAngle, velocity, spin);
        }
        /// <summary>
        /// 指定したパラメータで打ち出す時に最適な射出角度を取得
        /// </summary>
        /// <param name="velocity"></param>
        /// <param name="distanceFromNet"></param>
        /// <param name="y"></param>
        /// <returns>射出角度(届かない場合はnull)</returns>
        float? getShotAngleY(float velocity,float distanceFromNet,float y,bool low)
        {
            float distance = Math.Abs(position.Z) + distanceFromNet;
            //calcBuoyancy(velocity, 0);
            float A = (Gravity + buoyancy) * distance * distance / (2 * velocity * velocity);
            float b = -distance;
            float c = -position.Y + y + A;
            float d = b * b - 4 * A * c;
            if (d < 0)
            {
                return null;
            }
            int mark = low ? -1 : 1;
            float res = MathHelper.ToDegrees(
                (float)Math.Atan((-b + mark * Math.Sqrt(d)) / (2*A))
            );
            //System.Windows.Forms.MessageBox.Show("" + res);
            return res;
        }

        public void ShotByNetHeight(float shotAngleX,float velocity,float height,float spin)
        {
            calcBuoyancy(velocity, spin);
            float angleY = getShotAngleY(velocity, 0, height,true) ?? 45;
            ShotByVelocity(new Vector2(shotAngleX, angleY), velocity, spin);
        }
        /// <summary>
        /// ネットを越えられる射出角度を取得
        /// </summary>
        /// <param name="height">ネットのどれくらいの高さのところを狙うか</param>
        /// <returns></returns>
        public float GetSafetyAngle(float height, float depth, float velocity)
        {
            //float res = MathHelper.ToDegrees((float)Math.Atan((height - position.Y) / (float)Math.Abs(position.Z)));
            float res = getShotAngleY(velocity, 0, height,true) ?? 45;
            float min = GetMinAngle(depth);
            if (res < min)
            {
                //res = 15;
                res = min / 10;
            }
            return res;

        }
        /// <summary>
        /// ショットの瞬間に設定する必要のある値をセット
        /// </summary>
        /// <param name="shotAngles"></param>
        /// <param name="v0"></param>
        /// <param name="lastPosition"></param>
        void setParams(Vector2 shotAngles, float v0,Vector3 lastPosition)
        {
            this.lastShotAngle = shotAngles;
            this.v0 = v0;
            this.lastPosition = lastPosition;
            TouchedNet = false;
        }
        /// <summary>
        /// 放物線の式から高さyとなるXZ座標を取得
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public Vector3 GetPoint(float y)
        {
            double a = -(Gravity+buoyancy) / (2*v0*v0*Math.Cos(lastShotAngle.Y)*Math.Cos(lastShotAngle.Y));
            double b = Math.Tan(lastShotAngle.Y);
            double c = lastPosition.Y - y;
            double d = b * b - 4 * a * c;
            //d < 0ではその点は存在しない
            if (d < 0)
                return Vector3.Zero;
            double x1 = (-b + Math.Sqrt(b * b - 4 * a * c)) / (2 * a);
            double x2 = (-b - Math.Sqrt(b * b - 4 * a * c)) / (2 * a);

            float x = Math.Max((float)x1, (float)x2);
            if (Speed.Z < 0)
                x = -x;
            Vector3 res = new Vector3((float)(lastPosition.X + Math.Sin(lastShotAngle.X) * x), 0, (float)(lastPosition.Z + x*Math.Cos(lastShotAngle.X)));
            
            return res;
        }
        /// <summary>
        /// 指定した高さの点に達するまでのフレーム数
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public int GetFrames(float y)
        {
            Vector3 p = GetPoint(y);
            if (p == Vector3.Zero)
                return -1;
            Vector2 point = new Vector2(p.X, p.Z);
            Vector2 posi = new Vector2(position.X, position.Z);
            Vector2 spd = new Vector2(speed.X, speed.Z);

            int res = (int)((point - posi).Length() / spd.Length());
            if ((point - posi).Y * spd.Y < 0)
                res = -res;
            return res;
            
        }
        #endregion
        #region ショットに必要な計算
        /// <summary>
        /// 揚力を計算する
        /// </summary>
        /// <param name="velocity">球の速度</param>
        /// <param name="spin">球の回転量(回転数/秒)</param>
        /// <returns>揚力</returns>
        private void calcBuoyancy(float velocity, float spin)
        {
            //揚力による加速度計算
            float v = velocity / 0.27f * 100 * 60;//現実世界での速度(cm/s)
            float ballD = 6.7f;//現実世界での球の大きさ(cm)
            float ballWeight = 0.0000589f;//現実世界でのボールの質量(kgf*s^2/cm)
            float p = 0.000000001208f;//現実世界での空気密度らしい(kgf*s^2/cm^4)
            //ボールに働く揚力Fを計算
            float F = MathHelper.Pi * MathHelper.Pi / 8 * ballD * ballD * ballD * v * spin * p;
            //F = m*aよりa = F/m
            float a = F / ballWeight;
            //単位cm/s^2をm/framesに直す?
            //return a / 100 * 0.27f / 3600;
            buoyancy = a/100*0.27f/3600;
            //buoyancy = MathHelper.Pi * MathHelper.Pi / 8 * ballD * ballD * ballD * velocity  * spin * 0.000000001208f/0.0000589f/3600;
        }
        //追加
        void calcSpin(float velocity)
        {
            float v = velocity / 0.27f * 100 * 60;//現実世界での速度(cm/s)
            float ballD = 6.7f;//現実世界での球の大きさ(cm)
            float ballWeight = 0.0000589f;//現実世界でのボールの質量(kgf*s^2/cm)
            float p = 0.000000001208f;//現実世界での空気密度らしい(kgf*s^2/cm^4)

            float a = buoyancy * 100 / 0.27f * 3600;
            float F = a * ballWeight;
            float s = F / MathHelper.Pi / MathHelper.Pi * 8 / ballD / ballD / ballD / v / p;
            spin = s;
        }
        /// <summary>
        /// 抗力を計算する
        /// </summary>
        /// <param name="velocity">球の速度</param>
        /// <returns>抗力</returns>
        private float CalcDrag(float velocity)
        {
            float p = 0.000000001208f;//現実世界での空気密度らしい(kgf*s^2/cm^4)
            float v = velocity / 0.27f * 100 * 60;//現実世界での速度(cm/s)
            float ballWeight = 0.0000589f;//現実世界でのボールの質量(kgf*s^2/cm)
            float F = 0.5f * 0.5f * p * v * v * 35.26f;
            float a = F / ballWeight;
            return a / 100 * 0.27f / 3600;
        }
        /// <summary>
        /// 指定した距離に、指定した角度と回転で飛ばすのに適した初速を計算
        /// </summary>
        /// <param name="shotAngle"></param>
        /// <param name="distance"></param>
        /// <param name="spin"></param>
        /// <returns></returns>
        public float GetShotVelocity(Vector2 shotAngle, float distance, float spin = 0)
        {
            //角度をラジアンに変換
            shotAngle.X = MathHelper.ToRadians(shotAngle.X);
            shotAngle.Y = MathHelper.ToRadians(shotAngle.Y);

            //初速を決める
            float buf1 = Gravity * distance * distance;
            float buf2 = (float)(2 * Math.Cos(shotAngle.Y) * Math.Cos(shotAngle.Y) * (position.Y + distance * Math.Tan(shotAngle.Y)));//(float)Math.Sqrt((distance*Math.Tan(shotAngleY)-position.Y)*gravity/(Math.Sin(shotAngleY*2)*Math.Tan(shotAngleY)));
            if (buf1 <= 0)
                throw new Exception("ボールの速度設定がおかしくなった。Ball.cs参照。\nbuf1 = "+buf1+", buf2 = "+buf2);
            if (buf2 < 0)
                buf2 = 1;
            float v0 = (float)(Math.Sqrt(buf1/buf2));
            GameMain.debugStr["v0"] = (v0 / 0.27f * 60 * 3600 / 1000) + "km/h";

            //揚力
            calcBuoyancy(v0, spin);
            //抗力
            //drag = CalcDrag(v0);
            //float t = 2 * v0 / Gravity;

            if (buoyancy+Gravity < 0)
            {
                //buoyancy = 0;
                //System.Windows.Forms.MessageBox.Show("Ball.cs");
                buoyancy = -Gravity / 2;
                //throw new Exception("ボールの速度設定がおかしくなった2。Ball.cs参照。\nbuoyancy + Gravity= " + (buoyancy+Gravity));
            }
            //初速計算し直し
            buf1 = (Gravity + buoyancy) * distance * distance;
            
            return (float)(Math.Sqrt(buf1 / buf2));
        }
        #endregion

        #region プロパティ
        //ボールがバウンドしているかどうか
        public bool IsBounding { get { return speed.Y != 0; } }
        //protected override Vector3 Center
        //{
        //    get
        //    {
        //        return position + new Vector3(0, Radius, 0);
        //    }
        //}
        /// <summary>
        /// 最初に着弾する予定の座標
        /// </summary>
        public Vector3 BoundPoint { get; private set; }
        /// <summary>
        /// 最近着弾した座標
        /// </summary>
        public Vector3 BoundedPoint { get; private set; }
        /// <summary>
        /// 最初にバウンドした場所
        /// </summary>
        public Vector3 FirstBoundedPoint { get; private set; }
        /// <summary>
        /// バウンド回数を取得
        /// </summary>
        public int Bounds { get { return bounds; } }
        /// <summary>
        /// バウンドした瞬間の速度
        /// </summary>
        public Vector3 BoundVelocity { get; private set; }
        /// <summary>
        /// バウンド回数を１増やす
        /// </summary>
        public void Bound(Vector3 boundedPoint)
        {
            bounds++;
            //BoundedPoint = boundedPoint;
        }
        /// <summary>
        /// ボールの速度(km/h)
        /// </summary>
        public float SpeedKPH { get { return (speed.Length() / 0.27f * 60 * 3600 / 1000); } }
        /// <summary>
        /// アウトボールであったかどうか
        /// </summary>
        /// <returns></returns>
        public bool IsOut(TennisEvents tennisEvents)
        {
            if (tennisEvents == TennisEvents.Singles)
                return Math.Abs(BoundedPoint.X) > TennisCourt.SinglesWidth / 2 || Math.Abs(BoundedPoint.Z) > TennisCourt.CourtLength;
            else if(tennisEvents == TennisEvents.Doubles)
                return Math.Abs(BoundedPoint.X) > TennisCourt.DoublesWidth / 2 || Math.Abs(BoundedPoint.Z) > TennisCourt.CourtLength;

            return false;
        }
        public bool IsInServiceCourt(CourtSide side)
        {
            if (Math.Abs(BoundedPoint.Z) > TennisCourt.ServiceAreaLength)
                return false;
            else
            {
                if (Math.Abs(BoundedPoint.X) < TennisCourt.SinglesWidth / 2)
                {
                    if (side == CourtSide.Deuse)
                    {
                        return BoundedPoint.Z * BoundedPoint.X >= 0;
                    }
                    else
                    {
                        return BoundedPoint.Z * BoundedPoint.X <= 0;
                    }
                }
                return false;
            }
        }
        public bool TouchedNet { get; private set; }
        public bool ReachedToNet { get; private set; }
        /// <summary>
        /// ネットにかかったかどうか
        /// </summary>
        public bool IsNet { get; private set; }
        
        /// <summary>
        /// ネットを越えられる安全な射出角度(度数)
        /// </summary>
        //public float NetAngle
        //{ 
        //    get 
        //    {
        //        if (position.Z == 0) throw new InvalidOperationException();
        //        return MathHelper.ToDegrees((float)Math.Atan((TennisCourt.NetHeight - position.Y) / (float)Math.Abs(position.Z)));
        //    } 
        //}

        /// <summary>
        /// 着地地点に到達させるための最低角度を取得
        /// </summary>
        public float GetMinAngle(float distanceFromNet)
        {
            float dis = Math.Abs(position.Z) + distanceFromNet;
            if (dis == 0)
                throw new ArgumentException();
            return MathHelper.ToDegrees((float)Math.Atan(-position.Y / dis));
        }
        /// <summary>
        /// パーティクルのインスタンス
        /// </summary>
        public Particle Fire { get { return fire; } }
        public Particle BoundEffect { get { return boundEffect; } }
        public Particle ShotEffect { get { return shotEffect; } }
        //public Particle ShadowEffect { get { return shadow; } }
        public Particle BoundPointEffect { get { return boundPoint; } }
        //public Particle SmashPointEffect { get { return smashPoint; } }
        
        /// <summary>
        /// このボールを最後に打ったプレイヤー
        /// </summary>
        public Player LastHitter { get; set; }
        public Ray Ray
        {
            get
            {
                return new Ray(position, Vector3.Normalize(speed));
            }
        }
        #endregion

        #region エフェクト関連
        /// <summary>
        /// ボールのParticleエフェクトを変更する
        /// </summary>
        /// <param name="p">新しいParticle</param>
        public void SetFire(Particle p)
        {
            fire = p;
        }
        /// <summary>
        /// ボールのエフェクトのテクスチャを変更する
        /// </summary>
        /// <param name="texture"></param>
        public void SetFireTexture(Texture2D texture)
        {
            fire.InitialTexture = texture;
        }
        /// <summary>
        /// ボールのエフェクトの色を変更する
        /// </summary>
        /// <param name="iniColor"></param>
        /// <param name="finColor"></param>
        public void SetFireColors(Vector3? iniColor, Vector3? finColor)
        {
            if(iniColor != null)
                fire.InitialColor = (Vector3)iniColor;
            if(finColor != null)
                fire.FinalColor = (Vector3)finColor;
        }

        /// <summary>
        /// 打った時のエフェクトの色を変更する
        /// </summary>
        /// <param name="color"></param>
        public void SetShotEffectColor(Vector3 color)
        {
            shotEffect.InitialColor = color;
        }
        /// <summary>
        /// ボールのバウンドした時のエフェクトを変更する
        /// </summary>
        /// <param name="p"></param>
        //public void SetBoundParticle(Particle p)
        //{
        //    boundEffect = p;
        //}
        ///// <summary>
        ///// 爆発四散。
        ///// </summary>
        //public void Explode()
        //{
        //    ParticleParams buf = fire.Parameters;
        //    //fire.Parameters = Particle.GetExplosionParticle(Game1.Textures["fire"], camera, position, 0.1f);

        //    fire.Emit();
        //    ShotByVelocity(new Vector2(0, 90), 0.06f);
        //    fire.Parameters = buf;
        //    this.Dispose();
        //}
        //public void BlowFire()
        //{
        //    fire.Blow(new Vector3(0.27f/60*5, 0, 0));
        //}
        /// <summary>
        /// 火花出すだけ。
        /// </summary>
        public void ExplodeLittle()
        {
            shotEffect.EmitPoint = position;
            shotEffect.Scale = 0.27f * 3 * SpeedKPH / 200;
            shotEffect.Direction = -Vector3.Normalize(new Vector3(speed.X,0,speed.Z));
            Vector3 color;
            if (spin > 15)
                color = new Vector3(255,64,0);
            else if (spin < -15)
                color = new Vector3(0,128,255);
            else
                color = new Vector3(128,96,128);
            SetShotEffectColor(color);
            
            shotEffect.Emit();
        }
        #endregion
     
        #region 更新と描画
        public override void Update(GameTime gameTime)
        {
            //ボールに働く加速度
            accel = new Vector3(0,-Gravity-buoyancy,0);
            //if (isNetIn(TennisCourt.NetHeight-Radius))
            //{

            //}
            if (isNet())
            {
                if (Center.Y <= TennisCourt.NetHeight)
                {
                    if (speed.Z < 0)
                    {
                        position.Z = Radius + TennisCourt.NetZ / 2;
                    }
                    else if (speed.Z > 0)
                    {
                        position.Z = -Radius - TennisCourt.NetZ / 2;
                    }
                    speed.X /= 10;
                    speed.Z = 0;
                    speed.Y = 0;
                    spin = 0;
                    IsNet = true;
                }
                else
                {
                    spin *= 0.3f;
                    speed.X *= 0.3f;
                    speed.Z *= 0.3f;
                    Toss(0.27f * 0.3f);
                    TouchedNet = true;
                }
                calcBuoyancy(speed.Length(), spin);
            }
            GameMain.debugStr["isNet"] = IsNet + "";
            GameMain.debugStr["touchedNet"] = TouchedNet + "";

            GameMain.debugStr["ball.Center"] = "" + Center;
            //バウンド
            if (Position.Y < Radius)
            {
                if (bounds == 0)
                {
                    BoundedPoint = position;
                    if (BoundedPoint.Z * Speed.Z >= 0)
                    {
                        ReachedToNet = true;
                    }
                    else
                        ReachedToNet = false;
                }
                bound(Gravity * 5, 0.0001f);
            }
            else if (Position.Y != Radius)
            {
                //加速度...
                //x...横変化球
                //y...トップ、スライスなどによる縦変化、重力
                //z...追い風向かい風など？
                speed += accel;
                //重力で変化
                //speed.Y -= Gravity;
                //揚力で変化
                //buoyancy = CalcBuoyancy(speed.Length(), spin);
                //speed.Y -= buoyancy;
                //抗力で変化
                //drag = CalcDrag(speed.Length());
                //Vector3 dragVec = Vector3.Normalize(speed);
                //dragVec.Y = 0;
                //speed -= dragVec * drag;
                if (Position.Y < Radius)
                    position.Y = Radius;
            }
            else
            {
                spin = speed.Length()*60*60;
                speed *= 0.99f;
            }
            //テニスコートの壁に当たったら跳ね返る
            //foreach (HitVolume h in tennisCourt.Walls)
            //{
            //    if (HitVolume.Hit(h))
            //    {
            //        speed = Vector3.Reflect(speed, h.Normal);
            //    }
            //}

            //煙
            if (fire != null && Bounds < 2  && Scenes.Scene.Game.Components.Contains(fire))
            {
                if (bounds == 0 || BoundVelocity.Y / Speed.Y > 0)
                {
                    if (bounds == 0)
                    {
                        float maxSpin = 30;
                        Vector3 color = Vector3.Lerp(new Vector3(0, 64, 255), new Vector3(255, 64, 0), MathHelper.Clamp((spin + maxSpin) / (maxSpin * 2), 0, 1));
                        //SetShotEffectColor(color);
                        fire.FinalColor = fire.InitialColor = color;
                        fire.InitialVelocity = 0;
                    }
                    
                    if (speed != Vector3.Zero)
                    {
                        fire.Direction = Vector3.Normalize(-speed);
                    }
                    else
                    {
                        fire.Direction = Vector3.Up;
                    }
                    fire.EmitPoint = Center;
                    fire.Emit();

                    //fire.Direction = Vector3.Up;
                }
                //else
                //{
                //    fire.Reset();
                //}
            }

            //showShadow();
            showBoundPoints();
            
            //particle.Update(gameTime);
            GameMain.debugStr["ball.Speed"] = "" + speed;
            GameMain.debugStr["ballSpeed"] = (speed.Length()/0.27f*60*3600/1000) + "km/h";
            GameMain.debugStr["ballSpeedXZ"] = (new Vector2(speed.X,speed.Z).Length()/0.27f * 60 * 3600 / 1000) + "km/h";
            GameMain.debugStr["ballSpin"] = spin + "rot/sec";
            GameMain.debugStr["ballBounds"] = Bounds + "";
            base.Update(gameTime);
        }
        //private void showShadow()
        //{
        //    //影表示
        //    shadow.EmitPoint = new Vector3(position.X, 0.0027f, position.Z);
        //    shadow.SetView(camera.Position, camera.Target, camera.Up);
        //    shadow.SetProjection(camera.Projection);
        //    shadow.Emit();
        //}
        private void showBoundPoints()
        {
            if (BoundPoint != Vector3.Zero)
            {
                boundPoint.EmitPoint = BoundPoint + new Vector3(0, 0.0027f, 0);
                boundPoint.Emit();
                GameMain.debugStr["boundPoint.Count"] = boundPoint.Elements.Count+"";
                //fire.EmitPoint = BoundedPoint;
                //fire.Emit();
            }
        }
        protected override void DrawModel(GameTime gameTime, Matrix view, Matrix projection)
        {
            if (speed.Z > 0)
                rotation.X += MathHelper.ToRadians(MathHelper.Clamp(spin / 60.0f*360.0f,-60,60));
            else
                rotation.X -= MathHelper.ToRadians(MathHelper.Clamp(spin / 60.0f*360.0f,-60,60));

            base.DrawModel(gameTime, view, projection);

            fire.SetView(camera.Position, camera.Target, Vector3.Up);
            fire.SetProjection(camera.Projection);
            boundPoint.SetView(camera.Position, camera.Target, camera.Up);
            boundPoint.SetProjection(camera.Projection);
            boundEffect.SetView(camera.Position, camera.Target, Vector3.Up);
            boundEffect.SetProjection(camera.Projection);
            shotEffect.SetView(camera.Position, camera.Target, Vector3.Up);
            shotEffect.SetProjection(camera.Projection);
            

            //particle.Draw(gameTime);
        }
        #endregion

        #region 更新で使うメソッド
        /// <summary>
        /// ボールがネットにかかったかどうか判定する。
        /// </summary>
        /// <param name="netHeight">ネットの高さ</param>
        /// <returns>ネットならtrue,そうでなければfalse。</returns>
        bool isNet()
        {
            //if (this.HitVolume.Hit(TennisCourt.Net))
            //{
            //    return true;
            //}
            //return false;
            bool res = false;
            float aftZ = position.Z + speed.Z;//更新後のZ座標
            float aftY = position.Y + speed.Y;

            if (aftZ * position.Z <= 0)
            {
                float x = Math.Abs(aftZ) / Math.Abs(aftZ - position.Z);
                float val = MathHelper.Lerp(aftY, Position.Y, x);
                //Game1.debugStr = position.Y / 0.27f+"meter";
                if (val < TennisCourt.NetHeight)
                {
                    res = true;
                }
            }
            return res;
        }
        //bool isNetIn(float netHeight)
        //{
        //    Ray netCode1 = new Ray(new Vector3(-1.6398f, netHeight, 0), Vector3.Right);
        //    Ray netCode2 = new Ray(new Vector3(1.6398f, netHeight, 0), Vector3.Left);
        //    BoundingSphere sphere = new BoundingSphere(Center,Radius);
        //    if (sphere.Intersects(netCode1) != null && sphere.Intersects(netCode2)!=null)
        //    {
        //        return true;
        //    }
        //    return false;
        //}
        /// <summary>
        /// ボールをバウンドさせる
        /// </summary>
        void bound(float minSpeedY,float minSpeedXZ)
        {
            //座標補正
            position.Y = Radius;
            //スピンで加速
            float factor = 1;
            if (spin > 0)
            {
                factor *= MathHelper.Lerp(1, 1.3f, spin / 60.0f);
            }
            else
            {
                factor *= MathHelper.Lerp(1, 0.8f, -spin / 60.0f);
            }
            speed.X *= factor;
            speed.Z *= factor;
            //回転変更
            nextSpin();

            //バウンド係数適用XZ
            speed.X *= boundFactor.X;
            speed.Z *= boundFactor.X;
            //float spd = new Vector2(speed.Z, speed.X).Length();
            //float factorXZ = 1;
            //if (spd != 0)
            //    factorXZ = 1 + (boundFactor.X * (boundFactor.Y + 1) * speed.Y) / spd;
            //speed.X *= factorXZ;
            //speed.Z *= factorXZ;
            //バウンド係数適用Y
            speed.Y = -boundFactor.Y * speed.Y;
            //バウンド直後の速度
            BoundVelocity = speed;
            //浮力変更
            calcBuoyancy(speed.Length(), spin);
            //ある程度速度が小さくなったら停止
            if (Math.Abs(speed.Y) <= minSpeedY)
            {
                speed.Y = 0;
            }
            if (new Vector2(speed.X, speed.Z).Length() <= minSpeedXZ)
            {
                speed.Z = speed.X = 0;
            }
            if (Scenes.Scene.Game.Components.Contains(boundEffect))
            {
                //砂が飛び散る
                boundEffect.EmitPoint = position;
                boundEffect.InitialVelocity = speed.Length();
                boundEffect.Emit();
            }
            //バウンド音
            PlaySound("boundBall");
            //バウンド回数を増やす
            bounds++;
        }

        /// <summary>
        /// バウンド後のスピン量を計算する
        /// </summary>
        /// <returns></returns>
        void nextSpin()
        {
            float vy = speed.Y / 0.27f * 100 * 60;//現実世界での速度(cm/s)
            float ballD = 6.7f;//現実世界での球の大きさ(cm)
            float ballWeight = 0.0000589f;//現実世界でのボールの質量(kgf*s^2/cm)
            float moment = 3.614f*0.0001f;//ボールの回転軸周りの慣性モーメントらしい(kgf*cm*s^2)

            float spinFactor = 1;
            if(spin != 0)
                spinFactor = 1 - (ballD * ballWeight * boundFactor.X * (boundFactor.Y + 1) * vy) / (4 * MathHelper.Pi * moment*spin);
            spin *= spinFactor*0.75f;
            //一応制限
            //spin = MathHelper.Clamp(spin, -50, 50);
        }
        #endregion
    }
}
