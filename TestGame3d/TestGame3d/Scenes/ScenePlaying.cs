using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Tennis01.Objects;
using Tennis01.Input;
using Tennis01.Rules;
using Extention;
using _2DComponents;
namespace Tennis01.Scenes
{
    /// <summary>
    /// テニスの種目を表す
    /// </summary>
    enum TennisEvents
    {
        Singles,Doubles
    }
    enum CameraModes
    {
        Fixed,
        BackOfPlayer1,
        NearPlayer1,
        FPVPlayer1,
        BackOfPlayer2,
        NearPlayer2,
        FPVPlayer2,
        JudgeMan,
        Top,
        Kinect,
        Rotation,
        Debug
    }

    //テニスプレイ中のシーン
    class ScenePlaying:Scene
    {
        /// <summary>
        /// 使用されるボール
        /// </summary>
        Ball ball;
        /// <summary>
        /// プレーする選手たち
        /// </summary>
        Player[] players;
        /// <summary>
        /// 審判
        /// </summary>
        Human MrJudge;
        /// <summary>
        /// 現在のサーバーとレシーバー
        /// </summary>
        Player server, receiver;
        /// <summary>
        /// 現在のカメラワーク
        /// </summary>
        CameraModes cameraMode;
        /// <summary>
        /// カメラの初期位置
        /// </summary>
        Vector3 defaultCameraPosition;
        /// <summary>
        /// 使用するテニスコート
        /// </summary>
        TennisCourt tennisCourt;
        /// <summary>
        /// 現在の種目
        /// </summary>
        TennisEvents events;
        /// <summary>
        /// 現在のサイド
        /// </summary>
        CourtSide courtSide;
        bool assist;
        /// <summary>
        /// ロゴ系
        /// </summary>
        AnimatableLogo stateLogo,scoreLogo;
        AnimatableLogo start;
        AnimatableLogo cc;
        #region スコア管理
        /// <summary>
        /// 点数管理
        /// </summary>
        ScoreManager scores;
        /// <summary>
        /// この試合のルール
        /// </summary>
        MatchRule matchRule;
        /// <summary>
        /// ポイント終了時のball.LastHitter
        /// </summary>
        Player lastHitter;
        /// <summary>
        /// ボールの状態フラグのコレクション
        /// </summary>
        Dictionary<BallState, bool> ballStates = new Dictionary<BallState, bool>();
        /// <summary>
        /// ポイント終了したかどうか
        /// </summary>
        bool end = false;
        /// <summary>
        /// 次のポイントへ行くまでの猶予フレームとそのカウンタ
        /// </summary>
        int endFrames=120,frames;
        /// <summary>
        /// フォルト回数
        /// </summary>
        int faults = 0;
        bool pointEnd = false;
        bool gameEnd = false;
        bool isLet = false;
        class BallState
        {
            public readonly Action Action;
            string explain;
            public BallState(Action action,string explain)
            {
                this.Action = action;
                this.explain = explain;
            }
            public override string ToString()
            {
                return explain;
            }
        }
        /// <summary>
        /// ポイント終了のきっかけとなったボールの状態フラグ
        /// </summary>
        BallState pointEndReason;
        BallState Active = new BallState(() => { },"");
        BallState Net;
        BallState Out;
        BallState Fault;
        BallState Let;
        BallState TwoBounds;
        BallState NoBoundReceive;
        BallState Other1;
        #endregion

        /// <summary>
        /// このインスタンスの初期化されたコピー
        /// </summary>
        /// <returns></returns>
        public ScenePlaying Copy(Camera camera)
        {
            Ball b = this.ball.Copy(camera);
            foreach (Player p in players)
            {
                p.Ball = b;
            }
            return new ScenePlaying(events, camera, b, tennisCourt, matchRule,assist,players);
        }
        public ScenePlaying(TennisEvents events,Camera c,Ball b,TennisCourt tennisCourt,MatchRule rule,bool assist, params Player[] playersList):base
        (
            c,true,"veryGrandMusic"
        )
        {
            if (InvalidPlayers(events, playersList))
            {
                throw new ArgumentException("プレイヤーの数が無効です");
            }
            this.events = events;
            this.tennisCourt = tennisCourt;
            this.assist = assist;
            //カメラをいじる
            camera.Position = new Vector3(0, 7.11843f, 16.82682f);
            camera.FieldOfView = 13;

            //プレイヤーを登録
            players = playersList;
            AddObjects(players);
            //プレイヤーの付属品を追加
            //プレイヤーの視線の先にテニスボール
            //プレイヤー準備
            foreach (Player p in players)
            {
                p.TargetObject = b;
                p.Play();
                p.AddToGameComponents(this,assist);
                //AddComponents(p.Belonging, p.SlideEffect, p.ShotMark, p.SmashPoint2, p.SmashPoint);
            }

            //その他オブジェクトを登録
            ball = b;
            //Player player3 = new Player(Game1.Models["iroiroTest2"], new Vector3(0, 0, 0), new PlayerAbility(0.09f, 0.03f, 0.06f, 90, 0), new Vector2(0, 1), "TestSounds", "racket", ball,2);
            AddObjects(new Object3D(GameMain.Models[tennisCourt.ModelName],c)/*,new Object3D(Game1.Models["NormalCourt_Wall"])*/);

            ball.AddToComponents(this, assist);
            
            //コントロール対象の設定
            AddControleables(players);
            
            //コントローラを設定
            //Controlers[0] = new TestKinectControler4(0);//体の動きで移動、腕を振ってショット
            //Controlers[0] = new TestKinectControler3(0,players[0],ball);//腕がジョイスティック
            //Controlers[0] = new TestKinectControler2(0,players[0], players[1], ball);//腕を振ってショット。移動は自動
            //Controlers[0] = new TestKinectControler(0);//腕で操作、ショット
            //Controllers[0] = new TestAIControler(players[0],players[1], ball,0,0,(int)(0 / Player.Rate/Player.Rate));
            //Controllers[0] = new WiimoteController(players[0],WiimoteLib.InputReport.IRExtensionAccel);
            Controllers[2] = new TestAIControler(players[1], players[0], ball,0,0,(int)(0 / Player.Rate / Player.Rate));
            //Controlers[1] = new WiimoteController(camera, WiimoteLib.InputReport.IRExtensionAccel);
            //Controlers[0] = new MouseController(players[0]);
            
            //天気設定
            //Weather w = Weather.Snow(c, tennisCourt, 2);
            //Weather w = Weather.Rain(c, tennisCourt, 3);
            //w.AddToGameCompo(this);

            //審判を設置
            Human judgeMan = new Human(GameMain.Models["humanFat"], c, new Vector3(-2.1421f, 0.545f, 0), Vector2.Zero, new Vector2(1, 0),"Animation_10");
            judgeMan.TargetObject = ball;
            AddComponents(judgeMan);
            MrJudge = judgeMan;
            //BGM設定
            //SetBGM("veryGrandMusic");
            //SetBGM("pianoSong");

            //カメラの初期位置登録
            defaultCameraPosition = c.Position;
            //カメラモードの登録
            cameraMode = CameraModes.Fixed;
            if(GameMain.debug)
                cameraMode = CameraModes.Debug;
            frames = 0;

            //サーバとレシーバの初期設定
            server = players[0];
            receiver = players[1];
            //点数初期設定
            scores = new ScoreManager(events, rule,players);
            matchRule = rule;
            //スコア依存のイベント設定
            scores.Point += () => 
            { 
                pointEnd = true; 
                scoreLogo.Text = scores.ToString(Scores.Points, server, receiver);
            };
            scores.Game += () => 
            { 
                gameEnd = true; 
                scoreLogo.Text = scores.ToString(Scores.Games, server, receiver);
            };
            /* () =>
             {
                 changeServe(events);
                 if ((scores.GetScore(players[0]).Games + scores.GetScore(players[1]).Games) % 2 == 1)
                 {
                     changeCourt();
                 }
                 else
                 {
                     courtSide = CourtSide.Deuse;
                     initPlayersPosition(courtSide, events);
                 }
             };*/
            scores.Set += () => 
            { 
                scoreLogo.Text = scores.ToString(Scores.Sets, server, receiver);
                
            };
            scores.GameSet += () => { scoreLogo.Text = "Game Set"; };

            Let = new BallState(() => { isLet = true; }, "Let");
            Fault = new BallState(() =>
            {
                faults++;
                if (faults >= 2)
                {
                    //ダブルフォルト
                    scores.LostPoint(lastHitter);
                }
                else
                {
                    //フォルト
                    isLet = true;
                    
                    //resetPoint();
                }
            }, "Fault");
            TwoBounds = new BallState(() => { scores.GotPoint(lastHitter); },"");
            Out = new BallState(() => { scores.LostPoint(lastHitter); },"Out");
            Net = new BallState(() => { if(lastHitter != null)scores.LostPoint(lastHitter); },"Net");
            NoBoundReceive = new BallState(() => { scores.LostPoint(lastHitter); },"");
            Other1 = new BallState(() => { scores.LostPoint(lastHitter); },"");
            
            //score1 = new Score(0, 0, Points.Love);
            //score2 = new Score(0, 0, Points.Love);
            //ゲーム開始
            courtSide = CourtSide.Deuse;
            initPlayers(courtSide,events);
            //... = players[2];
            //... = players[3];
            //players[0].IsServer = true;
            int animationFrames = (int)(endFrames * 0.8f);
            int animationFrames2 = (int)(animationFrames * 0.8f);
            stateLogo = new AnimatableLogo(GameMain.LogoFont, Game.WindowRect, Vector2.One * 0.5f, new Vector2(0.5f, 0.3f),
                new Animation("test",
                    new AnimationKey(0, new LogoParams(null, "", Vector2.Zero, Vector2.One * 0.5f,0.0f.LogoSize(), Rotation.Zero, new Color(0.5f, 0, 0, 0.5f), new Color(0.25f, 0, 0, 0.25f)), RotationWays.ABSOLUTE),
                    new AnimationKey(animationFrames2 / 3, new LogoParams(null, "", new Vector2(0f, 0), Vector2.One * 0.5f,0.3f.LogoSize(), Rotation.Zero, Color.Red, Color.Red), RotationWays.ABSOLUTE),
                    new AnimationKey(animationFrames2 * 2 / 3, new LogoParams(null, "", new Vector2(0f, 0f), Vector2.One * 0.5f, 0.3f.LogoSize(), Rotation.Zero, Color.Red, Color.Red), RotationWays.ABSOLUTE),
                    new AnimationKey(animationFrames2, new LogoParams(null, "", new Vector2(0, 0), Vector2.One * 0.5f, 0.3f.LogoSize(), Rotation.Zero, Color.Transparent, Color.Transparent), RotationWays.ABSOLUTE)
                )
            );
            scoreLogo = new AnimatableLogo(GameMain.LogoFont, Game.WindowRect, new Vector2(0.5f, 0.5f), Vector2.One * 0.5f,
                new Animation("feedIn",
                    new AnimationKey(0, new LogoParams(null, "", new Vector2(0.5f, 0), new Vector2(0.25f,0.5f),0.3f.LogoSize(), Rotation.Zero, Color.White, Color.White), RotationWays.ABSOLUTE),
                    new AnimationKey(animationFrames / 3, new LogoParams(null, "", Vector2.Zero, Vector2.One * 0.5f, 0.3f.LogoSize(), Rotation.Zero, Color.White, Color.White), RotationWays.ABSOLUTE),
                    new AnimationKey(animationFrames * 2 / 3, new LogoParams(null, "", Vector2.Zero, Vector2.One * 0.5f, 0.3f.LogoSize(), Rotation.Zero, Color.White, Color.White), RotationWays.ABSOLUTE),
                    new AnimationKey(animationFrames, new LogoParams(null, "", new Vector2(-0.5f, 0), new Vector2(0.75f,0.5f), 0.3f.LogoSize(), Rotation.Zero, Color.Transparent, Color.Transparent), RotationWays.ABSOLUTE)
                )
            );
            string ruleStr = rule.ToString();
            start = new AnimatableLogo(GameMain.LogoFont, Game.WindowRect, new Vector2(0.5f, 0.5f), Vector2.One * 0.5f,
                new Animation("start",
                    new AnimationKey(0, new LogoParams(null, ruleStr, new Vector2(0,0.5f), new Vector2(0.5f,0), Vector2.One, Rotation.Zero, Color.White, Color.Red),RotationWays.RELATIVE),
                    new AnimationKey(60, new LogoParams(null, ruleStr, Vector2.Zero, new Vector2(0.5f,0.55f), Vector2.One, Rotation.Zero, Color.White, Color.Red),RotationWays.RELATIVE),
                    new AnimationKey(65, new LogoParams(null, ruleStr, Vector2.Zero, Vector2.One * 0.5f, Vector2.One, Rotation.Zero, Color.White, Color.Red),RotationWays.RELATIVE),
                    new AnimationKey(110, new LogoParams(null, ruleStr, Vector2.Zero, Vector2.One * 0.5f, Vector2.One, new Rotation(1080), Color.White, Color.Red), RotationWays.RELATIVE),
                    new AnimationKey(150, new LogoParams(null, "Start!", Vector2.Zero, Vector2.One * 0.5f, Vector2.Zero, Rotation.Zero, Color.White, Color.Red), RotationWays.RELATIVE),
                    new AnimationKey(151, new LogoParams(null, "Start!", Vector2.Zero, Vector2.One * 0.5f, Vector2.Zero, Rotation.Zero, Color.White, Color.Red), RotationWays.RELATIVE),
                    new AnimationKey(160, new LogoParams(null, "Start!", Vector2.Zero, Vector2.One * 0.5f, Vector2.One, Rotation.Zero, Color.Transparent, Color.Orange), RotationWays.RELATIVE),
                    new AnimationKey(180, new LogoParams(null, "", Vector2.Zero, Vector2.One * 0.5f, Vector2.One, Rotation.Zero, Color.Transparent, Color.Transparent), RotationWays.RELATIVE)
                )
            );
            start.EndAnimating += () =>
            {
                Scene.IsEnableControllers = true;
            };
            start.Animate("start", 1, true);
            cc = new AnimatableLogo(GameMain.LogoFont, Game.WindowRect, Vector2.One * 0.5f, Vector2.One * 0.5f,
                new Animation("changeCourt",
                    new AnimationKey(0, new LogoParams(null, "Change Court", new Vector2(0,0), Vector2.One*0.5f,Vector2.Zero, Rotation.Zero, Color.White, Color.Blue), RotationWays.RELATIVE),
                    new AnimationKey(15, new LogoParams(null, "Change Court", new Vector2(0,0), Vector2.One * 0.5f, Vector2.One, Rotation.Zero, Color.White, Color.Blue), RotationWays.RELATIVE),
                    new AnimationKey(30, new LogoParams(null, "Change Court", new Vector2(0, 0), Vector2.One * 0.5f, Vector2.One, Rotation.Zero, Color.White, Color.White), RotationWays.RELATIVE),
                    new AnimationKey(45, new LogoParams(null, "Change Court", new Vector2(0,0), Vector2.One * 0.5f, Vector2.One, Rotation.Zero, Color.White, Color.Blue), RotationWays.RELATIVE),
                    new AnimationKey(60, new LogoParams(null, "Change Court", new Vector2(0,0.5f), Vector2.One * 0.5f, Vector2.One, Rotation.Zero, Color.Transparent, Color.Transparent), RotationWays.RELATIVE)
                )
            );
            cc.StartAnimating += () =>
            {
                Scene.IsEnableControllers = false;
            };
            cc.EndAnimating += () =>
            {
                Scene.IsEnableControllers = true;
            };
            AnimatableLogo serveSpeed = new AnimatableLogo(GameMain.LogoFont, Game.WindowRect, Vector2.One * 0.5f, Vector2.One * 0.5f,
                    new Animation("ss",
                    new AnimationKey(0, new LogoParams(null, "", new Vector2(0, 0), Vector2.One * 0.5f, Vector2.Zero, Rotation.Zero, Color.White, Color.DeepPink), RotationWays.RELATIVE),
                    new AnimationKey(20, new LogoParams(null, "", new Vector2(0, 0), Vector2.One * 0.5f, Vector2.One * 0.3f, Rotation.Zero, Color.White, Color.DeepPink), RotationWays.RELATIVE),
                    new AnimationKey(30, new LogoParams(null, "", new Vector2(0, 0), Vector2.One * 0.5f, Vector2.One * 0.3f, Rotation.Zero, Color.White, Color.White), RotationWays.RELATIVE),
                    new AnimationKey(40, new LogoParams(null, "", new Vector2(0, 0), Vector2.One * 0.5f, Vector2.One*0.3f, Rotation.Zero, Color.White, Color.DeepPink), RotationWays.RELATIVE),
                    new AnimationKey(50, new LogoParams(null, "", new Vector2(0, 0), Vector2.One * 0.5f, Vector2.One*0.3f, Rotation.Zero, Color.White, Color.DeepPink), RotationWays.RELATIVE),
                    new AnimationKey(60, new LogoParams(null, "", new Vector2(0, 0), Vector2.One * 0.5f, Vector2.One * 0.3f, Rotation.Zero, Color.Transparent, Color.Transparent), RotationWays.RELATIVE)

                )
            );
            foreach (Player p in players)
            {
                p.ShottingBall += (shots) =>
                {
                    if (shots == Shots.SERVE)
                    {
                        serveSpeed.Text = (int)ball.SpeedKPH + " km/h";
                        Vector3 screen = Viewport.Project(server.HeadPosition, camera.Projection, camera.View, Matrix.Identity);
                        serveSpeed.DefaultPosition = new Vector2(screen.X / Game.WindowRect.Width, screen.Y / Game.WindowRect.Height);
                        //System.Windows.Forms.MessageBox.Show(screen.ToString());
                        serveSpeed.Animate("ss", 1, true);
                    }
                };
            }
            //scoreLogo.Visible = false;
            //stateLogo.Visible = false;
            AddComponents(stateLogo, scoreLogo,start,cc,serveSpeed);

        }
        Vector2 logoSize(float size)
        {
            return new Vector2(1, size);
        }
        private bool InvalidPlayers(TennisEvents events, Player[] players)
        {
            if ((events == TennisEvents.Singles && players.Length != 2) ||
               (events == TennisEvents.Doubles && players.Length != 4)
                )
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// そのプレーが終わったかどうか
        /// </summary>
        /// <returns></returns>
        void checkPointEnd()
        {
            ballStates[Let] = ball.BoundedPoint != Vector3.Zero && totalShots() == 1 && ball.IsInServiceCourt(courtSide) && ball.TouchedNet;
            ballStates[Fault] = totalShots() == 1 && (!ball.IsInServiceCourt(courtSide) || ball.IsNet);
            ballStates[TwoBounds] = ball.Bounds >= 2;
            ballStates[NoBoundReceive] = totalShots() == 2 && receiver.Bounds == 0;
            ballStates[Out] = ball.IsOut(events);
            ballStates[Net] = ball.IsNet;
            ballStates[Other1] = !ball.ReachedToNet;
            //ballStates["Fault"] = totalShots() == 1 && (!ball.IsInServiceCourt(courtSide) || ball.IsNet);
            //ballStates["Let"] = totalShots() == 1 && ball.IsInServiceCourt(courtSide);
            foreach (KeyValuePair<BallState,bool> b in ballStates)
            {
                if (b.Value)
                {
                    end = true;
                    pointEndReason = b.Key;
                    lastHitter = ball.LastHitter;
                    countScore();
                    stateLogo.Visible = true;
                    stateLogo.Text = b.Key.ToString();
                    if (stateLogo.Text == "Fault" && faults == 2)
                    {
                        stateLogo.Text = "Double Fault";
                    }
                    scoreLogo.Visible = true;
                    //scoreLogo.Text = "" + scores.ToString(Scores.Points,server,receiver);
                    stateLogo.Animate("test", 1,true);
                    if (stateLogo.Text != "Fault" && stateLogo.Text != "Let")
                    {
                        scoreLogo.Animate("feedIn", 1, true);
                        endFrames = 120;
                    }
                    else 
                    {
                        endFrames = 60;
                    }
                    break;
                }
            }
            GameMain.debugStr["Fault"] = "" + ballStates[Fault];
            GameMain.debugStr["totalShots"] = "" + totalShots();
        }
        void showPointEndReason()
        {

        }
        int totalShots()
        {
            int res = 0;
            foreach (Player p in players)
            {
                res += p.ShotsInPoint;
            }
            return res;
        }
        /// <summary>
        /// そのポイントをやり直す
        /// </summary>
        void resetPoint()
        {
            initPlayers(courtSide, events);
        }
        /// <summary>
        /// 次のポイントへ
        /// </summary>
        void nextPoint()
        {
            //サイド変更
            if (courtSide == CourtSide.Deuse)
                courtSide = CourtSide.Advantage;
            else
                courtSide = CourtSide.Deuse;
            if (scores.IsTieBreak())
            {
                int s0 = scores.GetScore(server).TieScore.Points;
                int s1 = scores.GetScore(receiver).TieScore.Points;
                if ((s0 + s1) % 2 == 1)
                {
                    changeServe(events);
                }
                if ((s0 + s1) % 6 == 0)
                {
                    changeCourt();
                }
            }
            initPlayers(courtSide,events);
            //フォルトなどのカウントリセット
            faults = 0;
        }
        /// <summary>
        /// コートサイドに応じてプレーヤの位置を初期化
        /// </summary>
        /// <param name="side"></param>
        /// <param name="events"></param>
        void initPlayers(CourtSide side,TennisEvents events)
        {
            server.SetToServerPosition(courtSide,events);
            receiver.SetToReceiverPosition(courtSide);
            //視線の設定
            //最初はレシーバを見る
            server.TargetObject = receiver;
            receiver.TargetObject = ball;
        }
        //サーバー交代
        void changeServe(TennisEvents events)
        {
            if (events == TennisEvents.Singles)
            {
                //??
                Player buf = server;
                server = receiver;
                receiver = buf;
                //server = players[1];
                //receiver = players[0];
            }
            else if (events == TennisEvents.Doubles)
            {

            }
        }
        /// <summary>
        /// ゲームが決まったときの処理
        /// </summary>
        void nextGame()
        {
            changeServe(events);
            if ((scores.GetScore(players[0]).Games + scores.GetScore(players[1]).Games) % 2 == 1)
            {
                changeCourt();
            }
            else
            {
                courtSide = CourtSide.Deuse;
                initPlayers(courtSide, events);
            }
        }
        /// <summary>
        /// チェンジコートして次のポイントへ
        /// </summary>
        void changeCourt()
        {
            courtSide = CourtSide.Deuse;
            foreach (Player p in players)
            {
                p.Position = new Vector3(p.Position.X, p.Position.Y, -p.Position.Z);
            }
            if (cameraMode == CameraModes.BackOfPlayer1 || cameraMode == CameraModes.BackOfPlayer2)
            {
                camera.Position = new Vector3(camera.Position.X, camera.Position.Y, -camera.Position.Z);
            }
            initPlayers(courtSide, events);
            cc.Animate("changeCourt", 1, true);
        }

        public override Scene NextScene
        {
            get 
            {
                return nextScene;
            }
        }
        Scene nextScene = null;

        public override void Update(GameTime gameTime)
        {
            if (start.IsAnimating)
                Scene.IsEnableControllers = false;
            if (GameMain.debug && Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                for (int j = 0; j < 4; j++)
                {
                    scores.GotPoint(players[0]);
                }
            }
            ControllerState controllerState = Controllers[0].GetState();
            if (controllerState.L == ControlerButtonStates.Down &&
                controllerState.R == ControlerButtonStates.Down &&
                controllerState.Pause == ControlerButtonStates.Down &&
                controllerState.Back == ControlerButtonStates.Down)
            {
                nextScene = new SceneTitle();
            }
            //ポイント終了
            if (!end)
            {
                checkPointEnd();
                //countScore();
            }
            else if (frames++ >= endFrames)//ポイント終了時の処理
            {
                //countScore();
                if (pointEnd)
                {
                    nextPoint();
                    pointEnd = false;
                }
                if (gameEnd)
                {
                    nextGame();
                    gameEnd = false;
                }
                if (isLet)
                {
                    resetPoint();
                    isLet = false;
                }
                if (scores.GameSetMatch)
                {
                    nextScene = new SceneResult(scores,tennisCourt.ModelName,scores.Winners[0] == players[0] ? "Player 1" : "Player 2",this);
                }
                //initNextPoint();
                initPoint();
            }
            GameMain.debugStr["end"] = end + "";
            //デバッグ表示
            int i = 0;
            foreach (Player p in players)
            {
                GameMain.debugStr["player" + i + ".Position"] = "" + p.Position;
                GameMain.debugStr["player" + i + ".Speed"] = (p.Speed.Length()*60*3600/0.27f/1000)+"km/h";
                GameMain.debugStr["player" + i + ".DistanceToBall"] = ""+p.DistanceToBall;// (p.Speed.Length() * 60 * 3600 / 0.27f / 1000) + "km/h";
                GameMain.debugStr["player" + i + ".DistanceToBall"] = p.DistanceToBall.ToString();
                i++;
            }
            //Game1.debugStr["score1"] = "" + score1.ToString();
            //Game1.debugStr["score2"] = "" + score2.ToString();
            GameMain.debugStr["currentScore"] = "" + scores.ToString(Scores.Points,server,receiver);
            //デバッグ
            //if (Controlers[0].GetState().Pause == ControlerButtonStates.Pressed)
            //{
            //    nextPoint();
            //}
            //if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.B))// == ControlerButtonStates.Pressed)
            //{
            //    test.Animate("test");
            //}
            //if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.A))// == ControlerButtonStates.Pressed)
            //{
            //    test.Animate("test2");
            //}
            if (GameMain.debug)
            {
                if (Controllers[0].GetState().L == ControlerButtonStates.Pressed)
                {
                    changeCourt();
                }
            }
            //ボールとプレイヤーとテニスコートの壁
            foreach (HitVolume h in tennisCourt.Walls)
            {
                if (ball.Hit(h))
                {
                    ball.Position += -ball.Speed;
                    ball.Speed = 0.5f*Vector3.Reflect(ball.Speed, h.Normal);
                    ball.Bound(ball.Position);
                }
                foreach (Player p in players)
                {
                    if (p.Hit(h))
                    {
                        p.Position = p.Position + h.Normal*p.Ability.MaxSpeed*p.Velocity;// h.Center;
                        //p.Position += p.Speed - Vector3.Dot(p.Speed, h.Normal) * h.Normal;// Vector3.Reflect(p.Speed, h.Normal);
                    }
                }
            }

            //プレイヤーとネット
            foreach (Player p in players)
            {
                if (Math.Abs(p.Position.Z) < p.HitBounds.Z / 2)
                {
                    if (p.Position.Z > 0)
                    {
                        p.Position = p.Position + Vector3.Backward * p.Ability.MaxSpeed * p.Velocity;// h.Center;
                    }
                    else
                    {
                        p.Position = p.Position + Vector3.Forward * p.Ability.MaxSpeed * p.Velocity;// h.Center;
                    }
                }
            }
            //カメラワーク
            //float height = 0.27f * 1.70f;
            Vector3 posi1 = players[0].HeadPosition;
            Vector3 posi2 = players[1].HeadPosition;
            Vector3 stdCameraPosition = new Vector3(0, 0.27f * 1.66f * 7,8f);
            GameMain.debugStr["cameraMode"] = "" + cameraMode;
            if (!(players[1].Index == 1) && GameMain.gamePadStates[0][0].IsButtonDown(Buttons.Start) && GameMain.gamePadStates[1][0].IsButtonUp(Buttons.Start))
            {
                if (cameraMode == CameraModes.Debug || cameraMode == CameraModes.FPVPlayer1)
                    cameraMode = CameraModes.Fixed;
                else 
                    cameraMode++;
            }
            switch (cameraMode)
            {
                case CameraModes.Debug:
                    debugCamera();
                    //if (Keyboard.GetState().IsKeyDown(Keys.Up))
                    //{
                    //    camera.FieldOfView+=0.1f;
                    //}
                    //else if (Keyboard.GetState().IsKeyDown(Keys.Down))
                    //{
                    //    camera.FieldOfView -= 0.1f;
                    //}
                    //camera.Position = new Vector3(players[0].Position.X, camera.Position.Y, players[0].Position.Z);
                    //camera.Position + Vector3.Forward;
                    break;
                case CameraModes.Fixed:
                    normalCamera();

                    //最初に設定したまま動かさない
                    break;
                case CameraModes.BackOfPlayer1:
                    normalCamera(players[0]);
                    //camera.Target = Vector3.Zero;
                    ////Vector3 pos = new Vector3(0, 7.11843f, 16.82682f);
                    //Vector3 pos = new Vector3(0,5.876172f,9.746821f);
                    //camera.Target = new Vector3(0,0,1.08f);
                    //if (players[0].Position.Z < 0)
                    //{
                    //    camera.Target = -camera.Target;
                    //    pos.Z = -pos.Z;
                    //}
                    //camera.Position = pos;
                    ////camera.FieldOfView = 13;
                    //camera.FieldOfView = 30;

                    break;
                case CameraModes.BackOfPlayer2:
                    normalCamera(players[1]);
                    //camera.Target = Vector3.Zero;
                    //Vector3 posi = new Vector3(0,5.876172f,9.746821f);
                    //camera.Target = new Vector3(0, 0, 1.08f);
                    //if (players[1].Position.Z < 0)
                    //{
                    //    camera.Target = -camera.Target;
                    //    posi.Z = -posi.Z;
                    //}
                    //camera.Position = posi;
                    ////camera.FieldOfView = 13;
                    //camera.FieldOfView = 30;
                    break;
                case CameraModes.NearPlayer1:
                    //camera.Target = posi1;
                    nearCamera(players[0]);
                    break;
                case CameraModes.NearPlayer2:
                    //camera.Target = posi2;
                    nearCamera(players[1]);
                    break;
                case CameraModes.FPVPlayer1:
                    //if (TennisCourt.GetCourtPlace(players[0]) == CourtPlace.North)
                    //{
                    //}
                    //else
                    //{
                    //    camera.Target = posi1 + Vector3.Backward*2;
                    //}
                    camera.Target = ball.Position;
                    camera.Position = posi1;
                    camera.FieldOfView = 45;
                    break;
                case CameraModes.FPVPlayer2:
                    camera.Target = ball.Position;
                    camera.Position = posi2;
                    camera.FieldOfView = 45;
                    break;
                case CameraModes.JudgeMan:
                    camera.Target = ball.Position;
                    camera.Position = new Vector3(-2.06f, 0.7762f, 0);
                    camera.FieldOfView = 45;
                    break;
                case CameraModes.Top:
                    float x = (float)Math.Sin(MathHelper.ToRadians((float)gameTime.TotalGameTime.TotalMilliseconds/100));
                    float z = (float)Math.Cos(MathHelper.ToRadians((float)gameTime.TotalGameTime.TotalMilliseconds/100));
                    camera.Position = new Vector3(x*7.2f,7.2f,z*7.2f);
                    camera.Target = Vector3.Zero;
                    camera.FieldOfView = 30;
                    break;
                case CameraModes.Rotation:
                    camera.Position = new Vector3(0, 0.27f * 1.70f, 2.7f);
                    x = (float)Math.Sin(MathHelper.ToRadians(180-(float)gameTime.TotalGameTime.TotalMilliseconds/200));
                    z = (float)Math.Cos(MathHelper.ToRadians(180-(float)gameTime.TotalGameTime.TotalMilliseconds/200));
                    camera.Target = camera.Position + new Vector3(x, 0, z);
                    break;
                case CameraModes.Kinect:
                    if (players[0].Position.Z > ball.Position.Z)
                    {
                        camera.Target = ball.Position;
                    }
                    else if (TennisCourt.GetCourtPlace(players[0]) == CourtPlace.South)
                    {
                        camera.Target = posi1 + Vector3.Backward*0.27f*4f;
                    }
                    else
                    {
                        camera.Target = posi1 + Vector3.Forward*0.27f*4f;
                    }
                    camera.FieldOfView = 50;
                    camera.Position = posi1;
                    break;
            }
            GameMain.debugStr["cameraMode"] = cameraMode.ToString();
            base.Update(gameTime);
        }

        private void normalCamera(Player p = null)
        {
            //camera.Position = new Vector3(0,7.11843f,16.82682f);
            Vector3 position = new Vector3(0, 5.876172f, 9.746821f);
            Vector3 target = allPlayerCenter();
            if (p != null && p.Position.Z < 0)
            {
                position.Z *= -1;
                //target.Z *= -1;
            }
            target.X = target.Y = 0;
            camera.Position = position;
            camera.Target = target;
            camera.Position += target;
            //camera.Target = new Vector3(0, 0, 1.08f);
            //camera.FieldOfView = 13;
            camera.FieldOfView = 30;
        }
        Vector3 allPlayerCenter()
        {
            Vector3 res = Vector3.Zero;
            int i = 0;
            foreach (Player p in players)
            {
                res += p.Position;
                i++;
            }
            if(i != 0)
                return res / i;
            return Vector3.Zero;
        }

        void nearCamera(Player p)
        {
            if (p.HeadPosition.Z * ball.Position.Z < 0 || Math.Abs(p.HeadPosition.Z) > Math.Abs(ball.Position.Z))
            {
                camera.Target = p.HeadPosition;
            }
            if (TennisCourt.GetCourtPlace(p) == CourtPlace.North)
            {
                camera.Position = p.HeadPosition + new Vector3(0, p.HitBounds.Y/3, p.HitBounds.Z*1.5f);
            }
            else
            {
                camera.Position = p.HeadPosition + new Vector3(0, p.HitBounds.Y/3, -p.HitBounds.Z*1.5f);
            }
      
            camera.FieldOfView = 45;

        }
        private void initPoint()
        {
            frames = 0;
            end = false;
            pointEndReason = Active;
            lastHitter = null;
        }

        private void countScore()
        {
            pointEndReason.Action();

            #region
            /*if (pointEndReason == "Let")
            {
                resetPoint();
            }
            else if (pointEndReason == "Fault")//ballStates["Fault"])
            {
                faults++;
                if (faults >= 2)
                {
                    //ダブルフォルト
                    scores.LostPoint(lastHitter);
                }
                else
                {
                    //フォルト
                    resetPoint();
                }
            }
            else if (pointEndReason == "TwoBounds")//ballStates["TwoBounds"])
            {
                //最後にボールを打った人側の得点
                scores.GotPoint(lastHitter);
                //if (ball.LastHitter == players[0])
                //{
                //    //score1++;

                //}
                //else if (ball.LastHitter == players[1])
                //{
                //    //score2++;
                //}
            }
            //else if (ballStates["Fault"])
            //{
            //    faults++;
            //    resetPoint();
            //}
            else if (ballStates["Net"] || ballStates["Out"] || ballStates["Other1"] || ballStates["NoBoundReceive"])
            {
                //最後にボールを打った人側の失点
                scores.LostPoint(lastHitter);
                //if (ball.LastHitter == players[0])
                //{
                //    //score2++;
                //}
                //else if (ball.LastHitter == players[1])
                //{
                //    //score1++;
                //}
            }*/
            //else if (ballStates["Let"])
            //{
            //    resetPoint();
            //}
            #endregion
        }
        
        public override void Draw(GameTime gameTime)
        {
            if (Controllers[0] is KinectControlerBase)
            {
                Microsoft.Xna.Framework.Graphics.Texture2D texture = (Controllers[0] as KinectControlerBase).ColorImage;
                DrawImage(texture,new Vector2(1,0),new Vector2(1.0f,0),0.25f,Color.White);
            }
            base.Draw(gameTime);
        }
    }
}
