using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Extention;
namespace Tennis01.Objects.PlayerStates
{
    abstract class ServeStateBase : PlayerState
    {
        protected CourtSide courtSide;
        public ServeStateBase(Player p, CourtSide side,string animationName,string soundName="",TimeSpan? animationStartTime=null,TimeSpan? animationStartTime2=null):
            base(p,animationName,soundName,animationStartTime,animationStartTime2)
        {
            courtSide = side;
        }
        protected Vector3 handPositionL
        {
            get
            {
                return (Player.HandTransformL * Matrix.CreateRotationY(Player.Rotation.Y) * Matrix.CreateTranslation(Player.Position)).Translation;
            }
        }
        protected void setBallToHand(Ball b)
        {
            b.Init(handPositionL);
            if (b.Position.Y < b.Radius)
            {
                b.Init(handPositionL + Vector3.One);
            }
        }
    }
    class ServeReady:ServeStateBase
    {
        float limit;
        public ServeReady(Player p,CourtSide side)
            : base(p,side,"Animation_11")
        {
            limit = TennisCourt.SinglesWidth / 2 - Player.ShoulderWidth;
            GooL = GooR = true;
        }
        public override void Update(Input.ControllerState controlerState)
        {
            setBallToHand(Player.Ball);
            //Player.Ball.Init(Player.Position + new Vector3(Player.Position.Z > 0 ? 0.27f/3 : -0.27f/3, Player.ShoulderY,0));
            Player.BodyDirection = Player.Position.Z > 0 ? new Vector2(1, 0) : new Vector2(-1, 0);
            float x = Math.Abs(Player.Position.X);
            if (controlerState.HasAnyInput(Input.ControllerState.Inputs.ShotButtons))
            {
                NextState = new ServeTossing(Player,courtSide,Player.TargetObject);
            }
            else if (Math.Abs(controlerState.JoyStick.X) >= 0.1f && 
                (
                    (x > Player.ShoulderWidth && controlerState.JoyStick.X*Player.Position.X < 0) || 
                    (x < limit && controlerState.JoyStick.X * Player.Position.X > 0)
                )
            )
            {
                NextState = new ServePositioning(Player, courtSide,limit);
            }
        }
    }
    class ServePositioning : ServeStateBase
    {
        float limit;
        public ServePositioning(Player p,CourtSide side,float limit)
            :base(p,side,"Animation_12","",new TimeSpan(0,0,0,0,500))
        {
            this.limit = limit;
            GooL = GooR = true;
        }
        public override void Update(Input.ControllerState controlerState)
        {
            //Player.Ball.Init(Player.Position + new Vector3(0,Player.ShoulderY,Player.Position.Z > 0 ? -0.27f/3 : 0.27f / 3));
            Player.Position += new Vector3(0.27f * 3 / 60, 0, 0) * controlerState.JoyStick.X;
            setBallToHand(Player.Ball);
            float posi = Math.Abs(Player.Position.X);
            if (controlerState.HasAnyInput(Input.ControllerState.Inputs.ShotButtons))
            {
                NextState = new ServeTossing(Player, courtSide, Player.TargetObject);
                Player.Velocity = 0;
            }
            else if (Math.Abs(controlerState.JoyStick.X) < 0.1f)
            {
                NextState = new ServeReady(Player, courtSide);
                Player.Velocity = 0;
            }
            else if(posi < Player.ShoulderWidth)
            {
                NextState = new ServeReady(Player, courtSide);
                float x = Player.Position.X > 0 ? Player.ShoulderWidth : -Player.ShoulderWidth;
                Player.Position = new Vector3(x, Player.Position.Y, Player.Position.Z);
            }
            else if (posi > limit)
            {
                NextState = new ServeReady(Player, courtSide);
                float x = Player.Position.X > 0 ? limit : -limit;
                Player.Position = new Vector3(x, Player.Position.Y, Player.Position.Z);                
            }
            //else //if(posi > Player.ShoulderWidth && posi < TennisCourt.SinglesWidth/2)
            //{
            //    Player.Position += new Vector3(0.27f * 3 / 60, 0, 0) * controlerState.JoyStick.X;

            //    if (posi < Player.ShoulderWidth || posi > TennisCourt.SinglesWidth / 2)
            //    {
            //        NextState = new ServeReady(Player, courtSide);
            //    }
            //    #region
            //    //if (Player.Position.Z > 0)
            //    //{
            //    //    if (posi < Player.ShoulderWidth)
            //    //        Player.Position = new Vector3(Player.Position.X > 0 ? Player.ShoulderWidth : -Player.ShoulderWidth, Player.Position.Y, Player.Position.Z);
            //    //    else if (posi > TennisCourt.SinglesWidth/2)
            //    //    {
            //    //        Player.Position = new Vector3(Player.Position.X > 0 ? TennisCourt.SinglesWidth/2 - Player.ShoulderWidth : -TennisCourt.SinglesWidth/2 + Player.ShoulderWidth, Player.Position.Y, Player.Position.Z);
            //    //    }
            //    //}
            //    //Vector3 vec = Vector3.Normalize(new Vector3(controlerState.JoyStick.X, 0, 0));
            //    //Player.BodyDirection = new Vector2(vec.X, vec.Z);
            //    //Player.Velocity = 0.6f;
            //    #endregion
            //}
            //Player.RotationY = Player.Position.Z > 0 ? new Vector2(1, 0).ToRadians() : new Vector2(-1, 0).ToRadians();
        }
    }
    class ServeTossing : ServeStateBase
    {
        Object3D lastTarget;
        bool tossed;
        float tossedHeight;
        //秒数に直す
        int frames = 20;
        public ServeTossing(Player p,CourtSide side,Object3D target)
            : base(p,side, "Animation_13","",null,new TimeSpan(0,0,0,0,(int)(69 / 120.0f * 1000)))
        {
            tossed = false;
            //p.Toss();
            lastTarget = target;
            p.TargetObject = p.Ball;
            GooR = true;
        }
        public override void Update(Input.ControllerState controlerState)
        {
            if (tossed)
            {
                //サーブを打つ
                if (controlerState.HasAnyInput(Input.ControllerState.Inputs.ShotButtons))
                {
                    NextState = new ServeSwinging(Player, courtSide, controlerState);
                    Player.BodyDirection = Player.Position.Z > 0 ? new Vector2(0, -1) : new Vector2(0, 1);
                }
                //準備に戻る
                else if (Player.Ball.Position.Y <= handPositionL.Y && Player.Ball.Speed.Y < 0)//tossedHeight)
                {
                    NextState = new ServeTossCancel(Player, courtSide);
                    Player.TargetObject = lastTarget;
                    setBallToHand(Player.Ball);
                    GooL = true;
                }
            }
            else
            {
                setBallToHand(Player.Ball);
                if (frames-- <= 0)//Player.Ball.Position.Y >= Player.HeadPosition.Y)
                {
                    tossed = true;
                    tossedHeight = Player.Ball.Position.Y;
                    Player.Toss();
                    GooL = false;
                }
            }
        }
    }
    class ServeTossCancel : ServeStateBase
    {
        //フレーム数...秒数でやった方が良い??
        int frames = 15;
        public ServeTossCancel(Player p, CourtSide side):base(p,side,"Animation_14","",null,new TimeSpan(0,0,0,0,249))
        {
            GooL = GooR = true;
        }
        public override void Update(Input.ControllerState controlerState)
        {
            setBallToHand(Player.Ball);
            if (frames-- == 0)
            {
                NextState = new ServeReady(Player, courtSide);
            }
        }
    }
    class ServeSwinging : ServeStateBase
    {
        int frames = 30;
        Input.ControllerState state; 
        public ServeSwinging(Player p,CourtSide side,Input.ControllerState controlerState)
            : base(p, side,"Animation_3","",new TimeSpan(0,0,0,0,(int)(434 / 120.0f * 1000)))
        {
            courtSide = side;
            state = controlerState;
            Player.Jump(0.27f * 0.2f);
            if (controlerState.JoyStick.Y *Player.Position.Z > 0)
            {
                Player.Velocity = 0.8f;
                //Vector3 ballDic = Player.Ball.Speed;
                //Player.BodyDirection = Vector2.Normalize(new Vector2(ballDic.X, ballDic.Z));
            }
            GooL = false;
            GooR = true;
        }
        public override void Update(Input.ControllerState controlerState)
        {
            if (frames <= 0)
            {
                NextState = new Running(Player);
            }
            else if (frames == 30 - 5)
            {
                Player.Serve(state, courtSide);
            }
            frames--;
            //throw new NotImplementedException();
        }
    }
}
