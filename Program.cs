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
        public class Pixel(Vector2 Position, float DistanceToSeed, int SeedIndex)
        {
            public Vector2 m_Position = Position;
            public float m_DistanceToIndex = DistanceToSeed;
            public int m_index = SeedIndex;
        }
        public class Cell
        {
            public List<Vector2> m_Vertices;
            public Vector2 m_Center;
            public Cell()
            {
                m_Vertices = [];
                m_Center = new();
            }
            public Cell(List<Vector2> vertices, Vector2 center)
            {
                m_Vertices = vertices;
                m_Center = center;
            }
        }
        public class Box : Cell
        {
            public Vector2 m_Min, m_Max;
            public Rectangle m_Bounds;
            public List<Cell> m_Cells;
            public Box() : base()
            {
                m_Min = new(0, 0);
                m_Max = new(0, 0);
                m_Bounds = new();
                m_Cells = [];
            }
            public Box(float Padding, List<Vector2> vertices, Vector2 center) : base(vertices, center)
            {
                if (vertices.Count != 4)
                    throw new Exception("Invalid box\n");
                m_Min = vertices[0] - new Vector2(Padding, Padding);
                m_Max = vertices[2] + new Vector2(Padding, Padding);
                m_Bounds = new(m_Min, new(m_Max.X - m_Min.X, m_Max.Y - m_Min.Y));
                m_Cells = [];
            }
        }
        public struct Bisector(Vector2 a, Vector2 b)
        {
            public Vector2 m_a = a;
            public Vector2 m_b = b;
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
                float mag = 10 + (40) * random.NextSingle();
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
        }

        public static int DefaultNumberOfSeeds = 110;
        public static List<Vector2> texcoords = [];
        public static List<Vector2> pts = [];
        public static Vector2 center = new(0, 0);
        public static List<Seed> seg = [];
        public static List<bool> segb = [];
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
            float mag = 5;
            for (int i = 0; i < DefaultNumberOfSeeds; i++)
            {
                float f = 8.0f;
                float angle = i * 2 * f * MathF.PI / DefaultNumberOfSeeds;
                float x = CurrentWidth / 2 + mag * i * MathF.Cos(angle);
                float y = CurrentHeight / 2 + mag * i * MathF.Sin(angle);
                x = float.Clamp(x, 11, CurrentWidth - 11);
                y = float.Clamp(y, 11, CurrentHeight - 11);
                Vector2 p = new() { X = x, Y = y };
                Color c = GetRandomColor();
                settings.AddSeed(new(p, c));
            }
            for (int i = 0; i < DefaultNumberOfSeeds; i++)
            {
                float f = 8.0f;
                float angle = i * 2 * f * MathF.PI / DefaultNumberOfSeeds;
                float x = CurrentWidth / 2 + mag * i * MathF.Cos(angle + MathF.PI);
                float y = CurrentHeight / 2 + mag * i * MathF.Sin(angle + MathF.PI);
                x = float.Clamp(x, 11, CurrentWidth - 11);
                y = float.Clamp(y, 11, CurrentHeight - 11);
                Vector2 p = new() { X = x, Y = y };
                Color c = GetRandomColor();
                settings.AddSeed(new(p, c));
            }
            settings.m_Changed = true;
        }
        public static void GenerateSeedsRandom()
        {
            settings.ClearSeeds();
            for (int i = 0; i < DefaultNumberOfSeeds; i++)
            {
                Vector2 p = new() { X = random.Next(10, Raylib.GetScreenWidth() - 10), Y = random.Next(10, Raylib.GetScreenHeight() - 10) };
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
        public static List<Cell> Vcells = [];
        public static void RenderVoronoiFast(List<Seed> sites)
        {
            if (settings.m_Changed)
            {
                float MinX = 10;
                float MinY = 10;
                float MaxX = CurrentWidth - 10;
                float MaxY = CurrentHeight - 10;
                float Padding = 5;
                Box box = new(Padding, [new(MinX, MinY), new(MaxX, MinY), new(MaxX, MaxY), new(MinX, MaxY)], new((MinX + MaxX) / 2.0f, (MinY + MaxY) / 2.0f));
                Vcells = GetVoronoiCellFast(sites, box);                
                Vcells.ForEach(cell => cell.m_Vertices.Reverse());
                settings.m_Changed = false;
            }
            if (Vcells.Count != sites.Count)
                throw new Exception("Vcells count doesn't equall sites count\n");
            for (int i = 0; i < sites.Count; i++)
            {
                DrawPoly(sites[i].m_Position, [.. Vcells[i].m_Vertices], sites[i].m_Color);
                //for (int j = 0; j < Vcells[i].m_Vertices.Count; j++)
                //{
                //    Raylib.DrawLineEx(Vcells[i].m_Vertices[j], Vcells[i].m_Vertices[(j + 1) % Vcells[i].m_Vertices.Count], 4, sites[i].m_Color);
                //}
            }
        }
        public static void RenderVoronoiCPU()
        {
            RenderVoronoiFast(settings.m_Seeds);
            //RenderVoronoiPrallelApproach();
            //RenderVoronoiClassicalApproach();
            for (int i = 0; i < settings.NumberOfSeeds; i++)
            {
                Seed CurrentSeed = settings.GetSeed(i);
                Raylib.DrawCircleV(CurrentSeed.m_Position, 2.5f, Color.White);
                Vector2 newpos = CurrentSeed.m_Position + (Raylib.GetFrameTime() * CurrentSeed.m_Velocity);
                continue;
                if (11 <= newpos.X && newpos.X <= CurrentWidth - 11)
                {
                    CurrentSeed.m_Position.X = newpos.X;
                }
                else
                {
                    CurrentSeed.m_Velocity.X *= -1;
                }
                if (11 <= newpos.Y && newpos.Y <= CurrentHeight - 11)
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
        public unsafe static void DrawTexturePoly(Texture2D texture, Vector2 center, Vector2* texcoords, int pointCount, Color tint)
        {
            Rlgl.SetTexture(texture.Id);

            Rlgl.Begin(DrawMode.Triangles);

            Rlgl.Color4ub(tint.R, tint.G, tint.B, tint.A);

            for (int i = 0; i < pointCount - 1; i++)
            {
                Rlgl.TexCoord2f(0.5f, 0.5f);
                Rlgl.Vertex2f(center.X, center.Y);

                Rlgl.TexCoord2f(texcoords[i].X, texcoords[i].Y);
                Rlgl.Vertex2f(texcoords[i].X, texcoords[i].Y);

                Rlgl.TexCoord2f(texcoords[i + 1].X, texcoords[i + 1].Y);
                Rlgl.Vertex2f(texcoords[i + 1].X, texcoords[i + 1].Y);
            }
            Rlgl.End();

            Rlgl.SetTexture(0);
        }
        public static bool loaded = false;
        public static Image image = Raylib.GenImageColor(CurrentWidth, CurrentHeight, Color.White);
        public static Texture2D texture = Raylib.LoadTextureFromImage(image);
        public unsafe static void DrawPoly(Vector2 center, Vector2[] texcoords, Color c)
        {
            fixed (Vector2* texcoordsPTR = texcoords)
            {
                DrawTexturePoly(texture, center, texcoordsPTR, texcoords.Length, c);
            }
        }
        public static (List<Vector2>?, int?, int?) CheckLineIntersectPoly(Vector2 StartPosisiton, Vector2 EndPosisiton, List<Vector2> Poly)
        {
            List<Vector2> ret = [];
            List<int> retints = [];
            for (int i = 0; i < Poly.Count; i++)
            {
                Vector2? v = CheckLineIntersection(StartPosisiton, EndPosisiton, Poly[i], Poly[(i + 1) % Poly.Count]);
                if (v.HasValue)
                {
                    ret.Add(v.Value);
                    retints.Add(i + 1);
                }
            }
            return ((ret.Count > 0) ? ret : null, (retints.Count > 0) ? retints[0] : null, (retints.Count > 1) ? retints[1] : null);
        }
        public static int Orientation(Vector2 p, Vector2 q, Vector2 r)
        {
            float val = (q.Y - p.Y) * (r.X - q.X) - (q.X - p.X) * (r.Y - q.Y);
            return (val <= 1e-6) ? 0 : ((val > 0) ? 1 : -1);
        }
        public static float Cross(Vector2 r, Vector2 s) => r.X * s.Y - r.Y * s.X;
        public static Vector2? POI(Vector2 p, Vector2 ppr, Vector2 q, Vector2 qps)
        {
            Vector2 r = ppr - p;
            Vector2 s = qps - q;
            // Define the 2-dimensional vector cross product r × s to be r(x) s(y) − r(y) s(x).
            float RcrossS = Cross(r, s);
            if (RcrossS != 0)
            {
                float t = Cross(q - p, s) / RcrossS;
                float u = Cross(q - p, r) / RcrossS;
                if (0 <= t && t <= 1 && 0 <= u && u <= 1)
                {
                    //p + t r = q + u s
                    float x = p.X + t * r.X;
                    float y = p.Y + t * r.Y;
                    return new(x, y);
                }
            }
            // t = (q − p) × s / (r × s)
            // u = (q − p) × r / (r × s)
            // If r × s ≠ 0 and 0 ≤ t ≤ 1 and 0 ≤ u ≤ 1, the two line segments meet at the point p + t r = q + u s.
            return null;
        }
        public static Vector2? CheckLineIntersection(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            //Vector2 vec;
            //bool col = false;
            //unsafe
            //{
            //    col = Raylib.CheckCollisionLines(a, b, c, d, &vec);
            //}
            //return col ? vec : null;
            float o1 = Orientation(a, b, c);
            float o2 = Orientation(a, b, d);
            float o3 = Orientation(c, d, a);
            float o4 = Orientation(c, d, b);
            if (o1 != o2 && o3 != o4)
            {
                Vector2? poi = POI(a, b, c, d);
                if (poi.HasValue)
                {
                    return poi;
                }
            }
            return null;
        }
        public static float BisectorEquation(float x, float a, float b, float c) => (a * x + c) / -b;
        public static Bisector GetBisector(Vector2 p1, Vector2 p2)
        {
            Vector2 v0 = new((p1.X + p2.X) / 2.0f, (p1.Y + p2.Y) / 2.0f);
            float a = p2.X - p1.X;
            float b = p2.Y - p1.Y;
            Vector2 diff = new(a, b);
            float c = -a * v0.X - b * v0.Y;
            // ax + by + c = 0
            // ax - c = -by
            // y = (ax - c) / b
            float angle = MathF.Atan(diff.Y / diff.X) + MathF.PI / 2.0f;
            Vector2 b1 = new(v0.X + 2*CurrentWidth * MathF.Cos(angle) , v0.Y + 2*CurrentHeight * MathF.Sin(angle));
            Vector2 b2 = new(v0.X - 2*CurrentWidth * MathF.Cos(angle) , v0.Y - 2*CurrentHeight * MathF.Sin(angle));
            return new(b2, b1);
        }
        public static List<Cell> GetVoronoiCellFast(List<Seed> sites, Box box)
        {
            List<Cell> output = [];
            for (int i = 0; i < sites.Count; i++)
            {
                Seed p = sites[i];
                Cell cell = new([box.m_Min, box.m_Min + new Vector2(box.m_Bounds.Width, 0), box.m_Max, box.m_Min + new Vector2(0, box.m_Bounds.Height), box.m_Min], p.m_Position);
                for (int j = 0; j < sites.Count; j++)
                {
                    if (i != j)
                    {
                        Seed q = sites[j];
                        Bisector bisector = GetBisector(p.m_Position, q.m_Position);
                        (List<Vector2>? intersects, int? xi, int? xj) = CheckLineIntersectPoly(bisector.m_a, bisector.m_b, cell.m_Vertices);
                        //Raylib.DrawLineEx(bisector.m_a, bisector.m_b, 3, q.m_Color);
                        if (intersects != null && intersects.Count == 2 && xi.HasValue && xj.HasValue)
                        {
                            List<Vector2> newCellVertices = [];
                            newCellVertices.Add(intersects[0]);
                            for (int k = xi.Value; ; k = (k + 1) % cell.m_Vertices.Count)
                            {
                                if (k == xj.Value)
                                    break;
                                newCellVertices.Add(cell.m_Vertices[k % cell.m_Vertices.Count]);
                            }
                            newCellVertices.Add(intersects[1]);
                            newCellVertices.Add(intersects[0]);

                            if (!Raylib.CheckCollisionPointPoly(cell.m_Center, [.. newCellVertices]))
                            {
                                newCellVertices.Clear();
                                newCellVertices.Add(intersects[1]);
                                for (int k = xj.Value;; k = (k + 1) % cell.m_Vertices.Count)
                                {
                                    if (k == xi.Value)
                                        break;
                                    newCellVertices.Add(cell.m_Vertices[k % cell.m_Vertices.Count]);
                                }
                                newCellVertices.Add(intersects[0]);
                                newCellVertices.Add(intersects[1]);
                            }
                            cell.m_Vertices.Clear();
                            cell.m_Vertices = newCellVertices;
                        }
                    }
                }
                output.Add(cell);
            }
            return output;
        }
        public static void ManagePoints()
        {
            if (Raylib.IsKeyPressed(KeyboardKey.R))
            {
                seg.Clear();
            }
            if (Raylib.IsMouseButtonPressed(MouseButton.Right))
            {
                seg.Add(new(Raylib.GetMousePosition(), GetRandomColor()));
                segb.Add(false);
            }
            for (int i = 0; i < seg.Count; i++)
            {
                Raylib.DrawCircleV(seg[i].m_Position, 4, Color.White);
                Vector2 mp = Raylib.GetMousePosition();
                if (Raylib.CheckCollisionPointCircle(mp, seg[i].m_Position, 5 * 4))
                {
                    if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                    {
                        segb[i] = true;
                    }
                    else if (Raylib.IsMouseButtonReleased(MouseButton.Left))
                    {
                        segb[i] = false;
                    }
                    if (segb[i])
                    {
                        seg[i].m_Position = mp;
                    }
                }
            }
        }
        public static void RenderSegments()
        {
            ManagePoints();
            List<Seed> sites = seg;
            float MinX = 0;
            float MinY = 0;
            float MaxX = CurrentWidth;
            float MaxY = CurrentHeight;
            float Padding = 5;
            Box box = new(Padding, [new(MinX, MinY), new(MaxX, MinY), new(MaxX, MaxY), new(MinX, MaxY)], new((MinX + MaxX) / 2.0f, (MinY + MaxY) / 2.0f));
            List<Cell> Vcells = GetVoronoiCellFast(seg, box);
            for (int i = 0; i < Vcells.Count; i++)
            {
                for (int j = 0; j < Vcells[i].m_Vertices.Count; j++)
                {
                    Raylib.DrawLineEx(Vcells[i].m_Vertices[j], Vcells[i].m_Vertices[(j + 1) % Vcells[i].m_Vertices.Count], 4, sites[i].m_Color);
                }
                Raylib.DrawCircleV(Vcells[i].m_Center, 4, Color.White);
            }
        }
        static void Main()
        {
            Raylib.SetConfigFlags(ConfigFlags.AlwaysRunWindow | ConfigFlags.ResizableWindow);
            Raylib.SetTargetFPS(0);
            //Raylib.InitWindow(800, 600, "Voronoi");
            Raylib.InitWindow(16 * 100, 9 * 100, "Voronoi");
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
