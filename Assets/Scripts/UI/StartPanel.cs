using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class StartPanel : MonoBehaviour {
    [SerializeField] private TMP_Text statusText;

    public Panel Panel;

    private void Awake() {
        Panel.OnFullyOpened += async () => await TryInitialize();
    }

    private async Task TryInitialize() {
        statusText.text = "Signing in...";
        try {
            await Services.Instance.Initialize();

            LoadSave();
            Panel.Close();
            GameManager.Instance.GameStateChanged(GameState.InMainMenu);
        }
        catch (Exception e) {
            Debug.LogException(e);
            PromptManager.Instance.CreatePrompt("Connection Error", $"Could not sign in to services. Please try again later", null, "Retry", "Offline Mode", null, async () => await TryInitialize(), () => {
                Services.Instance.InitializeOffline();

                LoadSave();
                Panel.Close();
                GameManager.Instance.GameStateChanged(GameState.InMainMenu);
            });
        }
    }

    private void LoadSave() {
        statusText.text = "Loading save data...";
        SaveManager.LoadProfile();

        if (!SaveManager.LoadSettings())
            SettingsManager.Instance.ApplyDefaultSettings();
    }
}
