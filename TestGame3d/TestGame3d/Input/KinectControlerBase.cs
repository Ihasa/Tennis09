using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Kinect;
using Tennis01.Objects;


namespace Tennis01.Input
{
    abstract class KinectControlerBase:Controller,IDisposable
    {
        #region フィールド
        KinectSensor kinect;
        //☆Kinectからどんな情報を取得するか

        /// <summary>
        /// 何フレーム分の情報を取得するか
        /// </summary>
        int getFrames;
        #endregion
        
        #region プロパティ
        /// <summary>
        /// ジョイントの情報。0が最新、過去のgetFrames - 1までの情報を取得
        /// </summary>
        protected JointCollection[] JointStates { get; private set; }
        protected int ActiveJointStates
        {
            get
            {
                for (int i = 0; i < getFrames; i++)
                {
                    if (JointStates[i] == null)
                    {
                        return i;
                    }
                }
                return getFrames;
            }
        }
        protected SkeletonPoint[] DepthData { get; private set; }
        public Texture2D ColorImage { get;private set; }
        #endregion
        
        #region 開始・終了処理(コンストラクタとか)
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="kinectNum">使用するKinectの番号。一台しかつないでない場合は0でよい。</param>
        public KinectControlerBase(int kinectNum)
        {
            if (kinectNum >= KinectSensor.KinectSensors.Count)
            {
                throw new ArgumentException("指定したKinectは接続されていません");
            }
            if (KinectSensor.KinectSensors[kinectNum].Status != KinectStatus.Connected)
            {
                throw new Exception("指定したKinectは接続されていません");
            }
            //Kinectの開始処理
            kinect = KinectSensor.KinectSensors[kinectNum];
            StartKinect();
            initObjects();
        }
        void initObjects()
        {
            getFrames = 60;
            JointStates = new JointCollection[60];
            ColorImage = new Texture2D(Scenes.Scene.Game.GraphicsDevice, kinect.ColorStream.FrameWidth, kinect.ColorStream.FrameHeight);
        }
        /// <summary>
        /// Kinectの開始処理
        /// </summary>
        /// <param name="kinect"></param>
        private void StartKinect()
        {
            //kinectのRGBカメラを有効にする
            kinect.ColorStream.Enable();
            //フレーム更新時のイベントを登録
            //kinect.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(getColorFrameInfo);

            //☆距離カメラの追加
            //同じく、有効にしてフレーム更新イベントを登録
            kinect.DepthStream.Enable();
            //kinect.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(getDepthFrameInfo);

            //☆プレイヤー情報の取得のための追加
            //スケルトンを有効にする
            kinect.SkeletonStream.Enable();
            //フレーム更新イベントを登録
            //kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(getSkeletonFrameInfo);

            //開始
            kinect.Start();
        }
        /// <summary>
        /// デストラクタのようなもの
        /// </summary>
        public void Dispose()
        {
            StopKinect();
        }
        //Kinectの終了処理
        //<param name = "kinect"></param>
        private void StopKinect()
        {
            if (kinect != null)
            {
                if (kinect.IsRunning)
                {
                    ////フレーム更新イベントを消す
                    //kinect.ColorFrameReady -= getColorFrameInfo;

                    //kinect.DepthFrameReady -= getDepthFrameInfo;

                    //kinect.SkeletonFrameReady -= getSkeletonFrameInfo;

                    //kinect停止
                    kinect.Stop();
                    //インスタンス破棄
                    kinect.Dispose();
                }
            }
        }
        #endregion

        #region オーバーライド
        public override void Update()
        {
            updateColorFrameInfo();
            updateDepthFrameInfo();
            updateSkeletonFrameInfo();
        }
        #endregion

        #region Kinectから情報を取得

        /// <summary>
        /// RGBデータを取得
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updateColorFrameInfo()
        {
            using (ColorImageFrame cFrame = kinect.ColorStream.OpenNextFrame(1))
            {
                if (cFrame != null)
                {
                    //Texture2Dに変換
                    byte[] imageData = new byte[cFrame.PixelDataLength];
                    cFrame.CopyPixelDataTo(imageData);
                    byte[] textureData = new byte[imageData.Length];
                    for (int i = 0; i + 3 < imageData.Length; i += 4)
                    {
                        textureData[i] = imageData[i + 2];//R
                        textureData[i + 1] = imageData[i + 1];//G
                        textureData[i + 2] = imageData[i];//B
                        textureData[i + 3] = 255;
                    }
                    ColorImage.SetData(textureData);
                    GameMain.debugStr["imageDataLength"] = ""+imageData.Length;
                    GameMain.debugStr["format"] = ""+cFrame.Format;
                }
            }
        }
        /// <summary>
        /// 深度情報を取得
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updateDepthFrameInfo()
        {
            using (DepthImageFrame dFrame = kinect.DepthStream.OpenNextFrame(1))
            {
                if (dFrame != null)
                {
                    CoordinateMapper mapper = new CoordinateMapper(kinect);
                    DepthData = new SkeletonPoint[dFrame.PixelDataLength];
                    DepthImagePixel[] depthData = new DepthImagePixel[dFrame.PixelDataLength];
                    dFrame.CopyDepthImagePixelDataTo(depthData);
                    mapper.MapDepthFrameToSkeletonFrame(DepthImageFormat.Resolution640x480Fps30, depthData, DepthData);
                }
            }
        }

        /// <summary>
        /// ジョイントの情報を取得
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updateSkeletonFrameInfo()
        {
            using (SkeletonFrame sFrame = kinect.SkeletonStream.OpenNextFrame(1))
            {
                if (sFrame != null)
                {
                    Skeleton[] skeletons = new Skeleton[sFrame.SkeletonArrayLength];
                    sFrame.CopySkeletonDataTo(skeletons);
                    //最初に検出したスケルトンを使用
                    for (int i = 0; i < skeletons.Length; i++)
                    {
                        if (skeletons[i].TrackingState == SkeletonTrackingState.Tracked)
                        {
                            updateJointStates(skeletons[i].Joints);
                            GameMain.debugStr["Kinect is"] = "Tracking";
                            break;
                        }else
                            GameMain.debugStr["Kinect is"] = "Not Tracking";
                    }
                }
            }
        }
        void updateJointStates(JointCollection current)
        {
            for (int i = getFrames - 1; i > 0; i--)
            {
                JointStates[i] = JointStates[i - 1];
            }
            JointStates[0] = current;
        }
        #endregion

    }
}
