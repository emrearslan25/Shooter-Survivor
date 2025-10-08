using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float lifetime = 5f;
    public GameObject hitEffect;
    public LayerMask enemyLayer;

    // State
    private float damage;
    private float speed;
    private Vector2 direction;
    private bool isInitialized = false;
    
    // Special Properties
    private bool isExplosive = false;
    private bool isFreezing = false;
    private int ricochetCount = 0;
    private int currentRicochets = 0;

    void Start()
    {
        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (!isInitialized) return;

        // Move projectile (2D)
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    public void Initialize(Vector2 fireDirection, float projectileSpeed, float projectileDamage)
    {
        direction = fireDirection.normalized;
        speed = projectileSpeed;
        damage = projectileDamage;
        isInitialized = true;

        // Rotate to face direction (2D)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
    
    public void SetExplosive(bool explosive)
    {
        isExplosive = explosive;
    }
    
    public void SetFreezing(bool freezing)
    {
        isFreezing = freezing;
    }
    
    public void SetRicochet(int maxRicochets)
    {
        ricochetCount = maxRicochets;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if hit enemy
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && !enemy.IsDead())
        {
            // Apply damage
            enemy.TakeDamage(damage);
            
            // Apply freezing effect
            if (isFreezing)
            {
                ApplyFreezingEffect(enemy);
            }
            
            // Life steal
            ApplyLifeSteal();
            
            // Explosive effect
            if (isExplosive)
            {
                CreateExplosion();
            }
            
            // Ricochet
            if (currentRicochets < ricochetCount)
            {
                TryRicochet(other.transform.position);
                return;
            }

            // Hit effect
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }

            // Destroy projectile
            Destroy(gameObject);
            return;
        }
        
        // Check if hit breakable box
        BreakableBox box = other.GetComponent<BreakableBox>();
        if (box != null)
        {
            box.TakeDamage(damage);
            
            if (isExplosive)
            {
                CreateExplosion();
            }
            
            Destroy(gameObject);
            return;
        }
        
        // Check if hit wall or obstacle
        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
    
    void ApplyFreezingEffect(Enemy enemy)
    {
        // TODO: Add freezing component to enemy
        // For now, just visual effect
        StartCoroutine(FreezingEffect(enemy.transform));
    }
    
    void ApplyLifeSteal()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && player.lifeStealPercent > 0)
        {
            float healAmount = damage * player.lifeStealPercent;
            player.Heal(healAmount);
        }
    }
    
    void CreateExplosion()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        float radius = player != null ? player.explosionRadius : 1.5f;
        float damageMultiplier = player != null ? player.explosionDamageMultiplier : 0.7f;
        
        // Explosion damage to nearby enemies
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, radius);
        
        foreach (var col in nearbyEnemies)
        {
            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy != null && !enemy.IsDead())
            {
                enemy.TakeDamage(damage * damageMultiplier);
            }
        }
        
        // Visual explosion effect
        CreateExplosionEffect(radius);
    }
    
    void CreateExplosionEffect(float radius)
    {
        GameObject explosion = new GameObject("Explosion");
        explosion.transform.position = transform.position;
        
        var lr = explosion.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.positionCount = 16;
        lr.widthMultiplier = 0.1f;
        lr.startColor = Color.yellow;
        lr.endColor = Color.red;
        
        var material = new Material(Shader.Find("Sprites/Default"));
        lr.material = material;
        lr.sortingOrder = 20;
        
        // Explosion circle with upgraded radius
        for (int i = 0; i < lr.positionCount; i++)
        {
            float angle = (i / (float)lr.positionCount) * Mathf.PI * 2f;
            lr.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
        }
        
        Destroy(explosion, 0.3f);
    }
    
    void TryRicochet(Vector3 hitPoint)
    {
        // Find nearest enemy for ricochet
        Enemy nearestEnemy = FindNearestEnemyFromPoint(hitPoint);
        if (nearestEnemy != null)
        {
            Vector2 newDirection = ((Vector2)nearestEnemy.transform.position - (Vector2)hitPoint).normalized;
            direction = newDirection;
            
            // Update rotation
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
            
            currentRicochets++;
            
            // Move to hit point and continue
            transform.position = hitPoint;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    Enemy FindNearestEnemyFromPoint(Vector3 point)
    {
        Enemy nearest = null;
        float nearestDistance = float.MaxValue;
        PlayerController player = FindObjectOfType<PlayerController>();
        float maxRange = player != null ? player.ricochetRange : 5f;
        
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in allEnemies)
        {
            if (enemy != null && !enemy.IsDead())
            {
                float distance = Vector2.Distance(point, enemy.transform.position);
                if (distance < nearestDistance && distance < maxRange)
                {
                    nearestDistance = distance;
                    nearest = enemy;
                }
            }
        }
        
        return nearest;
    }
    
    System.Collections.IEnumerator FreezingEffect(Transform target)
    {
        // Blue freezing effect
        GameObject effect = new GameObject("FreezingEffect");
        effect.transform.position = target.position;
        
        var sr = effect.AddComponent<SpriteRenderer>();
        var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
        var cols = new Color32[8 * 8];
        Color32 blueColor = new Color32(100, 200, 255, 150);
        
        for (int i = 0; i < cols.Length; i++)
        {
            cols[i] = blueColor;
        }
        
        tex.SetPixels32(cols);
        tex.Apply();
        
        var sprite = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 64f);
        sr.sprite = sprite;
        sr.sortingOrder = 15;
        
        yield return new WaitForSeconds(0.5f);
        Destroy(effect);
    }

    // Alternative: Use raycast for more precise collision (2D)
    void FixedUpdate()
    {
        if (!isInitialized) return;

        // Raycast for collision detection (2D)
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, speed * Time.fixedDeltaTime, enemyLayer);
        if (hit.collider != null)
        {
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null && !enemy.IsDead())
            {
                enemy.TakeDamage(damage);

                if (hitEffect != null)
                {
                    Instantiate(hitEffect, hit.point, Quaternion.identity);
                }

                Destroy(gameObject);
            }
        }
    }
}
