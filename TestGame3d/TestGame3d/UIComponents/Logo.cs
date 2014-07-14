using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Tennis01.UIComponents
{
    struct AnimationKey
    {
        public int KeyFrame;
        public LogoParams LogoParams;
        public AnimationKey(int keyFrame, LogoParams logoParams)
        {
            KeyFrame = keyFrame;
            LogoParams = logoParams;
        }
    }
    class Animation
    {
        //アニメーションの名前
        public string Name { get; private set; }
        //現在のキー
        AnimationKey currentKey;
        //次のキー
        AnimationKey nextKey;
        //キーフレームのコレクション
        AnimationKey[] keyCollection;
        //現在のフレーム
        int currentFrame;
        //最後のキーフレーム
        int maxFrames;
        //何回アニメーションしているか
        int looped=0;
        //アニメーションのループ回数(0以下で無限ループ)
        int loopMax;
        public Animation(string name,params AnimationKey[] keys)
        {
            this.Name = name;
            keyCollection = keys;
            keyCollection.OrderBy((val) => { return val.KeyFrame; });
            maxFrames = keyCollection[keyCollection.Length - 1].KeyFrame;
            currentFrame = 0;
        }
        public void UpdateAnimation()
        {
            foreach (AnimationKey key in keyCollection)
            {
                nextKey = key;
                if (key.KeyFrame > currentFrame)
                {
                    break;
                }
            }
            foreach (AnimationKey key in keyCollection)
            {
                if (key.KeyFrame > currentFrame)
                {
                    break;
                }
                currentKey = key;
            }
            if (IsAnimating && currentFrame++ >= maxFrames - 1)
            {
                if (loopMax > 0 && ++looped >= loopMax)
                {
                    IsAnimating = false;
                }
                else
                {
                    currentFrame = 0;
                    currentKey = keyCollection[0];
                }
            }
        }
        public void StartAnimation(int loops)
        {
            currentFrame = 0;
            this.loopMax = loops;
            this.looped = 0;
            IsAnimating = true;
        }
        public LogoParams GetCurrentParams()
        {
            float a = (float)(nextKey.KeyFrame - currentKey.KeyFrame);
            float amount;
            if(a != 0)
                amount = (currentFrame - currentKey.KeyFrame)/(float)(nextKey.KeyFrame - currentKey.KeyFrame);
            else
                amount = 0;
            //Game1.debugStr["amount"] = "" + amount;
            Vector2 offset = Vector2.Lerp(currentKey.LogoParams.Position,nextKey.LogoParams.Position,amount);
            Vector2 scale = Vector2.Lerp(currentKey.LogoParams.Scale, nextKey.LogoParams.Scale, amount);
            float strScale = MathHelper.Lerp(currentKey.LogoParams.StringScale, nextKey.LogoParams.StringScale, amount);
            Color color = Color.Lerp(currentKey.LogoParams.Color, nextKey.LogoParams.Color, amount);
            return new LogoParams(offset, scale,strScale,color);
        }
        public bool IsAnimating { get; private set; }
    }

    class AnimatableLogo : Logo
    {
        Vector2 defaultPosition;
        protected Animation currentAnimation = null;
        protected Rectangle bounds { get { return new Rectangle((int)((Parameters.Position.X - Parameters.Scale.X/2)*winWidth), (int)((Parameters.Position.Y-Parameters.Scale.Y/2)*winHeight), (int)(Parameters.Scale.X*winWidth), (int)(Parameters.Scale.Y*winHeight)); } }
        Dictionary<string, Animation> animations;
        public bool IsAnimating { get { return currentAnimation != null && currentAnimation.IsAnimating; } }
        public AnimatableLogo(Texture2D texture, Vector2 defPosition,Scenes.Scene scene, params Animation[] animationList)
            : base(texture,defPosition,0.5f*Vector2.One,Vector2.One,0,Color.White, scene)
        {
            init(defPosition, animationList);
        }
        public AnimatableLogo(string text, Vector2 defPosition, Scenes.Scene scene, params Animation[] animationList)
            : base(text, defPosition, 0.5f * Vector2.One, Vector2.One,0, Color.White, scene)
        {
            init(defPosition, animationList);
        }
        void init(Vector2 defPosition,Animation[] animationList)
        {
            animations = new Dictionary<string, Animation>();
            AddAnimations(animationList);
            defaultPosition = defPosition;
        }
        protected void AddAnimations(params Animation[] animationList)
        {
            foreach (Animation a in animationList)
            {
                animations.Add(a.Name, a);
            }
        }
        public override void Update(GameTime gameTime)
        {
            if (currentAnimation != null) 
            {
                currentAnimation.UpdateAnimation();
                //if (Visible && !currentAnimation.Animating)
                //{
                //    Visible = false;
                //}
            }
            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            if (currentAnimation != null)
            {
                LogoParams p = currentAnimation.GetCurrentParams();
                this.Parameters = new LogoParams(p.Position + defaultPosition, p.Scale,p.StringScale,p.Color);
            }
            base.Draw(gameTime);
        }
        public void Animate(string animationName,int loops)
        {
            currentAnimation = animations[animationName];
            currentAnimation.StartAnimation(loops);
            Visible = true;
        }
    }

    struct LogoParams
    {
        public Vector2 Position;
        public Vector2 Scale;
        public float StringScale;
        public Color Color;
        public LogoParams(Vector2 position, Vector2 scale, float strScale, Color color)
        {
            Position = position;
            Scale = scale;
            StringScale = strScale;
            Color = color;
        }
        public LogoParams(Vector2 position, Vector2 scale, Color color)
        {
            Position = position;
            Scale = scale;
            Color = color;
            StringScale = 0;
        }

    }
    class Logo:DrawableGameComponent
    {
        public Texture2D Texture { get; set; }
        public string Text { get; set; }
        Scenes.Scene scene;
        public LogoParams Parameters { get; set; }
        public Vector2 Clip { get; set; }
        protected int winWidth{get;private set;}
        protected int winHeight { get; private set; }
        //Vector2 offset;
        //int frames;
        //int showFrames = 60;
        public Logo(Texture2D tex, Scenes.Scene scene)
            : this(tex, Vector2.Zero,Vector2.Zero,Vector2.One,0,Color.White,scene)
        {
        }
        /// <summary>
        /// ロゴに画像を使う場合のコンストラクタ
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="posi"></param>
        /// <param name="clip"></param>
        /// <param name="scale"></param>
        /// <param name="color"></param>
        /// <param name="scene"></param>
        public Logo(Texture2D tex,Vector2 posi,Vector2 clip,Vector2 scale,float strScale,Color color,Scenes.Scene scene):base(Scenes.Scene.Game)
        {
            Texture = tex;
            this.scene = scene;
            this.Clip = clip;
            Parameters = new LogoParams(posi, scale,strScale,color);
            Text = "";
            winWidth = Scenes.Scene.Viewport.Width;
            winHeight = Scenes.Scene.Viewport.Height;

            //Enabled = false;
            //Visible = false;
            //VisibleChanged += (o, e) =>
            //{
            //    Enabled = Visible;
            //    frames = 0;
            //    scale = 1;
            //};
        }
        /// <summary>
        /// ロゴに普通の文字列描画を使う場合のコンストラクタ
        /// </summary>
        /// <param name="text"></param>
        /// <param name="posi"></param>
        /// <param name="clip"></param>
        /// <param name="scale"></param>
        /// <param name="color"></param>
        /// <param name="scene"></param>
        public Logo(string text, Vector2 posi, Vector2 clip, Vector2 scale, float strScale,Color color, Scenes.Scene scene)
            : base(Scenes.Scene.Game)
        {
            Text = text;
            this.scene = scene;
            this.Clip = clip;
            Parameters = new LogoParams(posi, scale, strScale,color);
            Texture = null;
            winWidth = Scenes.Scene.Viewport.Width;
            winHeight = Scenes.Scene.Viewport.Height;

        }
        public void Show()
        {
            Visible = true;
        }
        public override void Draw(GameTime gameTime)
        {
            if(Texture != null)
                scene.DrawImage(Texture, Parameters.Position, Clip, Parameters.Scale.X,Parameters.Scale.Y, Parameters.Color);
            if (Text != "")
                scene.DrawString(Text, Parameters.Position, Clip,Parameters.Color,Parameters.StringScale);
            base.Draw(gameTime);
        }
        public override void Update(GameTime gameTime)
        {
            //if (Enabled)
            //{
            //    if (frames++ >= showFrames)
            //    {
            //        Visible = false;
            //    }
            //}
            base.Update(gameTime);
        }
    }
}
