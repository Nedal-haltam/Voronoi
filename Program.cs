
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Xml;
using static Voronoi.Program;
using Color = Raylib_cs.Color;

namespace Voronoi
{
    internal class Program
    {
        public static readonly Random random = new();
        public static Settings settings;
        public static List<List<int>> Grid = [];
        public static readonly List<Color> COLOR_PALETTE =
        [
            new(){A = 0xFF, R = 0x00, G = 0x7B, B = 0xFF},
            new(){A = 0xFF, R = 0xFF, G = 0x00, B = 0x7F},
            new(){A = 0xFF, R = 0x39, G = 0xFF, B = 0x14},
            new(){A = 0xFF, R = 0xFF, G = 0xD7, B = 0x00},
            new(){A = 0xFF, R = 0xFF, G = 0x66, B = 0x00},
            new(){A = 0xFF, R = 0xDC, G = 0x14, B = 0x3C},
            new(){A = 0xFF, R = 0xFF, G = 0x45, B = 0x00},
            new(){A = 0xFF, R = 0xFF, G = 0xD3, B = 0x00},
            new(){A = 0xFF, R = 0x32, G = 0xCD, B = 0x32},
            new(){A = 0xFF, R = 0x00, G = 0xBF, B = 0xFF},
            new(){A = 0xFF, R = 0x94, G = 0x00, B = 0xD3},
            new(){A = 0xFF, R = 0xFF, G = 0x69, B = 0xB4},
            new(){A = 0xFF, R = 0x30, G = 0xD5, B = 0xC8},
            new(){A = 0xFF, R = 0xFF, G = 0xF4, B = 0x4F},
            new(){A = 0xFF, R = 0x80, G = 0x00, B = 0x80},
            new(){A = 0xFF, R = 0xFF, G = 0x1C, B = 0x00},
            new(){A = 0xFF, R = 0xFF, G = 0x00, B = 0x55},
            new(){A = 0xFF, R = 0xFF, G = 0x6B, B = 0x00},
            new(){A = 0xFF, R = 0xFF, G = 0xE6, B = 0x00},
            new(){A = 0xFF, R = 0x1E, G = 0xFF, B = 0x00},
            new(){A = 0xFF, R = 0x00, G = 0xF0, B = 0xFF},
            new(){A = 0xFF, R = 0x45, G = 0x00, B = 0xFF},
            new(){A = 0xFF, R = 0xB2, G = 0x00, B = 0xFF},
            new(){A = 0xFF, R = 0xFF, G = 0x00, B = 0xE6},
            new(){A = 0xFF, R = 0x00, G = 0xFF, B = 0x99},
            new(){A = 0xFF, R = 0xFF, G = 0x3C, B = 0xAC},
            new(){A = 0xFF, R = 0xFF, G = 0x9F, B = 0x00},
            new(){A = 0xFF, R = 0x00, G = 0xFF, B = 0xA6},
            new(){A = 0xFF, R = 0x8D, G = 0x00, B = 0xFF},
            new(){A = 0xFF, R = 0xFF, G = 0x17, B = 0x44},
            new(){A = 0xFF, R = 0x00, G = 0xC3, B = 0xFF},
            new(){A = 0xFF, R = 0xFF, G = 0xDB, B = 0x00},
        ];


        public static int CurrentWidth;
        public static int CurrentHeight;
        public enum DistanceType
        {
            Euclidean, Manhattan, Mix
        }
        public enum RenderType
        {
            CPU, GPU,
        }
        public enum RenderView
        {
            Color = 1, BlackWhite
        }
        public enum State
        {
            Rendering, WelcomeScreen
        }
        public struct Seed
        {
            public Vector2 Position;
            public Color Color;
            public Seed(Vector2 position, Color color)
            {
                Position = position;
                Color = color;
            }
        }
        public struct Settings
        {
            private List<Seed> seeds = [];
            private DistanceType m_DistanceType;
            public bool Changed;
            private float m_MixFactor = 0.5f;
            public float urandom;
            public Shader shader;
            public RenderView uID;
            public Settings()
            {
                seeds = [];
                DistanceType = DistanceType.Euclidean;
                Changed = true;
                urandom = random.NextSingle();
                m_renderType = RenderType.CPU;
                shader = Raylib.LoadShader(null, "Fshader.fs");
                uID = RenderView.Color;
            }
            public readonly void AddSeed(Seed seed)
            {
                seeds.Add(seed);
                settings.Changed = true;
            }
            public readonly Seed GetSeed(int i)
            {
                return seeds[i];
            }
            public readonly void ClearSeeds()
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
            public float MixFactor
            {
                readonly get => m_MixFactor;
                set
                {
                    m_MixFactor = value;
                    if (m_MixFactor > 1) m_MixFactor = 1.0f;
                    if (m_MixFactor < 0) m_MixFactor = 0.0f;
                    Changed = true;
                }
            }
            public readonly int NumberOfSeeds
            {
                get => seeds.Count;
            }
            private RenderType m_renderType;
            public RenderType RenderType
            {
                get => m_renderType;
                set
                {
                    m_renderType = value;
                    Changed = true;
                }
            }
            public readonly int DefaultNumberOfSeeds => 20;
        }
        public static bool IsSmallerEuclideanDistance(Vector2 Vec1, Vector2 Vec2)
        {
            return (Vec1.X * Vec1.X + Vec1.Y * Vec1.Y) < (Vec2.X * Vec2.X + Vec2.Y * Vec2.Y);
        }
        public static bool IsSmallerManhattanDistance(Vector2 Vec1, Vector2 Vec2)
        {
            return (MathF.Abs(Vec1.X) + MathF.Abs(Vec1.Y)) < (MathF.Abs(Vec2.X) + MathF.Abs(Vec2.Y));
        }
        public static bool IsSmallerMixDistance(Vector2 Vec1, Vector2 Vec2)
        {

            return
                (((1 - settings.MixFactor) * MathF.Sqrt(Vec1.X * Vec1.X + Vec1.Y * Vec1.Y)) + (settings.MixFactor * (MathF.Abs(Vec1.X) + MathF.Abs(Vec1.Y))))
                <
                (((1 - settings.MixFactor) * MathF.Sqrt(Vec2.X * Vec2.X + Vec2.Y * Vec2.Y)) + (settings.MixFactor * (MathF.Abs(Vec2.X) + MathF.Abs(Vec2.Y))));
        }
        public static bool IsSmallerDistance(Vector2 Vec1, Vector2 Vec2, DistanceType type)
        {
            if (type == DistanceType.Euclidean)
                return IsSmallerEuclideanDistance(Vec1, Vec2);
            if (type == DistanceType.Manhattan)
                return IsSmallerManhattanDistance(Vec1, Vec2);
            if (type == DistanceType.Mix)
                return IsSmallerMixDistance(Vec1, Vec2);

            throw new Exception("UNREACHABLE");
        }
        public static float EuclideanDistance(Vector2 Vec1, Vector2 Vec2)
        {
            Vector2 diff = Vec1 - Vec2;
            return MathF.Sqrt(diff.X * diff.X + diff.Y * diff.Y);
        }
        public static float ManhattanDistance(Vector2 Vec1, Vector2 Vec2)
        {
            Vector2 diff = Vec1 - Vec2;
            return MathF.Abs(diff.X) + MathF.Abs(diff.Y);
        }
        public static float Distance(Vector2 Vec1, Vector2 Vec2, DistanceType type)
        {
            if (type == DistanceType.Euclidean)
                return EuclideanDistance(Vec1, Vec2);
            if (type == DistanceType.Manhattan)
                return ManhattanDistance(Vec1, Vec2);

            throw new Exception("UNREACHABLE");
        }
        public static Color GetRandomColor()
        {
            return new() { A = 0xFF, R = (byte)random.Next(255), G = (byte)random.Next(255), B = (byte)random.Next(255) };
        }
        public static float[] GetRandomColorNormalized()
        {
            return [random.NextSingle(), random.NextSingle(), random.NextSingle(), 1.0f];
        }
        public static void ResetSeeds()
        {
            settings.ClearSeeds();
            for (int i = 0; i < settings.DefaultNumberOfSeeds; i++)
            {
                Vector2 p = new() { X = random.Next(Raylib.GetScreenWidth()), Y = random.Next(Raylib.GetScreenHeight()) };
                Color c = GetRandomColor();
                settings.AddSeed(new(p, c));
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
            return new() { A = 0xFF, R = (byte)(r + 150), G = (byte)(g + 150), B = (byte)(b + 150) };
        }
        public static void UpdateSettings()
        {
            if (settings.RenderType == RenderType.GPU)
            {
                if (Raylib.IsKeyPressed(KeyboardKey.R))
                {
                    settings.urandom = random.NextSingle();
                }
            }
            if (settings.RenderType == RenderType.CPU)
            {
                if (Raylib.IsKeyPressed(KeyboardKey.R))
                {
                    ResetSeeds();
                }
                if (Raylib.IsWindowResized())
                {
                    settings.Changed = true;
                }
                if (Raylib.IsKeyDown(KeyboardKey.D))
                {
                    if (Raylib.IsKeyPressed(KeyboardKey.U))
                        settings.DistanceType = DistanceType.Euclidean;
                    if (Raylib.IsKeyPressed(KeyboardKey.M))
                        settings.DistanceType = DistanceType.Manhattan;
                    if (Raylib.IsKeyPressed(KeyboardKey.I))
                        settings.DistanceType = DistanceType.Mix;
                }
                if (Raylib.IsKeyDown(KeyboardKey.F))
                {
                    if (Raylib.IsKeyPressed(KeyboardKey.Left))
                        settings.MixFactor -= 0.1f;
                    if (Raylib.IsKeyPressed(KeyboardKey.Right))
                        settings.MixFactor += 0.1f;

                }
                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    settings.AddSeed(new(Raylib.GetMousePosition(), GetRandomColor()));
                }
            }

        }
        public static void UpdateGrid()
        {
            Grid.Clear();
            for (int i = 0; i < CurrentWidth; i++)
            {
                List<int> temp = [];
                for (int j = 0; j < CurrentHeight; j++)
                {
                    int index = 0;
                    Vector2 pixel = new(i, j);
                    for (int k = 0; k < settings.NumberOfSeeds; k++)
                    {
                        Vector2 PixelandCurrentSmallestDistance = pixel - settings.GetSeed(index).Position;
                        Vector2 PixelandCurrentIndexDistance = pixel - settings.GetSeed(k).Position;
                        bool IfSmaller = IsSmallerDistance(PixelandCurrentIndexDistance, PixelandCurrentSmallestDistance, settings.DistanceType);
                        if (IfSmaller)
                            index = k;
                    }
                    temp.Add(index);
                }
                Grid.Add(temp);
            }
            settings.Changed = false;
        }
        public static void RenderVoronoi_Naive()
        {
            for (int i = 0; i < CurrentWidth; i++)
            {
                for (int j = 0; j < CurrentHeight; j++)
                {
                    Color c = settings.GetSeed(Grid[i][j]).Color;
                    Raylib.DrawPixel(i, j, c);
                }
            }
            for (int i = 0; i < settings.NumberOfSeeds; i++)
            {
                Raylib.DrawCircleV(settings.GetSeed(i).Position, 2.5f, Color.Black);
            }
        }
        public static void SwitchRenderTypeToCPU()
        {
            settings.RenderType = RenderType.CPU;
            Raylib.SetWindowSize(800, 600);
            CurrentWidth = Raylib.GetScreenWidth();
            CurrentHeight = Raylib.GetScreenHeight();
            ResetSeeds();
        }
        public static void SwitchRenderTypeToGPU()
        {
            settings.RenderType = RenderType.GPU;
            Raylib.SetWindowSize(16 * 80, 9 * 80);
            CurrentWidth = Raylib.GetScreenWidth();
            CurrentHeight = Raylib.GetScreenHeight();
        }
        public static void Render()
        {
            UpdateSettings();
            if (Raylib.IsKeyPressed(KeyboardKey.C))
            {
                SwitchRenderTypeToCPU();
            }
            if (Raylib.IsKeyPressed(KeyboardKey.G))
            {
                SwitchRenderTypeToGPU();
            }

            if (settings.RenderType == RenderType.GPU)
            {
                float[] ures = [CurrentWidth, CurrentHeight];
                if (Raylib.IsKeyPressed(KeyboardKey.One))
                    settings.uID = RenderView.Color;
                if (Raylib.IsKeyPressed(KeyboardKey.Two))
                    settings.uID = RenderView.BlackWhite;

                Raylib.SetShaderValue(settings.shader, Raylib.GetShaderLocation(settings.shader, "ures"), ures, ShaderUniformDataType.Vec2);
                Raylib.SetShaderValue(settings.shader, Raylib.GetShaderLocation(settings.shader, "urandom"), settings.urandom, ShaderUniformDataType.Float);
                Raylib.SetShaderValue(settings.shader, Raylib.GetShaderLocation(settings.shader, "uID"), settings.uID, ShaderUniformDataType.Int);

                Raylib.BeginShaderMode(settings.shader);
                Raylib.DrawRectangle(0, 0, CurrentWidth, CurrentHeight, Color.White);
                Raylib.EndShaderMode();
            }
            if (settings.RenderType == RenderType.CPU)
            {
                if (settings.Changed)
                {
                    UpdateGrid();
                }
                RenderVoronoi_Naive();
            }
        }
        public static void DisplayWelcomeScreen()
        {
            int FontSize = 30;
            int x = CurrentWidth / 3;
            string TextWelcomeMessage = "Welcom to Boids Simulation";
            int TextWelcomeMessageWidth = Raylib.MeasureText(TextWelcomeMessage, FontSize);
            Raylib.DrawText(TextWelcomeMessage, CurrentWidth / 2 - TextWelcomeMessageWidth / 2, CurrentHeight / 8, FontSize, Color.White);

            string Textmsg1 = "You can switch between modes of operation:";
            int Textmsg1Width = Raylib.MeasureText(Textmsg1, FontSize);
            Raylib.DrawText(Textmsg1, CurrentWidth / 2 - Textmsg1Width / 2, CurrentHeight / 5, FontSize, Color.White);
            string[] TextParameters =
            [
                "C: To switch to CPU mode",
                "\tR: To get another set of random seeds",
                "\tD: Press and Hold and then",
                "\t\tswitch to different distance modes (Euclidean(U), Manhattan(E), Mix(I))",
                "\tF: Press and Hold and then",
                "\t\tincrease or decrease the value using",
                "\t\tthe right and left arrows respectively for the Mix distance mode",
                "\tNOTE: You can add a seed by clicking the left mouse button",
                "G: To switch to GPU mode",
                "\tR: To get another set of random seeds",
                "\t1: To switch to color view",
                "\t2: To switch to Black/White view",
            ];
            for (int i = 0; i < TextParameters.Length; i++)
            {
                string TempText = TextParameters[i];
                Raylib.DrawText(TempText, x - Textmsg1Width / 2, CurrentHeight / 5 + 100 + i * (int)(1.5f * FontSize), FontSize, Color.White);
            }



            string TextContinue = "Press Enter to continue.";
            int TextContinueWidth = Raylib.MeasureText(TextContinue, FontSize / 2);
            Raylib.DrawText(TextContinue, CurrentWidth / 2 - TextContinueWidth / 2, CurrentHeight - 20, FontSize / 2, Color.White);
        }
        static void Main()
        {
            Raylib.SetConfigFlags(ConfigFlags.AlwaysRunWindow | ConfigFlags.ResizableWindow);
            Raylib.SetTargetFPS(0);
            Raylib.InitWindow(1600, 900, "Voronoi");

            settings = new();
            State state = State.WelcomeScreen;
            while (!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);
                CurrentWidth = Raylib.GetScreenWidth();
                CurrentHeight = Raylib.GetScreenHeight();

                if (Raylib.IsKeyPressed(KeyboardKey.Enter) && state == State.WelcomeScreen)
                {
                    state = State.Rendering;
                    SwitchRenderTypeToGPU();
                }

                if (state == State.WelcomeScreen)
                {
                    DisplayWelcomeScreen();
                }
                else if (state == State.Rendering)
                {
                    Render();
                }

                Raylib.DrawFPS(0, 0);
                Raylib.EndDrawing();
            }

            Raylib.UnloadShader(settings.shader);
            Raylib.CloseWindow();
        }
    }
}
