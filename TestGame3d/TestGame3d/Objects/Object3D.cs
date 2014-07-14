using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SkinnedModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

using VisualEffects;

namespace Tennis01.Objects
{
    using Scenes;
    class Object3D:DrawableGameComponent
    {
        #region フィールド
        //3Dモデル関連
        /// <summary>
        /// このキャラクターのモデル
        /// </summary>
        public Model Model{get; set;}
        /// <summary>
        /// スキニングデータ
        /// </summary>
        protected SkinningData SkinningData { get; set; }
        /// <summary>
        /// このモデルが持っているアニメーションのコレクション
        /// </summary>
        protected Dictionary<string, AnimationClip> Clips;
        /// <summary>
        /// このオブジェクトを見ているカメラ
        /// </summary>
        protected Camera camera;

        /// <summary>
        /// 座標と速度
        /// </summary>
        protected Vector3 position, speed;
        /// <summary>
        /// 回転(ラジアン)
        /// </summary>
        protected Vector3 rotation;
        /// <summary>
        /// 回転、移動以外の変換
        /// </summary>
        public Matrix AnotherTransform{get;set;}


            #region SE関係
        /// <summary>
        /// サウンドエフェクト用。このオブジェクトのスピーカー。
        /// </summary>
        AudioEmitter audioEmitter=null;
        /// <summary>
        /// このオブジェクトのサウンドプレイヤー。
        /// </summary>
        SoundPlayer soundPlayer=null;
        /// <summary>
        /// サウンドバンクの名前
        /// </summary>
        string soundBankName=null;
            #endregion

        Particle shadow=null;
        #endregion

        #region コンストラクタ
        /// <summary>
        /// 位置や速度などを指定して初期化。
        /// </summary>
        /// <param name="m">このキャラクタの3Dモデル</param>
        /// <param name="pos">座標</param>
        /// <param name="spd">速度</param>
        public Object3D(Model m,Camera c,Vector3? pos = null,Vector3? spd=null,Vector3? rot = null,Matrix? transform=null,string soundBankName=null,float? shadowScale = null):base(Scene.Game)
        {
            //座標などの初期化
            position = pos??Vector3.Zero; 
            speed = spd ?? Vector3.Zero;
            rotation = rot ?? Vector3.Zero;
            AnotherTransform = transform ?? Matrix.Identity;
            //モデルの初期化
            initModel(m, c,transform ?? Matrix.Identity);
            //SoundPlayerを作成
            if (soundBankName != null)
            {
                initAudioEmitter(soundBankName);
                initSoundPlayer(c);
            }
            initShadow(shadowScale);
        }
        /// <summary>
        /// モデルと変換とサウンドを指定して初期化。
        /// </summary>
        /// <param name="m">このキャラクタの3Dモデル</param>
        /// <param name="pos">座標</param>
        /// <param name="spd">速度</param>
        public Object3D(Model m, Camera c,Matrix transform,string soundBankName = null,float? shadowScale = null):base(Scene.Game)
        {
            //座標などの初期化
            position = rotation = speed = Vector3.Zero;
            AnotherTransform = transform;
            //モデルの初期化
            initModel(m, c,transform);

            //SoundPlayerを作成
            if (soundBankName != null)
            {
                initAudioEmitter(soundBankName);
                initSoundPlayer(c);
            }
            initShadow(shadowScale);
        }
        #endregion

        #region 初期化(private)
        /// <summary>
        /// 3Dモデル関連の初期化
        /// </summary>
        /// <param name="m"></param>
        /// <param name="transform"></param>
        void initModel(Model m,Camera c,Matrix transform)
        {
            Model = m;
            camera = c;
            //isAnimating = false;
            SkinningData = Model.Tag as SkinningData;
            //スキンメッシュあり
            if (SkinningData != null)
            {
                //animationPlayer = new AnimationPlayer(skinningData);
                Clips = SkinningData.AnimationClips;
                foreach (ModelMesh mesh in Model.Meshes)
                {
                    foreach (SkinnedEffect effect in mesh.Effects)
                    {
                        //ライトを設定
                        effect.EnableDefaultLighting();
                        //変換させる
                        effect.World = transform;
                    }
                }
            }
            else//なし
            {
                foreach (ModelMesh mesh in Model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        //ライトを設定
                        effect.EnableDefaultLighting();
                        //変換
                        effect.World = transform;
                    }
                }
            }
        }
        /// <summary>
        /// サウンドプレーヤ関連の初期化
        /// </summary>
        /// <param name="soundBankName"></param>
        void initAudioEmitter(string soundBankName)
        {
            this.soundBankName = soundBankName;
            audioEmitter = new AudioEmitter();
            audioEmitter.Position = position;
            audioEmitter.Velocity = speed;
            audioEmitter.Up = Vector3.Up;
            audioEmitter.Forward = new Vector3((float)Math.Sin(rotation.Y), 0, (float)Math.Cos(rotation.Y));
            EnabledChanged +=(o,e)=>
            {
                if (!Enabled)
                {
                    StopSounds();
                }
            };
        }
        /// <summary>
        /// SoundPlayerを初期化する
        /// </summary>
        /// <param name="audioListener"></param>
        private void initSoundPlayer(AudioListener audioListener)
        {
            soundPlayer = new SoundPlayer(soundBankName, audioEmitter, audioListener);
        }

        /// <summary>
        /// 影を初期化
        /// </summary>
        protected void initShadow(float? scale)
        {
            if (scale != null)
            {
                shadow = Particle.CreateFromParFile("ParFiles/shadow.par");
                shadow.FinalColor = Vector3.Zero;
                shadow.Scale = (float)scale;
                shadow.Normal = Vector3.Normalize(new Vector3(0, 1, 0.0001f));
            }
        }
        #endregion

        #region 外部からの操作
        /// <summary>
        /// 再生中のすべてのサウンドを停止する
        /// </summary>
        public void StopSounds()
        {
            soundPlayer.StopSound();
        }
        #endregion

        #region 派生クラスからの操作


        /// <summary>
        /// 指定したサウンドを再生する
        /// </summary>
        /// <param name="soundName">再生するサウンドの名前</param>
        public void PlaySound(string soundName)
        {
            soundPlayer.PlaySound(soundName);
        }
        /// <summary>
        /// 指定したサウンドを停止する
        /// </summary>
        /// <param name="soundName">停止するサウンドの名前</param>
        public void StopSound(string soundName)
        {
            soundPlayer.StopSound(soundName);
        }

        #endregion

        #region プロパティ
        /// <summary>
        /// 影のパーティクルを取得
        /// </summary>
        public Particle Shadow { get { return shadow; } }
        /// <summary>
        /// スキンメッシュアニメーションを持っているか
        /// </summary>
        public bool HasAnimation
        {
            get
            {
                return SkinningData != null;
            }
        }
        /// <summary>
        /// サウンドエフェクトを持っているか
        /// </summary>
        public bool HasAudio
        {
            get
            {
                return soundBankName != null;
            }
        }
        /// <summary>
        /// カメラを取得
        /// </summary>
        public Camera Camera { get { return camera; } set { camera = value; } }
        /// <summary>
        /// スクリーン座標での位置
        /// </summary>
        public Vector2 ScreenPosition
        {
            get
            {
                Vector3 posi = Game.GraphicsDevice.Viewport.Project(Position, camera.Projection, camera.View, Matrix.Identity);
                return new Vector2(posi.X, posi.Y);
            }
        }
        #endregion

        #region 更新と描画
        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            position += speed;
//            boundingSphere = new BoundingSphere(position, boundingSphere.Radius);

            if (HasAudio)
            {
                updateAudio();
            }

            //影を出す
            if (shadow != null)// && GameMain.gamePadStates[0][0].IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.RightTrigger))
            {
                shadow.EmitPoint = new Vector3(position.X, 0.0027f, position.Z);
                //shadow.Reset();
                shadow.Emit();
            }
            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            if (Visible)
            {
                if (shadow != null)
                {
                    shadow.SetView(camera.Position, camera.Target, camera.Up);
                    shadow.SetProjection(camera.Projection);
                }
                DrawModel(gameTime, camera.View, camera.Projection);
            }
            base.Draw(gameTime);
        }

        #endregion
        /// <summary>
        /// 実際の描画処理
        /// </summary>
        protected virtual void DrawModel(GameTime gameTime, Matrix view, Matrix projection)
        {
            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.View = view;
                    effect.Projection = projection;
                    effect.World = AnotherTransform * Matrix.CreateRotationX(rotation.X) * Matrix.CreateRotationY(rotation.Y) * Matrix.CreateRotationZ(rotation.Z) * Matrix.CreateTranslation(position);
                }
                mesh.Draw();
            }   
        }

        //audioEmitterの状態更新
        void updateAudio()
        {
            audioEmitter.Position = position;
            audioEmitter.Velocity = speed;
            audioEmitter.Up = Vector3.Up;
            audioEmitter.Forward = new Vector3((float)Math.Sin(rotation.Y), 0, (float)Math.Cos(rotation.Y));
            soundPlayer.Apply3D();
        }



        #region ゲッタ、セッタ
        public Vector3 Position { get { return position; } set { position = value; } }
        public Vector3 Speed { get { return speed; } set { speed = value; } }
        public float ModelRadius { get { return Model.Meshes[0].BoundingSphere.Radius; } }
        #endregion

        
    }
}
