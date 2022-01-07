using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PracticeOpenTK.Common;
using System;

namespace ShaserFractal
{
    class Window : GameWindow
    {
        private float FremeTime = 0.0f;
        private int FPS = 0;

        private readonly float[] _vertices =
        {
             1.0f,  1.0f, 0.0f, // в правом верхнем углу
             1.0f, -1.0f, 0.0f, // внизу справа
            -1.0f, -1.0f, 0.0f, // Нижняя левая
            -1.0f,  1.0f, 0.0f, // верхний левый
        };

        private readonly uint[] _index =
        {
            0,1,3,
            1,2,3
        };

        private int _vertexBufferObject;    //  VBO
        private int _vertexArrayObject;     //  VAO
        private int _elementBufferObject;   //  EBO
        private Shader _shader;             //  Shader
        private float Time;
        private bool Space = true;
        private Vector3 iResolution = new (0.0f);
        private Vector4 iMouse = new (0.0f);

        public Window(NativeWindowSettings nativeWindowSettings)
            : base(GameWindowSettings.Default, nativeWindowSettings)
        {
            Console.WriteLine(GL.GetString(StringName.Version));
            Console.WriteLine(GL.GetString(StringName.Vendor));
            Console.WriteLine(GL.GetString(StringName.Renderer));
            Console.WriteLine(GL.GetString(StringName.ShadingLanguageVersion));

            VSync = VSyncMode.On;
            CursorVisible = true;
        }
        protected override void OnLoad()
        {
            iResolution.Xy = Size;

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _index.Length * sizeof(int), _index, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            _shader = new Shader("Shaders/shader.vert","Shaders/shader.frag");
            _shader.Use();

            base.OnLoad();
        }
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
        }
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            iMouse.Xy = MousePosition;

            _shader.Use();
            _shader.SetVector3("iResolution", iResolution);
            _shader.SetFloat("iTime", Time);
            _shader.SetVector4("iMouse", iMouse);
            //_shader.SetFloat("iTimeDelta", (float)args.Time);
            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawElements(PrimitiveType.Triangles, _index.Length, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
            base.OnRenderFrame(args);
        }
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            var _mouseState = MouseState;


            if (_mouseState.IsButtonDown(MouseButton.Left)) CursorGrabbed = true;
            if (_mouseState.IsButtonDown(MouseButton.Right)) CursorGrabbed = false;

            if (Space) Time += (float)args.Time / 4.0f;

            FremeTime += (float)args.Time;
            FPS++;
            if (FremeTime >= 1.0f)
            {
                Title = $"PracticeOpenTK - FPS {FPS}";
                FremeTime = 0;
                FPS = 0;
            }
        }
        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
            switch (e.Key)
            {
                case Keys.Escape:
                    Close();
                    break;
                case Keys.Space:
                    Space = true;
                    break;
                default:
                    break;
            }
        }
        protected override void OnUnload()
        {
            base.OnUnload();
        }
    }
}
