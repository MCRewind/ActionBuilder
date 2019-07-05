using System;
using UnityEngine;

[Serializable]
public class Box : ScriptableObject
{
    public float X;
    public float Y;
    public float Width;
    public float Height;

    public Box(float x = 0, float y = 0, float width = 0, float height = 0)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}