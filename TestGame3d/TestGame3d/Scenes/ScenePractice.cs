using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace Tennis01.Scenes
{
    using Objects;
    using _2DComponents;
    class ScenePractice : Scene
    {
        Ball currentBall;
        List<Ball> balls;
        int ballMax = 5;
        int current = 0;
        Menu menu;
        Cursor cursor;
        TennisCourt court;
        TennisMachine tennisMachine;
        Player player;
        HitChecker hitChecker;
        bool pause = true;
        Scene nextScene = null;
        Action<float, float, int> machineShot;
        List<Target> targets;
        bool force = false;
        bool shotNext
        {
            get
            {
                if (!pause)
                {
                    if (currentBall != null && currentBall.Bounds >= 2)
                    {
                        return true;
                    }
                    return force;
                }
                return false;
            }
        }
        struct MachineParams
        {
            public Vector3 MoveLimit;
            public float ShotLimit;
            public int ShotDelay;
            public MachineParams(Vector3 moveLimit, float shotLimit, int shotDelay)
            {
                MoveLimit = moveLimit;
                ShotLimit = shotLimit;
                ShotDelay = shotDelay;
            }
        }
        MachineParams machineParams;
        public ScenePractice(Camera c)
            : base(c, true, "pianoSong")
        {
            court = TennisCourt.Court2;
            balls = new List<Ball>();
            Ball firstBall = new Ball(GameMain.Models["ball"], c, new Vector3(0, 0.27f, -TennisCourt.CourtLength), new Vector3(0, 0, 0), court);
            balls.Add(firstBall);
            currentBall = firstBall;
            player = new Player(GameMain.Models["human170"], c, new Vector3(0, 0, TennisCourt.CourtLength), PlayerAbility.StandardType, new Vector2(0, -1), "ObjectSEs", "racket3", currentBall, 0);
            AddObjects( player,new Object3D(GameMain.Models[court.ModelName],c,Matrix.Identity));
            player.AddToGameComponents(this,true);

            hitChecker = new HitChecker(court, firstBall, player);
            tennisMachine = new TennisMachine(GameMain.Models["tennisMachine"], c, new Vector3(0, 0.27f, -TennisCourt.CourtLength * 0.8f), currentBall);
            tennisMachine.Shot += () =>
            {
                Vector3 limit = Vector3.One - machineParams.MoveLimit;
                tennisMachine.Move(
                    new Vector3(
                        ((float)GameMain.Random.NextDouble() * TennisCourt.SinglesWidth - TennisCourt.SinglesWidth * 0.5f) * limit.X,
                        (float)(GameMain.Random.NextDouble()-0.5f) * 0.27f*2f*limit.Y +0.27f + 0.14f,
                        (float)GameMain.Random.NextDouble() * TennisCourt.CourtLength*limit.Z * 0.75f - TennisCourt.CourtLength*0.85f
                    ), 
                    30
                );
            };
            AddObjects(tennisMachine);
            AddComponents(hitChecker, tennisMachine.Smoke,tennisMachine.Smoke2);
            AddObjects(firstBall);
            AddComponents(currentBall.ShotEffect, currentBall.Fire, currentBall.BoundPointEffect,currentBall.BoundEffect);
         
            //メニューを作って、いろいろ練習
            MenuButtonParams[] mParams = new MenuButtonParams[]{
                new MenuButtonParams("Basic",GameMain.Textures["buttonBase"],true),
                new MenuButtonParams("Top Spin",GameMain.Textures["buttonBase"],true),
                new MenuButtonParams("Slice Spin",GameMain.Textures["buttonBase"],true),
                new MenuButtonParams("Chance Ball",GameMain.Textures["buttonBase"],true),
                new MenuButtonParams("Smash",GameMain.Textures["buttonBase"],true),
                new MenuButtonParams("Defence",GameMain.Textures["buttonBase"],true),
                new MenuButtonParams("Random",GameMain.Textures["buttonBase"],true),
                new MenuButtonParams("Exit",GameMain.Textures["buttonBase"],true)
            };
            cursor = MyCursor.GetStdCursor(0);// new MyCursor(GameMain.Textures["cursor"], Color.Red, Game.WindowRect, Vector2.One * 0.3f, Vector2.Zero, Controllers[0], Vector2.One * 0.05f, 0);
            //cursor.Enabled = cursor.Visible = false;
            menu = new Menu("Practice Menu", Game.WindowRect, Vector2.Zero, Vector2.Zero, Vector2.One, Color.White, GameMain.LogoFont, cursor, new GridLayout(2, 0.7f, 0.7f, mParams.Length), mParams);
            menu.ButtonPressed += (e) =>
            {
                switch (e.SelectedButtonName)
                {
                    case "Basic":
                        machineShot = tennisMachine.SlowBall;
                        machineParams = new MachineParams(new Vector3(1, 1, 1), 0.5f, 20);
                        break;
                    case "Top Spin":
                        machineShot = tennisMachine.TopSpin;
                        machineParams = new MachineParams(new Vector3(0.5f, 0.25f, 0.5f), 0.5f, 20);
                        break;
                    case "Slice Spin":
                        machineShot = tennisMachine.SliceSpin;
                        machineParams = new MachineParams(new Vector3(0.5f, 0.25f, 0.5f), 0.5f, 20);
                        break;
                    case "Chance Ball":
                        machineShot = tennisMachine.ChanceBall2;
                        machineParams = new MachineParams(new Vector3(0.75f, 1, 0.25f), 0.5f, 20);
                        break;
                    case "Smash":
                        machineShot = tennisMachine.Lob;
                        machineParams = new MachineParams(new Vector3(0, 0, 0.5f), 0.75f, 20);
                        break;
                    case "Defence":
                        machineShot = (side,height,delay) =>
                        {
                            if (GameMain.Random.Next(0, 10) == 0)
                            {
                                tennisMachine.Drop(side, height, delay);
                            }
                            else
                            {
                                tennisMachine.SpeedBall(side, height, delay);
                            }
                        };
                        machineParams = new MachineParams(new Vector3(0.2f, 0.75f, 0.6f), 0.3f, 20);
                        break;
                    case "Random":
                        machineShot = (side, height, delay) =>
                        {
                            switch (GameMain.Random.Next(0, 6))
                            {
                                case 0:
                                    tennisMachine.TopSpin(side, height, delay);
                                    break;
                                case 1:
                                    tennisMachine.SliceSpin(side, height, delay);
                                    break;
                                case 2:
                                    tennisMachine.Drop(side, height, delay);
                                    break;
                                case 3:
                                    tennisMachine.Lob(side, height, delay);
                                    break;
                                case 4:
                                    tennisMachine.SpeedBall(side, height, delay);
                                    break;
                                case 5:
                                    tennisMachine.ChanceBall(side, height, delay);
                                    break;
                            }
                        };
                        machineParams = new MachineParams(new Vector3(0, 0, 0.3f), 0.3f, 20);
                        break;
                    case "Exit":
                        nextScene = new SceneMainMenu(new SceneTitle());
                        break;
                }
                player.Visible = player.Enabled = true;
                player.SetPosture(new Vector3(0, 0, TennisCourt.CourtLength), new Vector2(0, -1));
                menu.FeedOut(30,0,new Vector2(1,0));
                Scene.IsEnableControllers = false;
            };
            menu.FeedIning += () =>
            {
                player.Visible = player.Enabled = false;
                cursor.Visible = cursor.Enabled = true;
            };
            menu.FeedOuting += () =>
            {
                Scene.IsEnableControllers = true;
                pause = false;
                hit = true;
                force = true;
                //cursor.FeedOut(cursor.Scale, 30, 0, new Vector2(-1,0));
                cursor.Visible = cursor.Enabled = false;
            };
            cursor.FeedOuting += () =>
            {
                cursor.Visible = cursor.Enabled = false;
            };
            menu.FeedIn(30, 30, new Vector2(1, 0));
            AddComponents(menu,cursor);
            player.Visible = player.Enabled = false;

            //Targetの設置
            targets = new List<Target>();
            targets.Add(new Target(GameMain.Models["targetB"], new Vector3(TennisCourt.SinglesWidth*0.5f, 0.27f * 1.5f, -TennisCourt.CourtLength-0.27f), camera, currentBall,3));
            targets.Add(new Target(GameMain.Models["targetR"], new Vector3(TennisCourt.SinglesWidth * 0.0f, 0.27f * 1.5f, -TennisCourt.CourtLength - 0.27f), camera, currentBall, 3,0.27f,0.5f));
            targets.Add(new Target(GameMain.Models["targetB"], new Vector3(-TennisCourt.SinglesWidth*0.5f, 0.27f*1.5f, -TennisCourt.CourtLength-0.27f), camera, currentBall, 3));
            //targets.Add(new Target(GameMain.Models["targetB"], new Vector3(TennisCourt.SinglesWidth * 0.3f, 0.27f * 1.5f, -TennisCourt.CourtLength * 0.5f), camera, currentBall, 2,0));
            //targets.Add(new Target(GameMain.Models["targetR"], new Vector3(-TennisCourt.SinglesWidth * 0.3f, 0.27f * 1.5f, -TennisCourt.CourtLength * 0.5f), camera, currentBall, 2,0));
            //ターゲットに当たった時のanimatableLogo
            AnimatableLogo combo = new AnimatableLogo(GameMain.LogoFont, Game.WindowRect, Vector2.One * 0.5f, Vector2.Zero,
                new Animation("combo",
                    new AnimationKey(0, new LogoParams(null, "", Vector2.Zero, Vector2.One * 0.5f, Vector2.Zero, Rotation.Zero, Color.White, Color.White), RotationWays.RELATIVE),
                    new AnimationKey(10, new LogoParams(null, "", Vector2.Zero, Vector2.One * 0.5f, Vector2.One * 0.2f, Rotation.Zero, Color.White, Color.Red), RotationWays.RELATIVE),
                    new AnimationKey(20, new LogoParams(null, "", new Vector2(0, -0.01f), Vector2.One * 0.5f, Vector2.One * 0.2f, Rotation.Zero, Color.White, Color.Yellow), RotationWays.RELATIVE),
                    new AnimationKey(30, new LogoParams(null, "", new Vector2(0, -0.01f), Vector2.One * 0.5f, Vector2.One * 0.2f, Rotation.Zero, Color.White, Color.Red), RotationWays.RELATIVE),
                    new AnimationKey(60, new LogoParams(null, "", new Vector2(0, -0.01f), Vector2.One * 0.5f, Vector2.One * 0.2f, Rotation.Zero, Color.White, Color.Red), RotationWays.RELATIVE),
                    new AnimationKey(80, new LogoParams(null, "", new Vector2(0, -0.03f), Vector2.One * 0.5f, Vector2.One * 0.2f, Rotation.Zero, Color.White, Color.Transparent), RotationWays.RELATIVE)
               )
            );

            AddComponents(combo);

            for (int i = 0; i < targets.Count; i++)
            {
                Target t = targets[i];
                AddObjects(t);
                AddComponents(t.Explode);
                t.Hitted += () =>
                {
                    hit = true;
                };
                t.Dead += () =>
                {
                    combo.Text = (++this.combo) + " Combo!";
                    Vector2 position = t.ScreenPosition;
                    combo.DefaultPosition = position / new Vector2(Game.WindowRect.Width, Game.WindowRect.Height);
                    combo.Animate("combo", 1, true);
                    t.Scale = MathHelper.Clamp(MathHelper.Lerp(3,1,this.combo / 50.0f),1,3);// 3 * MathHelper.Clamp(1.0f / (this.combo + 1), 0.5f, 1);
                };
            }
            //foreach (Target t in targets)
            //{
            //    AddObjects(t);
            //    AddComponents(t.Explode);
            //    t.Hitted += () =>
            //    {
            //        hit = true;
            //    };
            //    t.Dead += () =>
            //    {
            //        combo.Text = (++this.combo) + " Combo!";
            //        Vector2 position = t.ScreenPosition;
            //        combo.DefaultPosition = position / new Vector2(Game.WindowRect.Width, Game.WindowRect.Height);
            //        combo.Animate("combo", 1, true);
            //    };
            //}
        }
        int combo = 0;
        bool hit = true;
        void changeBall()
        {
            if(currentBall != null)
            {
                Game.Components.Remove(currentBall.ShotEffect);
                Game.Components.Remove(currentBall.Fire);
                Game.Components.Remove(currentBall.BoundPointEffect);
                Game.Components.Remove(currentBall.BoundEffect);
                Game.Components.Remove(currentBall.Shadow);
                resetBall();
            }
            if (balls.Count >= ballMax)
            {
                currentBall = balls[current++];
                if (current >= balls.Count)
                {
                    current = 0;
                }
            }
            else
            {
                Ball b = new Ball(GameMain.Models["ball"], camera, tennisMachine.Position, Vector3.Zero, court);
                balls.Add(b);
                currentBall = b;
                AddObjects(b);
                hitChecker.AddBall(b);
            }
            player.Ball = currentBall;
            player.TargetObject = currentBall;
            tennisMachine.Ball = currentBall;
            foreach (Target target in targets)
            {
                target.Ball = currentBall;
                target.Active = true;
            }
            resetBall();
            AddComponents(currentBall.ShotEffect, currentBall.Fire, currentBall.BoundPointEffect,currentBall.BoundEffect);
            if (!Game.Components.Contains(currentBall.Shadow))
            {
                AddComponents(currentBall.Shadow);
            }
            //ターゲット
            if (!hit)
            {
                this.combo = 0;
                foreach (Target t in targets)
                {
                    t.Scale = 3;
                    t.Active = true;
                }
                
            }
            hit = false;
            force = false;
        }
        void resetBall()
        {
            currentBall.Shadow.Reset();
            currentBall.BoundEffect.Reset();
            currentBall.BoundPointEffect.Reset();
        }
        public override void Update(GameTime gameTime)
        {
            debugCamera();
            //moveCamera();
            Input.ControllerState state = Controllers[0].GetState();
            if (!pause)
            {
                if (state.Pause == Input.ControlerButtonStates.Pressed)
                {
                    menu.FeedIn(30, 0, new Vector2(1, 0));
                    tennisMachine.Cancel();
                    pause = true;
                    //changeBall();
                    //currentBall.Init(Vector3.One);
                    //tennisMachine.SlowBall(0, 0, 60);
                    //tennisMachine.SnipeBall(player, 0, 30);
                }
            }
            if (shotNext && machineShot != null)
            {
                changeBall();
                //currentBall.Init(Vector3.One);
                machineShot(/*((float)GameMain.Random.NextDouble() * 2 - 1)*/(GameMain.Random.Next(0,2) == 0 ? -1 : 1) * (1 - machineParams.ShotLimit), 0, machineParams.ShotDelay);
            }
            if (pause && state.L == Input.ControlerButtonStates.Pressed)
            {
                camera.Position = new Vector3(camera.Position.X, camera.Position.Y, -camera.Position.Z);
            }
            GameMain.debugStr["p.Enabled"] = "" + player.Enabled;
            base.Update(gameTime);
        }
        void moveCamera()
        {
            GamePadState state = GameMain.gamePadStates[0][0];
            float posiY = camera.Position.Y + state.ThumbSticks.Right.Y * 0.27f * 0.5f;
            if (posiY < 2.3f)
                posiY = 2.3f;
            if (posiY > Math.Abs(camera.Position.Z/2))
                posiY = Math.Abs(camera.Position.Z/2);
            camera.Position = new Vector3(camera.Position.X, posiY, camera.Position.Z);
        }
        public override Scene NextScene
        {
            get 
            {
                return nextScene;          
            }
        }
    }
}
