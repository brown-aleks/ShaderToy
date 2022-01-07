using OpenTK.Mathematics;
using System;

namespace LearnOpenTK.Common
{
    // Это класс камеры, который можно настроить после руководств на веб-сайте.
    // Важно отметить, что есть несколько способов настроить эту камеру.
    // Например, вы также могли управлять вводом игрока внутри класса камеры,
    // и многие свойства можно было бы превратить в функции.

    // TL; DR: Это лишь один из многих способов, которыми мы могли бы настроить камеру.
    // Просмотрите веб-версию, если вы не знаете, почему мы делаем что-то конкретное, или хотите узнать больше о коде.
    public class CameraTTC  // to the center
    {
        // Эти векторы представляют собой направления, указывающие от камеры наружу, чтобы определить, как она вращается.
        private Vector3 _position = -Vector3.UnitZ;

        private Vector3 _up = Vector3.UnitY;

        private Vector3 _right = Vector3.UnitX;

        // Вращение вокруг оси X (радианы)
        private float _pitch;

        // Вращение вокруг оси Y (радианы)
        private float _yaw = -MathHelper.PiOver2; // Без этого вы бы начали повернуты на 90 градусов вправо.

        // Поле зрения камеры (радианы)
        private float _fov = MathHelper.PiOver2;

        public CameraTTC(float distance, float aspectRatio)
        {
            Distance = distance;
            AspectRatio = aspectRatio;
        }

        // Положение камеры
        public float Distance { get; set; }

        // Это просто соотношение сторон области просмотра, используемое для матрицы проекции.
        public float AspectRatio { private get; set; }

        public Vector3 Position => _position;

        public Vector3 Up => _up;

        public Vector3 Right => _right;

        // Мы конвертируем градусы в радианы, как только свойство установлено для повышения производительности.
        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(_pitch);
            set
            {
                // Мы зажимаем значение шага между -89 и 89, чтобы камера не перевернулась, и кучу
                // странных "ошибок" при использовании углов Эйлера для вращения.
                // Если вы хотите узнать больше об этом, вы можете попробовать изучить тему, называемую блокировкой кардана
                var angle = value;// MathHelper.Clamp(value, -89f, 89f);
                _pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }

        // Мы конвертируем градусы в радианы, как только свойство установлено для повышения производительности.
        public float Yaw
        {
            get => MathHelper.RadiansToDegrees(_yaw);
            set
            {
                _yaw = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }

        // Поле зрения (FOV) - это вертикальный угол обзора камеры.
        // Это обсуждалось более подробно в предыдущем уроке,
        // но в этом руководстве вы также узнали, как мы можем использовать это для имитации функции масштабирования.
        // Мы конвертируем градусы в радианы, как только свойство установлено для повышения производительности.
        public float Fov
        {
            get => MathHelper.RadiansToDegrees(_fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 45f);
                _fov = MathHelper.DegreesToRadians(angle);
            }
        }

        // Получение матрицы просмотра с помощью замечательной функции LookAt, более подробно описанной в веб-руководствах
        public Matrix4 GetViewMatrix()
        {
            //Vector3 _distance = Vector3.C * Distance;
            return Matrix4.LookAt(_position * Distance, Vector3.Zero, _up);
        }

        // Получаем матрицу проекции, используя тот же метод, который мы использовали до этого момента
        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.01f, 100f);
        }

        // Эта функция будет обновлять вершины направления, используя математику, изученную в веб-руководствах.
        private void UpdateVectors()
        {
            // Во-первых, передняя матрица вычисляется с использованием некоторой базовой тригонометрии.
            _position.X = MathF.Cos(_pitch) * MathF.Cos(_yaw);
            _position.Y = MathF.Sin(_pitch);
            _position.Z = MathF.Cos(_pitch) * MathF.Sin(_yaw);

            // Нам нужно убедиться, что все векторы нормализованы, иначе мы получим неприятные результаты.
            _position = Vector3.Normalize(_position);

            // Вычислить как правый, так и восходящий вектор, используя кросс-произведение.
            // Обратите внимание, что мы вычисляем справа от глобального вверх; такое поведение может
            // не будет тем, что вам нужно для всех камер, поэтому имейте это в виду, если вам не нужна камера FPS.
            _right = Vector3.Normalize(Vector3.Cross(_position, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, _position));
        }
    }
}