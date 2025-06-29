using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Midi
{
    public class VisualizationBrushes : IDisposable
    {
        public SolidBrush ActiveNote { get; }
        public SolidBrush InactiveNote { get; }
        public SolidBrush Background { get; }
        public SolidBrush Guideline { get; }

        public VisualizationBrushes(VisualizationConfig config)
        {
            ActiveNote = new SolidBrush(config.ActiveNoteColor);
            InactiveNote = new SolidBrush(config.InactiveNoteColor);
            Background = new SolidBrush(config.BackgroundColor);
            Guideline = new SolidBrush(config.GuidelineColor);
        }

        public void Dispose()
        {
            ActiveNote?.Dispose();
            InactiveNote?.Dispose();
            Background?.Dispose();
            Guideline?.Dispose();
        }
    }

}
