using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tennis01.Input
{
    /// <summary>
    /// 何もしないコントローラ
    /// </summary>
    class NullControler:Controller
    {
        public override void Update(){}
        protected override ControllerState getState() { return ControllerState.NoInput; }
    }
}
