using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperienceSystem : MonoBehaviour
{
    [Header("Experience Settings")]
    public int maxLevel = 50;
    public float baseXPRequirement = 100f;
    public float xpMultiplier = 1.2f;

    [Header("UI")]
    public UnityEngine.UI.Text levelText;
    public UnityEngine.UI.Text xpText;
    public UnityEngine.UI.Slider xpBar;

    // State
    private int currentLevel = 1;
    private float currentXP = 0f;
    private float xpToNextLevel;
    private bool shouldLevelUp = false;

    void Start()
    {
        UpdateXPRequirement();
        UpdateUI();
    }

    void Update()
    {
        UpdateUI();
    }

    public void GainExperience(float amount)
    {
        currentXP += amount;

        // Check for level up
        while (currentXP >= xpToNextLevel && currentLevel < maxLevel)
        {
            LevelUp();
        }

        UpdateUI();
    }

    void LevelUp()
    {
        currentXP -= xpToNextLevel;
        currentLevel++;
        shouldLevelUp = true;

        UpdateXPRequirement();

        Debug.Log($"Leveled up to {currentLevel}!");

        // Level up effects
        // You can add particle effects, sounds, etc. here
    }

    void UpdateXPRequirement()
    {
        xpToNextLevel = baseXPRequirement * Mathf.Pow(xpMultiplier, currentLevel - 1);
    }

    void UpdateUI()
    {
        if (levelText != null)
            levelText.text = $"Level {currentLevel}";

        if (xpText != null)
            xpText.text = $"{Mathf.FloorToInt(currentXP)} / {Mathf.FloorToInt(xpToNextLevel)} XP";

        if (xpBar != null)
            xpBar.value = currentXP / xpToNextLevel;
    }

    public bool ShouldLevelUp()
    {
        if (shouldLevelUp)
        {
            shouldLevelUp = false;
            return true;
        }
        return false;
    }

    public int GetCurrentLevel() => currentLevel;
    public float GetCurrentXP() => currentXP;
    public float GetXPToNextLevel() => xpToNextLevel;
    public float GetXPProgress() => currentXP / xpToNextLevel;
}
