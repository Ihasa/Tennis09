using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Tennis01.Objects;

namespace Tennis01
{
    class SoundPlayer
    {
        #region フィールド
        /// <summary>
        /// 3Dオーディオを使う場合のスピーカー。
        /// </summary>
        AudioEmitter audioEmitter=null;
        /// <summary>
        /// このオブジェクトから発せられた音を聞く人。
        /// </summary>
        AudioListener audioListener=null;

        //再生するサウンド
        Sounds sounds;
        //再生中のキューのリスト
        Dictionary<string, Cue> playingCues = new Dictionary<string, Cue>();
        #endregion


        /*static SoundPlayer()
        {
             audioEngine = new AudioEngine(@"Content\Sound\HelloXact.xgs");
        }*/
        /// <summary>
        /// 3Dオーディオを使わずにサウンドプレイヤーを初期化する。
        /// </summary>
        /// <param name="soundBankName">使用するサウンドバンクの名前</param>
        public SoundPlayer(string soundBankName)
        {
            InitSoundBanks(soundBankName);
        }
        /// <summary>
        /// 3Dオーディオを使ってサウンドプレイヤーを初期化する。
        /// </summary>
        /// <param name="soundBankName">使用するサウンドバンクの名前</param>
        /// <param name="emitter">音を発するAudioEmitterオブジェクト</param>
        /// <param name="listener">音を聞くAudioListenerオブジェクト</param>
        public SoundPlayer(string soundBankName, AudioEmitter emitter, AudioListener listener)
        {
            InitSoundBanks(soundBankName);
            initEmitterAndListener(emitter, listener);
        }
        /// <summary>
        /// このオブジェクトのオーディオを初期化する
        /// </summary>
        /// <param name="listener">使用するAudioListenerオブジェクト</param>
        private void initEmitterAndListener(AudioEmitter emitter,AudioListener listener)
        {
            //AudioListenerを登録
            audioListener = listener;
            //AudioEmitterの設定
            audioEmitter = emitter;
            audioEmitter.Up = Vector3.Up;
            //デフォルトではaudioListenerのほうへ向く
            Vector3 forward = audioListener.Position - audioEmitter.Position;
            forward.Y = 0;
            audioEmitter.Forward = forward;
        }
        
        /// <summary>
        /// SoundBankとWaveBankを初期化。SoundBankとWaveBankの名前は同じにしておくこと。
        /// </summary>
        /// <param name="name">SoundBankとWaveBankの名前(共通)</param>
        private void InitSoundBanks(string name)
        {
            sounds = GameMain.Sounds[name];
        }
        #region 外部からの操作
        /*
        /// <summary>
        /// AudioEngineの状態を更新
        /// </summary>
        public static void UpdateEngine()
        {
            audioEngine.Update();
        }*/
        /// <summary>
        /// 指定したサウンドエフェクトを再生する
        /// </summary>
        /// <param name="soundName">サウンドエフェクトの名前</param>
        public void PlaySound(string soundName)
        {
            //同じサウンドがあったら最初からかけなおす
            if (playingCues.ContainsKey(soundName))
            {
                StopSound(soundName);
            }
            Cue cue = sounds.soundBank.GetCue(soundName);
            
            //3Dサウンドの適用
            if (Has3DAudio)
            {
                cue.Apply3D(audioListener, audioEmitter);
            }
            //再生、再生リストに追加
            cue.Play();
            playingCues.Add(soundName, cue);
        }
        /// <summary>
        /// 再生中のすべてのサウンドエフェクトを停止する
        /// </summary>
        public void StopSound()
        {
            foreach (Cue cue in playingCues.Values)
            {
                if (cue.IsPlaying)
                {
                    cue.Stop(AudioStopOptions.AsAuthored);
                }
            }
            playingCues.Clear();
        }
        /// <summary>
        /// 指定したサウンドエフェクトを停止する
        /// </summary>
        /// <param name="soundName"></param>
        public void StopSound(string soundName)
        {
            Cue cue;
            if (playingCues.TryGetValue(soundName, out cue))
            {
                if (cue.IsPlaying || cue.IsPaused)
                {
                    cue.Stop(AudioStopOptions.AsAuthored);
                    
                }
                playingCues.Remove(soundName);
            }
            else
            {
                //throw new InvalidOperationException("そのようなサウンドはありません");
            }
        }
        /// <summary>
        /// 再生中のサウンドに3Dオーディオを適用する
        /// </summary>
        public void Apply3D()
        {
            foreach (Cue cue in playingCues.Values)
            {
                cue.Apply3D(audioListener, audioEmitter);
            }
        }
        #endregion
        #region プロパティ
        /// <summary>
        /// 3Dオーディオに対応できるか
        /// </summary>
        public bool Has3DAudio { get { return audioEmitter != null&&audioListener!=null;} }
        #endregion
    }
}
