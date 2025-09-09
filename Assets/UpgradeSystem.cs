using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeSystem : MonoBehaviour
{
    [System.Serializable]
    public class UpgradeOption
    {
        public string name;
        public string description;
        public Sprite icon;
        public UpgradeType type;
        public float value;
        public bool isWeaponUpgrade;
        public string weaponName;
    }

    public enum UpgradeType
    {
        Health,
        Speed,
        Damage,
        FireRate,
        PickupRange,
        NewWeapon,
        WeaponUpgrade
    }

    [Header("Upgrade Settings")]
    public UpgradeOption[] availableUpgrades;
    public int upgradesPerLevel = 3;

    [Header("UI")]
    public GameObject upgradePanel;
    public Button[] upgradeButtons;
    public Text[] upgradeNames;
    public Text[] upgradeDescriptions;
    public Image[] upgradeIcons;

    // State
    private List<UpgradeOption> currentOptions = new List<UpgradeOption>();
    private PlayerController player;
    private WeaponSystem weaponSystem;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        weaponSystem = FindObjectOfType<WeaponSystem>();

        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }

    public void ShowUpgradeOptions()
    {
        currentOptions.Clear();

        // Select random upgrades
        List<UpgradeOption> availableOptions = new List<UpgradeOption>(availableUpgrades);

        for (int i = 0; i < Mathf.Min(upgradesPerLevel, availableOptions.Count); i++)
        {
            int randomIndex = Random.Range(0, availableOptions.Count);
            currentOptions.Add(availableOptions[randomIndex]);
            availableOptions.RemoveAt(randomIndex);
        }

        // Update UI
        UpdateUpgradeUI();

        // Show panel
        if (upgradePanel != null)
            upgradePanel.SetActive(true);

        // Pause game
        Time.timeScale = 0f;
    }

    void UpdateUpgradeUI()
    {
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (i < currentOptions.Count)
            {
                UpgradeOption option = currentOptions[i];

                // Update button text
                if (upgradeNames[i] != null)
                    upgradeNames[i].text = option.name;

                if (upgradeDescriptions[i] != null)
                    upgradeDescriptions[i].text = option.description;

                if (upgradeIcons[i] != null && option.icon != null)
                    upgradeIcons[i].sprite = option.icon;

                // Set button listener
                int index = i; // Capture for lambda
                upgradeButtons[i].onClick.RemoveAllListeners();
                upgradeButtons[i].onClick.AddListener(() => SelectUpgrade(index));

                upgradeButtons[i].gameObject.SetActive(true);
            }
            else
            {
                upgradeButtons[i].gameObject.SetActive(false);
            }
        }
    }

    void SelectUpgrade(int index)
    {
        if (index >= currentOptions.Count) return;

        UpgradeOption selectedUpgrade = currentOptions[index];
        ApplyUpgrade(selectedUpgrade);

        // Hide panel and resume game
        if (upgradePanel != null)
            upgradePanel.SetActive(false);

        Time.timeScale = 1f;

        // Notify game manager
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.ResumeGame();
        }
    }

    void ApplyUpgrade(UpgradeOption upgrade)
    {
        switch (upgrade.type)
        {
            case UpgradeType.Health:
                if (player != null)
                {
                    player.Heal(upgrade.value);
                    player.maxHealth += upgrade.value;
                }
                break;

            case UpgradeType.Speed:
                if (player != null)
                {
                    player.moveSpeed += upgrade.value;
                }
                break;

            case UpgradeType.Damage:
                // Global damage increase for all weapons
                if (weaponSystem != null)
                {
                    WeaponSystem.WeaponData[] weapons = weaponSystem.GetActiveWeapons();
                    foreach (WeaponSystem.WeaponData weapon in weapons)
                    {
                        weapon.damage += upgrade.value;
                    }
                }
                break;

            case UpgradeType.FireRate:
                // Global fire rate increase
                if (weaponSystem != null)
                {
                    WeaponSystem.WeaponData[] weapons = weaponSystem.GetActiveWeapons();
                    foreach (WeaponSystem.WeaponData weapon in weapons)
                    {
                        weapon.fireRate += upgrade.value;
                    }
                }
                break;

            case UpgradeType.PickupRange:
                if (player != null)
                {
                    player.pickupRange += upgrade.value;
                }
                break;

            case UpgradeType.NewWeapon:
                if (weaponSystem != null && !string.IsNullOrEmpty(upgrade.weaponName))
                {
                    weaponSystem.UnlockWeapon(upgrade.weaponName);
                }
                break;

            case UpgradeType.WeaponUpgrade:
                if (weaponSystem != null && !string.IsNullOrEmpty(upgrade.weaponName))
                {
                    weaponSystem.UpgradeWeapon(upgrade.weaponName);
                }
                break;
        }

        Debug.Log($"Applied upgrade: {upgrade.name}");
    }

    // Method to be called by GameManager when leveling up
    public void OnLevelUp()
    {
        ShowUpgradeOptions();
    }
}
