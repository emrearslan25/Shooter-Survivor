using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public enum PlayerClass
{
    Ranger,
    Melee
}

public class ClassSelectionManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject classSelectionPanel;
    public Button rangerButton;
    public Button meleeButton;
    public TextMeshProUGUI rangerDescription;
    public TextMeshProUGUI meleeDescription;
    public Button backButton;
    
    [Header("Class Info")]
    public string rangerDesc = "Ranger: Uzak mesafe savaşçısı. Mermi atar ve hızlıdır.";
    public string meleeDesc = "Melee: Yakın mesafe savaşçısı. Güçlü ama yavaştır.";
    
    public static PlayerClass selectedClass = PlayerClass.Ranger;
    
    void Start()
    {
        SetupUI();
    }
    
    void SetupUI()
    {
        // If no UI assigned, create it programmatically
        if (classSelectionPanel == null)
        {
            CreateClassSelectionUI();
        }
        else
        {
            classSelectionPanel.SetActive(false);
        }
        
        // Setup button listeners
        if (rangerButton != null)
        {
            rangerButton.onClick.AddListener(() => SelectClass(PlayerClass.Ranger));
        }
        
        if (meleeButton != null)
        {
            meleeButton.onClick.AddListener(() => SelectClass(PlayerClass.Melee));
        }
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(BackToMainMenu);
        }
        
        // Set descriptions
        if (rangerDescription != null)
        {
            rangerDescription.text = rangerDesc;
        }
        
        if (meleeDescription != null)
        {
            meleeDescription.text = meleeDesc;
        }
    }
    
    public void ShowClassSelection()
    {
        if (classSelectionPanel != null)
        {
            classSelectionPanel.SetActive(true);
        }
        
        // Pause time
        Time.timeScale = 0f;
    }
    
    public void HideClassSelection()
    {
        if (classSelectionPanel != null)
        {
            classSelectionPanel.SetActive(false);
        }
        
        // Resume time
        Time.timeScale = 1f;
    }
    
    void SelectClass(PlayerClass playerClass)
    {
        selectedClass = playerClass;
        Debug.Log($"Selected class: {playerClass}");
        
        HideClassSelection();
        
        // Start the game
        StartGame();
    }
    
    void StartGame()
    {
        Debug.Log("Starting game with class: " + selectedClass);
        
        // Check if we're already in the game scene
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            // We're already in game, just configure the player
            ConfigurePlayer();
        }
        else
        {
            // We need to load the game scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        }
    }
    
    void ConfigurePlayer()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.ConfigureForClass(selectedClass);
        }
    }
    
    void BackToMainMenu()
    {
        HideClassSelection();
        
        // Show main menu again
        MainMenu mainMenu = FindObjectOfType<MainMenu>();
        if (mainMenu != null)
        {
            // Assuming MainMenu has a method to show itself
            GameObject mainMenuPanel = GameObject.Find("MainMenu");
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(true);
            }
        }
    }
    
    // Method to create UI programmatically if needed
    void CreateClassSelectionUI()
    {
        // Create main panel
        GameObject panel = new GameObject("ClassSelectionPanel");
        panel.transform.SetParent(GameObject.Find("Canvas").transform, false);
        
        var panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        
        var rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        classSelectionPanel = panel;
        
        // Create title
        GameObject title = new GameObject("Title");
        title.transform.SetParent(panel.transform, false);
        var titleText = title.AddComponent<TextMeshProUGUI>();
        titleText.text = "Karakter Sınıfı Seçin";
        titleText.fontSize = 36;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;
        
        var titleRect = title.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 200);
        titleRect.sizeDelta = new Vector2(400, 50);
        
        // Create Ranger button
        CreateClassButton("Ranger", new Vector2(-150, 0), PlayerClass.Ranger);
        
        // Create Melee button
        CreateClassButton("Melee", new Vector2(150, 0), PlayerClass.Melee);
        
        // Create Back button
        CreateBackButton();
    }
    
    void CreateClassButton(string className, Vector2 position, PlayerClass playerClass)
    {
        GameObject buttonObj = new GameObject(className + "Button");
        buttonObj.transform.SetParent(classSelectionPanel.transform, false);
        
        var button = buttonObj.AddComponent<Button>();
        var image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.6f, 1f, 0.8f);
        
        var rect = buttonObj.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(200, 100);
        
        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        var text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = className;
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Set button reference
        if (playerClass == PlayerClass.Ranger)
        {
            rangerButton = button;
        }
        else
        {
            meleeButton = button;
        }
        
        button.onClick.AddListener(() => SelectClass(playerClass));
    }
    
    void CreateBackButton()
    {
        GameObject buttonObj = new GameObject("BackButton");
        buttonObj.transform.SetParent(classSelectionPanel.transform, false);
        
        backButton = buttonObj.AddComponent<Button>();
        var image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.8f, 0.2f, 0.2f, 0.8f);
        
        var rect = buttonObj.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0, -150);
        rect.sizeDelta = new Vector2(150, 50);
        
        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        var text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "Geri";
        text.fontSize = 20;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        backButton.onClick.AddListener(BackToMainMenu);
    }
}