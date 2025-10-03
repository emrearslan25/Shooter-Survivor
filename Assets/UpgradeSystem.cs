using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        WeaponUpgrade,
        Shield,
        DualShot,
        ShieldExpansion,
        OrbitingSphere
    }

    [Header("Upgrade Settings")]
    public UpgradeOption[] availableUpgrades;
    public int upgradesPerLevel = 3;

    [Header("UI")]
    public GameObject upgradePanel;
    public Button[] upgradeButtons;
    public TMP_Text[] upgradeNames;
    public TMP_Text[] upgradeDescriptions;
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

        // Resolve upgradePanel from UIManager if not assigned
        if (upgradePanel == null && UIManager.Instance != null)
        {
            upgradePanel = UIManager.Instance.upgradePanel;
        }

        // Show panel first so buttons exist (UIManager may auto-create them)
        if (UIManager.Instance != null && UIManager.Instance.upgradePanel != null)
        {
            UIManager.Instance.ShowUpgradePanel();
        }
        else if (upgradePanel != null)
        {
            upgradePanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("UpgradeSystem: upgradePanel is not assigned and UIManager has none; cannot show.");
        }

        // Build selection pool (fallbacks if none configured)
        List<UpgradeOption> pool = new List<UpgradeOption>();
        if (availableUpgrades != null && availableUpgrades.Length > 0)
        {
            pool.AddRange(availableUpgrades);
        }
        else
        {
            pool.Add(new UpgradeOption { name = "Can +20", description = "Maksimum can +20", type = UpgradeType.Health, value = 20 });
            pool.Add(new UpgradeOption { name = "Hız +1", description = "Hareket hızı +1", type = UpgradeType.Speed, value = 1 });
            pool.Add(new UpgradeOption { name = "Hasar +5", description = "Mermi hasarı +5", type = UpgradeType.Damage, value = 5 });
            pool.Add(new UpgradeOption { name = "Atış Hızı +1", description = "Daha hızlı ateş", type = UpgradeType.FireRate, value = 1 });
            pool.Add(new UpgradeOption { name = "Toplama +1", description = "XP çekim menzili +1", type = UpgradeType.PickupRange, value = 1 });
            
            // Special upgrades with proper checks
            if (player != null && !player.hasShield)
            {
                pool.Add(new UpgradeOption { name = "Koruyucu Kalkan", description = "Etrafında sarı kalkan oluştur", type = UpgradeType.Shield, value = 1 });
            }
            if (player != null && !player.hasDualShot)
            {
                pool.Add(new UpgradeOption { name = "Çift Atış", description = "Aynı anda 2 mermi atar", type = UpgradeType.DualShot, value = 1 });
            }
            if (player != null && player.hasShield && !player.hasExpandedShield)
            {
                pool.Add(new UpgradeOption { name = "Kalkan Genişletme", description = "Kalkan alanını büyütür", type = UpgradeType.ShieldExpansion, value = 1 });
            }
            if (player != null && !player.hasOrbitingSphere)
            {
                pool.Add(new UpgradeOption { name = "Döner Küre", description = "Etrafta dönen mor hasar küresi", type = UpgradeType.OrbitingSphere, value = 1 });
            }
        }

        // Select random upgrades
        List<UpgradeOption> availableOptions = new List<UpgradeOption>(pool);
        for (int i = 0; i < Mathf.Min(upgradesPerLevel, availableOptions.Count); i++)
        {
            int randomIndex = Random.Range(0, availableOptions.Count);
            currentOptions.Add(availableOptions[randomIndex]);
            availableOptions.RemoveAt(randomIndex);
        }

        // Update UI now that panel/buttons exist
        UpdateUpgradeUI();

        // Pause game (safety; GameManager also pauses)
        Time.timeScale = 0f;
    }

    void UpdateUpgradeUI()
    {
        // Auto-discover buttons if not provided
        if ((upgradeButtons == null || upgradeButtons.Length == 0) && upgradePanel != null)
        {
            upgradeButtons = upgradePanel.GetComponentsInChildren<Button>(true);
        }

        if (upgradeButtons == null || upgradeButtons.Length == 0)
        {
            Debug.LogWarning("UpgradeSystem: No upgrade buttons found. Assign in Inspector or place Buttons under upgradePanel.");
            return;
        }

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (i < currentOptions.Count)
            {
                UpgradeOption option = currentOptions[i];

                // Update button text
                bool wroteLabel = false;
                if (upgradeNames != null && i < upgradeNames.Length && upgradeNames[i] != null)
                {
                    upgradeNames[i].text = option.name;
                    // Make shield upgrade name yellow
                    if (option.type == UpgradeType.Shield)
                    {
                        upgradeNames[i].color = Color.yellow;
                    }
                    else if (option.type == UpgradeType.OrbitingSphere)
                    {
                        upgradeNames[i].color = new Color(0.8f, 0f, 0.8f); // Purple
                    }
                    else
                    {
                        upgradeNames[i].color = Color.black;
                    }
                    wroteLabel = true;
                }
                if (upgradeDescriptions != null && i < upgradeDescriptions.Length && upgradeDescriptions[i] != null)
                {
                    upgradeDescriptions[i].text = option.description;
                    // Make shield upgrade description yellow
                    if (option.type == UpgradeType.Shield)
                    {
                        upgradeDescriptions[i].color = Color.yellow;
                    }
                    else if (option.type == UpgradeType.OrbitingSphere)
                    {
                        upgradeDescriptions[i].color = new Color(0.8f, 0f, 0.8f); // Purple
                    }
                    else
                    {
                        upgradeDescriptions[i].color = Color.black;
                    }
                }
                if (!wroteLabel)
                {
                    var tmp = upgradeButtons[i].GetComponentInChildren<TMP_Text>(true);
                    if (tmp != null)
                    {
                        tmp.text = option.name + "\n<size=80%>" + option.description + "</size>";
                        
                        // Make shield upgrade text yellow, orbiting sphere purple
                        if (option.type == UpgradeType.Shield)
                        {
                            tmp.color = Color.yellow;
                        }
                        else if (option.type == UpgradeType.OrbitingSphere)
                        {
                            tmp.color = new Color(0.8f, 0f, 0.8f); // Purple
                        }
                        else
                        {
                            tmp.color = Color.black; // Default color for other upgrades
                        }
                    }
                }

                if (upgradeIcons != null && i < upgradeIcons.Length && upgradeIcons[i] != null && option.icon != null)
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
        {
            if (UIManager.Instance != null)
                UIManager.Instance.HideUpgradePanel();
            else
                upgradePanel.SetActive(false);
        }

        Time.timeScale = 1f;

        // Notify game manager
    GameManager gameManager = FindObjectOfType<GameManager>();
    if (gameManager != null) gameManager.ResumeGame();
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
                // Increase simple shooting damage if enabled
                if (player != null && player.IsSimpleShootingEnabled())
                {
                    player.simpleDamage += upgrade.value;
                }
                // Global damage increase for all weapons
                else if (weaponSystem != null)
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

            case UpgradeType.Shield:
                if (player != null)
                {
                    player.hasShield = true;
                    player.ActivateShield();
                }
                break;

            case UpgradeType.DualShot:
                if (player != null)
                {
                    player.hasDualShot = true;
                }
                break;

            case UpgradeType.ShieldExpansion:
                if (player != null && player.hasShield)
                {
                    player.hasExpandedShield = true;
                    player.ExpandShield();
                }
                break;

            case UpgradeType.OrbitingSphere:
                if (player != null)
                {
                    player.hasOrbitingSphere = true;
                    player.CreateOrbitingSphere();
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
