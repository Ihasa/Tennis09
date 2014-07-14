using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Input;
namespace Tennis01.Input
{
    interface IControleable
    {
        /// <summary>
        /// このオブジェクトを操作するコントローラのインデックス
        /// </summary>
        int Index { get; set; }
        /// <summary>
        /// このオブジェクトを操作する
        /// </summary>
        /// <param name="controlerState">操作する時点でのコントローラの状態</param>
        void Control(ControllerState controlerState);
    }
}
