using UnityEngine;

public static class HelperExtensions
{
    public static T GetRandomElement<T>(this T[] inArray)
    {
        return inArray[Random.Range(0, inArray.Length)];
    }

    public static float GetRandomValue(this Vector2 vector2)
    {
        return Random.Range(vector2.x, vector2.y);
    }
}
