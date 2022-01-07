using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace PracticeOpenTK.Common
{
    // Простой класс, помогающий создавать шейдеры.
    public class Shader
    {
        public readonly int Handle;

        private readonly Dictionary<string, int> _uniformLocations;

        // Вот как вы создаете простой шейдер.
        // Шейдеры написаны на GLSL, который по своей семантике очень похож на C.
        // Исходный код GLSL компилируется * во время выполнения *, поэтому он может оптимизировать себя для видеокарты, на которой он в настоящее время используется.
        // Прокомментированный пример GLSL можно найти в shader.vert.
        public Shader(string vertPath, string fragPath)
        {
            // Существует несколько различных типов шейдеров, но для базового рендеринга вам нужны только два - вершинный и фрагментный шейдеры.
            // Вершинный шейдер отвечает за перемещение вершин и загрузку этих данных во фрагментный шейдер.
            // Вершинный шейдер здесь не будет иметь большого значения, но позже он станет более важным.
            // Фрагментный шейдер отвечает за последующее преобразование вершин в «фрагменты», которые представляют все данные, необходимые OpenGL для рисования пикселя.
            // Фрагментный шейдер - это то, что мы будем здесь чаще всего использовать.

            // Загружаем вершинный шейдер и компилируем
            var shaderSource = File.ReadAllText(vertPath);

            // GL.CreateShader создаст пустой шейдер (очевидно). Перечисление ShaderType указывает, какой тип шейдера будет создан.
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);

            // Теперь связываем исходный код GLSL
            GL.ShaderSource(vertexShader, shaderSource);

            // А затем компилируем
            CompileShader(vertexShader);

            // То же самое делаем для фрагментного шейдера.
            shaderSource = File.ReadAllText(fragPath);
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSource);
            CompileShader(fragmentShader);

            // Эти два шейдера затем необходимо объединить в программу шейдера, которая затем может использоваться OpenGL.
            // Для этого создаем программу ...
            Handle = GL.CreateProgram();

            // Присоединяем оба шейдера ...
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            // А затем свяжем их вместе.
            LinkProgram(Handle);

            // Когда программа шейдера связана, ей больше не нужны прикрепленные к ней отдельные шейдеры; скомпилированный код копируется в программу шейдера.
            // Отсоединить их, а затем удалить.
            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            // Шейдер теперь готов к работе, но сначала мы собираемся кэшировать все места униформы шейдера.
            // Запрос этого из шейдера очень медленный, поэтому мы делаем это один раз при инициализации и повторно используем эти значения
            // потом.

            // Во-первых, нам нужно получить количество активных форм в шейдере.
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            // Затем выделяем словарь для хранения местоположений.
            _uniformLocations = new Dictionary<string, int>();

            // Перебираем всю униформу,
            for (var i = 0; i < numberOfUniforms; i++)
            {
                // получаем название этой униформы,
                var key = GL.GetActiveUniform(Handle, i, out _, out _);

                // получаем местоположение,
                var location = GL.GetUniformLocation(Handle, key);

                // а затем добавить в словарь.
                _uniformLocations.Add(key, location);
            }
        }

        private static void CompileShader(int shader)
        {
            // Пытаемся скомпилировать шейдер
            GL.CompileShader(shader);

            // Проверяем ошибки компиляции
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                // Мы можем использовать GL.GetShaderInfoLog(shader) для получения информации об ошибке.
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
            }
        }

        private static void LinkProgram(int program)
        {
            // Связываем программу
            GL.LinkProgram(program);

            // Проверяем ошибки связывания
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                // Мы можем использовать GL.GetProgramInfoLog(program) для получения информации об ошибке.
                throw new Exception($"Error occurred whilst linking Program({program})");
            }
        }

        // Функция-оболочка, которая включает программу шейдера.
        public void Use()
        {
            GL.UseProgram(Handle);
        }

        // Источники шейдеров, предоставленные с этим проектом, используют жестко запрограммированный layout (location) -s. Если вы хотите сделать это динамически,
        // вы можете опустить строки макета (location = X) в вершинном шейдере и использовать их в VertexAttribPointer вместо жестко заданных значений.
        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }

        // Установщики униформы
        // Униформы - это переменные, которые могут быть установлены пользовательским кодом вместо чтения их из VBO.
        // Вы используете VBO для данных, связанных с вершинами, и униформы почти для всего остального.

        // Установка униформы почти всегда одна и та же, поэтому я объясню это здесь один раз, а не в каждом методе:
        // 1. Свяжите программу, на которую хотите установить униформу.
        // 2. Получить дескриптор местоположения униформы с помощью GL.GetUniformLocation.
        // 3. Используйте соответствующую функцию GL.Uniform *, чтобы установить униформу.

        /// <summary>
        /// Устанавливает униформу int на этом шейдере.
        /// </summary>
        /// <param name="name">Название униформы</param>
        /// <param name="data">Данные для установки</param>
        public void SetInt(string name, int data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        /// <summary>
        /// Устанавливает униформу float на этом шейдере.
        /// </summary>
        /// <param name="name">Название униформы</param>
        /// <param name="data">Данные для установки</param>
        public void SetFloat(string name, float data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        /// <summary>
        /// Устанавливает униформу Matrix4 на этом шейдере.
        /// </summary>
        /// <param name="name">Название униформы</param>
        /// <param name="data">Данные для установки</param>
        /// <remarks>
        ///   <para>
        ///   Матрица транспонируется перед отправкой в шейдер.
        ///   </para>
        /// </remarks>
        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(Handle);
            GL.UniformMatrix4(_uniformLocations[name], true, ref data);
        }

        /// <summary>
        /// Устанавливает униформу Vector2 на этом шейдере.
        /// </summary>
        /// <param name="name">Название униформы</param>
        /// <param name="data">Данные для установки</param>
        public void SetVector2(string name, Vector2 data)
        {
            GL.UseProgram(Handle);
            GL.Uniform2(_uniformLocations[name], data);
        }

        /// <summary>
        /// Устанавливает униформу Vector3 на этом шейдере.
        /// </summary>
        /// <param name="name">Название униформы</param>
        /// <param name="data">Данные для установки</param>
        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(Handle);
            GL.Uniform3(_uniformLocations[name], data);
        }

        /// <summary>
        /// Устанавливает униформу Vector4 на этом шейдере.
        /// </summary>
        /// <param name="name">Название униформы</param>
        /// <param name="data">Данные для установки</param>
        public void SetVector4(string name, Vector4 data)
        {
            GL.UseProgram(Handle);
            GL.Uniform4(_uniformLocations[name], data);
        }
    }
}
