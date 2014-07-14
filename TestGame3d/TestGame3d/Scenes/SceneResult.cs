using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using _2DComponents;
using Extention;
using Microsoft.Xna.Framework;

namespace Tennis01.Scenes
{
    using Rules;
    using Objects;
    using _2DComponents;
    class SceneResult:Scene
    {
        enum CameraModes
        {
            Winner,
            Winner2,
            Loser,
            Loser2
        }
        CameraModes cameraMode;
        ScoreManager score;
        AnimatableLogo logo;
        AnimatableLogo[] scoresLogo;
        AnimatableLogo pressAny;
        Menu endMenu;
        Cursor cursor;
        Scene nextScene = null;
        int step = 0;
        int frames = 0;
        ScenePlaying playingScene;
        public SceneResult(ScoreManager s,string backModelName,string winnerName,ScenePlaying scenePlaying)
            : base(new Camera(new Vector3(0,0.27f*1.5f,0.27f*6),new Vector3(0,0.27f,0),45,Scene.Viewport),true,"")
        {
            cameraMode = (CameraModes)GameMain.Random.Next(0, 3);
            playingScene = scenePlaying;
            score = s;
            float posiX = 0;
            foreach (Player p in score.Winners)
            {
                p.Won();
                p.BodyDirection = new Vector2(0, 1);
                p.Position = new Vector3(posiX, 0, 0.27f*2);
                posiX -= p.Radius;
                //p.Camera = camera;
            }
            posiX = 0.27f;
            foreach (Player p in score.Losers)
            {
                p.Lost();
                p.BodyDirection = Vector2.Normalize(new Vector2(1, -1));
                p.Position = new Vector3(posiX, 0, -0.27f*2);
                posiX += p.Radius;
                //p.Camera = camera;
            }

            AddObjects(score.Winners);
            AddObjects(score.Losers);
            AddObjects(new Object3D(GameMain.Models[backModelName], camera, Matrix.Identity));
            //勝者を表示するロゴ
            logo = new AnimatableLogo(GameMain.LogoFont,Game.WindowRect,Vector2.One*0.5f,new Vector2(0.5f,0.1f),new Animation[]{
                new Animation("animation",new AnimationKey[]{
                    new AnimationKey(0,new LogoParams(null,"Winner "+winnerName,Vector2.Zero,Vector2.One*0.5f,0f.LogoSize(),new Rotation(720),new Color(0,0,0,0),Color.Transparent),RotationWays.RELATIVE),
                    new AnimationKey(90,new LogoParams(null,"Winner "+winnerName,Vector2.Zero,Vector2.One*0.5f,1f.LogoSize(),Rotation.Zero,Color.White,Color.Red),RotationWays.ABSOLUTE),
                })
            });
            logo.Animate("animation", 1,true);
            //スコアを表示するロゴ
            scoresLogo = new AnimatableLogo[score.PastScores.Length];
            for (int i = 0; i < scoresLogo.Length; i++)
            {
                scoresLogo[i] = new AnimatableLogo(GameMain.LogoFont,Game.WindowRect,Vector2.One*0.5f,new Vector2(0.5f, 0.25f + i * 0.1f),
                    new Animation("animation", new AnimationKey[]{
                        new AnimationKey(0,new LogoParams(null,"Set"+ (i+1)+" : " +score.PastScores[i],new Vector2(i%2==1?1:-1,0),new Vector2(i % 2 == 1?0:1,0),new Vector2(0.25f,0.32f),Rotation.Zero,new Color(0,0,0,0),Color.Transparent),RotationWays.ABSOLUTE),
                        new AnimationKey(30,new LogoParams(null,"Set"+ (i+1)+" : " +score.PastScores[i],Vector2.Zero,Vector2.One*0.5f,new Vector2(0.25f,0.32f),Rotation.Zero,Color.White,Color.White),RotationWays.ABSOLUTE)
                    })
                );
                scoresLogo[i].Visible = false;
            }
            //ボタン入力催促ロゴ
            pressAny = new AnimatableLogo(GameMain.LogoFont, Game.WindowRect, new Vector2(1, 0.5f), new Vector2(1, 0.8f),
                new Animation("animation",
                    new AnimationKey(0, new LogoParams(null, "Press Any Button", Vector2.Zero, new Vector2(1, 0.5f), Vector2.One * 0.5f, Rotation.Zero, Color.Transparent, Color.Transparent), RotationWays.ABSOLUTE),
                    new AnimationKey(30, new LogoParams(null, "Press Any Button", Vector2.Zero, new Vector2(1, 0.5f), Vector2.One * 0.5f, Rotation.Zero, Color.Red, Color.Red), RotationWays.ABSOLUTE),
                    new AnimationKey(60, new LogoParams(null, "Press Any Button", Vector2.Zero, new Vector2(1, 0.5f), Vector2.One * 0.5f, Rotation.Zero, Color.Transparent, Color.Transparent), RotationWays.ABSOLUTE)
                )
            );
            //pressAny = new AnimatableLogo("Press Any Button", new Vector2(0.75f, 0.9f), this, new Animation[]{
            //    new Animation("animation",new AnimationKey[]{
            //        new AnimationKey(0,new LogoParams(Vector2.Zero,Vector2.One*0.1f,0.1f,new Color(0,0,0,0))),
            //        new AnimationKey(30,new LogoParams(Vector2.Zero,Vector2.One*0.1f,0.1f,Color.Red)),
            //        new AnimationKey(60,new LogoParams(Vector2.Zero,Vector2.One*0.1f,0.1f,new Color(0,0,0,0)))
            //    })
            //});
            pressAny.Visible = false;
            AddComponents(logo);
            AddComponents(scoresLogo);
            AddComponents(pressAny);

            //メヌーの作成
            cursor = MyCursor.GetStdCursor(0);//new MyCursor(GameMain.Textures["cursor"],Color.White,Game.WindowRect,Vector2.One*0.3f,Vector2.Zero,Controllers[0],Vector2.One*0.2f,1);
            MenuButtonParams[] mParams = new MenuButtonParams[]{
                new MenuButtonParams("Retry",GameMain.Textures["buttonBase"],true),
                new MenuButtonParams("Exit",GameMain.Textures["buttonBase"],true)
            };
            Action<MenuEventArgs> menuAction = (e) =>
            {
                switch (e.SelectedButtonName)
                {
                    case "Retry":
                        nextScene = playingScene.Copy(camera);
                        break;
                    case "Exit":
                        Game.Components.Clear();
                        nextScene = new SceneMainMenu(new SceneTitle());
                        break;
                }
            };
            Rectangle rect = Game.WindowRect;
            rect.Y = rect.Height/2;
            rect.Height /= 2;
            endMenu = new Menu("end", rect, Vector2.One * 0.5f, Vector2.One * 0.5f, Vector2.One, Color.White, GameMain.LogoFont, cursor, new GridLayout(2, 0.3f, 0.6f, mParams.Length), mParams);
            endMenu.ButtonPressed += menuAction;
            endMenu.Visible = endMenu.Enabled = false;
            AddComponents(endMenu, cursor);
        }
        public override void Update(GameTime gameTime)
        {
            debugCamera();
            cameraWork();
            GameMain.debugStr["cameraMode"] = cameraMode.ToString();
            if (HasAnyInput() || frames++ % 120 == 119)
            {
                if (step < scoresLogo.Length)
                {
                    scoresLogo[step++].Animate("animation", 1,true);
                }
                else if (step == scoresLogo.Length)
                {
                    pressAny.Animate("animation", 0,true);
                }
            }
            if (!endMenu.Visible && HasAnyInput() && step == scoresLogo.Length && (scoresLogo.Length == 0 || !scoresLogo.Last().IsAnimating))
            {
                endMenu.Enabled = endMenu.Visible = true;
                //System.Windows.Forms.MessageBox.Show("OK");
                Game.Components.Remove(pressAny);
            }
            base.Update(gameTime);
        }

        private void cameraWork()
        {
            switch (cameraMode)
            {
                case CameraModes.Winner:
                    Camera.Position = new Vector3(0, 0.27f * 1.5f, 0.27f * 6);
                    camera.Target = new Vector3(0, 0.27f, 0);
                    break;
                case CameraModes.Winner2:
                    Camera.Position = score.Winners[0].HeadPosition + new Vector3((float)Math.Sin(MathHelper.ToRadians(frames*0.3f)),0.27f*2,(float)Math.Cos(MathHelper.ToRadians(frames*0.3f)));
                    camera.Target = score.Winners[0].HeadPosition;
                    break;
                case CameraModes.Loser:
                    Camera.Position = new Vector3(0.1551f, 0.4807f, 0.9311f);
                    camera.Target = new Vector3(0, 0.27f, 0);
                    break;
                case CameraModes.Loser2:
                    camera.Target = score.Winners[0].HeadPosition;
                    Camera.Position = new Vector3(0.2807f, 0.2867f, -0.9133f);
                    break;
            }
        }
        public override void Draw(GameTime gameTime)
        {
            //2Dでスコア表示
            base.Draw(gameTime);
        }
        public override Scene NextScene
        {
            get 
            {
                //if (HasAnyInput() && step == scoresLogo.Length && !scoresLogo.Last().IsAnimating)

                //{
                //    return new SceneTitle();
                //}
                return nextScene;
            }
        }
    }
}
