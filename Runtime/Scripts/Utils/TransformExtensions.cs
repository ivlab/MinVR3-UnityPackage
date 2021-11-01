using UnityEngine;


public static class TransformExtensions
{

    // ok
    public static void TranslateByLocalVector(this Transform t, Vector3 deltaTrans)
    {
        t.position += t.TransformVector(deltaTrans);
    }

    // ok
    public static void TranslateByWorldVector(this Transform t, Vector3 deltaTrans)
    {
        t.position += deltaTrans;
    }

    // ok
    public static void RotateAroundLocalOrigin(this Transform t, Quaternion deltaRot)
    {
        t.localRotation = t.localRotation * deltaRot;
    }

    // ok
    public static void RotateAroundWorldOrigin(this Transform t, Quaternion deltaRot)
    {
        t.position = deltaRot * t.position;
        t.rotation = t.rotation * (Quaternion.Inverse(t.rotation) * deltaRot * t.rotation);
    }

    // ok
    public static void RotateAroundLocalPoint(this Transform t, Vector3 point, Quaternion deltaRot)
    {
        float angle;
        Vector3 localAxis;
        deltaRot.ToAngleAxis(out angle, out localAxis);
        Vector3 worldAxis = t.TransformDirection(localAxis);
        Vector3 worldPoint = t.TransformPoint(point);
        t.RotateAround(worldPoint, worldAxis, angle); // unity built-in works with world axis angle
    }

    // ok
    public static void RotateAroundWorldPoint(this Transform t, Vector3 point, Quaternion deltaRot)
    {
        Vector3 worldPos = t.position;
        Vector3 dif = worldPos - point;
        dif = deltaRot * dif;
        worldPos = point + dif;
        t.position = worldPos;
        t.rotation = t.rotation * (Quaternion.Inverse(t.rotation) * deltaRot * t.rotation);
    }

    // ok
    public static void ScaleAroundLocalOrigin(this Transform t, float deltaScale)
    {
        t.localScale = t.localScale * deltaScale;
    }


    public static void ScaleAroundWorldOrigin(this Transform t, float deltaScale)
    {
        t.ScaleAroundWorldPoint(Vector3.zero, deltaScale);
    }

    // ok
    public static void ScaleAroundLocalPoint(this Transform t, Vector3 point, float deltaScale)
    {
        Vector3 pointAfterScale = point * deltaScale;
        Vector3 diff = point - pointAfterScale;
        t.TranslateByLocalVector(diff);
        t.localScale = t.localScale * deltaScale;
    }

    // ok
    public static void ScaleAroundWorldPoint(this Transform t, Vector3 point, float deltaScale)
    {
        t.ScaleAroundLocalPoint(t.InverseTransformPoint(point), deltaScale);
    }

}
