using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Main Menu Buttons")]
    public Button playButton;
    public Button quitButton;

    [Header("Settings")]
    public string gameSceneName = "Game";
    
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public ClassSelectionManager classSelectionManager;

    void Start()
    {
        // Find class selection manager if not assigned
        if (classSelectionManager == null)
        {
            classSelectionManager = FindObjectOfType<ClassSelectionManager>();
        }
        
        // Setup button listeners
        if (playButton != null)
            playButton.onClick.AddListener(ShowClassSelection);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    public void ShowClassSelection()
    {
        // Hide main menu
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
        
        // Show class selection
        if (classSelectionManager != null)
        {
            classSelectionManager.ShowClassSelection();
        }
        else
        {
            // Fallback: directly start game with Ranger class
            StartGameDirectly();
        }
    }
    
    public void StartGameDirectly()
    {
        // Set default player name and class
        GameManager.SetCurrentPlayerName("Oyuncu");
        ClassSelectionManager.selectedClass = PlayerClass.Ranger;
        
        // Load game scene
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
        
        // For editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}