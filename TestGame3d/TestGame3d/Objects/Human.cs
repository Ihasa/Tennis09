using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SkinnedModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Extention;
namespace Tennis01.Objects
{
    using Scenes;
    class Human:HittableObject3D
    {
        #region フィールド
        //首の角度
        float neckRadY, neckRadX;
        //見つめるもの
        public Object3D TargetObject { get; set; }
        //向いている方向ベクトル(X,Z)
        protected Vector2 bodyDirection;
        //背の高さ
        float height;
        //持ち物
        public Object3D Belonging { get; protected set; }
        List<Matrix> offsets;
        //体回転中
        bool turningBody = false;
        //手をグーにするかどうか
        protected bool gooL=false, gooR=false;
        /// <summary>
        /// スキンメッシュアニメーションの再生機
        /// </summary>
        protected AnimationPlayer animationPlayer;
        //protected AnimationPlayer animationPlayer2;
        /// <summary>
        /// アニメーションしているか
        /// </summary>
        bool isAnimating;
        #endregion

        #region コンストラクタ
        /// <summary>
        /// 3Dモデル、位置、ZX平面での速度、変換、使用するサウンドバンクを指定して初期化
        /// </summary>
        /// <param name="m">使用する3Dモデル</param>
        /// <param name="position">位置</param>
        /// <param name="speedZX">ZX平面における速度</param>
        /// <param name="direction">ZX平面におけるモデルの向きを表す正規化されたベクトル</param>
        /// <param name="transform">特殊な回転やスケーリングなど</param>
        /// <param name="soundBankName">使用するサウンドバンクの名前</param>
        public Human(Model m, Camera c,Vector3 position, Vector2 speedZX,Vector2? direction=null, string animationName=null,Matrix? transform = null, string soundBankName = null,string belongingName=null)
            : base(m,c, HitType.Box,position, new Vector3(speedZX.X, 0, speedZX.Y), null, transform, soundBankName)
        {
            //モデルの向きの初期化
            bodyDirection = direction ?? new Vector2(0, 1);// MathHelper.ToDegrees((float)Math.Acos(dir.Y / dir.Length()));
            if (bodyDirection.Length() != 1)
                throw new ArgumentException("正規化されていない方向ベクトルが渡されました。");
            
            neckRadY = 0;
            neckRadX = 0;

            height = Radius * 2;
            //持ち物
            if (belongingName != null)
            {
                Belonging = new Object3D(GameMain.Models[belongingName],c);
                Belonging.DrawOrder = 9;
                //Radius += Game1.Models[belongingName].Meshes[0].BoundingSphere.Radius;
            }
            //バインドポーズを人間用に
            //Vector3[] translations = new Vector3[SkinningData.BindPose.Count];
            //for (int i = 0; i < translations.Length; i++)
            //{
            //    translations[i] = SkinningData.BindPose[i].Translation;
            //}
            //Matrix[] bindPose = new Matrix[Game1.HumanBindPose.Count];
            //for (int i = 0; i < bindPose.Length; i++)
            //{
            //    bindPose[i] = Game1.HumanBindPose[i];
            //    bindPose[i].Translation = translations[i];
            //}
            //SkinningData.BindPose = new List<Matrix>(bindPose);
            
            
            //アニメーションプレイヤーを初期化
            animationPlayer = new AnimationPlayer(SkinningData);
            //animationPlayer2 = new AnimationPlayer(SkinningData);
            //人間用モーションは全員共通。
            Clips = GameMain.HumanMotions;
            offsets = new List<Matrix>(SkinningData.BindPose);
            for (int i = 0; i < offsets.Count; i++)
            {
                offsets[i] -= GameMain.HumanBindPose[i];
            }
            //通常時のアニメーションを開始
            StartAnimation(animationName??"Animation_1");
            //animationPlayer2.StartClip(Clips["Animation_12"]);
            VisibleChanged += (o, e) =>
            {
                 Belonging.Visible = Visible;
            };
        }
        #endregion

        #region 操作
        /// <summary>
        /// 位置と向きをセットする
        /// </summary>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        public void SetPosture(Vector3 position, Vector2 direction)
        {
            this.position = position;
            this.bodyDirection = direction;
        }
        /// <summary>
        /// 指定した名前のアニメーションを開始する
        /// </summary>
        /// <param name="animationName"></param>
        protected void StartAnimation(string animationName, TimeSpan? firstTimeValue = null, TimeSpan? firstTimeValue2RAftr = null)
        {
            animationPlayer.StartClip(Clips[animationName], firstTimeValue, firstTimeValue2RAftr);
            isAnimating = true;
        }
        #endregion

        #region プロパティ
        /// <summary>
        /// 体が回転中かどうか
        /// </summary>
        protected bool TurningBody { get { return turningBody; } }
        /// <summary>
        /// 衝突判定の(おおよその)中心座標
        /// </summary>
        protected override Vector3 Center
        {
            get
            {
                return position + new Vector3(0, Radius, 0);
            }
        }
        #endregion

        #region 更新、描画
        public override void Update(GameTime gameTime)
        {
            float radian = bodyDirection.ToRadians();
            turnBody(radian);
            turnNeck(rotation.Y);
            
            //アニメーション
            if (HasAnimation && isAnimating)
            {
                Matrix[] bones = animationPlayer.GetBoneTransforms();

                animationPlayer.UpdateBoneTransforms(gameTime.ElapsedGameTime, true);

                for (int i = 0; i < bones.Length; i++)
                {
                    bones[i].Translation += offsets[i].Translation;
                }
                
                animationPlayer.UpdateWorldTransforms(Matrix.Identity);

                animationPlayer.UpdateSkinTransforms();
                for (int i = 0; i < bones.Length; i++)
                {
                    bones[i].Translation -= offsets[i].Translation;
                }
                //animationPlayer2.UpdateBoneTransforms(gameTime.ElapsedGameTime, true);
                //Matrix[] bones2 = animationPlayer2.GetBoneTransforms();
                //for (int i = 0; i < bones2.Length; i++)
                //{
                //    bones2[i].Translation += offsets[i].Translation;
                //}
                //animationPlayer2.UpdateWorldTransforms(Matrix.Identity);

                //animationPlayer2.UpdateSkinTransforms();
                //for (int i = 0; i < bones.Length; i++)
                //{
                //    bones2[i].Translation -= offsets[i].Translation;
                //}
                //animationPlayer.Update(gameTime.ElapsedGameTime, true, Matrix.Identity);
                //首回転
                TransformBones(3, Matrix.CreateRotationX(-neckRadX) * Matrix.CreateRotationY(neckRadY));
                //手をグーにする
                float rad = MathHelper.ToRadians(85);
                //右手
                if (gooR)
                {
                    for (int i = 13; i <= 27; i++)
                    {
                        if (i <= 15)
                            TransformBones(i, Matrix.CreateRotationX(rad));
                        else
                            TransformBones(i, Matrix.CreateRotationZ(rad));
                    }
                }
                //左手
                if (gooL)
                {
                    for (int i = 32; i <= 46; i++)
                    {
                        if (i <= 34)
                            TransformBones(i, Matrix.CreateRotationX(rad));
                        else
                            TransformBones(i, Matrix.CreateRotationZ(-rad));
                    }
                }
            }
            base.Update(gameTime);
        }
        #region 首と体の回転

        /// <summary>
        /// 首の回転
        /// </summary>
        /// <param name="radian">体の回転角度(ラジアン)</param>
        private void turnNeck(float radian)
        {
            neckRadY = neckRadX = 0;
            if (TargetObject != null)
            {
                //自分から見てターゲットがある方向(Y軸回転)
                Vector2 vec = new Vector2(TargetObject.Position.X - position.X, TargetObject.Position.Z - position.Z);
                float rad;
                if (vec.Length() != 0)
                {
                    rad = vec.ToRadians();
                    //首の角度=ボールの方向-体の向いている方向
                    neckRadY = rad - radian;
                }

                //π/2以下だったら、X軸でも回転
                GameMain.debugStr["" + this] = "" + MathHelper.ToDegrees(neckRadY);
                float radAbs = (float)Math.Abs(neckRadY);
                if (radAbs <= MathHelper.PiOver2 * 1.5f || radAbs >= MathHelper.PiOver2 * 2.5f)
                {
                    //自分から見てターゲットがある方向(X軸回転)
                    Vector3 vec3 = TargetObject.Position - position;
                    vec3.Y -= height;
                    if (vec3.Length() != 0)
                    {
                        rad = (float)Math.Asin(vec3.Y / vec3.Length());
                        neckRadX = rad;
                    }
                    //neckRadX = 0;
                }
                else
                {
                    neckRadX = 0;
                    neckRadY = 0;
                }
            }
        }
        /// <summary>
        /// 体の回転
        /// </summary>
        /// <param name="radian">進行方向を表す角度(ラジアン)</param>
        private void turnBody(float radian)
        {
            //角度を-π~πに直す
            radian = MathHelper.WrapAngle(radian);
            rotation.Y = MathHelper.WrapAngle(rotation.Y);
            //一定角度ずつ回転
            float angle = 15;
            if (rotation.Y > radian)
            {
                if (rotation.Y - radian > Math.PI)
                {
                    rotation.Y += MathHelper.ToRadians(angle);
                }
                else
                {
                    rotation.Y -= MathHelper.ToRadians(angle);
                }
            }
            else if (rotation.Y != radian)
            {
                if (radian - rotation.Y > Math.PI)
                {
                    rotation.Y -= MathHelper.ToRadians(angle);
                }
                else
                {
                    rotation.Y += MathHelper.ToRadians(angle);
                }
            }
            if (Math.Abs(rotation.Y - radian) <= MathHelper.ToRadians(angle))
            {
                rotation.Y = radian;
                turningBody = false;
            }
            else
            {
                turningBody = true;
            }
        }
        #endregion
        protected override void DrawModel(GameTime gameTime, Matrix view, Matrix projection)
        {

            Matrix[] bones = animationPlayer.GetSkinTransforms();
            //Matrix[] bones2 = animationPlayer2.GetSkinTransforms();

            Matrix[] skinTransforms = new Matrix[bones.Length];
            for (int i = 0; i < skinTransforms.Length; i++)
            {
                skinTransforms[i] = bones[i] /* * bones2[i]*/;
            }

            //Game1.debugStr["bones"] = bones.Length + "";
            //Game1.debugStr["Bones.Count"] = model.Bones.Count + "";

            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.SetBoneTransforms(skinTransforms);
                    effect.View = view;
                    effect.Projection = projection;
                    effect.World = AnotherTransform * Matrix.CreateRotationX(rotation.X) * Matrix.CreateRotationY(rotation.Y) * Matrix.CreateRotationZ(rotation.Z) * Matrix.CreateTranslation(position);
                }
                mesh.Draw();
            }
            //base.DrawModel(gameTime, view, projection);
            if (Belonging != null)
            {
                Matrix[] m = new Matrix[Model.Bones.Count];
                Model.CopyAbsoluteBoneTransformsTo(m);
                Vector3 offset = new Vector3(-0.04f, -0.02f, 0);
                Belonging.AnotherTransform = Matrix.CreateScale(4) * HandMatrix();// Matrix.CreateTranslation(offset) * animationPlayer.GetWorldTransforms()[12] * Matrix.CreateRotationY(rotation.Y) * Matrix.CreateTranslation(position);
                //Belonging.Draw(gameTime);
            }

            DrawHitVolume(view, projection);
        }
        protected Matrix HandMatrix()
        {
            Vector3 offset = new Vector3(-0.04f, -0.02f, 0);
            return Matrix.CreateTranslation(offset) * animationPlayer.GetWorldTransforms()[12] * 
                Matrix.CreateRotationX(rotation.X)*Matrix.CreateRotationY(rotation.Y) * Matrix.CreateRotationZ(rotation.Z) * 
                Matrix.CreateTranslation(position);
        }
        /// <summary>
        /// indexとその子ボーンをtransformで変換させる。Y軸回転以外はやらないほうがいいかも
        /// </summary>
        /// <param name="index">変換させたいルートボーンのインデックス</param>
        /// <param name="transform">変換行列</param>
        protected void TransformBones(int index, Matrix transform)
        {
            if (!HasAnimation)
                throw new InvalidOperationException();
            int endIndex = index + countBones(Model.Bones[index + 3]);
            Matrix[] absoluteBones = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(absoluteBones);
            Matrix translation = Matrix.CreateTranslation(absoluteBones[index + 3].Translation);
            for (int i = index; i <= endIndex; i++)
            {
                animationPlayer.skinTransforms[i] = Matrix.Invert(translation) * transform * translation * animationPlayer.skinTransforms[i];
            }
        }

        //引数boneと、子ボーンの数を合計した数を返す
        int countBones(ModelBone rootBone)
        {
            int res = 0;
            res += rootBone.Children.Count;
            for (int i = 0; i < rootBone.Children.Count; i++)
            {
                ModelBone childBone = rootBone.Children[i];
                res += countBones(childBone);
            }
            return res;
        }
        #endregion
    }
}
