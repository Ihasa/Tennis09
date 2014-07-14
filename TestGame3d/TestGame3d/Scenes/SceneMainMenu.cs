//#define Debug
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using _2DComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Tennis01.Scenes
{
    using Objects;
    using Input;
    class SceneMainMenu:Scene
    {
        MyCursor cursor,cursor2p;
        Menu menu;
        Scene nextScene = null;
        SceneTitle sceneTitle;
        MySlideShow htp;
        AnimatableLogo pages;
        static bool assist = true;
        Menu select1p = null, select2p = null, ruleEdit = null,pMenu = null;
        bool vsCom = true;
        Tuple <string,PlayerAbility> player1, player2;//初期化が必要なのに違和感
        Texture2D buttonTex;
        Button getButton(Texture2D buttonTex, string text)
        {
            return new Button(GameMain.LogoFont, Game.WindowRect, Vector2.Zero, Vector2.Zero, Vector2.One, buttonTex, Color.White, cursor, text);
        }

        
        public SceneMainMenu(SceneTitle prevScene)
            : base(
                prevScene.Camera,true,"pianoSong"
                )
        {
            //if (Game1.Wiimote.Connected)
            //{
            //    cursor = new WiiCursor(Game, "cursor",1,this);
            //    System.Windows.Forms.MessageBox.Show("WiiCursorを使用");
            //}
            //else
            cursor = MyCursor.GetStdCursor(0);// new MyCursor(GameMain.Textures["cursor"], Color.Blue, Game.WindowRect, Vector2.One * 0.2f, Vector2.Zero, Controllers[0], Vector2.One * 0.1f, 0);
            cursor2p = MyCursor.GetStdCursor(1);// new MyCursor(GameMain.Textures["cursor"], Color.Red, Game.WindowRect, Vector2.One * 0.2f, Vector2.Zero, Controllers[1], Vector2.One * 0.1f, 1);
            buttonTex = GameMain.Textures["buttonBase"];
            MenuButtonParams[] pMenuParams;
            if (Microsoft.Xna.Framework.Input.GamePad.GetState(PlayerIndex.One).IsConnected)
            {
                pMenuParams = new MenuButtonParams[]{
                    new MenuButtonParams("vs2P",buttonTex,true),
                    new MenuButtonParams("vsCom",buttonTex,true),
                    new MenuButtonParams("Cancel",buttonTex,true)
                };
            }
            else
            {
                pMenuParams = new MenuButtonParams[]{
                    new MenuButtonParams("vsCom",buttonTex,true),
                    new MenuButtonParams("Cancel",buttonTex,true)
                };
            }
            pMenu = new Menu("players", Game.WindowRect, Vector2.Zero, Vector2.Zero, Vector2.One, Color.White, GameMain.LogoFont, cursor, new GridLayout(pMenuParams.Length, 0.8f, 0.3f, pMenuParams.Length),
                pMenuParams
            );

            Func<Cursor, string, Menu> getTypeMenu = (c, name) =>
            {
                return new Menu(name, Game.WindowRect, Vector2.Zero, Vector2.Zero, Vector2.One, Color.White, GameMain.LogoFont, c, new GridLayout(3, 0.8f, 0.6f, 6),
                    new MenuButtonParams[]{
                        new MenuButtonParams("Standard",buttonTex,true),
                        new MenuButtonParams("HardHit",buttonTex,true),
                        new MenuButtonParams("Defence",buttonTex,true),
                        new MenuButtonParams("Counter",buttonTex,true),
                        new MenuButtonParams("BackHand",buttonTex,true),
                        new MenuButtonParams("Cancel",buttonTex,true)
                    }
                    );
            };

            Func<string, Tuple<string,PlayerAbility>> getAbility = (str) =>
            {
                string modelName;
                PlayerAbility ability;
                switch (str)
                {
                    case "Standard":
                        modelName = "human170";
                        ability = PlayerAbility.StandardType;
                        break;
                    case "HardHit":
                        modelName = "humanFat";
                        ability = PlayerAbility.HardHitType;
                        break;
                    case "Defence":
                        modelName = "boxMan";
                        ability = PlayerAbility.SpeedType;
                        break;
                    case "Counter":
                        modelName = "human";
                        ability = PlayerAbility.CounterType;
                        break;
                    case "BackHand":
                        modelName = "judgeMan";
                        ability = PlayerAbility.VolleyType;
                        break;
                    default:
                        modelName = "human170";
                        ability = PlayerAbility.ExtremeType;
                        break;
                }
                return new Tuple<string,PlayerAbility>(modelName,ability);
            };
            Action<MenuEventArgs> playerSelect = (me) =>
            {
                vsCom = false;
                switch (me.SelectedButtonName)
                {
                    case "vs2P":
                        select1p = getTypeMenu(cursor, "player1 Select");
                        select2p = getTypeMenu(cursor2p, "Select2");
                        break;
                    case "vsCom":
                        select1p = getTypeMenu(cursor, "player1 Select");
                        select2p = getTypeMenu(cursor, "Select2");
                        vsCom = true;
                        break;
                    case "Cancel":
                        pMenu.ReturnParentMenu(30);
                        return;
                    default:
                        select1p = null;
                        select2p = null;
                        return;
                }
                
                select1p.AddSubMenu(select2p);
                select1p.ButtonPressed += (me2) =>
                {
                    if (me2.SelectedButtonName != "Cancel")
                    {

                        player1 = getAbility(me2.SelectedButtonName);
                        select1p.CallSubMenu("Select2", 30);
                        if (!vsCom && !Game.Components.Contains(cursor2p))
                        {
                            AddComponents(cursor2p);
                            //cursor2p.Visible = cursor2p.Enabled = true;
                        }
                    }
                    else
                        select1p.ReturnParentMenu(30);
                };
                select2p.AddSubMenu(ruleEdit);
                select2p.ButtonPressed += (me3) =>
                {
                    player2 = getAbility(me3.SelectedButtonName);
                    if (me3.SelectedButtonName != "Cancel")
                    {
                        select2p.CallSubMenu("ruleEdit", 30);
                    }
                    else
                    {
                        select2p.ReturnParentMenu(30);
                    }
                };
                pMenu.ClearSubMenu();
                pMenu.AddSubMenu(select1p);
                pMenu.CallSubMenu("player1 Select", 30);
                AddComponents(select1p, select2p);

            };
            pMenu.ButtonPressed += playerSelect;

            MenuButtonParams[] mParams = new MenuButtonParams[]
            {
                new MenuButtonParams("Tennis!",buttonTex,true),
                new MenuButtonParams("Practice",buttonTex,true),
#if Debug
                new MenuButtonParams("Test Play",buttonTex,true),
#endif
                new MenuButtonParams("Options",buttonTex,true),
                new MenuButtonParams("How to Play?",buttonTex,true),
                new MenuButtonParams("Exit",buttonTex,true)
            };
            TuneButton spdTuner = new TuneButton("Racket Weight",Game.WindowRect, Vector2.Zero, Vector2.Zero, Vector2.One, cursor, buttonTex, 0.2f, 0.0f, 1.0f, 0.02f, (Player.Rate-0.8f) / 0.4f);
            //TuneButton weightTuner = new TuneButton("Racket Weight", Game.WindowRect, Vector2.Zero, Vector2.Zero, Vector2.One, cursor, buttonTex, 0.2f, 2, 12, 1, Player.Delay);
            SwitchButton assistSelect = new SwitchButton(Game.WindowRect, Vector2.Zero, Vector2.Zero, Vector2.One, cursor, 0.1f, "Assistant", Color.Red,assist ? 0 : 1,
                new MenuButtonParams("On", buttonTex, true),
                new MenuButtonParams("Off", buttonTex, true)
                );
            //SwitchButton sw = new SwitchButton(Game.WindowRect, Vector2.Zero, Vector2.Zero, Vector2.One, cursor, 0.1f,"Answer",Color.Red,
            //    new MenuButtonParams("A", buttonTex, true),
            //    new MenuButtonParams("B", buttonTex, true),
            //    new MenuButtonParams("C", buttonTex, true),
            //    new MenuButtonParams("D", buttonTex, true)
            //    );
            Button ok = new Button(GameMain.LogoFont, Game.WindowRect, Vector2.Zero, Vector2.Zero, Vector2.One, buttonTex, Color.White, cursor, "OK");
            Button reset = new Button(GameMain.LogoFont, Game.WindowRect, Vector2.Zero, Vector2.Zero, Vector2.One, buttonTex, Color.White, cursor, "Default");
            Component2D[] optionButtons = new Component2D[]{
                spdTuner,assistSelect,reset,ok
            };
            Menu optionMenu = new Menu("options", Game.WindowRect, Vector2.One * 0.5f, Vector2.One * 0.5f, Vector2.One, Color.White, GameMain.LogoFont, cursor, new GridLayout(1, 0.7f, 0.7f, optionButtons.Length), optionButtons);
            ok.Pressed += () =>
            {
                Player.Rate = MathHelper.Lerp(0.8f,1.2f,spdTuner.CurrentValue);
                Player.Delay = (int)MathHelper.Lerp(2,11,spdTuner.CurrentValue);
                assist = assistSelect.CurrentValue == "On";
                optionMenu.ReturnParentMenu(30);
                GameMain.debugStr["Player.Delay"] = "" + Player.Delay;
            };
            reset.Pressed += () =>
            {
                spdTuner.CurrentValue = 0.5f;
                assistSelect.ChangeCurrent(0);
            };
            SwitchButton games = new SwitchButton(Game.WindowRect, Vector2.Zero, Vector2.Zero, Vector2.One, cursor, 0.1f, "Games", Color.Green,0,
                new MenuButtonParams("2", buttonTex, true),
                new MenuButtonParams("4", buttonTex, true),
                new MenuButtonParams("6", buttonTex, true),
                new MenuButtonParams("8", buttonTex, true)
                );
            SwitchButton sets = new SwitchButton(Game.WindowRect, Vector2.Zero, Vector2.Zero, Vector2.One, cursor, 0.1f, "Sets", Color.Blue,0,
                new MenuButtonParams("1", buttonTex, true),
                new MenuButtonParams("3", buttonTex, true),
                new MenuButtonParams("5", buttonTex, true)
            );
            SwitchButton deuse = new SwitchButton(Game.WindowRect, Vector2.Zero, Vector2.Zero, Vector2.One, cursor, 0.1f, "Deuse", Color.Orange,0,
                new MenuButtonParams("On", buttonTex, true),
                new MenuButtonParams("Off", buttonTex, true)
            );
            SwitchButton tie = new SwitchButton(Game.WindowRect, Vector2.Zero, Vector2.Zero, Vector2.One, cursor, 0.1f, "TieBreak", Color.Orange,0,
                new MenuButtonParams("On", buttonTex, true),
                new MenuButtonParams("Off", buttonTex, true)
            );
            Button ruleOk = getButton(buttonTex, "Let's Play!");
            Button ruleCancel = getButton(buttonTex, "Cancel");
            Rules.MatchRule rule;
            ruleOk.Pressed += () =>
            {
                rule = new Rules.MatchRule(int.Parse(games.CurrentValue), int.Parse(sets.CurrentValue) / 2 + 1, deuse.CurrentValue == "On", tie.CurrentValue == "On");
                TennisCourt court = TennisCourt.Court1;
                Ball newBall = new Ball(GameMain.Models["ball"], camera, new Vector3(0, 2, 3 * TennisCourt.CourtLength), Vector3.Zero, court);
                nextScene = new ScenePlaying(TennisEvents.Singles, camera, newBall,
                    court, rule, assist,
                    new Player(GameMain.Models[player1.Item1], camera, new Vector3(0, 0, 3.3f), player1.Item2, new Vector2(0, -1), "ObjectSEs", "racket", newBall, 0),
                    new Player(GameMain.Models[player2.Item1], camera, new Vector3(0, 0, -3.3f), player2.Item2, new Vector2(0, 1), "ObjectSEs", "racket2", newBall, vsCom ? 2 : 1)
                );
            };

            ruleEdit = new Menu("ruleEdit",Game.WindowRect,Vector2.Zero,Vector2.Zero,Vector2.One,Color.White,GameMain.LogoFont,cursor,new GridLayout(1,0.6f,0.8f,6),
                new Component2D[]{games,sets,deuse,tie,ruleOk,ruleCancel});
            ruleCancel.Pressed += () =>
            {
                ruleEdit.ReturnParentMenu(30);
            };

            pages = new AnimatableLogo(GameMain.LogoFont, Game.WindowRect, Vector2.One, Vector2.One,
                new Animation("def",
                    new AnimationKey(0, new LogoParams(null, "",Vector2.Zero, Vector2.One, new Vector2(0.1f, 0.1f), Rotation.Zero, Color.White,Color.Black)),
                    new AnimationKey(120, new LogoParams(null, "",Vector2.Zero, Vector2.One, new Vector2(0.1f, 0.1f), Rotation.Zero, Color.Transparent,Color.Transparent))
                )
            );
            htp = new MySlideShow(Game.WindowRect,Controllers[0], GameMain.Textures["controller"], GameMain.Textures["basic"], GameMain.Textures["shots"],GameMain.Textures["smash"],GameMain.Textures["shotTech"]);
            htp.OnAdd += () =>
            {
                cursor.Enabled = cursor.Visible = false;
                menu.Enabled = menu.Visible = false;
                pages.Animate("def", 1, true);
                pages.Text = htp.CurrentSlide + "/" + htp.Slides;
                SoundEffect("swing4");
            };
            htp.OnRemove += () =>
            {
                cursor.Enabled = cursor.Visible = true;
                menu.Enabled = menu.Visible = true;
                pages.FeedOut(pages.Scale, 5, 0, new Vector2(1, 0));
            };
            htp.OnChange += () =>
            {
                pages.Animate("def", 1, true);
                pages.Text = htp.CurrentSlide + "/" + htp.Slides;
            };
            htp.OnNext += () =>
            {
                SoundEffect("swing4");
            };
            htp.OnBack += () =>
            {
                SoundEffect("swing3");
            };

            pages.DrawOrder = htp.DrawOrder + 1;
            //howToMenu = new Menu("howToPlay", Game.WindowRect, Vector2.One * 0.5f, Vector2.One * 0.5f, Vector2.One, Color.White, GameMain.LogoFont, cursor, new GridLayout(1, 1, 1, 1),
            //    new Component2D[]{htp}
            //);
            menu = new Menu("main", Game.WindowRect, Vector2.One * 0.5f, Vector2.One * 0.5f, Vector2.One, Color.White, GameMain.LogoFont, cursor, new GridLayout(3, 0.6f, 0.6f, mParams.Length),mParams,null
            );
            menu.AddSubMenu(optionMenu);
            menu.AddSubMenu(pMenu);
            //menu.AddSubMenu(howToMenu);
            menu.ButtonPressed += (e) => 
            {
                switch (e.SelectedButtonName)
                {
                    case "Tennis!":
                        menu.CallSubMenu("players", 30);
                        //TennisCourt court = TennisCourt.Court1;
                        //Ball newBall = new Ball(GameMain.Models["ball"], camera, new Vector3(0, 2, 3*TennisCourt.CourtLength), Vector3.Zero,court);
                        //nextScene = new ScenePlaying(TennisEvents.Singles, camera, newBall,
                        //    court, new Rules.MatchRule(2, 1, false, true),true,
                        //    new Player(GameMain.Models["human170"], camera, new Vector3(0, 0, 3.3f), PlayerAbility.StandardType, new Vector2(0, -1), "ObjectSEs", "racket3", newBall, 0),
                        //    new Player(GameMain.Models["human170"], camera, new Vector3(0, 0, -3.3f), PlayerAbility.StandardType, new Vector2(0, 1), "ObjectSEs", "racket2", newBall, 1)
                        //);
                        break;
                    case "Practice":
                        nextScene = new ScenePractice(new Camera(new Vector3(0, 0.27f * 20, TennisCourt.CourtLength * 3f)*0.9f, Vector3.Zero, 30, Viewport));
                        break;
#if Debug
                    case "Test Play":
                        nextScene = new TestScene(new Camera(new Vector3(0, 0.27f * 1.66f * 8, 9), Vector3.Zero, 20, Viewport));
                        break;
#endif
                    case "Options":
                        menu.CallSubMenu("options", 30);
                        break;
                    case "How to Play?":
                        htp.AddToGameComponents(Game);
                        break;
                    case "Exit":
                        Game.Exit();
                        break;
                }
            };
            sceneTitle = prevScene;
            AddComponents(sceneTitle.currentModel,cursor,menu,optionMenu,pages,ruleEdit,pMenu/*,howToMenu*/);
            menu.FeedIn(30, 0, new Vector2(1, 0));
        }
        public override void Update(GameTime gameTime)
        {
            //Scene.Updateは一回だけ呼び出す
            //sceneTitle.Update(gameTime);
            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            //OK.
            sceneTitle.Draw(gameTime);
            base.Draw(gameTime);
        }
        public override Scene NextScene
        {
            get { return nextScene; }
        }

    }
}
