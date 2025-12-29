using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Main Menu controller responsible for:
/// - Reading the player name from a TMP_InputField
/// - Saving the name using PlayerPrefs
/// - Loading the first gameplay scene
/// - Quitting the application
/// 
/// This script is typically connected to UI Button OnClick events.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    // Input field used to capture player name on the main menu.
    public TMP_InputField nameInputField;

    /// <summary>
    /// Starts the game:
    /// 1) Reads the player name from the input field
    /// 2) Applies a default name if empty
    /// 3) Saves it using PlayerPrefs
    /// 4) Loads the gameplay scene
    /// </summary>
    public void PlayGame()
    {
        string playerName = "";

        // 1) Read name from InputField (if assigned).
        if (nameInputField != null)
        {
            playerName = nameInputField.text;
        }

        // 2) If name is empty, use a default name.
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = "GUARDIAN";
        }

        // 3) Save the name for use in gameplay UI.
        PlayerPrefs.SetString("SavedPlayerName", playerName);

        // 4) Load the gameplay scene (ensure this scene name matches your Build Settings).
        SceneManager.LoadScene("SampleScene");
    }

    /// <summary>
    /// Quits the game application.
    /// Note: This works in a built game, not inside the Unity Editor.
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}
