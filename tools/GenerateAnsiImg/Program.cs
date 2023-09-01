using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

Console.WriteLine(GetChars(new Bitmap(Console.ReadLine())));

string GetChars(Bitmap bmp)

    StringBuilder sb = new();
    for (int h = 0; h < bmp.Height; h += 1)
    {
        for (int w = 0; w < bmp.Width; w += 1)
        {
            Color color = bmp.GetPixel(w, h);
            if (color.A == 0)
            {
                sb.Append($"\x1b[0m  ");
            }
            else
            {
                sb.Append($"\x1b[48;2;{color.R};{color.G};{color.B}m  ");
            }
        }
        sb.Append($"\x1b[0m");
        sb.AppendLine();
    }
    bmp.Dispose();
    return sb.ToString();
