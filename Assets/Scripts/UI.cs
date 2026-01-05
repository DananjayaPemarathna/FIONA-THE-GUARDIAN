using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class UI : MonoBehaviour
{
    public static UI instance;

    [Header("Game Objects to Disable")]
    public GameObject inGameUI;
    public GameObject player;

    [Header("Player UI")]
    public Slider playerHealthSlider;
    public TextMeshProUGUI playerNameText;

    [Header("Object to Protect UI")]
    public Slider objectHealthSlider;

    [Header("Game Panels")]
    public GameObject gameOverUI;

    [Header("Stats")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI killCountText;
    public int enemiesToWin = 25;

    [Header("Cutscene Settings")]
    public GameObject cutsceneCanvas;
    public VideoPlayer videoPlayer;

    [Header("Super Power UI")]
    public Slider superPowerSlider;
    public GameObject superPowerReadyText;   // ✅ stays GameObject (your current setup)
    public int killsToFillSuperPower = 15;

    private int enemiesKilled = 0;
    private float timeElapsed;
    private bool levelEnded = false;

    private bool superPowerReady = false;

    public bool IsLevelEnded => levelEnded;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (cutsceneCanvas != null) cutsceneCanvas.SetActive(false);
        if (inGameUI != null) inGameUI.SetActive(true);

        Time.timeScale = 1f;

        enemiesKilled = 0;
        timeElapsed = 0f;
        levelEnded = false;

        UpdateKillCountUI();

        string savedName = PlayerPrefs.GetString("SavedPlayerName", "GUARDIAN");
        if (playerNameText != null) playerNameText.text = savedName;

        // ✅ Super power start state
        superPowerReady = false;

        if (superPowerSlider != null)
            superPowerSlider.value = 0f;

        if (superPowerReadyText != null)
            superPowerReadyText.SetActive(false); // ✅ hidden at start
    }

    private void Update()
    {
        if (levelEnded) return;

        // ✅ Activate superpower only when ready
        if (superPowerReady && Input.GetKeyDown(KeyCode.F))
        {
            if (superPowerReadyText != null)
                superPowerReadyText.SetActive(false);

            LevelComplete();
            return;
        }

        // Timer
        timeElapsed += Time.deltaTime;
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeElapsed / 60F);
            int seconds = Mathf.FloorToInt(timeElapsed % 60F);
            timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
        }
    }

    public void UpdatePlayerHealth(float currentHealth, float maxHealth)
    {
        if (playerHealthSlider != null)
            playerHealthSlider.value = currentHealth / maxHealth;
    }

    public void UpdateObjectHealth(float currentHealth, float maxHealth)
    {
        if (objectHealthSlider != null)
            objectHealthSlider.value = currentHealth / maxHealth;
    }

    public void AddKillCount()
    {
        if (levelEnded) return;

        enemiesKilled++;
        UpdateKillCountUI();

        float fill = (float)enemiesKilled / killsToFillSuperPower;
        fill = Mathf.Clamp01(fill);

        if (superPowerSlider != null)
            superPowerSlider.value = fill;

        // ✅ show READY only when FULL
        if (!superPowerReady && fill >= 1f)
        {
            superPowerReady = true;

            if (superPowerReadyText != null)
                superPowerReadyText.SetActive(true);
        }
    }

    void UpdateKillCountUI()
    {
        if (killCountText != null)
            killCountText.text = enemiesKilled + " / " + enemiesToWin;
    }

    public void TriggerGameOver()
    {
        if (levelEnded) return;
        levelEnded = true;

        if (AudioManager.instance != null)
            AudioManager.instance.PlayGameOverSFX();

        if (gameOverUI != null)
            gameOverUI.SetActive(true);

        Time.timeScale = 0f;
    }

    public void LevelComplete()
    {
        if (levelEnded) return;
        levelEnded = true;

        if (player != null) player.SetActive(false);

        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in allEnemies)
            enemy.gameObject.SetActive(false);

        if (inGameUI != null) inGameUI.SetActive(false);

        Time.timeScale = 0f;

        if (cutsceneCanvas != null && videoPlayer != null)
        {
            cutsceneCanvas.SetActive(true);
            videoPlayer.Play();
            videoPlayer.loopPointReached += OnVideoEnd;
        }
        else
        {
            GoToMainMenu();
        }
        // ✅ Stop background music when cutscene starts
        if (AudioManager.instance != null)
            AudioManager.instance.StopMusic();

    }

    void OnVideoEnd(VideoPlayer vp)
    {
        vp.loopPointReached -= OnVideoEnd;
        GoToMainMenu();
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit!");
    }
}
