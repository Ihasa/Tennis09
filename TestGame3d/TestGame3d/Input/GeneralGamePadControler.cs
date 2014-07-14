using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tennis01.Input
{
    class GeneralGamePadControler:DeviceControler
    {
        public GeneralGamePadControler() : base(
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
        )
        {

        }
        protected override ControllerState getState()
        {
            throw new NotImplementedException();
        }
        public override void  Update()
        {
 	        throw new NotImplementedException();
        }
    }
}
