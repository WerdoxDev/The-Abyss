using UnityEngine;

public static class Utils {
    public static float Remap(float value, float from1, float from2, float to1, float to2) {
        return to1 + (value - from1) * (to2 - to1) / (from2 - from1);
    }

    public static float NormalizeAngle(float a) {
        return a - 180f * Mathf.Floor((a + 180f) / 180f);
    }

    public static float ModularClamp(float val, float min, float max, float rangemin = -180f, float rangemax = 180f) {
        var modulus = Mathf.Abs(rangemax - rangemin);
        if ((val %= modulus) < 0f) val += modulus;
        return Mathf.Clamp(val + Mathf.Min(rangemin, rangemax), min, max);
    }

    public static float ClampAngle(float current, float min, float max) {
        float dtAngle = Mathf.Abs(((min - max) + 180) % 360 - 180);
        float hdtAngle = dtAngle * 0.5f;
        float midAngle = min + hdtAngle;

        float offset = Mathf.Abs(Mathf.DeltaAngle(current, midAngle)) - hdtAngle;
        if (offset > 0)
            current = Mathf.MoveTowardsAngle(current, midAngle, offset);
        return current;
    }

}