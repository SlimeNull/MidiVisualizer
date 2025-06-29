using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Midi
{
    public static class InputHelper
    {
        public static double GetDoubleInput(string prompt, double defaultValue)
        {
            Console.Write($"{prompt} (默认值: {defaultValue}): ");
            string input = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
                return defaultValue;

            if (double.TryParse(input, out double result))
                return result;

            Console.WriteLine("输入无效，使用默认值。");
            return defaultValue;
        }

        public static int GetIntInput(string prompt, int defaultValue)
        {
            Console.Write($"{prompt} (默认值: {defaultValue}): ");
            string input = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
                return defaultValue;

            if (int.TryParse(input, out int result))
                return result;

            Console.WriteLine("输入无效，使用默认值。");
            return defaultValue;
        }

        public static string GetStringInput(string prompt, string defaultValue)
        {
            Console.Write($"{prompt} (默认值: {defaultValue}): ");
            string input = Console.ReadLine()?.Trim();
            return string.IsNullOrWhiteSpace(input) ? defaultValue : input;
        }

        public static Color GetColorInput(string prompt, Color defaultColor)
        {
            string defaultColorString = $"{defaultColor.Name} ({defaultColor.R},{defaultColor.G},{defaultColor.B})";
            Console.Write($"{prompt} (默认值: {defaultColorString}): ");
            string input = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
                return defaultColor;

            // 尝试解析十六进制颜色
            if (TryParseHexColor(input, out Color hexColor))
                return hexColor;

            // 尝试解析RGB颜色
            if (TryParseRgbColor(input, out Color rgbColor))
                return rgbColor;

            Console.WriteLine("颜色输入无效，使用默认颜色。");
            return defaultColor;
        }

        private static bool TryParseHexColor(string input, out Color color)
        {
            color = default;
            string hex = input.StartsWith("#") ? input.Substring(1) : input;

            if (hex.Length == 6 && int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int hexValue))
            {
                color = Color.FromArgb(hexValue | (0xFF << 24));
                return true;
            }
            return false;
        }

        private static bool TryParseRgbColor(string input, out Color color)
        {
            color = default;
            string[] rgbParts = input.Split(',');

            if (rgbParts.Length == 3 &&
                int.TryParse(rgbParts[0].Trim(), out int r) && IsValidColorValue(r) &&
                int.TryParse(rgbParts[1].Trim(), out int g) && IsValidColorValue(g) &&
                int.TryParse(rgbParts[2].Trim(), out int b) && IsValidColorValue(b))
            {
                color = Color.FromArgb(r, g, b);
                return true;
            }
            return false;
        }

        private static bool IsValidColorValue(int value) => value >= 0 && value <= 255;
    }

}
