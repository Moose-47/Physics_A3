using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject creditsPannel;
    public GameObject settingsPannel;
    public GameObject menuPannel;

    [Header("Buttons")]
    public Button playBtn;
    public Button settingBtn;
    public Button creditsBtn;
    public Button quitBtn;
    public Button returnFromCredBtn;
    public Button returnFromSettBtn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playBtn.onClick.AddListener(() => SceneManager.LoadScene("GameScene")); //Each Button Function per canvas
        settingBtn.onClick.AddListener(SettingsFlip);
        creditsBtn.onClick.AddListener(CreditsFlip);
        returnFromCredBtn.onClick.AddListener(BackToMain);
        returnFromSettBtn.onClick.AddListener(BackToMain);
        quitBtn.onClick.AddListener(QuitGame);

    }

    private void SettingsFlip() // to activate Settings Canvas
    {
        menuPannel.SetActive(false);
        creditsPannel.SetActive(false);
        settingsPannel.SetActive(true);

    }
    private void CreditsFlip() // Activate Credits Canvas
    {
        creditsPannel.SetActive(true);
        settingsPannel.SetActive(false);
        menuPannel.SetActive(false);
    }
    private void BackToMain() // Go back to main menu from either panel 
    {
        creditsPannel.SetActive(false);
        settingsPannel.SetActive(false);
        menuPannel.SetActive(true);
    }
    private void QuitGame() // Quit Game
    {
        Application.Quit();
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
    }
}

