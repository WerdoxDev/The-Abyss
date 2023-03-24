using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class StartPanel : MonoBehaviour {
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private GameObject[] disableOnStart;

    public Panel Panel;

    private void Awake() {
        Panel.OnFullyOpened += async () => await TryInitialize();
    }

    private async Task TryInitialize() {
        foreach (GameObject obj in disableOnStart) obj.SetActive(false);

        statusText.text = "Signing in...";
        try {
            await Services.Instance.Initialize();
            statusText.text = "Loading save data...";
            SaveManager.LoadProfile();

            if (!SaveManager.LoadSettings())
                SettingsManager.Instance.ApplyDefaultSettings();

            Panel.Close();
            GameManager.Instance.GameStateChanged(GameState.InMainMenu);

            foreach (GameObject obj in disableOnStart) obj.SetActive(true);
        }
        catch (Exception e) {
            PromptManager.Instance.CreatePrompt("Connection Error", $"Could not sign in to services. Please try again later", null, "Retry", "Offline Mode", null, () => TryInitialize(), () => Debug.Log("Offline"));
        }
    }
}
