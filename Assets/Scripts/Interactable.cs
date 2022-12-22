using UnityEngine;

public interface Interactable
{
    InteractHandler GetHandler(byte handlerData);
}
