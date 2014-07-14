using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
namespace Tennis01.Objects
{

    enum CourtPlace
    {
        North,//Z > 0
        South //Z < 0
    }
    enum CourtSide
    {
        Deuse,
        Advantage
    }
    /// <summary>
    /// テニスコートの特徴を決定するパラメータ
    /// </summary>
    struct TennisCourtParams
    {
        public string ModelName;
        public Vector2 BoundFactor;
        public Vector2 ValidArea;
        public float[] WallsHeight;
        public TennisCourtParams(string modelName, Vector2 boundFactor, Vector2 validArea, float[] wallsHeight)
        {
            this.ModelName = modelName;
            this.BoundFactor = boundFactor;
            this.ValidArea = validArea;
            this.WallsHeight = wallsHeight;
        }
    }
    class TennisCourt
    {
        //不変なメンバ(コートのラインの長さ等)
        public static readonly Vector2 LeftUp, RightUp, LeftBelow, RightBelow;//コートの左奥～右手前の座標
        public static readonly float NetHeight = 0.2666f;
        public static readonly float NetZ = 0.27f;
        public static readonly float NetWidth = 1.6432f * 2;
        public static readonly HitVolume Net = new HitVolume(Vector3.Up*NetHeight/2, NetWidth,NetHeight, NetZ);
        public static readonly float CourtLength = 3.20895f;
        public static readonly float SinglesWidth = 8.23f * 0.27f;
        public static readonly float DoublesWidth = 10.97f * 0.27f;
        public static readonly float ServiceAreaLength;
        //可変なメンバ
        public string ModelName { get; private set; }
        public Vector2 BoundFactor { get; private set; }
        public Vector2 CourtBounds { get; private set; }
        public HitVolume[] Walls { get; private set; }
        public Weather Weather { get; set; }
        //定数
        public static readonly TennisCourt Court1 = new TennisCourt(new TennisCourtParams("CenterCourt", new Vector2(0.6f, 0.77f) * new Vector2(0.8f,0.8f), new Vector2( 2.886f * 2, 5.5506f * 2), new float[] { 0.5701f, 0.5701f, 0.5701f, 0.5701f }));
        public static readonly TennisCourt Court2 = new TennisCourt(new TennisCourtParams("tennisCourt_big", new Vector2(0.6f, 0.77f) * new Vector2(0.8f, 0.9f), new Vector2(2.886f * 2, 5.5506f * 2), new float[] { 0.5701f, 0, 0.5701f, 0.5701f*2 }));
        public static readonly TennisCourt Court3;
        public TennisCourt(string modelName,Vector2 boundFactor,float validAreaX,float validAreaZ,float[] wallsHeight)
        {
            BoundFactor = boundFactor;
            ModelName = modelName;
            setWalls(validAreaX, validAreaZ, wallsHeight);
            CourtBounds = new Vector2(validAreaX, validAreaZ);
        }
        public TennisCourt(TennisCourtParams parameter)
        {
            BoundFactor = parameter.BoundFactor;
            ModelName = parameter.ModelName;
            CourtBounds = new Vector2(parameter.ValidArea.X, parameter.ValidArea.Y);
            setWalls(parameter.ValidArea.X, parameter.ValidArea.Y, parameter.WallsHeight);
        }
        private void setWalls(float validAreaX, float validAreaZ, float[] wallsHeight)
        {
            Walls = new HitVolume[4]; //周囲の壁4つ
            float wallLengthX = validAreaX;// 2.886f * 2;
            float wallLengthZ = validAreaZ;// 5.6005f * 2;
            for (int i = 0; i < Walls.Length; i++)
            {
                switch (i)
                {
                    case 0://奥
                        Walls[i] = new HitVolume(new Vector3(0, wallsHeight[i] / 2, -wallLengthZ / 2), wallLengthX, wallsHeight[i], 0.27f, new Vector3(0, 0, 1));
                        break;
                    case 1://右
                        Walls[i] = new HitVolume(new Vector3(wallLengthX / 2, wallsHeight[i] / 2, 0), 0.27f, wallsHeight[i], wallLengthZ, new Vector3(-1, 0, 0));
                        break;
                    case 2://手前
                        Walls[i] = new HitVolume(new Vector3(0, wallsHeight[i] / 2, wallLengthZ / 2), wallLengthX, wallsHeight[i], 0.27f, new Vector3(0, 0, -1));
                        break;
                    case 3://左
                        Walls[i] = new HitVolume(new Vector3(-wallLengthX / 2, wallsHeight[i] / 2, 0), 0.27f, wallsHeight[i], wallLengthZ, new Vector3(1, 0, 0));
                        break;
                }
            }
        }
        static TennisCourt()
        {
            //コートの隅の座標
            LeftUp = new Vector2(-1.06f, -3.2015f);
            RightUp = new Vector2(1.06f, -3.2015f);
            LeftBelow = new Vector2(-1.06f, 3.2015f);
            RightBelow = new Vector2(1.06f, 3.2015f);
            //サービスエリアの長さZ
            ServiceAreaLength = CourtLength * (21.0f / 39.0f);
            //コートの境界の座標
            //LeftUpWall = new Vector2(-2.886f, -5.6005f);
            //RightUpWall = new Vector2(2.886f, -5.6005f);
            //LeftBelowWall = new Vector2(-2.886f, 5.6005f);
            //RightBelowWall = new Vector2(2.886f, 5.6005f);
        }
        public static CourtPlace GetCourtPlace(Object3D o)
        {
            if (o.Position.Z > 0)
                return CourtPlace.North;
            return CourtPlace.South;
        }
        public static bool IsInCourt(Vector3 position)
        {
            if (position.X >= LeftUp.X && position.X <= RightUp.X &&
               position.Z >= LeftUp.Y && position.Z <= LeftBelow.Y)
            {
                return true;
            }
            return false;
        }
        public static CourtSide GetCourtSide(Object3D o)
        {
            if (GetCourtPlace(o) == CourtPlace.North)
            {
                if (o.Position.X >= 0)
                    return CourtSide.Deuse;
                return CourtSide.Advantage;
            }
            else
            {
                if (o.Position.X <= 0)
                    return CourtSide.Deuse;
                return CourtSide.Advantage;
            }
        }
    }
}
