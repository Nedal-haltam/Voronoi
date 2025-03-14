
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using static Voronoi.Program;
using Color = Raylib_cs.Color;

namespace Voronoi
{
    internal class Program
    {
        public static readonly Random random = new();
        public static Settings settings;
        public static List<List<Color>> Grid = [];
        
        public static int CurrentWidth;
        public static int CurrentHeight;
        public enum DistanceType
        {
            Euclidean, Manhattan
        }
        public struct Settings
        {
            private List<Vector2> seeds = [];
            private DistanceType m_DistanceType;
            public bool Changed;
            public Settings()
            {
                seeds = [];
                DistanceType = DistanceType.Euclidean;
                Changed = true;
            }
            public void AddSeed(Vector2 seed)
            {
                seeds.Add(seed);
                settings.Changed = true;
            }
            public Vector2 GetSeed(int i)
            {
                return seeds[i];
            }
            public void ClearSeeds()
            {
                seeds.Clear();
            }
            public DistanceType DistanceType
            {
                readonly get => m_DistanceType;
                set
                {
                    m_DistanceType = value;
                    Changed = true;
                }
            }
            public readonly int NumberOfSeeds
            {
                get => seeds.Count;
            }
            public readonly int DefualtNumberOfSeeds => 20;
        }
        public static bool EuclideanDistance(Vector2 dist, Vector2 smaller)
        {
            return (dist.X * dist.X + dist.Y * dist.Y) < (smaller.X * smaller.X + smaller.Y * smaller.Y);
        }
        public static bool ManhattanDistance(Vector2 dist, Vector2 smaller)
        {
            return (MathF.Abs(dist.X) + MathF.Abs(dist.Y)) < (MathF.Abs(smaller.X) + MathF.Abs(smaller.Y));
        }
        public static bool Distance(Vector2 dist, Vector2 smaller, DistanceType type)
        {
            if (type == DistanceType.Euclidean)
                return EuclideanDistance(dist, smaller);
            if (type == DistanceType.Manhattan)
                return ManhattanDistance(dist, smaller);

            throw new Exception("UNREACHABLE");
        }
        public static void ResetSeeds()
        {
            settings.ClearSeeds();
            for (int i = 0; i < settings.DefualtNumberOfSeeds; i++)
            {
                settings.AddSeed(new() { X = random.Next(Raylib.GetScreenWidth()), Y = random.Next(Raylib.GetScreenHeight()) });
            }
            settings.Changed = true;
        }
        public static Color PointToColor(Vector2 p)
        {
            ushort x = (ushort)p.X;
            ushort y = (ushort)p.Y;
            UInt32 c = (uint)(y << 16 | x);
            byte r = (byte)((c >>> 0 * 8) & 0xFF);
            byte g = (byte)((c >>> 1 * 8) & 0xFF);
            byte b = (byte)((c >>> 2 * 8) & 0xFF);
            return new() { A = 0xFF, R = r, G = g, B = b };
        }
        public static void RenderVoronoi()
        {
            for (int i = 0; i < CurrentWidth; i++)
            {
                for (int j = 0; j < CurrentHeight; j++)
                {
                    Raylib.DrawPixel(i, j, Grid[i][j]);
                }
            }
            for (int i = 0; i < settings.NumberOfSeeds; i++)
            {
                Raylib.DrawCircleV(settings.GetSeed(i), 5, Color.White);
            }
        }
        public static void UpdateGrid()
        {
            Grid.Clear();
            for (int i = 0; i < CurrentWidth; i++)
            {
                List<Color> temp = [];
                for (int j = 0; j < CurrentHeight; j++)
                {
                    int index = 0;
                    Vector2 pixel = new(i, j);
                    for (int k = 0; k < settings.NumberOfSeeds; k++)
                    {
                        Vector2 CurrentSmallerDistance = pixel - settings.GetSeed(index);
                        Vector2 CurrentDistance = pixel - settings.GetSeed(k);
                        bool IfSmaller = Distance(CurrentDistance, CurrentSmallerDistance, settings.DistanceType);
                        if (IfSmaller)
                            index = k;
                    }
                    temp.Add(PointToColor(settings.GetSeed(index)));
                }
                Grid.Add(temp);
            }
            settings.Changed = false;
        }
        public static void UpdateSettings()
        {
            if (Raylib.IsWindowResized())
            {
                settings.Changed = true;
            }
            if (Raylib.IsKeyPressed(KeyboardKey.R))
            {
                ResetSeeds();
            }
            if (Raylib.IsKeyDown(KeyboardKey.D))
            {
                if (Raylib.IsKeyPressed(KeyboardKey.U))
                    settings.DistanceType = DistanceType.Euclidean;
                if (Raylib.IsKeyPressed(KeyboardKey.M))
                    settings.DistanceType = DistanceType.Manhattan;
            }
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                settings.AddSeed(Raylib.GetMousePosition());
            }
        }
        static void Main()
        {
            Raylib.SetConfigFlags(ConfigFlags.AlwaysRunWindow | ConfigFlags.ResizableWindow);
            Raylib.SetTargetFPS(0);
            Raylib.InitWindow(800, 600, "Voronoi");

            settings = new();
            ResetSeeds();
            
            while(!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);
                CurrentWidth = Raylib.GetScreenWidth();
                CurrentHeight = Raylib.GetScreenHeight();

                UpdateSettings();
                if (settings.Changed)
                {
                    UpdateGrid();
                }
                RenderVoronoi();

                Raylib.DrawFPS(0, 0);
                Raylib.EndDrawing();
            }
            Raylib.CloseWindow();
        }
    }
}
