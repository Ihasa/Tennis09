using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tennis01.Input
{
    abstract class Controller
    {
        public abstract void Update();
        public ControllerState GetState()
        {
            if (!Enabled)
            {
                return new ControllerState();
            }
            ControllerState state = getState();
            return state;
        }
        protected abstract ControllerState getState();
        public bool Enabled { get; set; }
    }
}
