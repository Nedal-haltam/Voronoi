
using Raylib_cs;

namespace Voronoi
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Raylib.SetConfigFlags(ConfigFlags.AlwaysRunWindow | ConfigFlags.ResizableWindow);
            Raylib.SetTargetFPS(0);
            Raylib.InitWindow(800, 600, "Voronoi");
            while(!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Red);
                Raylib.EndDrawing();
            }
            Raylib.CloseWindow();
        }
    }
}
