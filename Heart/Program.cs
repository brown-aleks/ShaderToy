using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;

namespace ShaserFractal
{
    class Program
    {
        static void Main(string[] args)
        {
            NativeWindowSettings nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(800, 600),
                Title = "PracticeOpenTK",
                WindowState = WindowState.Normal,
                Flags = OpenTK.Windowing.Common.ContextFlags.Default,
                Profile = ContextProfile.Core
            };
            using (Window game = new Window(nativeWindowSettings))
            {
                game.Run();
            }

        }
    }
}
