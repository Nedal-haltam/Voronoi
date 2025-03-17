
using Raylib_cs;
//using static Raylib_cs.Rlgl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Net.Mail;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Xml;
using static Voronoi.Program;
using Color = Raylib_cs.Color;
using Rectangle = Raylib_cs.Rectangle;

namespace Voronoi
{
    internal class Program
    {
        public class Pixel(Vector2 Position, float DistanceToSeed, int SeedIndex)
        {
            public Vector2 m_Position = Position;
            public float m_DistanceToIndex = DistanceToSeed;
            public int m_index = SeedIndex;
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
            Euclidean, Manhattan, Mix, Minkowski
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
            public Vector2 m_Position;
            public Vector2 m_Velocity;
            public Color m_Color;
            public Seed(Vector2 position, Color color)
            {
                m_Position = position;
                m_Color = color;
                float angle = 2 * MathF.PI * random.NextSingle();
                float mag = 100 + (400) * random.NextSingle();
                m_Velocity = new(mag * MathF.Cos(angle), mag * MathF.Sin(angle));
            }
        }
        public struct Settings
        {
            public readonly List<Seed> m_Seeds = [];
            private DistanceType m_DistanceType;
            public bool m_Changed;
            private float m_MixFactor = 0.5f;
            public float m_urandom;
            public Shader m_shader;
            public RenderView m_uID;
            public RenderTexture2D m_texture;
            public Settings()
            {
                m_Seeds = [];
                m_DistanceType = DistanceType.Euclidean;
                m_Changed = true;
                m_urandom = random.NextSingle();
                m_renderType = RenderType.CPU;
                m_shader = Raylib.LoadShader(null, "Fshader.fs");
                m_uID = RenderView.Color;
                m_texture = Raylib.LoadRenderTexture(CurrentWidth, CurrentHeight);
            }
            public readonly void AddSeed(Seed seed)
            {
                m_Seeds.Add(seed);
                settings.m_Changed = true;
            }
            public readonly Seed GetSeed(int i)
            {
                return m_Seeds[i];
            }
            public readonly void ClearSeeds()
            {
                m_Seeds.Clear();
            }
            public readonly void SetSeedPosition(int i, Vector2 NewPosition)
            {
                m_Seeds[i].m_Position = NewPosition;
            }
            public readonly void SetSeedVelocity(int i, Vector2 NewVelocity)
            {
                m_Seeds[i].m_Velocity = NewVelocity;
            }
            public readonly void SetSeed(int i, Seed seed)
            {
                m_Seeds[i] = seed;
            }
            public DistanceType DistanceType
            {
                readonly get => m_DistanceType;
                set
                {
                    m_DistanceType = value;
                    m_Changed = true;
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
                    m_Changed = true;
                }
            }
            public readonly int NumberOfSeeds
            {
                get => m_Seeds.Count;
            }
            private RenderType m_renderType;
            public RenderType RenderType
            {
                readonly get => m_renderType;
                set
                {
                    m_renderType = value;
                    m_Changed = true;
                }
            }
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1822 // Mark members as static
            public readonly int DefaultNumberOfSeeds => 3;
#pragma warning restore IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning restore CA1822 // Mark members as static
        }
#pragma warning restore IDE0079 // Remove unnecessary suppression
        public static bool IsSmallerEuclideanDistance(Vector2 Vec1, Vector2 Vec2)
        {
            return Vec1.LengthSquared() < Vec2.LengthSquared();
        }
        public static bool IsSmallerMinkowskiDistance(Vector2 Vec1, Vector2 Vec2)
        {
            float p = 3.0f;
            return MathF.Pow((MathF.Pow(MathF.Abs(Vec1.X), p) + MathF.Pow(MathF.Abs(Vec1.Y), p)), 1.0f / p) < MathF.Pow((MathF.Pow(MathF.Abs(Vec2.X), p) + MathF.Pow(MathF.Abs(Vec2.Y), p)), 1.0f / p);
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
            if (type == DistanceType.Minkowski)
                return IsSmallerMinkowskiDistance(Vec1, Vec2);

            throw new Exception("UNREACHABLE");
        }
        public static float EuclideanDistance(Vector2 Vec1, Vector2 Vec2)
        {
            float p = 2.0f;
            Vector2 diff = Vec1 - Vec2;
            return MathF.Pow((MathF.Pow(MathF.Abs(diff.X), p) + MathF.Pow(MathF.Abs(diff.Y), p)), 1.0f / p);
        }
        public static float MinkowskiDistance(Vector2 Vec1, Vector2 Vec2)
        {
            float p = 2.0f;
            Vector2 diff = Vec1 - Vec2;
            return MathF.Pow(MathF.Pow(MathF.Abs(diff.X), p) + MathF.Pow(MathF.Abs(diff.Y), p), 1.0f / p);
        }
        public static float ManhattanDistance(Vector2 Vec1, Vector2 Vec2)
        {
            Vector2 diff = Vec1 - Vec2;
            return MathF.Abs(diff.X) + MathF.Abs(diff.Y);
        }
        public static float MixDistance(Vector2 Vec1, Vector2 Vec2)
        {
            Vector2 diff = Vec1 - Vec2;
            return ((1 - settings.MixFactor) * MathF.Sqrt(diff.X * diff.X + diff.Y * diff.Y)) + (settings.MixFactor * (MathF.Abs(diff.X) + MathF.Abs(diff.Y)));
        }
        public static float Distance(Vector2 Vec1, Vector2 Vec2, DistanceType type)
        {
            if (type == DistanceType.Euclidean)
                return EuclideanDistance(Vec1, Vec2);
            if (type == DistanceType.Manhattan)
                return ManhattanDistance(Vec1, Vec2);
            if (type == DistanceType.Mix)
                return MixDistance(Vec1, Vec2);
            if (type == DistanceType.Minkowski)
                return MinkowskiDistance(Vec1, Vec2);

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
        public static void GenerateSeedsPatterns()
        {
            settings.ClearSeeds();
            for (int i = 0; i < settings.DefaultNumberOfSeeds; i++)
            {
                //float angle = i * MathF.PI / 5;
                //Vector2 p = new() { X = random.Next(Raylib.GetScreenWidth()), Y = random.Next(Raylib.GetScreenHeight()) };
                float f = 8.0f;
                float angle = i * 2 * f * MathF.PI / settings.DefaultNumberOfSeeds;
                float x = CurrentWidth / 2 + i * MathF.Cos(angle);
                float y = CurrentHeight / 2 + i * MathF.Sin(angle);
                if (!(0 <= x && x <= CurrentWidth)) x = float.Clamp(x, 0, CurrentWidth);
                if (!(0 <= y && y <= CurrentHeight)) y = float.Clamp(y, 0, CurrentHeight);
                Vector2 p = new() { X = x, Y = y };
                Color c = GetRandomColor();
                settings.AddSeed(new(p, c));
            }
            for (int i = 0; i < settings.DefaultNumberOfSeeds; i++)
            {
                //float angle = i * MathF.PI / 5;
                //Vector2 p = new() { X = random.Next(Raylib.GetScreenWidth()), Y = random.Next(Raylib.GetScreenHeight()) };
                float f = 8.0f;
                float angle = i * 2 * f * MathF.PI / settings.DefaultNumberOfSeeds;
                float x = CurrentWidth / 2 + i * MathF.Cos(angle + MathF.PI);
                float y = CurrentHeight / 2 + i * MathF.Sin(angle + MathF.PI);
                if (!(0 <= x && x <= CurrentWidth)) x = float.Clamp(x, 0, CurrentWidth);
                if (!(0 <= y && y <= CurrentHeight)) y = float.Clamp(y, 0, CurrentHeight);
                Vector2 p = new() { X = x, Y = y };
                Color c = GetRandomColor();
                settings.AddSeed(new(p, c));
            }
            settings.m_Changed = true;
        }
        public static void GenerateSeedsRandom()
        {
            settings.ClearSeeds();
            for (int i = 0; i < settings.DefaultNumberOfSeeds; i++)
            {
                Vector2 p = new() { X = random.Next(Raylib.GetScreenWidth()), Y = random.Next(Raylib.GetScreenHeight()) };
                Color c = GetRandomColor();
                settings.AddSeed(new(p, c));
            }
            settings.m_Changed = true;
        }
        public static void GenerateSeeds()
        {
            GenerateSeedsPatterns();
            //GenerateSeedsRandom();
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
                    settings.m_urandom = random.NextSingle();
                }
            }
            if (settings.RenderType == RenderType.CPU)
            {
                if (Raylib.IsKeyPressed(KeyboardKey.R))
                {
                    GenerateSeeds();
                }
                if (Raylib.IsWindowResized())
                {
                    settings.m_Changed = true;
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
                    if (Raylib.IsKeyPressed(KeyboardKey.K))
                        settings.DistanceType = DistanceType.Minkowski;
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
            Raylib.UnloadRenderTexture(settings.m_texture);
            settings.m_texture = Raylib.LoadRenderTexture(CurrentWidth, CurrentHeight);
            Raylib.BeginTextureMode(settings.m_texture);
            Raylib.ClearBackground(Color.White);
            for (int i = 0; i < Grid.Count; i++)
            {
                for (int j = 0; j < Grid[i].Count; j++)
                {
                    Color c = settings.GetSeed(Grid[i][j].m_index).m_Color;
                    Raylib.DrawPixel(i, j, c);
                }
            }
            Raylib.EndTextureMode();
        }
        public static void UpdateGridPrallelApproach()
        {
            Vector2 CurrentLocation;
            for (int i = 0; i < CurrentWidth; i++)
            {
                List<Pixel> temp = [];
                for (int j = 0; j < CurrentHeight; j++)
                {
                    CurrentLocation = new(i, j);
                    temp.Add(new(CurrentLocation, Distance(CurrentLocation, settings.GetSeed(0).m_Position, settings.DistanceType), 0));
                }
                Grid.Add(temp);
            }
            for (int k = 0; k < settings.NumberOfSeeds; k++)
            {
                Parallel.For(0, CurrentWidth, i =>
                {
                    Parallel.For(0, CurrentHeight, j =>
                    {
                        Vector2 PixelPosition = Grid[i][j].m_Position;
                        Vector2 PixelandCurrentSmallestDistance = PixelPosition - settings.GetSeed(Grid[i][j].m_index).m_Position;
                        Vector2 PixelandCurrentIndexDistance = PixelPosition - settings.GetSeed(k).m_Position;
                        bool IfSmaller = IsSmallerDistance(PixelandCurrentIndexDistance, PixelandCurrentSmallestDistance, settings.DistanceType);
                        if (IfSmaller)
                        {
                            Grid[i][j].m_DistanceToIndex = Distance(PixelPosition, settings.GetSeed(k).m_Position, settings.DistanceType);
                            Grid[i][j].m_index = k;
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
                        Vector2 PixelandCurrentSmallestDistance = pixel - settings.GetSeed(index).m_Position;
                        Vector2 PixelandCurrentIndexDistance = pixel - settings.GetSeed(k).m_Position;
                        bool IfSmaller = IsSmallerDistance(PixelandCurrentIndexDistance, PixelandCurrentSmallestDistance, settings.DistanceType);
                        if (IfSmaller)
                            index = k;
                    }
                    temp.Add(new(pixel, Distance(pixel, settings.GetSeed(index).m_Position, settings.DistanceType), index));
                }
                Grid.Add(temp);
            }
        }
        public static void RenderVoronoiClassicalApproach()
        {
            if (settings.m_Changed)
            {
                Grid.Clear();
                UpdateGridClassicalApproach();
                UpdateTexture();
                settings.m_Changed = false;
            }
            Raylib.DrawTextureRec(settings.m_texture.Texture, new() { X = 0, Y = 0, Width = CurrentWidth, Height = -CurrentHeight }, new() { X = 0, Y = 0 }, Color.White);
        }
        public static void RenderVoronoiPrallelApproach()
        {
            if (settings.m_Changed)
            {
                Grid.Clear();
                UpdateGridPrallelApproach();
                UpdateTexture();
                settings.m_Changed = false;
            }
            Raylib.DrawTextureRec(settings.m_texture.Texture, new() { X = 0, Y = 0, Width = CurrentWidth, Height = -CurrentHeight }, new() { X = 0, Y = 0 }, Color.White);
        }
        public static void RenderVoronoiCPU()
        {
            //RenderVoronoiClassicalApproach();
            RenderVoronoiPrallelApproach();
            //RenderVoronoiBetterAlgorithim();
            for (int i = 0; i < settings.NumberOfSeeds; i++)
            {
                Seed CurrentSeed = settings.GetSeed(i);
                Raylib.DrawCircleV(CurrentSeed.m_Position, 2.5f, Color.Black);
                continue;
                Vector2 newpos = CurrentSeed.m_Position + (Raylib.GetFrameTime() * CurrentSeed.m_Velocity);
                if (0 <= newpos.X && newpos.X <= CurrentWidth)
                {
                    CurrentSeed.m_Position.X = newpos.X;
                }
                else
                {
                    CurrentSeed.m_Velocity.X *= -1;
                }
                if (0 <= newpos.Y && newpos.Y <= CurrentWidth)
                {
                    CurrentSeed.m_Position.Y = newpos.Y;
                }
                else
                {
                    CurrentSeed.m_Velocity.Y *= -1;
                }
                settings.SetSeed(i, CurrentSeed);
                settings.m_Changed = true;
            }
        }
        public static void SwitchRenderTypeToCPU()
        {
            settings.RenderType = RenderType.CPU;
            GenerateSeeds();
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
                    settings.m_uID = RenderView.Color;
                if (Raylib.IsKeyPressed(KeyboardKey.Two))
                    settings.m_uID = RenderView.BlackWhite;

                Raylib.SetShaderValue(settings.m_shader, Raylib.GetShaderLocation(settings.m_shader, "ures"), ures, ShaderUniformDataType.Vec2);
                Raylib.SetShaderValue(settings.m_shader, Raylib.GetShaderLocation(settings.m_shader, "urandom"), settings.m_urandom, ShaderUniformDataType.Float);
                Raylib.SetShaderValue(settings.m_shader, Raylib.GetShaderLocation(settings.m_shader, "uID"), settings.m_uID, ShaderUniformDataType.Int);

                Raylib.BeginShaderMode(settings.m_shader);
                Raylib.DrawRectangle(0, 0, CurrentWidth, CurrentHeight, Color.White);
                Raylib.EndShaderMode();
            }
            if (settings.RenderType == RenderType.CPU)
            {
                RenderVoronoiCPU();
            }
        }
        public static void DisplayWelcomeScreen()
        {
            int FontSize = 30;
            int x = CurrentWidth / 4;
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
                "\t\tswitch to different distance modes by pressing (Euclidean(U), Manhattan(M), Mix(I), Minkowski(K))",
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
            Raylib.InitWindow(800, 600, "Voronoi");
            //Raylib.InitWindow(16 * 30, 9 * 30, "Voronoi");
            settings = new();
            State state = State.WelcomeScreen;
            CurrentWidth = Raylib.GetScreenWidth();
            CurrentHeight = Raylib.GetScreenHeight();
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
            Raylib.UnloadRenderTexture(settings.m_texture);
            Raylib.UnloadShader(settings.m_shader);
            Raylib.CloseWindow();
        }
    }
}
