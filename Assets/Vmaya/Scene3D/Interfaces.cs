using System;
using UnityEngine;
using UnityEngine.Events;

namespace Vmaya.Scene3D
{
    public interface IPositioned
    {
        void setPosition(Vector3 value);
        void setPosition(Vector3 value, Quaternion rotate);
        Vector3 getPosition();
        Quaternion getRotate();
        void addListener(UnityAction action);
        void removeListener(UnityAction action);
    }

    public interface IRotatable
    {
        void setAngle(float angle);
        float getAngle();
        Vector3 getAxis();
        Vector3 getBaseVector();
        bool rotateAvailable();
    }

    public interface ILinked : IPositioned
    {
        bool isResist(int index);
        Vector3 givenResist(Vector3 checkValue, int index);
        void setPoint(int index, Vector3 pos);
        Vector3 getPoint(int index);
        int getPointCount();
        Limit getAngleLimit(int index);
    }

    public interface ISwitchable
    {
        void setOn(bool value);
    }

    public interface ISelectable: ISwitchable
    {
        bool isFocus();
        void removeListener(UnityAction<bool> listener);
        void addListener(UnityAction<bool> listener);
        int GetColor();
        void SetColor(int idx);
    }

    public interface IDraggable
    {
        void drag(Vector3 delta, Vector3 plane);
        void endDrag();
        Bounds getBounds(); // center - должен отображать смещение центра от границ 
        ILimiter getLimiter();
        bool allowedDrag();
    }

    public interface IHandle : IPositioned
    {
        bool isFocus();
        IDraggable getDraggable();
    }

    public interface ILimiter
    {
        Vector3 checkLimits(Bounds current, Vector3 delta);
        Vector3 degreesFreedom(Bounds current);
    }

    [System.Serializable]
    public class OnChangeSelectedEvent : UnityEvent<bool> { }

    [System.Serializable]
    public struct Limit
    {
        public float min;
        public float max;

        public static Limit init(float a_min, float a_max)
        {
            Limit data;
            data.min = a_min;
            data.max = a_max;
            return data;
        }

        public static readonly Limit Default = init(float.MaxValue, float.MinValue);
        public float size => max - min;

        public bool isEmpty => (min == 0) && (max == 0);

        public bool Between(float value)
        {
            return (value >= min) && (value <= max);
        }

        public float Clamp(float value)
        {
            return isEmpty ? value : Mathf.Clamp(value, min, max);
        }

        internal float ClampAngle(float value)
        {
            while (value > max + 90) value -= 180;
            while (value < min - 90) value += 180;

            return isEmpty ? value : Mathf.Clamp(value, min, max);
        }

        public float Random()
        {
            return UnityEngine.Random.Range(min, max);
        }

        internal static Limit init(float v)
        {
            return Limit.init(v, v);
        }
    }

    [System.Serializable]
    public struct VectorLimit
    {
        public Vector3 min;
        public Vector3 max;

        public static VectorLimit init(Vector3 a_min, Vector3 a_max)
        {
            VectorLimit data;
            data.min = a_min;
            data.max = a_max;
            return data;
        }

        public Vector3 size => max - min;

        public bool Between(Vector3 value)
        {
            return (value.x >= min.x) && (value.x <= max.x) &&
                (value.y >= min.y) && (value.y <= max.y) &&
                (value.z >= min.z) && (value.z <= max.z);
        }

        public Vector3 Clamp(Vector3 value)
        {
            return new Vector3(Mathf.Clamp(value.x, min.x, max.x),
                                Mathf.Clamp(value.y, min.y, max.y),
                                Mathf.Clamp(value.z, min.z, max.z));
        }
    }

    [System.Serializable]
    public struct IntLimit
    {
        public int min;
        public int max;

        public static IntLimit init(int a_min, int a_max)
        {
            IntLimit data;
            data.min = a_min;
            data.max = a_max;
            return data;
        }

        public bool isCorrect { get => min <= max; }

        public static readonly IntLimit Default = init(int.MaxValue, int.MinValue);
    }

    public interface IFreezer
    {
        bool Freeze();
    }
}