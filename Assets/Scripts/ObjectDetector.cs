using System;
using UnityEngine;

public class ObjectDetector : MonoBehaviour
{
    public Action<Collider, bool> onChange;

    private void OnTriggerEnter(Collider col)
    {
        onChange?.Invoke(col, true);
    }

    private void OnTriggerExit(Collider col)
    {
        onChange?.Invoke(col, false);
    }
}