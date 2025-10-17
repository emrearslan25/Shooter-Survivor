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
        // Basic upgrades (both classes)
        Health,
        Speed,
        Damage,
        FireRate,
        PickupRange,
        NewWeapon,
        WeaponUpgrade,
        Shield,
        
        // RANGER SKILLS
        DualShot,
        ShieldExpansion,
        OrbitingSphere,
        CriticalHit,
        ExplosiveShots,
        FreezingShots,
        LifeSteal,
        SpeedBoost,
        PoisonAura,
        RicochetShots,
        TimeStop,
        MultipleOrbs,
        ShieldReflection,
        // Ranger sub-skills
        CriticalDamage,
        CriticalChance,
        ExplosionRadius,
        ExplosionDamage,
        FreezeChance,
        FreezeDuration,
        LifeStealAmount,
        LifeStealRange,
        SpeedBoostDuration,
        SpeedBoostThreshold,
        PoisonDamage,
        PoisonRadius,
        RicochetCount,
        RicochetRange,
        TimeStopDuration,
        TimeStopChance,
        OrbDamage,
        OrbSpeed,
        ShieldDamage,
        ShieldRadius,
        
        // MELEE EXCLUSIVE SKILLS
        Berserker,             // Attack speed increases when health is low
        GroundSlam,            // AOE slam attack
        BloodThirst,           // Kill enemies to gain damage boost
        IronSkin,              // Reduce damage taken
        Charge,                // Dash through enemies
        Whirlwind,             // Spin attack hitting all around
        Intimidation,          // Enemies move slower near you
        Regeneration,          // Passive health regeneration
        DoubleStrike,          // Chance to attack twice
        Earthquake,            // Slam creates shockwaves
        
        // Melee sub-skills
        BerserkerRage,         // Berserker effect enhanced
        BerserkerDuration,     // Berserker lasts longer
        SlamRadius,            // Ground slam area increase
        SlamDamage,            // Ground slam damage increase
        BloodThirstStacks,     // More stacks of blood thirst
        BloodThirstDuration,   // Blood thirst lasts longer
        IronSkinArmor,         // More damage reduction
        IronSkinReflection,    // Reflect damage back
        ChargeDistance,        // Charge goes further
        ChargeDamage,          // Charge does more damage
        WhirlwindSpeed,        // Whirlwind spins faster
        WhirlwindDuration,     // Whirlwind lasts longer
        IntimidationRadius,    // Intimidation affects larger area
        IntimidationSlow,      // Enemies slowed more
        RegenRate,             // Regenerate health faster
        RegenCombat,           // Regenerate even in combat
        DoubleStrikeChance,    // Higher chance for double strike
        DoubleStrikeCrit,      // Double strikes can crit
        EarthquakeWaves,       // More earthquake waves
        EarthquakeRange        // Earthquake travels further
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
    
    // Track unlocked skills and taken upgrades
    private HashSet<UpgradeType> unlockedSkills = new HashSet<UpgradeType>();
    private HashSet<UpgradeType> takenUpgrades = new HashSet<UpgradeType>();

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
            
            // Class-based skills
            if (player != null && player.playerClass == PlayerClass.Ranger)
            {
                AddRangerSkills(pool);
            }
            else if (player != null && player.playerClass == PlayerClass.Melee)
            {
                AddMeleeSkills(pool);
            }
            
            if (!unlockedSkills.Contains(UpgradeType.MultipleOrbs) && player != null && player.hasOrbitingSphere)
            {
                pool.Add(new UpgradeOption { name = "Çoklu Küreler", description = "2 ek döner küre ekler", type = UpgradeType.MultipleOrbs, value = 2 });
            }
            if (!unlockedSkills.Contains(UpgradeType.ShieldReflection) && player != null && player.hasShield)
            {
                pool.Add(new UpgradeOption { name = "Yansıtıcı Kalkan", description = "Kalkan mermileri yansıtır", type = UpgradeType.ShieldReflection, value = 1 });
            }
            
            // Sub-skills (only if main skill is unlocked)
            if (unlockedSkills.Contains(UpgradeType.CriticalHit))
            {
                pool.Add(new UpgradeOption { name = "Kritik Hasarı", description = "Kritik vuruşlar 3x hasar verir", type = UpgradeType.CriticalDamage, value = 1f });
                pool.Add(new UpgradeOption { name = "Kritik Şansı", description = "Kritik vuruş şansını +%10 artırır", type = UpgradeType.CriticalChance, value = 0.1f });
            }
            
            if (unlockedSkills.Contains(UpgradeType.ExplosiveShots))
            {
                pool.Add(new UpgradeOption { name = "Patlama Alanı", description = "Patlama yarıçapını +50% artırır", type = UpgradeType.ExplosionRadius, value = 0.75f });
                pool.Add(new UpgradeOption { name = "Patlama Hasarı", description = "Patlama hasarını +%50 artırır", type = UpgradeType.ExplosionDamage, value = 0.5f });
            }
            
            if (unlockedSkills.Contains(UpgradeType.FreezingShots))
            {
                pool.Add(new UpgradeOption { name = "Dondurucu Şansı", description = "Dondurma şansını +%30 artırır", type = UpgradeType.FreezeChance, value = 0.3f });
                pool.Add(new UpgradeOption { name = "Dondurucu Süresi", description = "Dondurma süresini 2x artırır", type = UpgradeType.FreezeDuration, value = 2f });
            }
            
            if (unlockedSkills.Contains(UpgradeType.LifeSteal))
            {
                pool.Add(new UpgradeOption { name = "Can Çalma Oranı", description = "Can çalma yüzdesini +%15 artırır", type = UpgradeType.LifeStealAmount, value = 0.15f });
                pool.Add(new UpgradeOption { name = "Alan Can Çalma", description = "Yakındaki ölümlerden de can çalar", type = UpgradeType.LifeStealRange, value = 1 });
            }
            
            if (unlockedSkills.Contains(UpgradeType.PoisonAura))
            {
                pool.Add(new UpgradeOption { name = "Zehir Hasarı", description = "Zehir hasarını +66% artırır", type = UpgradeType.PoisonDamage, value = 10f });
                pool.Add(new UpgradeOption { name = "Zehir Alanı", description = "Zehir alanını +50% genişletir", type = UpgradeType.PoisonRadius, value = 1.5f });
            }
            
            if (unlockedSkills.Contains(UpgradeType.RicochetShots))
            {
                pool.Add(new UpgradeOption { name = "Sekme Sayısı", description = "1 ek sekme hakkı", type = UpgradeType.RicochetCount, value = 1 });
                pool.Add(new UpgradeOption { name = "Sekme Menzili", description = "Sekme menzilini +40% artırır", type = UpgradeType.RicochetRange, value = 2f });
            }
            
            if (unlockedSkills.Contains(UpgradeType.TimeStop))
            {
                pool.Add(new UpgradeOption { name = "Zaman Süresi", description = "Zaman durdurma süresini +2sn artırır", type = UpgradeType.TimeStopDuration, value = 2f });
                pool.Add(new UpgradeOption { name = "Zaman Şansı", description = "Zaman durdurma şansını +%15 artırır", type = UpgradeType.TimeStopChance, value = 0.15f });
            }
            
            if (unlockedSkills.Contains(UpgradeType.MultipleOrbs))
            {
                pool.Add(new UpgradeOption { name = "Küre Hasarı", description = "Orbital küre hasarını +50% artırır", type = UpgradeType.OrbDamage, value = 5f });
                pool.Add(new UpgradeOption { name = "Küre Hızı", description = "Kürelerin dönüş hızını 2x artırır", type = UpgradeType.OrbSpeed, value = 120f });
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
                    else if (option.type == UpgradeType.OrbitingSphere || option.type == UpgradeType.MultipleOrbs)
                    {
                        upgradeNames[i].color = new Color(0.8f, 0f, 0.8f); // Purple
                    }
                    else if (option.type == UpgradeType.CriticalHit || option.type == UpgradeType.ExplosiveShots)
                    {
                        upgradeNames[i].color = Color.red; // Red for damage skills
                    }
                    else if (option.type == UpgradeType.FreezingShots || option.type == UpgradeType.TimeStop)
                    {
                        upgradeNames[i].color = Color.cyan; // Cyan for control skills
                    }
                    else if (option.type == UpgradeType.LifeSteal || option.type == UpgradeType.SpeedBoost)
                    {
                        upgradeNames[i].color = Color.green; // Green for utility skills
                    }
                    else if (option.type == UpgradeType.PoisonAura)
                    {
                        upgradeNames[i].color = new Color(0.5f, 1f, 0f); // Lime for poison
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
                
            case UpgradeType.CriticalHit:
                if (player != null)
                {
                    player.criticalChance += upgrade.value; // +20% crit chance
                }
                break;
                
            case UpgradeType.ExplosiveShots:
                if (player != null)
                {
                    player.hasExplosiveShots = true;
                }
                break;
                
            case UpgradeType.FreezingShots:
                if (player != null)
                {
                    player.hasFreezingShots = true;
                }
                break;
                
            case UpgradeType.LifeSteal:
                if (player != null)
                {
                    player.lifeStealPercent += upgrade.value; // +10% life steal
                }
                break;
                
            case UpgradeType.SpeedBoost:
                if (player != null)
                {
                    player.hasSpeedBoost = true;
                }
                break;
                
            case UpgradeType.PoisonAura:
                if (player != null)
                {
                    player.hasPoisonAura = true;
                    player.CreatePoisonAura();
                }
                break;
                
            case UpgradeType.RicochetShots:
                if (player != null)
                {
                    player.ricochetCount += (int)upgrade.value; // +3 ricochets
                }
                break;
                
            case UpgradeType.TimeStop:
                if (player != null)
                {
                    player.hasTimeStop = true;
                    // Small chance to activate immediately
                    if (Random.value < 0.3f)
                    {
                        player.ActivateTimeStop();
                    }
                }
                break;
                
            case UpgradeType.MultipleOrbs:
                if (player != null)
                {
                    player.CreateAdditionalOrbs((int)upgrade.value);
                    unlockedSkills.Add(UpgradeType.MultipleOrbs);
                    takenUpgrades.Add(UpgradeType.MultipleOrbs);
                }
                break;
                
            case UpgradeType.ShieldReflection:
                if (player != null)
                {
                    player.hasShieldReflection = true;
                    unlockedSkills.Add(UpgradeType.ShieldReflection);
                    takenUpgrades.Add(UpgradeType.ShieldReflection);
                }
                break;
                
            // Sub-skills
            case UpgradeType.CriticalDamage:
                if (player != null)
                {
                    player.criticalMultiplier = 3f; // Upgrade from 2x to 3x
                    takenUpgrades.Add(UpgradeType.CriticalDamage);
                }
                break;
                
            case UpgradeType.CriticalChance:
                if (player != null)
                {
                    player.criticalChance += upgrade.value;
                    takenUpgrades.Add(UpgradeType.CriticalChance);
                }
                break;
                
            case UpgradeType.ExplosionRadius:
                if (player != null)
                {
                    player.explosionRadius += upgrade.value;
                    takenUpgrades.Add(UpgradeType.ExplosionRadius);
                }
                break;
                
            case UpgradeType.ExplosionDamage:
                if (player != null)
                {
                    player.explosionDamageMultiplier = 1f + upgrade.value;
                    takenUpgrades.Add(UpgradeType.ExplosionDamage);
                }
                break;
                
            case UpgradeType.FreezeChance:
                if (player != null)
                {
                    player.freezeChance += upgrade.value;
                    takenUpgrades.Add(UpgradeType.FreezeChance);
                }
                break;
                
            case UpgradeType.FreezeDuration:
                if (player != null)
                {
                    player.freezeDuration *= upgrade.value;
                    takenUpgrades.Add(UpgradeType.FreezeDuration);
                }
                break;
                
            case UpgradeType.LifeStealAmount:
                if (player != null)
                {
                    player.lifeStealPercent += upgrade.value;
                    takenUpgrades.Add(UpgradeType.LifeStealAmount);
                }
                break;
                
            case UpgradeType.LifeStealRange:
                if (player != null)
                {
                    player.hasAreaLifeSteal = true;
                    takenUpgrades.Add(UpgradeType.LifeStealRange);
                }
                break;
                
            case UpgradeType.PoisonDamage:
                if (player != null)
                {
                    player.poisonDamage += upgrade.value;
                    takenUpgrades.Add(UpgradeType.PoisonDamage);
                }
                break;
                
            case UpgradeType.PoisonRadius:
                if (player != null)
                {
                    player.poisonRadius += upgrade.value;
                    takenUpgrades.Add(UpgradeType.PoisonRadius);
                }
                break;
                
            case UpgradeType.RicochetCount:
                if (player != null)
                {
                    player.ricochetCount += (int)upgrade.value;
                    takenUpgrades.Add(UpgradeType.RicochetCount);
                }
                break;
                
            case UpgradeType.RicochetRange:
                if (player != null)
                {
                    player.ricochetRange += upgrade.value;
                    takenUpgrades.Add(UpgradeType.RicochetRange);
                }
                break;
                
            case UpgradeType.TimeStopDuration:
                if (player != null)
                {
                    player.timeStopDuration += upgrade.value;
                    takenUpgrades.Add(UpgradeType.TimeStopDuration);
                }
                break;
                
            case UpgradeType.TimeStopChance:
                if (player != null)
                {
                    player.timeStopChance += upgrade.value;
                    takenUpgrades.Add(UpgradeType.TimeStopChance);
                }
                break;
                
            case UpgradeType.OrbDamage:
                if (player != null)
                {
                    player.orbDamage += upgrade.value;
                    takenUpgrades.Add(UpgradeType.OrbDamage);
                }
                break;
                
            case UpgradeType.OrbSpeed:
                if (player != null)
                {
                    player.sphereOrbitSpeed += upgrade.value;
                    takenUpgrades.Add(UpgradeType.OrbSpeed);
                }
                break;
                
            // MELEE SKILLS
            case UpgradeType.Berserker:
                if (player != null)
                {
                    player.hasBerserker = true;
                    unlockedSkills.Add(UpgradeType.Berserker);
                    takenUpgrades.Add(UpgradeType.Berserker);
                }
                break;
                
            case UpgradeType.GroundSlam:
                if (player != null)
                {
                    player.hasGroundSlam = true;
                    unlockedSkills.Add(UpgradeType.GroundSlam);
                    takenUpgrades.Add(UpgradeType.GroundSlam);
                }
                break;
                
            case UpgradeType.BloodThirst:
                if (player != null)
                {
                    player.hasBloodThirst = true;
                    unlockedSkills.Add(UpgradeType.BloodThirst);
                    takenUpgrades.Add(UpgradeType.BloodThirst);
                }
                break;
                
            case UpgradeType.IronSkin:
                if (player != null)
                {
                    player.hasIronSkin = true;
                    unlockedSkills.Add(UpgradeType.IronSkin);
                    takenUpgrades.Add(UpgradeType.IronSkin);
                }
                break;
                
            case UpgradeType.Charge:
                if (player != null)
                {
                    player.hasCharge = true;
                    unlockedSkills.Add(UpgradeType.Charge);
                    takenUpgrades.Add(UpgradeType.Charge);
                }
                break;
                
            case UpgradeType.Whirlwind:
                if (player != null)
                {
                    player.hasWhirlwind = true;
                    unlockedSkills.Add(UpgradeType.Whirlwind);
                    takenUpgrades.Add(UpgradeType.Whirlwind);
                }
                break;
                
            case UpgradeType.Intimidation:
                if (player != null)
                {
                    player.hasIntimidation = true;
                    unlockedSkills.Add(UpgradeType.Intimidation);
                    takenUpgrades.Add(UpgradeType.Intimidation);
                }
                break;
                
            case UpgradeType.Regeneration:
                if (player != null)
                {
                    player.hasRegeneration = true;
                    unlockedSkills.Add(UpgradeType.Regeneration);
                    takenUpgrades.Add(UpgradeType.Regeneration);
                }
                break;
                
            case UpgradeType.DoubleStrike:
                if (player != null)
                {
                    player.hasDoubleStrike = true;
                    unlockedSkills.Add(UpgradeType.DoubleStrike);
                    takenUpgrades.Add(UpgradeType.DoubleStrike);
                }
                break;
                
            case UpgradeType.Earthquake:
                if (player != null)
                {
                    player.hasEarthquake = true;
                    unlockedSkills.Add(UpgradeType.Earthquake);
                    takenUpgrades.Add(UpgradeType.Earthquake);
                }
                break;
                
            // Melee sub-skills
            case UpgradeType.BerserkerRage:
                if (player != null)
                {
                    player.berserkerAttackSpeedBonus += upgrade.value;
                    takenUpgrades.Add(UpgradeType.BerserkerRage);
                }
                break;
                
            case UpgradeType.SlamRadius:
                if (player != null)
                {
                    player.slamRadius += upgrade.value;
                    takenUpgrades.Add(UpgradeType.SlamRadius);
                }
                break;
                
            case UpgradeType.SlamDamage:
                if (player != null)
                {
                    player.slamDamage += upgrade.value;
                    takenUpgrades.Add(UpgradeType.SlamDamage);
                }
                break;
                
            case UpgradeType.BloodThirstStacks:
                if (player != null)
                {
                    // This will be handled in the skill logic
                    takenUpgrades.Add(UpgradeType.BloodThirstStacks);
                }
                break;
                
            case UpgradeType.BloodThirstDuration:
                if (player != null)
                {
                    player.bloodThirstDuration += upgrade.value;
                    takenUpgrades.Add(UpgradeType.BloodThirstDuration);
                }
                break;
                
            case UpgradeType.IronSkinArmor:
                if (player != null)
                {
                    player.ironSkinReduction += upgrade.value;
                    takenUpgrades.Add(UpgradeType.IronSkinArmor);
                }
                break;
                
            case UpgradeType.ChargeDistance:
                if (player != null)
                {
                    player.chargeDistance += upgrade.value;
                    takenUpgrades.Add(UpgradeType.ChargeDistance);
                }
                break;
                
            case UpgradeType.ChargeDamage:
                if (player != null)
                {
                    player.chargeDamage += upgrade.value;
                    takenUpgrades.Add(UpgradeType.ChargeDamage);
                }
                break;
                
            case UpgradeType.WhirlwindDuration:
                if (player != null)
                {
                    player.whirlwindDuration += upgrade.value;
                    takenUpgrades.Add(UpgradeType.WhirlwindDuration);
                }
                break;
                
            case UpgradeType.DoubleStrikeChance:
                if (player != null)
                {
                    player.doubleStrikeChance += upgrade.value;
                    takenUpgrades.Add(UpgradeType.DoubleStrikeChance);
                }
                break;
                
            case UpgradeType.EarthquakeWaves:
                if (player != null)
                {
                    player.earthquakeWaves += (int)upgrade.value;
                    takenUpgrades.Add(UpgradeType.EarthquakeWaves);
                }
                break;
                
            case UpgradeType.EarthquakeRange:
                if (player != null)
                {
                    player.earthquakeRange += upgrade.value;
                    takenUpgrades.Add(UpgradeType.EarthquakeRange);
                }
                break;
        }

        Debug.Log($"Applied upgrade: {upgrade.name}");
    }

    void AddRangerSkills(List<UpgradeOption> pool)
    {
        // Main Ranger skills
        if (!unlockedSkills.Contains(UpgradeType.CriticalHit))
        {
            pool.Add(new UpgradeOption { name = "Kritik Vuruş", description = "%15 şansla 2x hasar", type = UpgradeType.CriticalHit, value = 0.15f });
        }
        if (!unlockedSkills.Contains(UpgradeType.ExplosiveShots))
        {
            pool.Add(new UpgradeOption { name = "Patlayıcı Mermiler", description = "Mermiler patlayarak alan hasarı verir", type = UpgradeType.ExplosiveShots, value = 1 });
        }
        if (!unlockedSkills.Contains(UpgradeType.FreezingShots))
        {
            pool.Add(new UpgradeOption { name = "Dondurucu Atış", description = "Düşmanları yavaşlatır", type = UpgradeType.FreezingShots, value = 1 });
        }
        if (!unlockedSkills.Contains(UpgradeType.LifeSteal))
        {
            pool.Add(new UpgradeOption { name = "Can Çalma", description = "Hasar verirken can kazanır", type = UpgradeType.LifeSteal, value = 0.2f });
        }
        if (!unlockedSkills.Contains(UpgradeType.PoisonAura))
        {
            pool.Add(new UpgradeOption { name = "Zehir Aura", description = "Yakındaki düşmanlar zehirlenir", type = UpgradeType.PoisonAura, value = 1 });
        }
        if (!unlockedSkills.Contains(UpgradeType.RicochetShots))
        {
            pool.Add(new UpgradeOption { name = "Sekme Mermiler", description = "Mermiler düşmanlar arasında sekir", type = UpgradeType.RicochetShots, value = 2 });
        }
        if (!unlockedSkills.Contains(UpgradeType.TimeStop))
        {
            pool.Add(new UpgradeOption { name = "Zaman Durdurma", description = "Düşük canda zaman yavaşlar", type = UpgradeType.TimeStop, value = 1 });
        }
        
        // Ranger sub-skills
        if (unlockedSkills.Contains(UpgradeType.CriticalHit))
        {
            pool.Add(new UpgradeOption { name = "Kritik Hasarı", description = "Kritik vuruşlar 3x hasar verir", type = UpgradeType.CriticalDamage, value = 1f });
            pool.Add(new UpgradeOption { name = "Kritik Şansı", description = "Kritik vuruş şansını +%10 artırır", type = UpgradeType.CriticalChance, value = 0.1f });
        }
        
        if (unlockedSkills.Contains(UpgradeType.ExplosiveShots))
        {
            pool.Add(new UpgradeOption { name = "Patlama Alanı", description = "Patlama yarıçapını +50% artırır", type = UpgradeType.ExplosionRadius, value = 0.75f });
            pool.Add(new UpgradeOption { name = "Patlama Hasarı", description = "Patlama hasarını +%50 artırır", type = UpgradeType.ExplosionDamage, value = 0.5f });
        }
    }
    
    void AddMeleeSkills(List<UpgradeOption> pool)
    {
        // Main Melee skills (all passive)
        if (!unlockedSkills.Contains(UpgradeType.Berserker))
        {
            pool.Add(new UpgradeOption { name = "Berserker Öfkesi", description = "Düşük canda otomatik saldırı hızı artışı", type = UpgradeType.Berserker, value = 1 });
        }
        if (!unlockedSkills.Contains(UpgradeType.GroundSlam))
        {
            pool.Add(new UpgradeOption { name = "Otomatik Çarpma", description = "3+ düşmanla çevrilince otomatik alan saldırısı", type = UpgradeType.GroundSlam, value = 1 });
        }
        if (!unlockedSkills.Contains(UpgradeType.BloodThirst))
        {
            pool.Add(new UpgradeOption { name = "Kan Emici", description = "Öldürme başına kalıcı hasar bonusu (5 stack)", type = UpgradeType.BloodThirst, value = 1 });
        }
        if (!unlockedSkills.Contains(UpgradeType.IronSkin))
        {
            pool.Add(new UpgradeOption { name = "Demir Deri", description = "Sürekli %30 hasar azaltma korumas", type = UpgradeType.IronSkin, value = 1 });
        }
        if (!unlockedSkills.Contains(UpgradeType.Charge))
        {
            pool.Add(new UpgradeOption { name = "Savaş Çılgınlığı", description = "Düşük canda uzak düşmanlara otomatik atılır", type = UpgradeType.Charge, value = 1 });
        }
        if (!unlockedSkills.Contains(UpgradeType.Whirlwind))
        {
            pool.Add(new UpgradeOption { name = "Kasırga Savunma", description = "5+ düşmanla çevrilince otomatik dönerek saldırır", type = UpgradeType.Whirlwind, value = 1 });
        }
        if (!unlockedSkills.Contains(UpgradeType.Intimidation))
        {
            pool.Add(new UpgradeOption { name = "Korku Alanı", description = "Yakındaki düşmanlar sürekli yavaşlar ve hasar alır", type = UpgradeType.Intimidation, value = 1 });
        }
        if (!unlockedSkills.Contains(UpgradeType.Regeneration))
        {
            pool.Add(new UpgradeOption { name = "Hızlı İyileşme", description = "Saniyede 2 can otomatik yenilenir", type = UpgradeType.Regeneration, value = 1 });
        }
        if (!unlockedSkills.Contains(UpgradeType.DoubleStrike))
        {
            pool.Add(new UpgradeOption { name = "Çift Saldırı", description = "%25 şansla otomatik iki kez vuruş", type = UpgradeType.DoubleStrike, value = 1 });
        }
        if (!unlockedSkills.Contains(UpgradeType.Earthquake))
        {
            pool.Add(new UpgradeOption { name = "Şok Dalgaları", description = "Yer çarpması otomatik 3 dalga yaratır", type = UpgradeType.Earthquake, value = 1 });
        }
        
        // Melee sub-skills
        if (unlockedSkills.Contains(UpgradeType.Berserker))
        {
            pool.Add(new UpgradeOption { name = "Berserker Öfkesi", description = "Berserker %100 daha güçlü", type = UpgradeType.BerserkerRage, value = 1f });
            pool.Add(new UpgradeOption { name = "Berserker Süresi", description = "Berserker daha uzun sürer", type = UpgradeType.BerserkerDuration, value = 5f });
        }
        
        if (unlockedSkills.Contains(UpgradeType.GroundSlam))
        {
            pool.Add(new UpgradeOption { name = "Çarpma Alanı", description = "Yer çarpması alanını +50% artırır", type = UpgradeType.SlamRadius, value = 1.5f });
            pool.Add(new UpgradeOption { name = "Çarpma Hasarı", description = "Yer çarpması hasarını +%75 artırır", type = UpgradeType.SlamDamage, value = 30f });
        }
        
        if (unlockedSkills.Contains(UpgradeType.BloodThirst))
        {
            pool.Add(new UpgradeOption { name = "Kan Yığını", description = "Daha fazla kan yığını biriktir", type = UpgradeType.BloodThirstStacks, value = 3 });
            pool.Add(new UpgradeOption { name = "Kan Süresi", description = "Kan bonusu 2x uzun sürer", type = UpgradeType.BloodThirstDuration, value = 10f });
        }
        
        if (unlockedSkills.Contains(UpgradeType.IronSkin))
        {
            pool.Add(new UpgradeOption { name = "Zırh Kalınlığı", description = "%50 hasar azaltma", type = UpgradeType.IronSkinArmor, value = 0.2f });
            pool.Add(new UpgradeOption { name = "Hasar Yansıtma", description = "Alınan hasarın %25'ini yansıtır", type = UpgradeType.IronSkinReflection, value = 1 });
        }
        
        if (unlockedSkills.Contains(UpgradeType.Charge))
        {
            pool.Add(new UpgradeOption { name = "Hücum Mesafesi", description = "2x uzak mesafeye hücum", type = UpgradeType.ChargeDistance, value = 5f });
            pool.Add(new UpgradeOption { name = "Hücum Hasarı", description = "Hücum hasarını +%100 artırır", type = UpgradeType.ChargeDamage, value = 30f });
        }
        
        if (unlockedSkills.Contains(UpgradeType.Whirlwind))
        {
            pool.Add(new UpgradeOption { name = "Kasırga Hızı", description = "2x hızlı döner", type = UpgradeType.WhirlwindSpeed, value = 1f });
            pool.Add(new UpgradeOption { name = "Kasırga Süresi", description = "Kasırga 2x uzun sürer", type = UpgradeType.WhirlwindDuration, value = 2f });
        }
        
        if (unlockedSkills.Contains(UpgradeType.DoubleStrike))
        {
            pool.Add(new UpgradeOption { name = "Çifte Şansı", description = "Çifte vuruş şansını +%25 artırır", type = UpgradeType.DoubleStrikeChance, value = 0.25f });
            pool.Add(new UpgradeOption { name = "Çifte Kritik", description = "Çifte vuruşlar kritik olabilir", type = UpgradeType.DoubleStrikeCrit, value = 1 });
        }
        
        if (unlockedSkills.Contains(UpgradeType.Earthquake))
        {
            pool.Add(new UpgradeOption { name = "Deprem Dalgaları", description = "5 şok dalgası yaratır", type = UpgradeType.EarthquakeWaves, value = 2 });
            pool.Add(new UpgradeOption { name = "Deprem Menzili", description = "Şok dalgaları 2x uzağa gider", type = UpgradeType.EarthquakeRange, value = 6f });
        }
    }
    
    // Method to be called by GameManager when leveling up
    public void OnLevelUp()
    {
        ShowUpgradeOptions();
    }
}
