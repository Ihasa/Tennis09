using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using Tennis01.Objects;
using Tennis01.Input;
namespace Tennis01.Scenes
{
    class TestScene:Scene
    {
        Ball ball;
        Player test1, test2;
        public TestScene(Camera c):base
        (
            c,true,""
            //new Player(Game1.Models["iroiroTest2"],Vector3.Zero,new PlayerAbility(0.03f,0.03f,0.03f,90,0),new Vector2(0,1),"TestSounds","racket",null,Game1.gamePadStates[0])
        )
        {
            //ball = new Ball(Game1.Models["ball"], camera, new Vector3(0, 2, 3), Vector3.Zero, new Vector2(0.6f, 0.77f));
            TennisCourt court = new TennisCourt(new TennisCourtParams("CenterCourt", new Vector2(0.6f, 0.77f), new Vector2(2*0.27f, 2*0.27f), new float[] { 10, 10, 10, 10 }));
            ball = new Ball(GameMain.Models["ball"], camera, new Vector3(0, 2, -TennisCourt.CourtLength), Vector3.Zero, court);
            test1 = new Player(GameMain.Models["humanFat"], camera, new Vector3(0, 0, 4), PlayerAbility.HardHitType, new Vector2(0, -1), "ObjectSEs", "racket",ball, 0);
            test2 = new Player(GameMain.Models["humanFat"], camera, new Vector3(0, 0, -3.3f), PlayerAbility.HardHitType, new Vector2(0, 1), "ObjectSEs", "racket2", ball, 1);

            test1.TargetObject = ball;
            //Controllers[0] = new TestAIControler(p, p2, ball, 0, 0, 0);
            Controllers[1] = new TestAIControler(test2, test1, ball, 0, 0, 0);
            AddObjects(new Object3D(GameMain.Models["CenterCourt"],c),
                test1,test1.Belonging,ball,test2,test2.Belonging);
           
            AddControleables(test1,test2);
        }

        public override Scene NextScene
        {
            get
            {
                if (Controllers[0].GetState().L == Input.ControlerButtonStates.Pressed)
                {
                    //foreach (GameComponent o in Game.Components)
                    //{
                    //    if (o is Object3D)
                    //    {
                    //        if (((Object3D)o).HasAudio)
                    //        {
                    //            ((Object3D)o).StopSounds();
                    //        }
                    //    }
                    //}//オーディオの停止？
                    TennisCourt court = TennisCourt.Court1;
                    Ball newBall = new Ball(GameMain.Models["ball"], camera, new Vector3(0, 2, 3*TennisCourt.CourtLength), Vector3.Zero,court);
                    return new ScenePlaying(TennisEvents.Singles,camera,newBall,
                                court,new Rules.MatchRule(2,5,false,true),true,
                                new Player(GameMain.Models["human170"], camera,new Vector3(0,0,3.3f), PlayerAbility.StandardType, new Vector2(0, -1), "ObjectSEs", "racket3", newBall,0),
                                new Player(GameMain.Models["human170"], camera,new Vector3(0, 0, -3.3f), PlayerAbility.StandardType, new Vector2(0, 1), "ObjectSEs", "racket2",newBall,1)
                            );
                }
                return null;
            }
        }
        public override void Update(GameTime gameTime)
        {
            debugCamera();
            if (ball.Bounds > 3 || Math.Abs(ball.Position.Z) > 5 || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Start))
            {
                ball.Init(new Vector3(0,0.27f*2,0));
                ball.ShotByDistance(new Vector2(GameMain.Random.Next(-20,20), 25), TennisCourt.CourtLength*(float)(GameMain.Random.NextDouble()), 0.1f);
            }
            if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.B))
            {
                test1.SetToServerPosition(CourtSide.Deuse, TennisEvents.Singles);
                test2.SetToReceiverPosition(CourtSide.Deuse);
            }
            //Vector3 vec = new Vector3(Game1.gamePadStates[0].ThumbSticks.Right.X / 10.0f, Game1.gamePadStates[0].ThumbSticks.Right.Y / 10,Game1.gamePadStates[0].Triggers.Left-Game1.gamePadStates[0].Triggers.Right);
            //camera.Position += vec;
            base.Update(gameTime);
        }
    }
}
