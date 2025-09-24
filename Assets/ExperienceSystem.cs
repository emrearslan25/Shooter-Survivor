using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExperienceSystem : MonoBehaviour
{
    [Header("Experience Settings")]
    public int maxLevel = 50;
    public float baseXPRequirement = 100f;
    public float xpMultiplier = 1.2f;

    [Header("UI")]
    public TMP_Text levelText;
    public TMP_Text xpText;
    public Slider xpBar;

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
    if (UIManager.Instance != null) UIManager.Instance.PulseXP();

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
        {
            xpText.text = $"{Mathf.FloorToInt(currentXP)} / {Mathf.FloorToInt(xpToNextLevel)} XP";
            // Prevent text scaling issues by resetting scale
            if (xpText.transform.localScale != Vector3.one)
                xpText.transform.localScale = Vector3.one;
        }

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
