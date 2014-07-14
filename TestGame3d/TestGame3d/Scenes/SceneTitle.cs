using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tennis01.Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using _2DComponents;
namespace Tennis01.Scenes
{
    class SceneTitle:Scene
    {
        Model[] courts;
        public Object3D currentModel { get; set; }
        CameraModes cameraMode;
        AnimatableLogo titleLogo;
        AnimatableLogo pressAny;
        static int animeNum;
        static SceneTitle()
        {
            animeNum = GameMain.Random.Next(0, 4);
        }
        Animation getAnimation()
        {
            Texture2D titleTex = GameMain.Textures["title"];
            Vector2 ori = new Vector2(0.5f, 0.5f);
            Vector2 scale = Vector2.One*0.75f;
            Animation animation = null;
            int randValue = animeNum;// GameMain.Random.Next(0, 4);
            if (++animeNum >= 4)
            {
                animeNum = 0;
            }
            switch(randValue){
                case 0:
                    animation = new Animation("anime",
                        new AnimationKey(0, new LogoParams(titleTex, new Vector2(-0.3f, 0.6f), ori, scale * 0.1f, new Rotation(360*2), Color.White), RotationWays.RELATIVE,Interpolations.SMOOTHSTEP,Interpolations.LERP),
                        new AnimationKey(20, new LogoParams(titleTex, new Vector2(0.3f, -0.25f), ori, scale * 0.1f, new Rotation(360 * 2), Color.White), RotationWays.RELATIVE, Interpolations.SMOOTHSTEP, Interpolations.LERP),
                        new AnimationKey(40, new LogoParams(titleTex, new Vector2(0.3f, 0.3f), ori, scale * 0.2f, new Rotation(360 * 2), Color.White), RotationWays.RELATIVE, Interpolations.SMOOTHSTEP, Interpolations.LERP),
                        new AnimationKey(60, new LogoParams(titleTex, new Vector2(-0.3f, -0.3f), ori, scale * 0.4f, new Rotation(360 * 2), Color.White), RotationWays.RELATIVE, Interpolations.SMOOTHSTEP, Interpolations.LERP),
                        new AnimationKey(80, new LogoParams(titleTex, new Vector2(-0.3f, 0.4f), ori, scale * 0.8f, new Rotation(360 * 2), Color.White), RotationWays.RELATIVE, Interpolations.SMOOTHSTEP, Interpolations.LERP),
                        new AnimationKey(100, new LogoParams(titleTex, Vector2.Zero, ori, Vector2.One * 2f, Rotation.Zero, Color.White)),
                        new AnimationKey(120, new LogoParams(titleTex, Vector2.Zero, ori, scale, Rotation.Zero, Color.White))
                    );
                    animation.Animating += (time) =>
                    {
                        if (time <= 90 && time % 20 == 0)
                        {
                            SoundEffect("shot");
                        }
                        else if (time == 119)
                        {
                            SoundEffect("explode");
                        }
                    };
                    break;
                case 1:
                    animation = new Animation("anime",
                        new AnimationKey(0,new LogoParams(titleTex,new Vector2(0,0),ori,scale*0.1f,new Rotation(360 * 4+60),Color.Transparent),RotationWays.RELATIVE),
                        new AnimationKey(60,new LogoParams(titleTex,Vector2.Zero,ori,scale,Rotation.Zero,Color.White)),
                        new AnimationKey(80,new LogoParams(titleTex,Vector2.Zero,ori,scale,new Rotation(-60),Color.White),RotationWays.RELATIVE),
                        new AnimationKey(120,new LogoParams(titleTex,Vector2.Zero,ori,scale,Rotation.Zero,Color.White))
                        );
                    animation.Animating += (time) =>
                    {
                        if (time <= 60 && time % 10 == 0)
                        {
                            SoundEffect("swing2");
                        }
                        else if (time == 110)
                        {
                            SoundEffect("swing6");
                        }
                    };

                    break;
                case 2:
                    animation = new Animation("anime",
                        new AnimationKey(0,new LogoParams(titleTex,new Vector2(0,0),ori,scale * new Vector2(1.0f,1.0f),Rotation.Zero,Color.White)),
                        new AnimationKey(50, new LogoParams(titleTex, new Vector2(0, 0), ori, scale * new Vector2(3.0f, 0.333f), Rotation.Zero, Color.White)),
                        new AnimationKey(60, new LogoParams(titleTex, new Vector2(0, 0), ori, scale * new Vector2(1.0f, 1.5f), Rotation.Zero, Color.White)),
                        new AnimationKey(70, new LogoParams(titleTex, new Vector2(0, 0), ori, scale * new Vector2(1.5f, 1), Rotation.Zero, Color.White)),
                        new AnimationKey(80, new LogoParams(titleTex, new Vector2(0, 0), ori, scale * new Vector2(1.0f, 1.25f), Rotation.Zero, Color.White)),
                        new AnimationKey(90, new LogoParams(titleTex, new Vector2(0, 0), ori, scale * new Vector2(1.25f, 1.0f), Rotation.Zero, Color.White)),
                        new AnimationKey(100, new LogoParams(titleTex, new Vector2(0, 0), ori, scale * new Vector2(1.0f, 1.125f), Rotation.Zero, Color.White)),
                        new AnimationKey(110, new LogoParams(titleTex, new Vector2(0, 0), ori, scale * new Vector2(1.125f, 1.0f), Rotation.Zero, Color.White)),
                        new AnimationKey(120, new LogoParams(titleTex, new Vector2(0, 0), ori, scale * new Vector2(1.0f, 1.0f), Rotation.Zero, Color.White))
                        );
                    break;
                case 3:
                    animation = new Animation("anime",
                        new AnimationKey(0,new LogoParams(titleTex,new Vector2(0.0f,0),ori,scale*0,Rotation.Zero,Color.Transparent)),
                        new AnimationKey(40,new LogoParams(titleTex,new Vector2(0,0),ori,scale * new Vector2(1.0f,0.05f),Rotation.Zero,Color.Black)),
                        new AnimationKey(70, new LogoParams(titleTex, new Vector2(0, 0), ori, scale, Rotation.Zero, Color.Black)),
                        new AnimationKey(120,new LogoParams(titleTex,new Vector2(0,0),ori,scale,Rotation.Zero,Color.White))
                        );
                    animation.Animating += (time) =>
                    {

                    };
                    break;

            }
            return animation;
        }
        public SceneTitle()
            : base(
                new Camera(Vector3.Zero, Vector3.Forward,45,Viewport),true,""
                )
        {
            courts = new Model[]{
                GameMain.Models["CenterCourt"],GameMain.Models["tennisCourt_big"],GameMain.Models["tennisCourt_big2"]
            };
            currentModel = new Object3D(courts[2],camera);
            AddComponents(currentModel);
            cameraMode = CameraModes.Top;

            Texture2D titleTex = GameMain.Textures["title"];
            Vector2 ori = new Vector2(0.5f,0.5f);
            float scale = 0.75f;

            Animation animation = getAnimation();
            
            titleLogo = new AnimatableLogo(GameMain.LogoFont, Game.WindowRect, ori, new Vector2(0.5f,scale/2),
                animation
            );
            Vector2 ori2 = new Vector2(0.5f,1);
            Vector2 strScale = new Vector2(0.75f, 0.1f);
            pressAny = new AnimatableLogo(GameMain.LogoFont, Game.WindowRect, ori, new Vector2(0.5f, 0.98f),
                new Animation("anime",
                    new AnimationKey(0,new LogoParams(null,"Press Any Button!",Vector2.Zero,ori2,strScale,Rotation.Zero,Color.White,Color.Transparent)),
                    new AnimationKey(120,new LogoParams(null,"Press Any Button!",Vector2.Zero,ori2,strScale,Rotation.Zero,Color.White,Color.Red)),
                    new AnimationKey(240,new LogoParams(null,"Press Any Button!",Vector2.Zero,ori2,strScale,Rotation.Zero,Color.White,Color.Transparent))
                )
            );
            AddComponents(titleLogo,pressAny);
            titleLogo.EndAnimating += () =>
            {
                pressAny.Animate("anime", 0, true);
            };
            titleLogo.Animate("anime", 1, true);
        }
        public override void Update(GameTime gameTime)
        {
            //if (Controllers[0].GetState().Button3 == Input.ControlerButtonStates.Pressed)
            //{
            //    titleLogo.Animate("anime", 1, true);
            //}
            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            float x = (float)Math.Sin(MathHelper.ToRadians((float)gameTime.TotalGameTime.TotalMilliseconds / 100));
            float z = (float)Math.Cos(MathHelper.ToRadians((float)gameTime.TotalGameTime.TotalMilliseconds / 100));
            switch (cameraMode)
            {
                case CameraModes.Debug:
                    Vector3 vec = new Vector3(GameMain.gamePadStates[0][0].ThumbSticks.Right.X / 10.0f, GameMain.gamePadStates[0][0].ThumbSticks.Right.Y / 10, GameMain.gamePadStates[0][0].Triggers.Left - GameMain.gamePadStates[0][0].Triggers.Right);
                    camera.Position += vec;
                    //camera.Position = new Vector3(players[0].Position.X, camera.Position.Y, players[0].Position.Z);
                    //camera.Position + Vector3.Forward;
                    break;
                case CameraModes.Fixed:
                    //最初に設定したまま動かさない
                    break;
                case CameraModes.JudgeMan:
                    camera.Position = new Vector3(-2.06f, 0.7762f, 0);
                    camera.Target = camera.Position + new Vector3(x, 0, z);
                    camera.FieldOfView = 45;
                    break;
                case CameraModes.Top:
                    camera.Position = new Vector3(x * 7.2f, 7.2f, z * 7.2f);
                    camera.Target = Vector3.Zero;
                    camera.FieldOfView = 30;
                    break;
                case CameraModes.Rotation:
                    camera.Position = new Vector3(0, 0.27f * 1.70f, 2.7f);
                    camera.Target = camera.Position + new Vector3(x, 0, z);
                    camera.FieldOfView = 60;
                    break;
            }
            base.Draw(gameTime);
        }
        public override Scene NextScene
        {
            get
            {
                if (buttonPressed() && !titleLogo.IsAnimating)
                {
                    return new SceneMainMenu(this);
                }
                return null;
            }
        }
        bool buttonPressed()
        {
            for (int i = 0; i < Controllers.Length; i++)
            {
                if (Controllers[i].GetState().HasAnyInput(Input.ControllerState.Inputs.FrontButtons))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
