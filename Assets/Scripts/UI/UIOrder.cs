using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIOrder : MonoBehaviour
{
    [SerializeField] private int order;

    public void Order()
    {
        if (!TryGetComponent<GraphicRaycaster>(out GraphicRaycaster graphicRaycaster))
            gameObject.AddComponent<GraphicRaycaster>();
        if (!TryGetComponent<Canvas>(out Canvas canvas))
            canvas = gameObject.AddComponent<Canvas>();

        canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1;
        canvas.overrideSorting = true;
        canvas.sortingOrder = order;
    }

    public void RemoveOrder()
    {
        Destroy(GetComponent<GraphicRaycaster>());
        Destroy(GetComponent<Canvas>());
    }
}
