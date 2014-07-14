using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Tennis01.Objects
{
    using Scenes;
    class HittableObject3D:Object3D
    {
        #region フィールド
        /// <summary>
        /// 衝突判定に使う図形
        /// </summary>
        HitVolume hitVolume;
        public float Radius
        {
            get { return hitVolume.Radius; }
            set { hitVolume.Radius = value; }
        }
        protected virtual Vector3 Center { get { return position; } }
        /// <summary>
        /// 衝突判定の領域を取得
        /// </summary>
        protected HitVolume HitVolume { get { return hitVolume; } set { hitVolume = value; } }

        #endregion

        #region コンストラクタ
        /// <summary>
        /// 位置や速度などを指定して初期化。
        /// </summary>
        /// <param name="m">このキャラクタの3Dモデル</param>
        /// <param name="pos">座標</param>
        /// <param name="spd">速度</param>
        public HittableObject3D(Model m,Camera c,HitType? hitType = null,Vector3? pos = null,Vector3? spd=null,Vector3? rot = null,Matrix? transform=null,string soundBankName=null)
            :base(m,c,pos,spd,rot,transform,soundBankName)
        {
            hitVolume = new HitVolume(m, position, hitType ?? HitType.Sphere);
            initShadow(Radius);
        }
        /// <summary>
        /// モデルと変換とサウンドを指定して初期化。
        /// </summary>
        /// <param name="m">このキャラクタの3Dモデル</param>
        /// <param name="pos">座標</param>
        /// <param name="spd">速度</param>
        public HittableObject3D(Model m, Camera c,HitType hitType,Matrix transform,string soundBankName = null)
            :base(m,c,transform,soundBankName)
        {
            hitVolume = new HitVolume(m, position, hitType);
            initShadow(Radius);
        }

        #endregion

        public bool Hit(HittableObject3D obj)
        {
            return hitVolume.Hit(obj.hitVolume);
        }
        public bool Hit(HitVolume h)
        {
            return hitVolume.Hit(h);
        }

        #region 更新と描画
        /// <summary>
        /// boundingSphereの位置更新を追加
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            hitVolume.Update(Center);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="view"></param>
        /// <param name="projection"></param>
        protected override void DrawModel(GameTime gameTime, Matrix view, Matrix projection)
        {
            base.DrawModel(gameTime, view, projection);
            DrawHitVolume(view, projection);
        }
        protected void DrawHitVolume(Matrix view,Matrix projection)
        {
            if (GameMain.debug && GameMain.gamePadStates[0][0].IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.LeftShoulder))
                hitVolume.Draw(view, projection);
        }

        #endregion
    }
}
