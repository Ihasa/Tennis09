namespace Tennis01.Objects
{
    /// <summary>
    /// プレイヤーの能力を表す
    /// </summary>
    struct PlayerAbility
    {
        #region 移動関連
        /// <summary>
        /// 加速度
        /// </summary>
        public float Acceleration;
        /// <summary>
        /// 移動の入力がないときの減速度
        /// </summary>
        public float Deceleration;
        /// <summary>
        /// 最大速度
        /// </summary>
        public float MaxSpeed;
        /// <summary>
        /// どれくらいの角度までなら減速せずに方向転換できるか(度数)
        /// </summary>
        public float RotAngle;
        /// <summary>
        /// RotAngle以上の方向転換をしたときに、速度が何倍になるか(0～1)
        /// </summary>
        public float QuickNess;
        #endregion

        #region ショット関連
        /// <summary>
        /// コントロールのばらつきの少なさ
        /// </summary>
        public float Nicety;
        /// <summary>
        /// コートギリギリを狙えるかどうか
        /// </summary>
        public float Angle;
        /// <summary>
        /// 通常ショットの速度(km/h)
        /// </summary>
        public float Power;
        /// <summary>
        /// スライスの鋭さ
        /// </summary>
        public float Slice;
        /// <summary>
        /// ドロップの精度
        /// </summary>
        public float Drop;
        /// <summary>
        /// ロブの速度(km/h)
        /// </summary>
        public float LobPower;
        /// <summary>
        /// ボレーの速度(基準)(km/h)
        /// </summary>
        public float VolleyPower;
        /// <summary>
        /// サーブの能力(0 ~ 1)
        /// </summary>
        public float ServePower;
        /// <summary>
        /// 威力減衰率(打点との距離)
        /// </summary>
        public float PliabilityX;
        /// <summary>
        /// 威力減衰率(打点の高低)
        /// </summary>
        public float PliabilityY;
        /// <summary>
        /// 威力減衰率(打点の前後)
        /// </summary>
        public float PliabilityZ;
        /// <summary>
        /// トップスピン量
        /// </summary>
        public float TopSpin;
        /// <summary>
        /// スライススピン量
        /// </summary>
        public float SliceSpin;
        /// <summary>
        /// 最大射出角度Y
        /// </summary>
        public float MaxAngleY;
        /// <summary>
        /// バックハンドの強さ
        /// </summary>
        public float BackHand;
        #endregion

        public PlayerAbility(float accel, float decel, float maxS, float rotAng, float quick,float nicety,float angle,float power,float slice,float drop,float lob,float volley,float serve,float pliX,float pliY,float pliZ,float top,float sliceSpin,float maxAngle,float back)
        {
            Acceleration = accel;
            Deceleration = decel;
            MaxSpeed = maxS;
            RotAngle = rotAng;
            QuickNess = quick;
            Nicety = nicety;
            Angle = angle;
            Power = power;
            Slice = slice;
            Drop = drop;
            LobPower = lob;
            VolleyPower = volley;
            ServePower = serve;
            PliabilityX = pliX;
            PliabilityY = pliY;
            PliabilityZ = pliZ;
            TopSpin = top;
            SliceSpin = sliceSpin;
            MaxAngleY = maxAngle;
            BackHand = back;
        }
        static float Plus = 0.0f;
        public static readonly PlayerAbility StandardType = new PlayerAbility(0.6f, 0.6f, 4.8f * 0.27f / 60, 60, 0.4f, 0.75f, 0.8f, 0.7f * (1 + Plus), 0.8f, 0.8f, 0.7f, 55, 0.6f * (1 + Plus),0.6f*(1-Plus), 0.8f * (1 - Plus), 0.75f * (1 - Plus), 45, 35,32, 0.9f);
        public static readonly PlayerAbility HardHitType = new PlayerAbility(0.5f, 0.7f, 4.2f * 0.27f / 60, 60, 0.2f, 0.9f, 0.8f, 1.0f * (1 + Plus), 0.55f, 0.4f, 0.6f, 52.5f, 0.7f * (1 + Plus), 0.4f * (1 - Plus), 0.85f * (1 - Plus), 0.6f * (1 - Plus), 25, 25, 28, 0.97f);
        public static readonly PlayerAbility SpeedType = new PlayerAbility(0.9f, 0.4f, 5.0f * 0.27f / 60, 60, 0.5f, 0.4f, 0.8f, 0.3f * (1 + Plus), 0.8f, 0.8f, 0.8f, 50, 0.5f * (1 + Plus), 0.8f * (1 - Plus), 0.75f * (1 - Plus), 0.8f * (1 - Plus), 50, 30, 35, 0.94f);
        public static readonly PlayerAbility CounterType = new PlayerAbility(0.8f, 0.2f, 4.6f * 0.27f / 60, 60, 0.3f, 0.6f, 0.8f, 0.9f * (1 + Plus), 0.8f, 1.0f, 1.0f, 55, 0.55f * (1 + Plus), 0.5f * (1 - Plus), 1.0f * (1 - Plus), 0.2f * (1 - Plus), 40, 20, 25, 0.85f);
        public static readonly PlayerAbility VolleyType = new PlayerAbility(0.7f, 0.9f, 4.7f * 0.27f / 60, 60, 0.25f, 1.0f, 0.8f, 0.6f * (1 + Plus), 0.9f, 0.95f, 0.4f, 60, 0.58f * (1 + Plus), 0.7f * (1 - Plus), 0.2f * (1 - Plus), 0.9f * (1 - Plus), 40, 40, 33, 1.3f);
        public static readonly PlayerAbility ExtremeType = new PlayerAbility(0.12f, 0.02f, 6.5f * 0.27f / 60, 30, 0f, 0.8f, 0.8f, 0.0f * (1 + Plus), 0.55f, 1f, 0, 40, 0.45f * (1 + Plus), 0.8f * (1 - Plus), 0.4f * (1 - Plus), 0.75f * (1 - Plus), 50, 60, 28, 0.98f);
    }
}