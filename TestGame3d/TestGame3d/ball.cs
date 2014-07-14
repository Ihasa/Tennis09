using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Tennis01
{
    class Ball1
    {
        public Vector3 position,speed;//座標と速度
        float gravity;//重力
        public Model model;//publicにすべきではない
        double distance;//飛距離
        int bounds;//バウンド回数
        float shotAngleY;//打ち出される角度(上下
        Random rand;//いろいろと使われる乱数
        //本来はプレイヤーキャラクタに持たせるべきメンバ?
        Vector2 leftUp, rightUp, leftBelow, rightBelow;//コートの左奥～右手前の座標
        
        public Ball1(Model m, Vector3 pos, Vector3 sp)
        {
            model = m;
            position = pos;
            speed = sp;
            gravity = 0.002f;
            distance = 5.5;
            bounds = 0;
            //本来は(ry
            //コートの隅の座標
            leftUp = new Vector2(-1.06f, -3.2015f);
            rightUp = new Vector2(1.06f, -3.2015f);
            leftBelow = new Vector2(-1.06f, 3.2015f);
            rightBelow = new Vector2(1.06f, 3.2015f);
            shotAngleY = MathHelper.ToRadians(10);
            rand = new Random();
        }
        public void Update()
        {
            //基本処理
            position += speed;
            if (position.Y < 0)
            {
                position.Y = 0;
                bounds++;
                speed.X = 0.5f * speed.X;
                speed.Y = -0.75f*speed.Y;
                speed.Z = 0.5f * speed.Z;
               
            }
            speed.Y -= gravity;

            //「プレイヤーキャラに打たれたときの処理」
            if (position.Z > 3.2767f)//手前の人
            {
                //↓玉を打ち出すのに必要なパラメータをいじって遊んでいる
                shotAngleY = (float)MathHelper.ToRadians((float)rand.NextDouble()*10+20);
                //distance = 3.2f + 1.75f + rand.NextDouble() * 1.1f;
                bounds = 0;
                position.Z = 3.2767f;
                float v0 = (float)Math.Sqrt(gravity*distance*distance/(2*Math.Cos(shotAngleY)*Math.Cos(shotAngleY)*(position.Y+distance*Math.Tan(shotAngleY))));//(float)Math.Sqrt((distance*Math.Tan(shotAngleY)-position.Y)*gravity/(Math.Sin(shotAngleY*2)*Math.Tan(shotAngleY)));
                double maxAngleL = Math.Atan((position.X - leftUp.X) / (position.Z - leftUp.Y));
                double maxAngleR = Math.Atan((position.X - rightUp.X) / (position.Z - rightUp.Y));

                speed.Y = (float)(v0 * Math.Sin(shotAngleY));
                speed.Z = (float)-(v0 * Math.Cos(shotAngleY));

                //X速度はランダムにしてみる
                float shotAngleX;
                /*double val = rand.NextDouble();
                if (val < 0.5)
                {
                    shotAngleX = (float)((val+0.5) * maxAngleL);
                }else
                {
                    shotAngleX = (float)(val * maxAngleR);
                }*/
                //必ずコーナーに返す
                shotAngleX = (float)maxAngleL;
                speed.X = (float)(speed.Z * Math.Tan(shotAngleX));
            }
            else if (position.Z < -3.2767f)//奥の人
            {
                //↓玉を打ち出すのに必要なパラメータをいじって遊んでいる
                shotAngleY = (float)MathHelper.ToRadians((float)rand.NextDouble()*20+10);
                //distance = 3.2f + 1.75f + rand.NextDouble() * 1.1f;
                bounds = 0;
                position.Z = -3.2767f;
                float v0 = (float)Math.Sqrt(gravity * distance *distance/ (2 * Math.Cos(shotAngleY) * Math.Cos(shotAngleY) * (position.Y + distance * Math.Tan(shotAngleY))));//(float)Math.Sqrt((distance * Math.Tan(shotAngleY) - position.Y) * gravity / (Math.Sin(shotAngleY * 2) * Math.Tan(shotAngleY)));
                double maxAngleL = Math.Atan((position.X - leftBelow.X) / (position.Z - leftBelow.Y));
                double maxAngleR = Math.Atan((position.X - rightBelow.X) / (position.Z - rightBelow.Y));
                
                speed.Y = (float)(v0 * Math.Sin(shotAngleY));
                speed.Z = (float)(v0 * Math.Cos(shotAngleY));

                //必ずコーナーに返す
                float shotAngleX = (float)maxAngleL;
                
                speed.X = (float)(speed.Z * Math.Tan(shotAngleX));
            }
        }
        public void Draw()
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = Matrix.CreateTranslation(position);
                }
                mesh.Draw();
            }
        }
    }
}
