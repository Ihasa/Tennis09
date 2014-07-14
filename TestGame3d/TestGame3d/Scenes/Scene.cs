using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Tennis01.Objects;
using Tennis01.Input;
using _2DComponents;
namespace Tennis01.Scenes
{
    abstract class Scene
    {
        #region フィールド
        /// <summary>
        /// このシーンを使うゲームのインスタンス
        /// </summary>
        public static GameMain Game;
        /// <summary>
        /// キャラクタリスト
        /// </summary>
        //protected List<Object3D> characters;//game.Conponentsで代用
        /// <summary>
        /// このシーンで使うカメラ
        /// </summary>
        protected Camera camera;
        /// <summary>
        /// Game1.Viewportと同じ値
        /// </summary>
        public static Viewport Viewport { get; set; }
        /// <summary>
        /// 使用されるコントローラ
        /// </summary>
        static Controller[] controlers;
        /// <summary>
        /// コントローラで操作できるもの
        /// </summary>
        List<IControleable> controleables;
        /// <summary>
        /// コントローラの最大数
        /// </summary>
        const int MAX_CONTROLLERS = 4;
        /// <summary>
        /// BGM用のサウンドプレーヤー
        /// </summary>
        static SoundPlayer bgmPlayer;
        /// <summary>
        /// SE用のサウンドプレイヤー
        /// </summary>
        static SoundPlayer sePlayer;
        /// <summary>
        /// このシーンで流すBGMの名前
        /// </summary>
        string bgmName;
        /// <summary>
        /// 画像や文字の描画用
        /// </summary>
        private Drawer2D drawer2D;
        /// <summary>
        /// シーン切り替え時の2D演出
        /// </summary>
        AnimatableLogo blackIn, blackOut;

        #endregion

        #region コンストラクタ
        //コントローラ等の初期化
        static Scene()
        {
            //最大4つのコントローラをサポート
            controlers = new Controller[MAX_CONTROLLERS];
            //Xboxコントローラがつながれていればそれを適用
            PlayerIndex index;
            for (index = PlayerIndex.One; index <= PlayerIndex.Four; index++)
            {
                if (GamePad.GetCapabilities(index).IsConnected)
                {
                    controlers[(int)index] = new XboxControler(index);
                }
                else
                {
                    break;
                }
            }
            //キーボードにする
            controlers[(int)index] = new KeyBoardControler();
            //その他は空
            for (int i = (int)index + 1; i < MAX_CONTROLLERS; i++)
            {
                controlers[i] = new NullControler();
            }
            IsEnableControllers = true;

            //BGMプレーヤ

        }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="c"></param>
        /// <param name="args"></param>
        public Scene(Camera c, bool clear,string bgmName,params GameComponent[] args)
        {
            //カメラセット
            camera = c;
            //キャラクタリスト初期化
            //characters = new List<Object3D>();
            //前のシーンで使われていたサウンドを停止
            foreach (GameComponent o in Game.Components)
            {
                if (o is Object3D)
                {
                    if (((Object3D)o).HasAudio)
                    {
                        ((Object3D)o).StopSounds();
                    }
                }
            }
            if(clear)
                Game.Components.Clear();
            AddComponents(args);
            

            controleables = new List<IControleable>();
            
            
            //BGMプレイヤーの初期化
            if(bgmPlayer == null)
                bgmPlayer = new SoundPlayer("BGMs");
            if (sePlayer == null)
                sePlayer = new SoundPlayer("ObjectSEs");
            //if (bgmName == "")
            //    StopBGM();
            //else
            //    SetBGM(bgmName);
            this.bgmName = bgmName;
            //Drawer2Dの初期化
            //念のためFontを指定(妥協案)
            drawer2D = new Drawer2D(Game.GraphicsDevice,GameMain.LogoFont);
            Transition = false;

            //シーン切り替えアニメーション
            blackIn = new AnimatableLogo(GameMain.LogoFont, Game.WindowRect, Vector2.One * 0.5f, Vector2.One * 0.5f,
                new Animation("blackIn",
                    new AnimationKey(0, new LogoParams(GameMain.Textures["null"], Vector2.Zero, Vector2.One * 0.5f, Vector2.One, Rotation.Zero, Color.Black), RotationWays.RELATIVE),
                    new AnimationKey(30, new LogoParams(GameMain.Textures["null"], Vector2.Zero, Vector2.One * 0.5f, Vector2.One, Rotation.Zero, Color.Transparent), RotationWays.RELATIVE)
                )
            );
            blackOut = new AnimatableLogo(GameMain.LogoFont, Game.WindowRect, Vector2.One * 0.5f, Vector2.One * 0.5f,
                new Animation("blackOut",
                    new AnimationKey(0, new LogoParams(GameMain.Textures["null"], Vector2.Zero, Vector2.One * 0.5f, Vector2.One, Rotation.Zero, Color.Transparent), RotationWays.RELATIVE),
                    new AnimationKey(30, new LogoParams(GameMain.Textures["null"], Vector2.Zero, Vector2.One * 0.5f, Vector2.One, Rotation.Zero, Color.Black), RotationWays.RELATIVE)
                )
            );
            AddComponents(blackIn, blackOut);
        }
        #endregion
        #region 操作
        public void AddObjects(params Object3D[] args)
        {
            AddComponents(args);
            foreach (Object3D o in args)
            {
                if (o.Shadow != null)
                {
                    AddComponents(o.Shadow);
                }
            }
        }
        public void AddComponents(params GameComponent[] args)
        {
            foreach (GameComponent cha in args)
            {
                /*if (cha is Object3D && (cha as Object3D).HasAudio)
                {
                    (cha as Object3D).InitSoundPlayer(camera);
                }*/
                Game.Components.Add(cha);
                //characters.Add(cha);
            }
        }
        protected void GotoNextScene(Scene scene)
        {
        }

        #endregion
        #region コントローラ関係
        /// <summary>
        /// コントローラのコレクションを返す
        /// </summary>
        public static Controller[] Controllers
        {
            get { return controlers; }
            set
            {
                if (value.Length != MAX_CONTROLLERS)
                    throw new ArgumentException();
                controlers = value;
            }
        }
        public static bool IsEnableControllers 
        {
            get
            {
                return Controllers[0].Enabled;
            }
            set
            {
                foreach (Controller c in Controllers)
                {
                    c.Enabled = value;
                }
            }
        }
        /// <summary>
        /// コントロール対象を登録する
        /// </summary>
        /// <param name="collection"></param>
        protected void AddControleables(params IControleable[] collection)
        {
            foreach (IControleable c in collection)
                controleables.Add(c);
        }
        protected bool HasAnyInput()
        {
            foreach (Controller c in Controllers)
            {
                if (c.GetState().HasAnyInput(ControllerState.Inputs.AllButtons))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion
        /// <summary>
        /// オブジェクトの状態に応じて、次のシーンを返す。
        /// </summary>
        /// <returns>遷移先のSceneオブジェクト。遷移できないときはnull</returns>
        public abstract Scene NextScene { get; }

        #region BGM管理
        public void SetBGM()
        {
            bgmPlayer.StopSound();
            if(bgmName != "")
                bgmPlayer.PlaySound(bgmName);
        }
        public void SetBGM(string bgmName)
        {
            bgmPlayer.StopSound();
            bgmPlayer.PlaySound(bgmName);
        }
        public void StopBGM()
        {
            bgmPlayer.StopSound();
        }
        public void StopBGM(string bgmName)
        {
            bgmPlayer.StopSound(bgmName);
        }
        public void SoundEffect(string seName)
        {
            sePlayer.PlaySound(seName);
        }
        #endregion

        #region プロパティ
        public Camera Camera { get { return camera; } }
        public bool Transition { get; protected set; }
        #endregion
        
        #region 2D要素の描画
        public void DrawImage(Texture2D texture, Vector2 position, Vector2 clip,float scaleX, float scaleY,Color color)
        {
            //drawer2Dでやる
            //Vector2 texBounds = new Vector2((float)texture.Width/Game.GraphicsDevice.Viewport.Width*scaleX, (float)texture.Height/Game.GraphicsDevice.Viewport.Height*scaleY);
            drawer2D.AddTexture(texture,position,clip,scaleX,scaleY,color);
        }
        public void DrawImage(Texture2D texture, Vector2 position,Vector2 clip,Color color)
        {
            DrawImage(texture, position, clip, 1, 1, color);
            //drawer2D.AddTexture(texture, position,color);
        }
        public void DrawImage(Texture2D texture, Vector2 position,Vector2 clip, float scale,Color color)
        {
            DrawImage(texture, position, clip, scale, scale, color);
            //drawer2D.AddTexture(texture, position, scale,color);
        }
        public void DrawImage(Texture2D texture, Rectangle rect, Color color)
        {
            drawer2D.AddTexture(texture, rect, color);
        }
        public void DrawString(string str, Vector2 position,Vector2 clip,Color color,float scale)
        {
            drawer2D.AddString(str, position,clip,color,scale);
        }
        #endregion

        #region 更新と描画

        /// <summary>
        /// 更新処理。カメラの操作やオブジェクトの状態管理など
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime)
        {
            //カメラの更新
            camera.Update();
            ////キャラクタリストの更新
            //foreach (Object3D character in characters)
            //{
            //    character.Update(gameTime);
            //}
            //コントローラの更新
            //別の場所でやる
            int i = 0;
            foreach (Controller c in Controllers)
            {
                c.Update();
                ControllerState s = c.GetState();
                GameMain.debugStr["Controlers" + i + ".Enabled"] = ""+c.Enabled;
                //GameMain.debugStr["Controlers" + i + ".Button1"] = s.Button1 + "";
                GameMain.debugStr["Controlers" + i] = "" + c.ToString();
                i++;
            }
            foreach (GameComponent c in Game.Components)
            {
                if (c is Object3D)
                {
                    ((Object3D)c).Camera = this.Camera;
                }
            }
            //操作ターゲットにコントローラの操作を反映
            //foreach (IControleable c in controleables)
            //{
            //    c.Control(Controlers[c.Index].GetState());
            //}
        }

        /// <summary>
        /// 描画処理
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Draw(GameTime gameTime)
        {
            GameMain.debugStr["Camera"] = Camera.Position.ToString();
            drawer2D.Draw();
        }

        #endregion

        protected void debugCamera()
        {
            if (GameMain.debug)
            {
                Vector3 vec = new Vector3(GameMain.gamePadStates[0][0].ThumbSticks.Right.X / 10.0f, GameMain.gamePadStates[0][0].ThumbSticks.Right.Y / 10, GameMain.gamePadStates[0][0].Triggers.Left - GameMain.gamePadStates[0][0].Triggers.Right);
                camera.Position += vec;
                if (camera.FieldOfView > 1 && GameMain.gamePadStates[0][0].IsButtonDown(Buttons.DPadDown))
                {
                    camera.FieldOfView--;
                }
                if (camera.FieldOfView < 90 && GameMain.gamePadStates[0][0].IsButtonDown(Buttons.DPadUp))
                {
                    camera.FieldOfView++;
                }
                KeyboardState kState = Keyboard.GetState();
                if (kState.IsKeyDown(Keys.Up))
                {
                    Vector3 v = new Vector3(0, 0, -0.27f);
                    camera.Position += v;
                    camera.Target += v;
                }
                else if (kState.IsKeyDown(Keys.Down))
                {
                    Vector3 v = new Vector3(0, 0, 0.27f);
                    camera.Position += v;
                    camera.Target += v;
                }
                GameMain.debugStr["fov"] = "" + camera.FieldOfView;
                GameMain.debugStr["cameraTarget"] = "" + camera.Target;

            }
        }
    }
}
