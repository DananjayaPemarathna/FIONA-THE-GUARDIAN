using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

/// <summary>
/// Central UI Manager (Singleton-style) responsible for:
/// - Updating health UI (Player + Object to Protect)
/// - Tracking level timer and kill count
/// - Handling Game Over and Level Complete states
/// - Playing an ending cutscene (VideoPlayer) and returning to Main Menu
/// - Providing public button functions for UI buttons (Restart, Quit, Main Menu)
/// </summary>
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

    // Runtime tracking
    private int enemiesKilled = 0;
    private float timeElapsed;
    private bool levelEnded = false;

    private void Awake()
    {
        // Basic singleton pattern:
        // Ensures there is a globally accessible UI instance in the scene.
        if (instance == null) instance = this;
    }

    private void Start()
    {
        // Initial UI state setup
        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (cutsceneCanvas != null) cutsceneCanvas.SetActive(false);
        if (inGameUI != null) inGameUI.SetActive(true);

        // Ensure background music restarts correctly when reloading scenes.
        RestartMusic();

        // Resume game time (important if coming from paused states)
        Time.timeScale = 1f;

        // Initialize stats UI
        UpdateKillCountUI();

        // Load and display saved player name (fallback to "GUARDIAN" if not found)
        string savedName = PlayerPrefs.GetString("SavedPlayerName", "GUARDIAN");
        if (playerNameText != null)
        {
            playerNameText.text = savedName;
        }
    }

    /// <summary>
    /// Restarts any looping audio sources that are currently not playing.
    /// Useful when a scene reload stops some AudioSources unexpectedly.
    /// </summary>
    void RestartMusic()
    {
        AudioSource[] allAudio = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource audio in allAudio)
        {
            if (audio.loop && !audio.isPlaying)
            {
                audio.Play();
            }
        }
    }

    private void Update()
    {
        // Once the level ends (win/lose), stop updating timer and other runtime UI.
        if (levelEnded) return;

        // Track elapsed time and format it as M:SS
        timeElapsed += Time.deltaTime;

        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeElapsed / 60F);
            int seconds = Mathf.FloorToInt(timeElapsed % 60F);
            timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
        }
    }

    /// <summary>
    /// Updates the Player health bar using normalized values (0 to 1).
    /// </summary>
    public void UpdatePlayerHealth(float currentHealth, float maxHealth)
    {
        if (playerHealthSlider != null)
            playerHealthSlider.value = currentHealth / maxHealth;
    }

    /// <summary>
    /// Updates the protected object health bar using normalized values (0 to 1).
    /// </summary>
    public void UpdateObjectHealth(float currentHealth, float maxHealth)
    {
        if (objectHealthSlider != null)
            objectHealthSlider.value = currentHealth / maxHealth;
    }

    /// <summary>
    /// Increments kill count and checks win condition.
    /// Called from Enemy.Die().
    /// </summary>
    public void AddKillCount()
    {
        if (levelEnded) return;

        enemiesKilled++;
        UpdateKillCountUI();

        // Trigger level complete when kill target is reached.
        if (enemiesKilled >= enemiesToWin)
        {
            LevelComplete();
        }
    }

    /// <summary>
    /// Refreshes the kill count label (e.g., "5 / 25").
    /// </summary>
    void UpdateKillCountUI()
    {
        if (killCountText != null)
            killCountText.text = enemiesKilled + " / " + enemiesToWin;
    }

    /// <summary>
    /// Handles level completion:
    /// - Disables Player and all active enemies
    /// - Stops background audio (except the cutscene video player's audio)
    /// - Hides in-game UI
    /// - Pauses time and plays ending cutscene video
    /// - Returns to main menu when the video finishes
    /// </summary>
    public void LevelComplete()
    {
        if (levelEnded) return;
        levelEnded = true;

        // Disable player on win.
        if (player != null) player.SetActive(false);

        // Disable all enemies remaining in the scene (prevents frozen/bugged enemies staying visible).
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in allEnemies)
        {
            enemy.gameObject.SetActive(false);
        }

        // Stop all background audio sources (except the cutscene's own audio source, if any).
        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource audio in allAudioSources)
        {
            if (videoPlayer != null && audio.gameObject != videoPlayer.gameObject)
            {
                audio.Stop();
            }
        }

        // Hide gameplay UI.
        if (inGameUI != null) inGameUI.SetActive(false);

        // Pause the game world during cutscene.
        Time.timeScale = 0f;

        // If cutscene is configured, play it. Otherwise, fallback to main menu.
        if (cutsceneCanvas != null && videoPlayer != null)
        {
            cutsceneCanvas.SetActive(true);

            // Subscribe before/after play to ensure we capture end event reliably.
            videoPlayer.loopPointReached += OnVideoEnd;
            videoPlayer.Play();
        }
        else
        {
            GoToMainMenu();
        }
    }

    /// <summary>
    /// Called automatically when the VideoPlayer reaches its end.
    /// </summary>
    void OnVideoEnd(VideoPlayer vp)
    {
        vp.loopPointReached -= OnVideoEnd;
        GoToMainMenu();
    }

    // -----------------------------
    // BUTTON FUNCTIONS (UI Buttons)
    // -----------------------------

    /// <summary>
    /// Loads the Main Menu scene.
    /// </summary>
    public void GoToMainMenu()
    {
        // Always restore time scale when leaving paused states.
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Restarts the current level.
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Quits the application (works in build, not in Unity Editor).
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit!"); // Visible in Editor only.
    }

    /// <summary>
    /// Enables the Game Over UI and pauses gameplay.
    /// Called when the Player or ObjectToProtect dies.
    /// </summary>
    public void EnableGameOverUI()
    {
        if (levelEnded) return;
        levelEnded = true;

        if (gameOverUI != null) gameOverUI.SetActive(true);

        // Pause gameplay on game over.
        Time.timeScale = 0f;
    }
}
