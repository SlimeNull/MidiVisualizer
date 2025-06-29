using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Midi
{
    public class VisualizationConfig
    {
        public int CanvasWidth { get; set; } = 1920;
        public int CanvasHeight { get; set; } = 1080;
        public int GuideLineX { get; set; } = 100;
        public int NoteHeight { get; set; } = 15;
        public double PixelsPerSecond { get; set; } = 192.0;
        public int FramesPerSecond { get; set; } = 30;
        public string MidiFilePath { get; set; } = "2.mid";
        public Color ActiveNoteColor { get; set; } = Color.White;
        public Color InactiveNoteColor { get; set; } = Color.FromArgb(100, 100, 100);
        public Color BackgroundColor { get; set; } = Color.Black;
        public Color GuidelineColor { get; set; } = Color.White;
        public int GuidelineWidth { get; set; } = 1;
    }

}
