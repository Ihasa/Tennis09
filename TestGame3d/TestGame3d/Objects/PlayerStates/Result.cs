using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tennis01.Objects.PlayerStates
{
    abstract class Result:PlayerState
    {
        public Result(Player p,string animationName)
            : base(p, animationName)
        {
            p.Velocity = 0;
            p.TargetObject = null;
            GooL = GooR = false;
        }
    }
    class Won : Result
    {
        public Won(Player p)
            : base(p, "Animation_24")
        {
            
        }
        public override void Update(Input.ControllerState controlerState)
        {
        }
    }
    class Lost : Result
    {
        public Lost(Player p):base(p,"Animation_23")
        {
                        
        }
        public override void Update(Input.ControllerState controlerState)
        {
        }
    }
}
