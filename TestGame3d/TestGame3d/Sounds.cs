using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Audio;

namespace Tennis01
{
    public struct Sounds
    {
        public SoundBank soundBank;
        public WaveBank waveBank;
        public Sounds(SoundBank s, WaveBank w)
        {
            soundBank = s;
            waveBank = w;
        }
    }
}
