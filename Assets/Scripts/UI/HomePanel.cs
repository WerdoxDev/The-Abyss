using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomePanel : MonoBehaviour {
    [SerializeField] private CustomButton multiplayerButton;
    [SerializeField] private CustomButton profileButton;

    private void Awake() {
        multiplayerButton.OnClick += () => UIManager.Instance.ChangeTab(TabGroupIndex.Menu, "Online");
        profileButton.OnClick += () => UIManager.Instance.ChangeTab(TabGroupIndex.Menu, "Profile");
    }
}
