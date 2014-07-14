using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tennis01.Input;
using Microsoft.Xna.Framework;

namespace Tennis01.Objects.PlayerStates
{
    abstract class PlayerState
    {
        //操作対象のPlayerインスタンス
        protected Player Player { get; private set; }
        public PlayerState(Player player,string animationName, string soundName="", TimeSpan? start=null, TimeSpan? start2=null)
        {
            this.Player = player;
            AnimationName = animationName;
            SoundName = soundName;
            AnimationStartTime = start ?? TimeSpan.Zero;
            AnimationStartTime2 = start2 ?? AnimationStartTime;
        }

        //public abstract void Update(ControlerState controlerState, PlayerAbility ability, ref float v, ref Vector3 position,ref Vector2 direction,ref Vector3 rotation);
        public abstract void Update(ControllerState controlerState);
        public bool GooL { get; protected set; }
        public bool GooR { get; protected set; }
        public PlayerState NextState { get; protected set; }
        public string AnimationName { get; protected set; }
        public TimeSpan AnimationStartTime { get; protected set; }
        public TimeSpan AnimationStartTime2 { get; protected set; }
        public string SoundName { get; protected set; }
    }
}
