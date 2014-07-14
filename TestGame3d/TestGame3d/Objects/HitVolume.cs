using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Tennis01.Objects
{
    enum HitType
    {
        Box, Sphere
    }
    struct HitVolume
    {
        #region フィールド
        /// <summary>
        /// 衝突判定の形
        /// </summary>
        HitType hitType;
        /// <summary>
        /// 使用する衝突箱
        /// </summary>
        BoundingBox boundingBox;
        /// <summary>
        /// 使用する衝突球
        /// </summary>
        BoundingSphere boundingSphere;
        /// <summary>
        /// 法線方向
        /// </summary>
        public Vector3 Normal;
        #endregion

        #region コンストラクタ
        /// <summary>
        /// 形と位置を指定してモデルを基に初期化。
        /// </summary>
        /// <param name="model"></param>
        /// <param name="position"></param>
        /// <param name="type"></param>
        public HitVolume(Model model, Vector3 position, HitType type,Vector3? normal = null)
        {
            hitType = type;
            //先に球を初期化
            boundingSphere = new BoundingSphere(position, model.Meshes[0].BoundingSphere.Radius);
            //それに合わせて箱を初期化
            boundingBox = BoundingBox.CreateFromSphere(boundingSphere);
            if (normal != null)
                Normal = (Vector3)normal;
            else 
                Normal = Vector3.Zero;
        }

        /// <summary>
        /// 球で初期化
        /// </summary>
        /// <param name="position"></param>
        /// <param name="radius"></param>
        public HitVolume(Vector3 position, float radius,Vector3? normal = null)
        {
            hitType = HitType.Sphere;
            boundingSphere = new BoundingSphere(position, radius);
            boundingBox = BoundingBox.CreateFromSphere(boundingSphere);
            if (normal != null)
                Normal = (Vector3)normal;
            else 
                Normal = Vector3.Zero;
        }

        /// <summary>
        /// 直方体で初期化。三辺の長さを指定。
        /// </summary>
        /// <param name="center"></param>
        /// <param name="lx"></param>
        /// <param name="ly"></param>
        /// <param name="lz"></param>
        public HitVolume(Vector3 position, float lx, float ly, float lz,Vector3? normal = null)
        {
            hitType = HitType.Box;
            boundingBox = new BoundingBox(
                
                    new Vector3(position.X - lx/2,position.Y - ly/2, position.Z - lz/2),
                    new Vector3(position.X + lx/2,position.Y + ly/2, position.Z + lz/2)
                
            );
            boundingSphere = BoundingSphere.CreateFromBoundingBox(boundingBox);
            GameMain.debugStr["min"] = boundingBox.Min+"";
            GameMain.debugStr["max"] = boundingBox.Max + "";
            if (normal != null)
                Normal = (Vector3)normal;
            else Normal = Vector3.Zero;
        }
        #endregion
        /// <summary>
        /// 大きさを表す
        /// </summary>
        public float Radius
        {
            get { return boundingSphere.Radius; }
            set
            {
                float oldRadius = boundingSphere.Radius;
                boundingSphere.Radius = value;
                //boundingBox = BoundingBox.CreateFromSphere(boundingSphere);
                boundingBox.Min *= value / oldRadius;
                boundingBox.Max *= value / oldRadius;
            }
        }
        /// <summary>
        /// あたり判定の中心座標
        /// </summary>
        public Vector3 Center
        {
            get { return boundingSphere.Center; }
        }
        /// <summary>
        /// 状態の更新
        /// </summary>
        /// <param name="center">中心座標</param>
        public void Update(Vector3 center)
        {
            Vector3 offset = center - boundingSphere.Center;
            boundingSphere.Center = center;
            boundingBox = new BoundingBox(boundingBox.Min + offset, boundingBox.Max + offset);
        }
        public void Draw(Matrix view, Matrix projection)
        {
            Model m;
            if (hitType == HitType.Sphere)
                m = GameMain.Models["debugSphire"];
            else
                m = GameMain.Models["debugBox"];

            foreach (ModelMesh mesh in m.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.View = view;
                    effect.Projection = projection;
                    if (hitType == HitType.Sphere)
                        effect.World = Matrix.CreateScale(Radius) * Matrix.CreateTranslation(boundingSphere.Center);
                    else if (hitType == HitType.Box)
                        effect.World = Matrix.CreateScale(new Vector3(boundingBox.Max.X - Center.X, boundingBox.Max.Y - Center.Y, boundingBox.Max.Z - Center.Z)) * Matrix.CreateTranslation(Center);
                }
                mesh.Draw();
            }

        }
        public bool Hit(HitVolume volume)
        {
            switch (this.hitType)
            {
                case HitType.Sphere:
                    switch (volume.hitType)
                    {
                        case HitType.Sphere:
                            return boundingSphere.Intersects(volume.boundingSphere);
                        case HitType.Box:
                            return boundingSphere.Intersects(volume.boundingBox);
                    }
                    break;
                case HitType.Box:
                    switch (volume.hitType)
                    {
                        case HitType.Sphere:
                            return boundingBox.Intersects(volume.boundingSphere);
                        case HitType.Box:
                            return boundingBox.Intersects(volume.boundingBox);
                    }
                    break;
            }
            return false;
        }
    }


}
