using Midi;
using NAudio.Midi;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;

namespace Midi
{
    class Program
    {
        [SupportedOSPlatform("windows6.1")]
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("=== MIDI 可视化工具 V1.0.0 ===");
                Console.WriteLine("欢迎使用本工具");
                Console.WriteLine("按 Enter 键接受默认值，或输入新值");
                Console.WriteLine("注意：请合理设置音符高度和画布大小的关系");
                Console.WriteLine("目前不支持变速 MIDI 文件");
                Console.WriteLine("==============================\n");

                var config = GetUserConfiguration();
                DisplayConfiguration(config);

                var visualizer = new MidiVisualizer(config);
                visualizer.GenerateFrames();

                Console.WriteLine("\n按任意键退出程序...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"程序执行出错: {ex.Message}");
                Console.WriteLine("按任意键退出...");
                Console.ReadKey();
            }
        }

        private static VisualizationConfig GetUserConfiguration()
        {
            var config = new VisualizationConfig();

            config.MidiFilePath = InputHelper.GetStringInput("MIDI 文件路径", config.MidiFilePath).Trim('\"');
            config.CanvasWidth = InputHelper.GetIntInput("画布宽度 (像素)", config.CanvasWidth);
            config.CanvasHeight = InputHelper.GetIntInput("画布高度 (像素)", config.CanvasHeight);
            config.GuideLineX = InputHelper.GetIntInput("判定线 X 坐标 (像素)", config.GuideLineX);
            config.NoteHeight = InputHelper.GetIntInput("音符高度 (像素)", config.NoteHeight);
            config.PixelsPerSecond = InputHelper.GetDoubleInput("每秒滚动像素", config.PixelsPerSecond);
            config.FramesPerSecond = InputHelper.GetIntInput("视频帧率 (FPS)", config.FramesPerSecond);
            config.ActiveNoteColor = InputHelper.GetColorInput("活跃音符颜色", config.ActiveNoteColor);
            config.InactiveNoteColor = InputHelper.GetColorInput("非活跃音符颜色", config.InactiveNoteColor);
            config.BackgroundColor = InputHelper.GetColorInput("背景颜色", config.BackgroundColor);
            config.GuidelineColor = InputHelper.GetColorInput("判定线颜色", config.GuidelineColor);
            config.GuidelineWidth = InputHelper.GetIntInput("判定线宽度 (像素，0=不显示)", config.GuidelineWidth);

            return config;
        }

        private static void DisplayConfiguration(VisualizationConfig config)
        {
            Console.WriteLine("\n=== 参数确认 ===");
            Console.WriteLine($"MIDI 文件: {config.MidiFilePath}");
            Console.WriteLine($"画布尺寸: {config.CanvasWidth}×{config.CanvasHeight}");
            Console.WriteLine($"判定线位置: X={config.GuideLineX}, 宽度={config.GuidelineWidth}");
            Console.WriteLine($"音符高度: {config.NoteHeight}");
            Console.WriteLine($"滚动速度: {config.PixelsPerSecond} 像素/秒");
            Console.WriteLine($"帧率: {config.FramesPerSecond} FPS");
            Console.WriteLine($"颜色配置:");
            Console.WriteLine($"  活跃音符: {FormatColor(config.ActiveNoteColor)}");
            Console.WriteLine($"  非活跃音符: {FormatColor(config.InactiveNoteColor)}");
            Console.WriteLine($"  背景: {FormatColor(config.BackgroundColor)}");
            Console.WriteLine($"  判定线: {FormatColor(config.GuidelineColor)}");
            Console.WriteLine("===============\n");
        }

        private static string FormatColor(Color color) =>
            $"{color.Name} ({color.R},{color.G},{color.B})";
    }

}
