using NAudio.Midi;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
namespace MidiVisualizer
{
    class Program
    {
        private static int DefaultCanvasWidth = 1920;
        private static int DefaultCanvasHeight = 1080;
        private static int DefaultGuideLineX = 100;
        private static int DefaultNoteDisplayHeight = 15;
        private static double DefaultPixelsPerSecond = 192.0;
        private static int DefaultFps = 30;
        private static string DefaultMidiFilePath = "2.mid"; // 默认MIDI文件路径
        private static Color DefaultActiveNoteColor = Color.White; // 默认活跃音符颜色
        private static Color DefaultInactiveNoteColor = Color.FromArgb(100, 100, 100); // 默认非活跃音符颜色
        private static Color DefaultBackgroundColor = Color.Black; // 默认背景色
        private static Color DefaultGuidelineColor = Color.White; // 默认引导线颜色
        private static int DefaultGuidelineWidth = 1; // 默认判定线宽度
        static void parseMidiFile(string filePath, List<Note> notes)
        {
            var midiFile = new MidiFile(filePath);
            for (int track = 0; track < midiFile.Tracks; track++)
            {
                foreach (var midiEvent in midiFile.Events[track])
                {
                    // 检查是否是 NoteOnEvent 并且力度大于 0 (即音符开启)
                    if (midiEvent is NoteOnEvent noteOn && noteOn.Velocity > 0)
                    {
                        //Console.Write($"音高: {noteOn.NoteNumber} ({noteOn.NoteName}) ");
                        //Console.Write($"力度: {noteOn.Velocity} ");
                        //Console.Write($"通道: {noteOn.Channel} ");
                        //Console.Write($"开始: {noteOn.AbsoluteTime} ticks ");


                        // 获取对应的音符关闭事件
                        if (noteOn.OffEvent != null)
                        {
                            notes.Add(new Note(noteOn.AbsoluteTime, noteOn.OffEvent.AbsoluteTime, noteOn.NoteNumber, noteOn.NoteName));
                        }
                        else
                        {
                            // 处理没有匹配的 OffEvent 的情况 (例如，MIDI 文件损坏或音符未正确关闭)
                            Console.WriteLine("结束:未找到对应的OffEvent");
                        }
                    }
                }
            }
        }
        static double getBpmFromMidiFile(string filePath)
        {
            var midiFile = new MidiFile(filePath);
            double bpm = 120.000; // 默认值
            for (int track = 0; track < midiFile.Tracks; track++)
            {
                foreach (var midiEvent in midiFile.Events[track])
                {
                    if (midiEvent is TempoEvent tempoEvent)
                    {
                        return Math.Round(60000000.0 / tempoEvent.MicrosecondsPerQuarterNote, 3);

                    }
                }
            }
            return bpm;
        }
        static double GetDoubleInput(string prompt, double defaultValue)
        {
            Console.Write($"{prompt} (默认值:{defaultValue}): ");
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                return defaultValue;
            }
            if (double.TryParse(input, out double result))
            {
                return result;
            }
            Console.WriteLine("输入无效,将使用默认值。");
            return defaultValue;
        }
        static int GetIntInput(string prompt, int defaultValue)
        {
            Console.Write($"{prompt} (默认值:{defaultValue}): ");
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                return defaultValue;
            }
            if (int.TryParse(input, out int result))
            {
                return result;
            }
            Console.WriteLine("输入无效,将使用默认值");
            return defaultValue;
        }
        static string GetStringInput(string prompt, string defaultValue)
        {
            Console.Write($"{prompt} (默认值:{defaultValue}):");
            string? input = Console.ReadLine(); // 使用可空类型 string?
            return string.IsNullOrWhiteSpace(input) ? defaultValue : input;
        }
        static Color GetColorInput(string prompt, Color defaultColor)
        {
            // 将默认颜色转换为可读的字符串形式，方便用户参考
            string defaultColorString = $"{defaultColor.Name} ({defaultColor.R},{defaultColor.G},{defaultColor.B})";
            Console.Write($"{prompt} (默认值:{defaultColorString}): ");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                return defaultColor;
            }
            // 尝试按十六进制解析 (如 "#FF0000" 或 "FF0000")
            string hex = input.Trim();
            if (hex.StartsWith("#"))
            {
                hex = hex.Substring(1);
            }
            if (hex.Length == 6 && int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int hexValue))
            {
                try
                {
                    return Color.FromArgb(hexValue | (0xFF << 24)); // 确保A通道为255 (不透明)
                }
                catch { /* 解析失败 */ }
            }
            string[] rgbParts = input.Split(',');
            if (rgbParts.Length == 3 &&
                int.TryParse(rgbParts[0].Trim(), out int r) && r >= 0 && r <= 255 &&
                int.TryParse(rgbParts[1].Trim(), out int g) && g >= 0 && g <= 255 &&
                int.TryParse(rgbParts[2].Trim(), out int b) && b >= 0 && b <= 255)
            {
                return Color.FromArgb(r, g, b);
            }
            Console.WriteLine("颜色输入无效(请使用十六进制或R,G,B格式),将使用默认颜色。");
            return defaultColor;
        }
        [SupportedOSPlatform("windows6.1")]
        static void Main(string[] args)
        {
            Console.WriteLine
                (
                "===Midi可视化V1.0.0===\n" +
                "欢迎使用本工具\n" +
                "请按 Enter 键接受默认值,或输入新值\n" +
                "特别提醒:请注意音符宽度和画布纵向大小的关系,可能出现纵向无法容纳全部音符的情况\n" +
                "目前不支持变速mid\n" +
                "全部默认请长按 Enter"
                );


            // --- 获取用户输入 ---
            string filePath = GetStringInput("MIDI 文件路径", DefaultMidiFilePath);
            filePath = filePath.Trim('\"');
            int canvasWidth = GetIntInput("画布横向大小(像素)", DefaultCanvasWidth);
            int canvasHeight = GetIntInput("画布纵向大小(像素)", DefaultCanvasHeight);
            int lineX = GetIntInput("判定线X坐标(像素)", DefaultGuideLineX);
            int noteHeight = GetIntInput("音符宽度(像素)", DefaultNoteDisplayHeight);
            double pixelsPerSecond = GetDoubleInput("每秒滚动像素", DefaultPixelsPerSecond);
            int fps = GetIntInput("视频帧率(FPS)", DefaultFps);
            Color activeNoteColor = GetColorInput("活跃音符颜色", DefaultActiveNoteColor);
            Color inactiveNoteColor = GetColorInput("非活跃音符颜色", DefaultInactiveNoteColor);
            Color backgroundColor = GetColorInput("背景颜色", DefaultBackgroundColor);
            Color guidelineColor = GetColorInput("判定线颜色", DefaultGuidelineColor);
            int guideLineWidth = GetIntInput("判定线宽度(像素,0表示不渲染,仅视觉)", DefaultGuidelineWidth);


            Console.WriteLine("\n===参数确认===");
            Console.WriteLine($"MIDI 文件:{filePath}");
            Console.WriteLine($"画布尺寸:{canvasWidth}x{canvasHeight}");
            Console.WriteLine($"判定线X:{lineX}");
            Console.WriteLine($"判定线宽度: {guideLineWidth} ({(guideLineWidth > 0 ? "将渲染" : "不渲染")})"); // 显示判定线宽度和是否渲染
            Console.WriteLine($"音符宽度:{noteHeight}");
            Console.WriteLine($"每秒像素:{pixelsPerSecond}");
            Console.WriteLine($"帧率:{fps}");
            Console.WriteLine($"活跃音符颜色: {activeNoteColor.Name} ({activeNoteColor.R},{activeNoteColor.G},{activeNoteColor.B})");
            Console.WriteLine($"非活跃音符颜色: {inactiveNoteColor.Name} ({inactiveNoteColor.R},{inactiveNoteColor.G},{inactiveNoteColor.B})");
            Console.WriteLine($"背景颜色: {backgroundColor.Name} ({backgroundColor.R},{backgroundColor.G},{backgroundColor.B})");
            Console.WriteLine($"判定线颜色: {guidelineColor.Name} ({guidelineColor.R},{guidelineColor.G},{guidelineColor.B})");
            Console.WriteLine("=============\n");

            List<Note> notes = new List<Note>();


            var midiFile = new MidiFile(filePath);
            parseMidiFile(filePath, notes);
            double bpm = 120.000;
            bpm = getBpmFromMidiFile(filePath);
            Console.WriteLine($"BPM:{bpm}");

            double totalDuration = 60 * notes[notes.Count - 1].End / (bpm * midiFile.DeltaTicksPerQuarterNote);



            Console.WriteLine($"每秒像素:{pixelsPerSecond}");
            double pixelsPerFrame = pixelsPerSecond / fps;
            double pixelsPerBeat = pixelsPerSecond * 60 / bpm;
            double pixelsPerTick = pixelsPerBeat / midiFile.DeltaTicksPerQuarterNote;
            Console.WriteLine($"每帧像素:{pixelsPerFrame}");


            using var activeBrush = new SolidBrush(activeNoteColor);
            using var inactiveBrush = new SolidBrush(inactiveNoteColor);
            using var backgroundBrush = new SolidBrush(backgroundColor);
            using var guidelineBrush = new SolidBrush(guidelineColor);

            int offestX = 0;
            int totalFrames = (int)Math.Ceiling((totalDuration * fps));
            string framePath = "";
            int noteScreenStartX = 0;
            int noteScreenEndX = 0;
            //一堆乱七八糟初始化
            if (!notes.Any())
            {
                Console.WriteLine("音符列表为空");
                return;
            }
            string midiFileDirectory = Path.GetDirectoryName(filePath); // 获取 MIDI 文件所在的目录
            string midiFileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath); // 获取不带扩展名的文件名
            string frameDir = Path.Combine(midiFileDirectory, midiFileNameWithoutExtension + "_frames");

            Directory.CreateDirectory(frameDir);
            int minPitch = notes.Min(note => note.Pitch);
            int maxPitch = notes.Max(note => note.Pitch);
            int mid = (minPitch + maxPitch) / 2;
            for (int i = 0; i < notes.Count; i++)
            {
                notes[i].PixelStartX = (long)(notes[i].Start * pixelsPerTick);
                notes[i].PixelLength = (long)((notes[i].End - notes[i].Start) * pixelsPerTick);
                notes[i].PixelEndX = (notes[i].PixelStartX + notes[i].PixelLength);
                notes[i].PixelY = canvasHeight / 2 + (mid - notes[i].Pitch) * noteHeight - noteHeight / 2;
            }


            Console.CursorVisible = false;
            int lineEndX = lineX - guideLineWidth + 1;
            Action<Graphics, int, int> drawGuideline = (g, x, h) => { };
            if (guideLineWidth > 0)
            {
                drawGuideline = (g, x, h) => g.FillRectangle(guidelineBrush, x, 0, guideLineWidth, h);
            }
            for (int frame = 0; frame <= (int)Math.Ceiling(notes[notes.Count - 1].End * 60 * fps / (bpm * midiFile.DeltaTicksPerQuarterNote)); frame++)
            {
                offestX = (int)(pixelsPerFrame * frame);
                using var bitmap = new Bitmap(canvasWidth, canvasHeight);
                using var graphics = Graphics.FromImage(bitmap);
                graphics.FillRectangle(backgroundBrush, 0, 0, canvasWidth, canvasHeight);

                foreach (var note in notes)
                {
                    noteScreenStartX = (int)(note.PixelStartX - offestX + lineX);
                    noteScreenEndX = (int)(note.PixelEndX - offestX + lineX);
                    if (noteScreenStartX > canvasWidth || noteScreenEndX < 0)
                    {
                        continue; // 如果音符超出画布范围，则跳过
                    }
                    graphics.FillRectangle
                        (
                        (note.PixelStartX - offestX + lineX <= lineX) && (lineX < note.PixelEndX - offestX + lineX) ? activeBrush : inactiveBrush,
                        note.PixelStartX - offestX + lineX,
                        note.PixelY,
                        note.PixelLength,
                        noteHeight
                        );
                }
                drawGuideline(graphics, lineEndX, canvasHeight);
                framePath = Path.Combine(frameDir, $"{frame:0000}.png");
                bitmap.Save(framePath, ImageFormat.Png);
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($"生成帧 {frame + 1}/{totalFrames}        ");
            }
            Console.CursorVisible = true;
            Console.WriteLine("\n所有帧已生成");
            Console.WriteLine($"保存到: {Path.GetFullPath(frameDir)}");
            try
            {
                // 尝试打开生成的视频帧文件夹
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = Path.GetFullPath(frameDir),
                    UseShellExecute = true,
                    Verb = "open" // 明确指定打开操作
                });
                Console.WriteLine($"已自动打开文件夹: {Path.GetFullPath(frameDir)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"无法自动打开文件夹。错误: {ex.Message}");
                Console.WriteLine("请手动前往以下路径查看帧图片:");
                Console.WriteLine(Path.GetFullPath(frameDir));
            }

            Console.WriteLine("\n按任意键退出程序...");
            Console.ReadKey();
        }




        // 下面的代码是为了输出音符信息，已被注释掉
        //foreach (var note in notes)
        //{
        //    // 输出音符信息
        //    Console.WriteLine($"音符: {note.Name} ({note.Pitch}) 开始: {note.Start} ticks 结束: {note.End} ticks 持续时间: {note.End - note.Start} ticks");
        //}
    }
    class Note
    {
        public long Start { get; }
        public long End { get; }
        public int Pitch { get; }
        public string Name { get; }
        public long PixelStartX { get; set; }
        public long PixelEndX { get; set; }
        public long PixelLength { get; set; }
        public int PixelY { get; set; }
        public Note(long start, long end, int pitch, string name)
        {
            Start = start;
            End = end;
            Pitch = pitch;
            Name = name;
        }
    }
}
