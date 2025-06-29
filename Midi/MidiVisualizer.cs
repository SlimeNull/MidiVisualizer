using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Midi
{
    [SupportedOSPlatform("windows6.1")]
    public class MidiVisualizer
    {
        private record struct GeneratedFrame(int FrameIndex, Bitmap Frame);

        private readonly VisualizationConfig _config;
        private readonly MidiParser.MidiData _midiData;
        private readonly List<Note> _processedNotes;
        private readonly string _outputDirectory;

        public MidiVisualizer(VisualizationConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _midiData = MidiParser.ParseMidiFile(_config.MidiFilePath);
            _processedNotes = ProcessNotes();
            _outputDirectory = CreateOutputDirectory();
        }

        public void GenerateFrames()
        {
            if (!_processedNotes.Any())
            {
                Console.WriteLine("没有找到有效的音符");
                return;
            }

            double totalDurationSeconds = CalculateTotalDuration();
            int totalFrames = (int)Math.Ceiling(totalDurationSeconds * _config.FramesPerSecond);

            Console.WriteLine($"开始生成 {totalFrames} 帧...");
            Console.CursorVisible = false;

            try
            {
                using var brushes = new VisualizationBrushes(_config);

                bool generationRunning = true;
                ConcurrentQueue<GeneratedFrame> framesQueue = new ConcurrentQueue<GeneratedFrame>();

                var saveFramesThread = new Thread(() =>
                {
                    while (framesQueue.Count > 0 || generationRunning)
                    {
                        if (!framesQueue.TryDequeue(out var generatedFrame))
                        {
                            continue;
                        }

                        ThreadPool.QueueUserWorkItem(_ =>
                        {
                            // 保存帧
                            string framePath = Path.Combine(_outputDirectory, $"{generatedFrame.FrameIndex:0000}.png");
                            generatedFrame.Frame.Save(framePath, ImageFormat.Png);
                            generatedFrame.Frame.Dispose();
                        });
                    }
                });

                saveFramesThread.IsBackground = true;
                saveFramesThread.Start();

                for (int frameIndex = 0; frameIndex <= totalFrames; frameIndex++)
                {
                    while (framesQueue.Count > 200)
                    {
                        // 等待队列中的帧被处理
                        Thread.Sleep(10);
                    }

                    var frame = GenerateFrame(frameIndex, brushes);

                    framesQueue.Enqueue(new GeneratedFrame(frameIndex, frame));

                    UpdateProgress(frameIndex + 1, totalFrames);
                }

                generationRunning = false;
                saveFramesThread.Join();
            }
            finally
            {
                Console.CursorVisible = true;
            }

            Console.WriteLine($"\n所有帧已生成到: {Path.GetFullPath(_outputDirectory)}");
            TryOpenOutputDirectory();
        }

        private Bitmap GenerateFrame(int frameIndex, VisualizationBrushes brushes)
        {
            double pixelsPerFrame = _config.PixelsPerSecond / _config.FramesPerSecond;
            int offsetX = (int)(pixelsPerFrame * frameIndex);

            var bitmap = new Bitmap(_config.CanvasWidth, _config.CanvasHeight);
            using var graphics = Graphics.FromImage(bitmap);

            // 绘制背景
            graphics.FillRectangle(brushes.Background, 0, 0, _config.CanvasWidth, _config.CanvasHeight);

            // 绘制音符
            DrawNotes(graphics, offsetX, brushes);

            // 绘制判定线
            DrawGuideline(graphics, brushes);

            return bitmap;
        }

        private void DrawNotes(Graphics graphics, int offsetX, VisualizationBrushes brushes)
        {
            foreach (var note in _processedNotes)
            {
                int noteScreenStartX = (int)(note.PixelStartX - offsetX + _config.GuideLineX);
                int noteScreenEndX = (int)(note.PixelEndX - offsetX + _config.GuideLineX);

                // 跳过超出画布范围的音符
                if (noteScreenStartX > _config.CanvasWidth || noteScreenEndX < 0)
                    continue;

                bool isActive = IsNoteActive(note, offsetX);
                var brush = isActive ? brushes.ActiveNote : brushes.InactiveNote;

                graphics.FillRectangle(brush,
                    noteScreenStartX,
                    note.PixelY,
                    (int)note.PixelLength,
                    _config.NoteHeight);
            }
        }

        private bool IsNoteActive(Note note, int offsetX)
        {
            int noteStartOnScreen = (int)(note.PixelStartX - offsetX + _config.GuideLineX);
            int noteEndOnScreen = (int)(note.PixelEndX - offsetX + _config.GuideLineX);
            return noteStartOnScreen <= _config.GuideLineX && _config.GuideLineX < noteEndOnScreen;
        }

        private void DrawGuideline(Graphics graphics, VisualizationBrushes brushes)
        {
            if (_config.GuidelineWidth > 0)
            {
                int guidelineX = _config.GuideLineX - _config.GuidelineWidth + 1;
                graphics.FillRectangle(brushes.Guideline,
                    guidelineX, 0, _config.GuidelineWidth, _config.CanvasHeight);
            }
        }

        private List<Note> ProcessNotes()
        {
            if (!_midiData.Notes.Any()) return new List<Note>();

            double pixelsPerBeat = _config.PixelsPerSecond * 60 / _midiData.BeatsPerMinute;
            double pixelsPerTick = pixelsPerBeat / _midiData.DeltaTicksPerQuarterNote;

            int minPitch = _midiData.Notes.Min(n => n.Pitch);
            int maxPitch = _midiData.Notes.Max(n => n.Pitch);
            int midPitch = (minPitch + maxPitch) / 2;

            foreach (var note in _midiData.Notes)
            {
                note.PixelStartX = (long)(note.StartTick * pixelsPerTick);
                note.PixelLength = (long)(note.DurationTicks * pixelsPerTick);
                note.PixelEndX = note.PixelStartX + note.PixelLength;
                note.PixelY = _config.CanvasHeight / 2 + (midPitch - note.Pitch) * _config.NoteHeight - _config.NoteHeight / 2;
            }

            return _midiData.Notes;
        }

        private double CalculateTotalDuration()
        {
            var lastNote = _processedNotes.OrderByDescending(n => n.EndTick).First();
            return 60.0 * lastNote.EndTick / (_midiData.BeatsPerMinute * _midiData.DeltaTicksPerQuarterNote);
        }

        private string CreateOutputDirectory()
        {
            string midiDirectory = Path.GetDirectoryName(_config.MidiFilePath) ?? "";
            string midiFileName = Path.GetFileNameWithoutExtension(_config.MidiFilePath);
            string frameDirectory = Path.Combine(midiDirectory, $"{midiFileName}_frames");

            Directory.CreateDirectory(frameDirectory);
            return frameDirectory;
        }

        private static void UpdateProgress(int current, int total)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write($"生成帧 {current}/{total} ({current * 100.0 / total:F1}%)        ");
        }

        private void TryOpenOutputDirectory()
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = Path.GetFullPath(_outputDirectory),
                    UseShellExecute = true,
                    Verb = "open"
                });
                Console.WriteLine("已自动打开输出文件夹");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"无法自动打开文件夹: {ex.Message}");
            }
        }
    }

}
