
using Raylib_cs;
using System.Drawing;
using System.Numerics;
using Color = Raylib_cs.Color;

namespace Voronoi
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Raylib.SetConfigFlags(ConfigFlags.AlwaysRunWindow | ConfigFlags.ResizableWindow);
            Raylib.SetTargetFPS(0);
            int w = 800;
            int h = 600;
            Raylib.InitWindow(w, h, "Voronoi");
            Random random = new();
            int N = 10;
            float radius = 5;
            List<Vector2> pts = [];
            for (int i = 0; i < N; i++)
            {
                pts.Add(new() { X = random.Next(w), Y = random.Next(h) });
            }
            while(!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);
                
                if (Raylib.IsKeyPressed(KeyboardKey.R))
                {
                    pts.Clear();
                    for (int i = 0; i < N; i++)
                    {
                        pts.Add(new() { X = random.Next(w), Y = random.Next(h) });
                    }
                }

                for (int i = 0; i < w; i++)
                {
                    for (int j = 0; j < h; j++)
                    {
                        int index = 0;
                        Vector2 pixel = new(i, j);
                        for (int k = 0; k < N; k++)
                        {
                            Vector2 smaller = pixel - pts[index];
                            Vector2 dist = pixel - pts[k];
                            if ((MathF.Abs(dist.X) + MathF.Abs(dist.Y) ) < (MathF.Abs(smaller.X) + MathF.Abs(smaller.Y) ))
                            //if ((dist.X * dist.X + dist.Y * dist.Y) < (smaller.X * smaller.X + smaller.Y * smaller.Y))
                            {
                                index = k;
                            }
                        }
                        ushort x = (ushort)pts[index].X;
                        ushort y = (ushort)pts[index].Y;
                        UInt32 c = (uint)(y << 16 | x);
                        byte r = (byte)((c >>> 0*8) & 0xFF);
                        byte g = (byte)((c >>> 1*8) & 0xFF);
                        byte b = (byte)((c >>> 2*8) & 0xFF);
                        Raylib.DrawPixelV(new() { X = i, Y = j }, new() { A = 0xFF, R = r, G = g, B = b});
                    }
                }


                for (int i = 0; i < N; i++)
                {
                    Raylib.DrawCircleV(pts[i], radius, Color.White);
                }

                Raylib.DrawFPS(0, 0);
                Raylib.EndDrawing();
            }
            Raylib.CloseWindow();
        }
    }
}
