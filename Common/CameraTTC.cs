using OpenTK.Mathematics;
using System;

namespace LearnOpenTK.Common
{
    // ��� ����� ������, ������� ����� ��������� ����� ���������� �� ���-�����.
    // ����� ��������, ��� ���� ��������� �������� ��������� ��� ������.
    // ��������, �� ����� ����� ��������� ������ ������ ������ ������ ������,
    // � ������ �������� ����� ���� �� ���������� � �������.

    // TL; DR: ��� ���� ���� �� ������ ��������, �������� �� ����� �� ��������� ������.
    // ����������� ���-������, ���� �� �� ������, ������ �� ������ ���-�� ����������, ��� ������ ������ ������ � ����.
    public class CameraTTC  // to the center
    {
        // ��� ������� ������������ ����� �����������, ����������� �� ������ ������, ����� ����������, ��� ��� ���������.
        private Vector3 _position = -Vector3.UnitZ;

        private Vector3 _up = Vector3.UnitY;

        private Vector3 _right = Vector3.UnitX;

        // �������� ������ ��� X (�������)
        private float _pitch;

        // �������� ������ ��� Y (�������)
        private float _yaw = -MathHelper.PiOver2; // ��� ����� �� �� ������ ��������� �� 90 �������� ������.

        // ���� ������ ������ (�������)
        private float _fov = MathHelper.PiOver2;

        public CameraTTC(float distance, float aspectRatio)
        {
            Distance = distance;
            AspectRatio = aspectRatio;
        }

        // ��������� ������
        public float Distance { get; set; }

        // ��� ������ ����������� ������ ������� ���������, ������������ ��� ������� ��������.
        public float AspectRatio { private get; set; }

        public Vector3 Position => _position;

        public Vector3 Up => _up;

        public Vector3 Right => _right;

        // �� ������������ ������� � �������, ��� ������ �������� ����������� ��� ��������� ������������������.
        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(_pitch);
            set
            {
                // �� �������� �������� ���� ����� -89 � 89, ����� ������ �� �������������, � ����
                // �������� "������" ��� ������������� ����� ������ ��� ��������.
                // ���� �� ������ ������ ������ �� ����, �� ������ ����������� ������� ����, ���������� ����������� �������
                var angle = value;// MathHelper.Clamp(value, -89f, 89f);
                _pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }

        // �� ������������ ������� � �������, ��� ������ �������� ����������� ��� ��������� ������������������.
        public float Yaw
        {
            get => MathHelper.RadiansToDegrees(_yaw);
            set
            {
                _yaw = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }

        // ���� ������ (FOV) - ��� ������������ ���� ������ ������.
        // ��� ����������� ����� �������� � ���������� �����,
        // �� � ���� ����������� �� ����� ������, ��� �� ����� ������������ ��� ��� �������� ������� ���������������.
        // �� ������������ ������� � �������, ��� ������ �������� ����������� ��� ��������� ������������������.
        public float Fov
        {
            get => MathHelper.RadiansToDegrees(_fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 45f);
                _fov = MathHelper.DegreesToRadians(angle);
            }
        }

        // ��������� ������� ��������� � ������� ������������� ������� LookAt, ����� �������� ��������� � ���-������������
        public Matrix4 GetViewMatrix()
        {
            //Vector3 _distance = Vector3.C * Distance;
            return Matrix4.LookAt(_position * Distance, Vector3.Zero, _up);
        }

        // �������� ������� ��������, ��������� ��� �� �����, ������� �� ������������ �� ����� �������
        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.01f, 100f);
        }

        // ��� ������� ����� ��������� ������� �����������, ��������� ����������, ��������� � ���-������������.
        private void UpdateVectors()
        {
            // ��-������, �������� ������� ����������� � �������������� ��������� ������� �������������.
            _position.X = MathF.Cos(_pitch) * MathF.Cos(_yaw);
            _position.Y = MathF.Sin(_pitch);
            _position.Z = MathF.Cos(_pitch) * MathF.Sin(_yaw);

            // ��� ����� ���������, ��� ��� ������� �������������, ����� �� ������� ���������� ����������.
            _position = Vector3.Normalize(_position);

            // ��������� ��� ������, ��� � ���������� ������, ��������� �����-������������.
            // �������� ��������, ��� �� ��������� ������ �� ����������� �����; ����� ��������� �����
            // �� ����� ���, ��� ��� ����� ��� ���� �����, ������� ������ ��� � ����, ���� ��� �� ����� ������ FPS.
            _right = Vector3.Normalize(Vector3.Cross(_position, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, _position));
        }
    }
}