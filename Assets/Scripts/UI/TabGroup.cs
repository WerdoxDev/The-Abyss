using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class TabGroup : MonoBehaviour
{
    [Tooltip("Important for event system to know which tab group this is")]
    public TabGroupIndex GroupIndex;
    [SerializeField] private bool resetOnDisable;

    private List<Tab> _tabs;
    private Tab _activeTab;

    private void OnDisable()
    {
        if (resetOnDisable) OnTabSelected(_tabs.FirstOrDefault(x => x.DefaultTab));
    }

    private void Awake()
    {
        UIManager.Instance.OnManualTabChanged += (groupIndex, name) =>
        {
            if (GroupIndex == groupIndex) SelectTabByName(name);
        };
    }

    private void Start()
    {
        ResetTabs();
    }

    public void Subscribe(Tab tab)
    {
        if (_tabs == null) _tabs = new List<Tab>();

        if (tab.DefaultTab) OnTabSelected(tab);
        _tabs.Add(tab);
    }

    public void OnTabEnter(Tab tab)
    {
        if (tab.name == _activeTab?.name) return;

        ResetTabs();
        tab.SetState(TabState.Hover);
    }

    public void OnTabExit(Tab tab)
    {
        ResetTabs();
    }

    public void OnTabSelected(Tab tab, bool setPanel = true)
    {
        _activeTab = tab;
        ResetTabs();
        _activeTab.SetState(TabState.Active);
        UIManager.Instance.TabChanged(GroupIndex, tab);
    }

    public void SelectTabByName(string name)
    {
        Tab tab = _tabs.FirstOrDefault(x => x.name == name);

        if (tab == null) return;
        OnTabSelected(tab, false);
    }

    public void ResetTabs()
    {
        foreach (Tab tab in _tabs)
            if (tab.name != _activeTab?.name)
                tab.SetState(TabState.Inactive);
    }
}

