//#define DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SkinnedModel;
using Tennis01.Scenes;
using _2DComponents;
namespace Tennis01
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    class GameMain : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public static SpriteFont debugFont,LogoFont;
        static long frames;
        public static long TotalFrames { get { return frames; } }
        Drawer2D drawer2D;
        DateTime startTime = DateTime.Now;
        RenderTarget2D ssTarget;
        public static bool debug { get; private set; }

        //���݂̃V�[��
        Scene currentScene=null;
        Scene nextScene = null;
        //�ŏ��̃V�[��
        private Scene firstScene
        {
            get
            {
                return new SceneTitle();
                //return new TestScene(new Camera(new Vector3(0, 0.27f*1.66f*8, 9f), Vector3.Zero,25,GraphicsDevice.Viewport));
            }
        }
        //���̃Q�[���Ŏg�p����I�[�f�B�I�G���W��
        AudioEngine audioEngine;
        //Wiimote�̃R���g���[��
        //public static MyWiimote Wiimote;
        //�Q�[���p�b�h�B�f�o�b�O�p
        public static GamePadState[][] gamePadStates { get; private set; }
        public Rectangle WindowRect;
        AnimatableLogo blackOut;
        AnimatableLogo blackIn;
        #region �O���[�o�����\�[�X
        //�f�o�b�O�p�ɕ\�����镶����ƃt�H���g
        public static Dictionary<string, string> debugStr = new Dictionary<string,string>();
        /// <summary>
        /// �l�Ԃ̃��[�V������ێ�
        /// </summary>
        public static Dictionary<string, AnimationClip> HumanMotions = new Dictionary<string, AnimationClip>();
        public static List<Matrix> HumanBindPose = new List<Matrix>();
        /// <summary>
        /// 3D���f���̃��X�g(�O���[�o��)
        /// </summary>
        public static Dictionary<string, Model> Models;
        /// <summary>
        /// �e�N�X�`���[�摜�̃��X�g(�O���[�o��)
        /// </summary>
        public static Dictionary<string, Texture2D> Textures;
        /// <summary>
        /// �T�E���h�o���N�ƃE�F�[�u�o���N�̃Z�b�g�̃R���N�V����
        /// </summary>
        public static Dictionary<string, Sounds> Sounds;
        public static Random Random = new Random();
        #endregion
        public GameMain(string title)
        {
            graphics = new GraphicsDeviceManager(this);
            //Content���Ƀt�H���_�������t�H���_�̖��O���w�肷�邱��
            Content.RootDirectory = "Content";
            Window.Title = title;

            GameConfig config = new GameConfig();
            config.ShowDialog();

            graphics.IsFullScreen = config.IsFullScreen;
            int width = config.GameWidth;
            int height = (int)(config.GameWidth / config.DisplayProp);
            graphics.PreferredBackBufferHeight = height;
            graphics.PreferredBackBufferWidth = width;
            //this.TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 128);

            //���̑�������
            frames = 0;
#if DEBUG 
            debug = true; 
#endif
            WindowRect = new Rectangle(0, 0, width,height);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            try
            {
                //VisualEffects.dll�̏�����
                VisualEffects.VisualEffect.Init(this);
                VisualEffects.Particle.WholeDrawOrder = 3;

                //2DComponents.dll�̏�����
                _2DComponents.Component2D.Init(this);
                _2DComponents.Component2D.MinimumDrawOrder = 4;

                //�f�o�b�O�p�̃Q�[���p�b�h�̏�Ԏ擾
                gamePadStates = new GamePadState[2][];
                for (int i = 0; i < gamePadStates.Length; i++)
                {
                    gamePadStates[i] = new GamePadState[4];
                }

                //�ق��̃N���X�̐ÓI�t�B�[���h������
                Scene.Game = this;
                Scene.Viewport = this.GraphicsDevice.Viewport;

                //�I�[�f�B�I�G���W���̏�����
                audioEngine = new AudioEngine(@"Content\Sound\GameSounds.xgs");
                //�T�E���h�o���N�ƃE�F�[�u�o���N�R���N�V�����̏�����
                Sounds = new Dictionary<string, Sounds>();
                string[] soundNames = new string[]{
                "BGMs","ObjectSEs"
            };
                foreach (string s in soundNames)
                {
                    Sounds.Add(s, new Sounds(new SoundBank(audioEngine, @"Content\Sound\" + s + ".xsb"), new WaveBank(audioEngine, @"Content\Sound\" + s + ".xwb")));
                }
                //this.IsMouseVisible = true;
                //BGM�v���C���[�̏�����
                base.Initialize();
                //�X�N���[���V���b�g�p�̃����_�[�^�[�Q�b�g
                ssTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
                //���ʂȃf�o�C�X�Ƃ̐ڑ�
                //Wiimote = new MyWiimote(this);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("���������ɗ�O���������܂���...\n" + e.ToString());
            }

        }
        
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
    
            //3D���f�����X�g�̏�����
            Models = new Dictionary<string, Model>();

            try
            {
                //���[�V������ǂݍ���
                Model motionModel = Content.Load<Model>("human12");
                Models.Add("human",motionModel);
                HumanBindPose = (motionModel.Tag as SkinningData).BindPose;
                HumanMotions = (motionModel.Tag as SkinningData).AnimationClips;


                //�e�X�g�p��Content��ǂݍ���
                //�t�H���g
                LogoFont = Content.Load<SpriteFont>("LogoFont");
                debugFont = Content.Load<SpriteFont>(@"Test\DebugFont");
                //3D���f��
                string[] testModels = new string[]{
                    "iroiroTest3","judgeMan","debugSphire","debugBox","bottle",
                    "human170","humanFat","boxMan","ballMan"
                };
                loadContents(Models,"Test", testModels);


                //���ʂ�3D���f����ǂݍ���
                string[] modelNames = new string[]{
                    "CenterCourt","NormalCourt","NormalCourt_Wall","ball","tennisCourt_big","tennisCourt_big2",
                    "racket","racket1","racket2","racket3","net","maguro","maguroRacket","tennisMachine","targetR","targetB"
                };
                loadContents(Models,"Models", modelNames);

                //�X�L�����b�V�����ꂽ3D���f����ǂݍ���
                string[] skinnedModelNames = new string[]{
                   "kumasan"
                };
                loadContents(Models,"SkinnedModels", skinnedModelNames);


                //�e�N�X�`���[(�摜)��ǂݍ���
                Textures = new Dictionary<string, Texture2D>();
                string[] textureAssetNames = new string[]{
                    "fire","line_Y","fireB","fireY","fireG","snow","rain","alpaca","sand","cursor",
                    "buttonBase","circle","circle2","null","gra","controller","basic","shots","shotTech","smash",
                    "title"
                };
                loadContents(Textures,"Textures", textureAssetNames);
                drawer2D = new Drawer2D(GraphicsDevice, debugFont);
                blackIn = new AnimatableLogo(LogoFont, new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height), Vector2.One * 0.5f, Vector2.One * 0.5f,
                    new Animation("blackIn",
                        new AnimationKey(0, new LogoParams(Textures["null"], Vector2.Zero, Vector2.One * 0.5f, Vector2.One, Rotation.Zero, Color.Black), RotationWays.RELATIVE),
                        new AnimationKey(30, new LogoParams(Textures["null"], Vector2.Zero, Vector2.One * 0.5f, Vector2.One, Rotation.Zero, Color.Transparent), RotationWays.RELATIVE)
                    )
                );
                blackOut = new AnimatableLogo(LogoFont, new Rectangle(0,0,Window.ClientBounds.Width,Window.ClientBounds.Height), Vector2.One * 0.5f, Vector2.One * 0.5f,
                    new Animation("blackOut",
                        new AnimationKey(0, new LogoParams(Textures["null"], Vector2.Zero, Vector2.One * 0.5f, Vector2.One, Rotation.Zero, Color.Transparent), RotationWays.RELATIVE),
                        new AnimationKey(30, new LogoParams(Textures["null"], Vector2.Zero, Vector2.One * 0.5f, Vector2.One, Rotation.Zero, Color.Black), RotationWays.RELATIVE)
                    )
                );
                blackOut.StartAnimating += () =>
                {
                    Scene.IsEnableControllers = false;
                };
                blackOut.EndAnimating += () =>
                {
                    currentScene.StopBGM();
                    currentScene = nextScene;
                    nextScene = null;
                    foreach (GameComponent c in Components)
                    {
                        c.Enabled = true;
                    }
                    blackOut.Visible = false;
                    doBlackIn("blackIn");
                    Scene.IsEnableControllers = true;
                };
                blackIn.EndAnimating += () =>
                {
                    currentScene.SetBGM();
                };
                Components.Add(blackOut);
                Components.Add(blackIn);
            }
            catch (Exception e)
            {
                throw new Exception("�v�����ꂽ�t�@�C���͑��݂��܂���B" + e.ToString());
            }
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            //graphics.PreferredBackBufferWidth = 1280;
            //graphics.PreferredBackBufferHeight = 960;
            //GraphicsDevice.Viewport = new Viewport(0, 0, 640, 480);
        }

        private void doBlackIn(string name)
        {
            if (!Components.Contains(blackIn))
                Components.Add(blackIn);
            blackIn.Animate(name, 1, true);
        }
        private void doBlackOut(string name)
        {
            if (!Components.Contains(blackOut))
                Components.Add(blackOut);
            blackOut.Enabled = true;
            blackOut.Animate(name, 1, true);
        }
        void loadContents<T>(Dictionary<string,T> dictionary,string directory, string[] modelNames)
        {
            foreach (string s in modelNames)
            {
                T content = Content.Load<T>(directory + "\\" + s);
                dictionary.Add(s,content);
                //Models.Add(s, Content.Load<T>(directory+@"\"+s));
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {   
            try
            {
                debugStr["fps"] = gameTime.ElapsedGameTime.Milliseconds+"";
                // Allows the game to exit
                if ( debug && (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Escape)))
                    this.Exit();
                gamePadStates[1][0] = gamePadStates[0][0];
                gamePadStates[1][1] = gamePadStates[0][1];
                gamePadStates[1][2] = gamePadStates[0][2];
                gamePadStates[1][3] = gamePadStates[0][3];

                gamePadStates[0][0] = GamePad.GetState(PlayerIndex.One);
                gamePadStates[0][1] = GamePad.GetState(PlayerIndex.Two);
                gamePadStates[0][2] = GamePad.GetState(PlayerIndex.Three);
                gamePadStates[0][3] = GamePad.GetState(PlayerIndex.Four);
                
                //�V�[���̏�ԍX�V
                if (currentScene != null)
                {
                    currentScene.Update(gameTime);
                    
                    if(nextScene == null)
                        nextScene = currentScene.NextScene;
                    if (nextScene != null && !blackOut.IsAnimating)//�V�[���J��
                    {
                        //currentScene = nextScene;
                         foreach (GameComponent c in Components)
                         {
                             c.Enabled = false;
                         }
                         doBlackOut("blackOut");
                    }
                    GameMain.debugStr["currentscene"] = "" + currentScene.ToString();
                    if(nextScene != null)
                        GameMain.debugStr["nextScene"] = "" + nextScene.ToString();
                    if (!pressed)
                    {
#if DEBUG
                        if (Keyboard.GetState().IsKeyDown(Keys.D))
                        {
                            debug = !debug;
                            pressed = true;
                        }
#endif
                    }
                    else if(Keyboard.GetState().IsKeyUp(Keys.D))
                        pressed = false;
                }
                else
                {
                    currentScene = firstScene;
                }
                //�T�E���h�v���[���̍X�V
                audioEngine.Update();
                //���ʂȃf�o�C�X�̍X�V
                //Wiimote.Update();
                frames++;
                base.Update(gameTime);

                //SS
                if (gamePadStates[0][0].IsButtonDown(Buttons.B) && gamePadStates[0][1].IsButtonUp(Buttons.B) && Keyboard.GetState().IsKeyDown(Keys.LeftShift))
                {
                    screenShot(startTime.ToString("yyyyMMddHHmmss"), gameTime);
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
            }
        }
        bool pressed = false;

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            try
            {
                if (currentScene != null)
                {
                    drawScreen(gameTime);
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
            }
        }

        private void drawScreen(GameTime gameTime)
        {
            base.Draw(gameTime);
            currentScene.Draw(gameTime);
            if (debug)
            {
                debugDraw();
            }
            drawer2D.Draw();
        }

        private void debugDraw()
        {
            float strY = 0;
            float sy = 1.0f / debugStr.Count;
            if (sy > 0.05f)
                sy = 0.05f;
            foreach (KeyValuePair<string, string> pair in debugStr)
            {
                drawer2D.AddString(pair.Key + "..." + pair.Value, new Vector2(0, strY), Vector2.Zero, Color.Red, sy);
                strY += sy;
            }
        }
        int ssCount = 0;
        void screenShot(string dirName,GameTime gameTime)
        {
            try
            {
                GraphicsDevice.SetRenderTarget(ssTarget);
                //GraphicsDevice.Clear(Color.AliceBlue);
                drawScreen(gameTime);
                GraphicsDevice.SetRenderTarget(null);
                System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo("../" + dirName);
                if (!directory.Exists)
                {
                    directory.Create();
                }

                using (System.IO.Stream stream = new System.IO.FileInfo("../" + dirName + "/ss" + (ssCount++) + ".png").Open(System.IO.FileMode.OpenOrCreate))
                {
                    ssTarget.SaveAsPng(stream, ssTarget.Width, ssTarget.Height);
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
            }
        }
    }
}
