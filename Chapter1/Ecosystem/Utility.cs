using UnityEngine;

public static class Utility
{
    public static Vector3 GetRandomPositionOnScreen()
    {
        Camera.main.orthographic = true;

        Vector2 min = Camera.main.ScreenToWorldPoint(Vector2.zero);
        Vector2 max = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));

        Vector3 location = new Vector3(
            Random.Range(min.x, max.x),
            Random.Range(min.y, max.y),
            0);

        return location;
    }
}
