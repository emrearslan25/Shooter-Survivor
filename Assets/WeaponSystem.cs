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

        // Ensure at least one weapon exists
        if (availableWeapons == null || availableWeapons.Length == 0)
        {
            availableWeapons = new WeaponData[1];
            availableWeapons[0] = new WeaponData
            {
                weaponName = "Default",
                fireRate = 4f,
                projectileSpeed = 14f,
                damage = 8f,
                projectileCount = 1,
                spreadAngle = 0f,
                isActive = true,
                level = 1,
                projectilePrefab = null // will be created at runtime if null
            };
        }

        // Initialize weapon dictionary
        foreach (WeaponData weapon in availableWeapons)
        {
            if (string.IsNullOrEmpty(weapon.weaponName)) weapon.weaponName = "Weapon" + Random.Range(0, 9999);
            weaponDictionary[weapon.weaponName] = weapon;
            lastFireTimes[weapon.weaponName] = 0f;
        }

        // Start with basic weapon
        if (availableWeapons.Length > 0)
        {
            availableWeapons[0].isActive = true;
        }

        // Fallback firePoint: use player's transform if none assigned
        if (firePoint == null && player != null)
        {
            firePoint = player.transform;
        }
    }

    void Update()
    {
        if (player == null || !player.IsAlive()) return;

        // If PlayerController is using simple shooting, do not auto-fire
    var pc = player != null ? player.GetComponent<PlayerController>() : null;
    if (pc != null && pc.IsSimpleShootingEnabled()) return;

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
    if (firePoint == null) return;

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

            SpawnProjectile(weapon, fireDirection);
        }
    }

    // Manual fire towards a world point (e.g., mouse position)
    public void ManualFireAt(Vector3 worldPos)
    {
        if (availableWeapons == null || availableWeapons.Length == 0)
        {
            Debug.LogWarning("WeaponSystem: No available weapons configured.");
            return;
        }
        foreach (var weapon in availableWeapons)
        {
            if (!weapon.isActive) continue;
            if (Time.time - lastFireTimes[weapon.weaponName] < 1f / weapon.fireRate) continue;

            if (firePoint == null) continue;

            Vector2 targetDir = ((Vector2)worldPos - (Vector2)firePoint.position).normalized;

            for (int i = 0; i < weapon.projectileCount; i++)
            {
                Vector2 fireDirection = targetDir;
                if (weapon.spreadAngle > 0f && weapon.projectileCount > 1)
                {
                    float angleOffset = (weapon.spreadAngle / (weapon.projectileCount - 1)) * (i - (weapon.projectileCount - 1) / 2f);
                    fireDirection = Quaternion.Euler(0f, 0f, angleOffset) * targetDir;
                }

                SpawnProjectile(weapon, fireDirection);
            }

            lastFireTimes[weapon.weaponName] = Time.time;
        }
    }

    void SpawnProjectile(WeaponData weapon, Vector2 fireDirection)
    {
        GameObject projectile;
        if (weapon.projectilePrefab != null)
        {
            projectile = Instantiate(weapon.projectilePrefab, firePoint.position, Quaternion.identity);
        }
        else
        {
            projectile = CreateRuntimeProjectile();
        }

        if (projectile == null) return;
        var projectileComponent = projectile.GetComponent<Projectile>();
        if (projectileComponent != null)
        {
            projectileComponent.Initialize(fireDirection, weapon.projectileSpeed, weapon.damage);
        }
        else
        {
            var rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = fireDirection * weapon.projectileSpeed;
            }
        }
    }

    GameObject CreateRuntimeProjectile()
    {
        // Create a simple 2D projectile at runtime (fallback)
        var go = new GameObject("RuntimeProjectile");
        go.transform.position = firePoint.position;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.color = Color.white;
        sr.sprite = null; // optional: can assign a default sprite
        var circle = go.AddComponent<CircleCollider2D>();
        circle.isTrigger = true;
        circle.radius = 0.12f;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var proj = go.AddComponent<Projectile>();
        // proj will be initialized by caller
        return go;
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
