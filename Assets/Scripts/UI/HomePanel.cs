using UnityEngine;

public class HomePanel : MonoBehaviour {
    [SerializeField] private CustomButton multiplayerButton;
    [SerializeField] private CustomButton profileButton;
    [SerializeField] private CustomButton exitButton;

    private void Awake() {
        multiplayerButton.OnClick += () => UIManager.Instance.ChangeTab(TabGroupIndex.Menu, "Online");
        profileButton.OnClick += () => UIManager.Instance.ChangeTab(TabGroupIndex.Menu, "Profile");
        exitButton.OnClick += () => Application.Quit();
    }
}
