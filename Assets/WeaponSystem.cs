using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    [System.Serializable]
    public class WeaponData
    {
        public string weaponName;
        public GameObject projectilePrefab;
        public float fireRate = 1f;
        public float projectileSpeed = 10f;
        public float damage = 10f;
        public int projectileCount = 1;
        public float spreadAngle = 0f;
        public bool isActive = false;
        public int level = 1;
    }

    [Header("Weapon Settings")]
    public WeaponData[] availableWeapons;
    public Transform firePoint;

    // State
    private Dictionary<string, WeaponData> weaponDictionary = new Dictionary<string, WeaponData>();
    private Dictionary<string, float> lastFireTimes = new Dictionary<string, float>();
    private PlayerController player;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();

        // Initialize weapon dictionary
        foreach (WeaponData weapon in availableWeapons)
        {
            weaponDictionary[weapon.weaponName] = weapon;
            lastFireTimes[weapon.weaponName] = 0f;
        }

        // Start with basic weapon
        if (availableWeapons.Length > 0)
        {
            availableWeapons[0].isActive = true;
        }
    }

    void Update()
    {
        if (player == null || !player.IsAlive()) return;

        // Auto-fire active weapons
        foreach (WeaponData weapon in availableWeapons)
        {
            if (weapon.isActive)
            {
                TryFireWeapon(weapon);
            }
        }
    }

    void TryFireWeapon(WeaponData weapon)
    {
        if (Time.time - lastFireTimes[weapon.weaponName] >= 1f / weapon.fireRate)
        {
            FireWeapon(weapon);
            lastFireTimes[weapon.weaponName] = Time.time;
        }
    }

    void FireWeapon(WeaponData weapon)
    {
        if (weapon.projectilePrefab == null || firePoint == null) return;

        // Find nearest enemy for targeting
        Enemy nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null) return;

        Vector2 targetDirection = ((Vector2)nearestEnemy.transform.position - (Vector2)firePoint.position).normalized;

        // Fire projectiles
        for (int i = 0; i < weapon.projectileCount; i++)
        {
            // Calculate spread
            Vector2 fireDirection = targetDirection;
            if (weapon.spreadAngle > 0f && weapon.projectileCount > 1)
            {
                float angleOffset = (weapon.spreadAngle / (weapon.projectileCount - 1)) * (i - (weapon.projectileCount - 1) / 2f);
                fireDirection = Quaternion.Euler(0f, 0f, angleOffset) * targetDirection;
            }

            // Create projectile
            GameObject projectile = Instantiate(weapon.projectilePrefab, firePoint.position, Quaternion.identity);
            Projectile projectileComponent = projectile.GetComponent<Projectile>();

            if (projectileComponent != null)
            {
                projectileComponent.Initialize(fireDirection, weapon.projectileSpeed, weapon.damage);
            }
            else
            {
                // Basic projectile movement (2D)
                Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = fireDirection * weapon.projectileSpeed;
                }
            }
        }
    }

    Enemy FindNearestEnemy()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null) return null;

        List<Enemy> enemies = gameManager.GetEnemies();
        Enemy nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        foreach (Enemy enemy in enemies)
        {
            if (enemy != null && !enemy.IsDead())
            {
                float distance = Vector2.Distance(firePoint.position, enemy.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemy;
                }
            }
        }

        return nearestEnemy;
    }

    public void UpgradeWeapon(string weaponName)
    {
        if (weaponDictionary.ContainsKey(weaponName))
        {
            WeaponData weapon = weaponDictionary[weaponName];
            weapon.level++;

            // Upgrade stats based on level
            weapon.damage *= 1.2f;
            weapon.fireRate *= 1.1f;

            if (weapon.level % 3 == 0)
            {
                weapon.projectileCount++;
            }
        }
    }

    public void UnlockWeapon(string weaponName)
    {
        if (weaponDictionary.ContainsKey(weaponName))
        {
            weaponDictionary[weaponName].isActive = true;
        }
    }

    public WeaponData[] GetActiveWeapons()
    {
        List<WeaponData> activeWeapons = new List<WeaponData>();
        foreach (WeaponData weapon in availableWeapons)
        {
            if (weapon.isActive)
            {
                activeWeapons.Add(weapon);
            }
        }
        return activeWeapons.ToArray();
    }

    public WeaponData[] GetAvailableWeapons()
    {
        return availableWeapons;
    }
}
