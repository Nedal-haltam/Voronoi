
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
        public class Pixel(float DistanceToindex, int index)
        {
            public float DistanceToIndex = DistanceToindex;
            public int index = index;
        }
        public static readonly Random random = new();
        public static Settings settings;
        public static List<List<Pixel>> Grid = [];
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
        public class Seed
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public Color Color;
            public Seed(Vector2 position, Color color)
            {
                Position = position;
                Color = color;
                float angle = 2 * MathF.PI * random.NextSingle();
                float mag = 100 + (400) * random.NextSingle();
                Velocity = new(mag * MathF.Cos(angle), mag * MathF.Sin(angle));
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
            public RenderTexture2D texture;
            public Settings()
            {
                seeds = [];
                DistanceType = DistanceType.Euclidean;
                Changed = true;
                urandom = random.NextSingle();
                m_renderType = RenderType.CPU;
                shader = Raylib.LoadShader(null, "Fshader.fs");
                uID = RenderView.Color;
                texture = Raylib.LoadRenderTexture(CurrentWidth, CurrentHeight);
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
            public void SetSeedPosition(int i, Vector2 NewPosition)
            {
                seeds[i].Position = NewPosition;
            }
            public void SetSeedVelocity(int i, Vector2 NewVelocity)
            {
                seeds[i].Velocity = NewVelocity;
            }
            public void SetSeed(int i, Seed seed)
            {
                seeds[i] = seed;
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
                    CurrentWidth = Raylib.GetScreenWidth();
                    CurrentHeight = Raylib.GetScreenHeight();
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
        public static void UpdateTexture()
        {
            Raylib.UnloadRenderTexture(settings.texture);
            settings.texture = Raylib.LoadRenderTexture(CurrentWidth, CurrentHeight);
            Raylib.BeginTextureMode(settings.texture);
            Raylib.ClearBackground(Color.White);
            for (int i = 0; i < Grid.Count; i++)
            {
                for (int j = 0; j < Grid[i].Count; j++)
                {
                    Color c = settings.GetSeed(Grid[i][j].index).Color;
                    Raylib.DrawPixel(i, j, c);
                }
            }
            Raylib.EndTextureMode();
        }
        public static void UpdateGridPrallelApproach()
        {
            for (int i = 0; i < CurrentWidth; i++)
            {
                List<Pixel> temp = [];
                for (int j = 0; j < CurrentHeight; j++)
                {
                    temp.Add(new(Distance(new(i, j), settings.GetSeed(0).Position, settings.DistanceType), 0));
                }
                Grid.Add(temp);
            }
            for (int k = 0; k < settings.NumberOfSeeds; k++)
            {
                Parallel.For(0, CurrentWidth, i =>
                {
                    Parallel.For(0, CurrentHeight, j =>
                    {
                        Vector2 PixelPosition = new(i, j);
                        Vector2 PixelandCurrentSmallestDistance = PixelPosition - settings.GetSeed(Grid[i][j].index).Position;
                        Vector2 PixelandCurrentIndexDistance = PixelPosition - settings.GetSeed(k).Position;
                        bool IfSmaller = IsSmallerDistance(PixelandCurrentIndexDistance, PixelandCurrentSmallestDistance, settings.DistanceType);
                        if (IfSmaller)
                        {
                            Grid[i][j].DistanceToIndex = Distance(new(i, j), settings.GetSeed(k).Position, settings.DistanceType);
                            Grid[i][j].index = k;
                        }
                    });
                });
            }
        }
        public static void UpdateGridClassicalApproach()
        {
            for (int i = 0; i < CurrentWidth; i++)
            {
                List<Pixel> temp = [];
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
                    temp.Add(new(Distance(pixel, settings.GetSeed(index).Position, settings.DistanceType), index));
                }
                Grid.Add(temp);
            }
        }
        public static void UpdateGrid()
        {
            Grid.Clear();
            UpdateGridPrallelApproach();

            //UpdateGridClassicalApproach();

            UpdateTexture();
            settings.Changed = false;
        }
        public static void RenderVoronoi_Naive()
        {
            Raylib.DrawTextureRec(settings.texture.Texture, new() { X = 0, Y = 0, Width = CurrentWidth, Height = -CurrentHeight }, new() { X = 0, Y = 0 }, Color.White);
            for (int i = 0; i < settings.NumberOfSeeds; i++)
            {
                Seed CurrentSeed = settings.GetSeed(i);
                Raylib.DrawCircleV(CurrentSeed.Position, 2.5f, Color.Black);

                Vector2 newpos = CurrentSeed.Position + (Raylib.GetFrameTime() * CurrentSeed.Velocity);
                if (0 <= newpos.X && newpos.X <= CurrentWidth)
                {
                    CurrentSeed.Position.X = newpos.X;
                }
                else
                {
                    CurrentSeed.Velocity.X *= -1;
                }
                if (0 <= newpos.Y && newpos.Y <= CurrentWidth)
                {
                    CurrentSeed.Position.Y = newpos.Y;
                }
                else
                {
                    CurrentSeed.Velocity.Y *= -1;
                }
                settings.SetSeed(i, CurrentSeed);
                settings.Changed = true;
            }
        }
        public static void SwitchRenderTypeToCPU()
        {
            settings.RenderType = RenderType.CPU;
            ResetSeeds();
        }
        public static void SwitchRenderTypeToGPU()
        {
            settings.RenderType = RenderType.GPU;
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
            string TextWelcomeMessage = "Welcom to Voronoi Diagram Rendering";
            int TextWelcomeMessageWidth = Raylib.MeasureText(TextWelcomeMessage, FontSize);
            Raylib.DrawText(TextWelcomeMessage, CurrentWidth / 2 - TextWelcomeMessageWidth / 2, CurrentHeight / 13, FontSize, Color.White);

            string Textmsg1 = "You can switch between modes of operation:";
            int Textmsg1Width = Raylib.MeasureText(Textmsg1, FontSize);
            Raylib.DrawText(Textmsg1, CurrentWidth / 2 - Textmsg1Width / 2, CurrentHeight / 7, FontSize, Color.White);
            string[] TextParameters =
            [
                "R: Reset, D: Distance Mode, F: Factor, C: CPU, G: GPU",
                "C: To switch to CPU mode",
                "\tR: To get another set of random seeds",
                "\tD: Press and Hold and then",
                "\t\tswitch to different distance modes by pressing (Euclidean(U), Manhattan(M), Mix(I))",
                "\tF: Press and Hold and then",
                "\t\tincrease or decrease the value using",
                "\t\tthe right and left arrows respectively for the Mix distance mode",
                "\tNOTE: You can add a seed by clicking the left mouse button",
                "G: To switch to GPU mode",
                "\tR: To get another set of random seeds",
                "\t1: To switch to color view",
                "\t2: To switch to Black/White view",
                "H: return to Welcome Screen",
            ];
            for (int i = 0; i < TextParameters.Length; i++)
            {
                string TempText = TextParameters[i];
                Raylib.DrawText(TempText, x - Textmsg1Width / 2, CurrentHeight / 7 + 75 + i * (int)(1.5f * FontSize), FontSize, Color.White);
            }
            string TextContinue = "Press Enter to continue.";
            int TextContinueWidth = Raylib.MeasureText(TextContinue, FontSize / 2);
            Raylib.DrawText(TextContinue, CurrentWidth / 2 - TextContinueWidth / 2, CurrentHeight - 20, FontSize / 2, Color.White);
        }
        static void Main()
        {
            Raylib.SetConfigFlags(ConfigFlags.AlwaysRunWindow | ConfigFlags.ResizableWindow);
            Raylib.SetTargetFPS(0);
            //Raylib.InitWindow(1600, 900, "Voronoi");
            Raylib.InitWindow(800, 600, "Voronoi");

            settings = new();
            State state = State.WelcomeScreen;
            while (!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);
                CurrentWidth = Raylib.GetScreenWidth();
                CurrentHeight = Raylib.GetScreenHeight();

                if (state == State.WelcomeScreen && Raylib.IsKeyPressed(KeyboardKey.Enter))
                {
                    state = State.Rendering;
                    SwitchRenderTypeToGPU();
                }
                if (state == State.Rendering && Raylib.IsKeyPressed(KeyboardKey.H))
                {
                    state = State.WelcomeScreen;
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
            Raylib.UnloadRenderTexture(settings.texture);
            Raylib.UnloadShader(settings.shader);
            Raylib.CloseWindow();
        }
    }
}
