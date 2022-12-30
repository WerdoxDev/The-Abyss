using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
using System;

public class TabGroup : MonoBehaviour {
    [Tooltip("Important for event system to know which tab group this is")]
    public TabGroupIndex GroupIndex;
    [SerializeField] private bool resetOnDisable;

    private event Action OnConfirmTabChange;
    public event Action<Action> OnBeforeTabChange;

    private List<Tab> _tabs;
    private Tab _activeTab;
    private int _activeTabIndex;

    private void OnDisable() {
        if (resetOnDisable) OnTabSelected(_tabs.FirstOrDefault(x => x.DefaultTab));
    }

    private void Awake() {
        UIManager.Instance.AddTabGroup(this);
    }

    private void Start() {
        ResetTabs();
    }

    public void AddTab(Tab tab) {
        if (_tabs == null) _tabs = new List<Tab>();

        if (tab.DefaultTab) OnTabSelected(tab);
        _tabs.Add(tab);
    }

    public void OnTabEnter(Tab tab) {
        if (tab.name == _activeTab?.name) {
            tab.SetState(TabState.ActiveHover);
            return;
        }

        ResetTabs();
        tab.SetState(TabState.Hover);
    }

    public void OnTabExit(Tab tab) {
        ResetTabs();
    }

    public void OnTabSelected(Tab tab, bool setPanel = true) {
        if (_activeTab == tab) return;
        OnConfirmTabChange = () => {
            _activeTab = tab;
            _activeTabIndex = GetTabIndex(tab);
            ResetTabs();
            _activeTab.SetState(TabState.ActiveHover);
            UIManager.Instance.TabChanged(GroupIndex, tab);
        };

        if (OnBeforeTabChange == null) OnConfirmTabChange?.Invoke();
        else OnBeforeTabChange?.Invoke(OnConfirmTabChange);
    }

    public void SelectNextTab() {
        _activeTabIndex++;
        if (_activeTabIndex == _tabs.Count) _activeTabIndex = 0;

        SelectTabByIndex(_activeTabIndex);
    }

    public void SelectPreviousTab() {
        _activeTabIndex--;
        if (_activeTabIndex < 0) _activeTabIndex = _tabs.Count - 1;

        SelectTabByIndex(_activeTabIndex);
    }

    public void SetBeforeTabChange(Action<Action> onBeforeTabChange) => OnBeforeTabChange = onBeforeTabChange;

    public void SelectTabByName(string name) {
        Tab tab = _tabs.FirstOrDefault(x => x.name == name);
        if (tab == null) return;

        OnTabSelected(tab, false);
    }

    public void SelectTabByIndex(int index) {
        Tab tab = _tabs.FirstOrDefault(x => x.name == transform.GetChild(index).name);
        if (tab == null) return;

        _activeTab = tab;
        ResetTabs();
        EventSystem.current.SetSelectedGameObject(tab.gameObject);
        _activeTab.SetState(TabState.ActiveHover);
        UIManager.Instance.TabChanged(GroupIndex, tab);
    }

    public void ResetTabs() {
        foreach (Tab tab in _tabs)
            if (tab.name != _activeTab?.name)
                tab.SetState(TabState.Inactive);
            else
                tab.SetState(TabState.Active);
    }

    private int GetTabIndex(Tab tab) {
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).name == tab.name) return i;
        }

        return -1;
    }
}
