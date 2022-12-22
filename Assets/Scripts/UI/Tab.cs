using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Tab : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public string Name;
    public bool DefaultTab;
    public TabGroup TabGroup;

    [SerializeField] private Graphic[] graphics;
    [SerializeField] private Color[] activeColors;
    [SerializeField] private Color[] inactiveColors;
    [SerializeField] private Color[] hoverColors;

    void Awake()
    {
        TabGroup.Subscribe(this);
    }

    public void SetState(TabState state)
    {
        switch (state)
        {
            case TabState.Hover:
                for (int i = 0; i < graphics.Length; i++)
                    graphics[i].color = hoverColors[i];
                break;
            case TabState.Active:
                for (int i = 0; i < graphics.Length; i++)
                    graphics[i].color = activeColors[i];
                break;
            case TabState.Inactive:
                for (int i = 0; i < graphics.Length; i++)
                    graphics[i].color = inactiveColors[i];
                break;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TabGroup.OnTabSelected(this);
    }

    public void OnPointerEnter(PointerEventData eventData) => TabGroup.OnTabEnter(this);

    public void OnPointerExit(PointerEventData eventData) => TabGroup.OnTabExit(this);
}

public enum TabState
{
    Hover, Active, Inactive
}
