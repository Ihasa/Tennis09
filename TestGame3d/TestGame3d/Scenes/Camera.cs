using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tennis01.Scenes
{
    class Camera:Microsoft.Xna.Framework.Audio.AudioListener
    {
        //public static Camera FrontCamera = new Camera(Vector3.Forward*5, Vector3.Zero, Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(30), Scene.Viewport.AspectRatio, 1.0f, 100.0f));
        //public static Camera UpCamera = new Camera(Vector3.Up*5, Vector3.Zero, Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(30), Scene.Viewport.AspectRatio, 1.0f, 100.0f));
        //public static Camera LeftCamera = new Camera(Vector3.Left*5, Vector3.Zero, Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(30), Scene.Viewport.AspectRatio, 1.0f, 100.0f));
        
        //PositionはAudioListenerと共有
        public Vector3 Target { get; set; }
        public Matrix View { get { return Matrix.CreateLookAt(Position, Target, Vector3.Up); } }
        public Matrix Projection { get { return Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FieldOfView), Viewport.AspectRatio, 0.27f, 100.0f); } }
        public float FieldOfView { get; set; }
        public Viewport Viewport { get; set; }
        public Vector3 Direction { get { return Target - Position; } }
        
        public Camera(Vector3 pos, Vector3 lookAt, float fieldOfView,Viewport viewPort)
        {
            Position = pos;
            Target = lookAt;
            FieldOfView = fieldOfView;
            Viewport = viewPort;
            //Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FieldOfView), Viewport.AspectRatio, 0.27f, 100.0f);
            //Projection = projection;
            //AudioListenerの設定
            base.Forward = new Vector3(Target.X-Position.X,0,Target.Z-Position.Z);
            base.Up = Vector3.Up;
            base.Velocity = Vector3.Zero;
        }
        public void Update()
        {
            Position += Velocity;
            base.Forward = new Vector3(Target.X - Position.X, 0, Target.Z - Position.Z);
        }
    }
}
