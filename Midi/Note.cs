using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Midi
{
    public class Note
    {
        public long StartTick { get; }
        public long EndTick { get; }
        public int Pitch { get; }
        public string NoteName { get; }
        public long PixelStartX { get; set; }
        public long PixelEndX { get; set; }
        public long PixelLength { get; set; }
        public int PixelY { get; set; }

        public Note(long startTick, long endTick, int pitch, string noteName)
        {
            StartTick = startTick;
            EndTick = endTick;
            Pitch = pitch;
            NoteName = noteName;
        }

        public long DurationTicks => EndTick - StartTick;
    }

}
