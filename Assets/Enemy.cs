using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Circle,     // Normal düşman
    Triangle,   // Ateş eden düşman  
    Square      // Büyük yavaş düşman
}

public class Enemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    public EnemyType enemyType = EnemyType.Circle;
    public float moveSpeed = 2f;
    public float maxHealth = 10f;
    public float attackDamage = 5f;
    public float attackRange = 0.2f;
    public float attackCooldown = 1f;
    public int experienceValue = 10;

    [Header("Triangle Enemy (Shooter)")]
    public float shootRange = 5f;
    public float shootCooldown = 2f;
    public float projectileSpeed = 8f;
    public float projectileDamage = 3f;

    [Header("Visual")]
    public GameObject model;
    public ParticleSystem deathEffect;

    // State
    private float currentHealth;
    private Transform target;
    private bool isAlive = true;
    private float lastAttackTime = 0f;
    private float lastShootTime = 0f;

    // Components
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        
        SetupEnemyByType();
    }

    void SetupEnemyByType()
    {
        switch (enemyType)
        {
            case EnemyType.Circle:
                SetupCircleEnemy();
                break;
            case EnemyType.Triangle:
                SetupTriangleEnemy();
                break;
            case EnemyType.Square:
                SetupSquareEnemy();
                break;
        }
    }

    void SetupCircleEnemy()
    {
        // KIRMIZI DAIRE DÜŞMAN - PLAYER İLE AYNI BOYUT (1.0 scale)
        CreateCircleVisual(Color.red, 1.0f);
        
        // Collider boyutu visual ile tam eşleşsin
        var collider = GetComponent<CircleCollider2D>();
        if (collider != null)
        {
            collider.radius = 0.5f; // 1.0 scale'in yarısı
        }
        
        // Transform scale ayarla
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }

    void SetupTriangleEnemy()
    {
        // SARI ÜÇGEN DÜŞMAN - ORTA BOYUT (1.0 scale) 
        maxHealth = 8f;
        currentHealth = maxHealth;
        moveSpeed = 2.5f;
        attackDamage = 3f;
        experienceValue = 15;
        
        CreateTriangleVisual(Color.yellow, 1.0f);
        
        // Collider boyutu visual ile tam eşleşsin
        var collider = GetComponent<CircleCollider2D>();
        if (collider != null)
        {
            collider.radius = 0.5f; // 1.0 scale'in yarısı
        }
        
        // Transform scale ayarla
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }

    void SetupSquareEnemy()
    {
        // MAVİ KARE DÜŞMAN - ORTA BÜYÜKLÜK (1.5 scale)
        maxHealth = 25f;
        currentHealth = maxHealth;
        moveSpeed = 1f;
        attackDamage = 12f;
        attackRange = 0.05f;
        experienceValue = 30;
        
        CreateSquareVisual(Color.blue, 1.5f);
        
        // Collider boyutu visual ile tam eşleşsin
        var collider = GetComponent<CircleCollider2D>();
        if (collider != null)
        {
            collider.radius = 0.75f; // 1.5 scale'in yarısı
        }
        
        // Transform scale ayarla
        transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
    }

    void Update()
    {
        if (!isAlive || target == null) return;

        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        // Triangle düşman ateş etmeyi dener
        if (enemyType == EnemyType.Triangle && distanceToTarget <= shootRange)
        {
            TryShoot();
        }

        MoveTowardsTarget();
        TryAttack();
    }

    public void Initialize(Transform playerTransform)
    {
        target = playerTransform;
    }

    void MoveTowardsTarget()
    {
        if (target == null) return;

        Vector2 direction = ((Vector2)target.position - rb.position).normalized;
        rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);

        // Rotate to face target (2D)
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        float distance = Vector2.Distance(transform.position, target.position);
        // Account for collider radii so enemies must be visually touching to attack
        float playerRadius = GetColliderRadius2D(target);
        float myRadius = GetColliderRadius2D(transform);
        float effectiveDistance = Mathf.Max(0f, distance - (playerRadius + myRadius));

        // Enemy must be visually touching the player to deal damage
        if (effectiveDistance <= 0.01f) // Very small threshold for contact
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    void TryShoot()
    {
        if (Time.time - lastShootTime < shootCooldown) return;
        if (enemyType != EnemyType.Triangle) return;

        Vector2 directionToPlayer = ((Vector2)target.position - (Vector2)transform.position).normalized;
        CreateEnemyProjectile(directionToPlayer);
        lastShootTime = Time.time;
    }

    void CreateEnemyProjectile(Vector2 direction)
    {
        GameObject proj = new GameObject("EnemyProjectile");
        proj.transform.position = transform.position;

        // Visual
        var sr = proj.AddComponent<SpriteRenderer>();
        var tex = new Texture2D(6, 6, TextureFormat.RGBA32, false);
        var cols = new Color32[6 * 6];
        for (int i = 0; i < cols.Length; i++) cols[i] = new Color32(255, 255, 0, 255); // Yellow
        tex.SetPixels32(cols);
        tex.Apply();
        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 64f);
        sr.sprite = sprite;
        sr.sortingOrder = 5;

        // Collider - make it larger for better collision detection
        var col = proj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.15f; // Increased from 0.08f

        // Movement
        var rb2d = proj.AddComponent<Rigidbody2D>();
        rb2d.gravityScale = 0f;
        rb2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Better collision detection
        rb2d.velocity = direction * projectileSpeed;

        // Damage component
        var damage = proj.AddComponent<EnemyProjectile>();
        damage.damage = projectileDamage;

        // Destroy after 3 seconds
        Destroy(proj, 3f);
    }

    void Attack()
    {
        PlayerController player = target.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(attackDamage);

            // Attack animation/effect
            // You can add attack particles or animation here
        }
    }

    public void TakeDamage(float damage)
    {
        if (!isAlive) return;

        currentHealth -= damage;

        // Damage effect
        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator DamageFlash()
    {
        // LineRenderer visual flash effect
        var visual = transform.GetComponentInChildren<LineRenderer>();
        if (visual != null)
        {
            Color originalColor = visual.startColor;
            visual.startColor = Color.white;
            visual.endColor = Color.white;
            yield return new WaitForSeconds(0.1f);
            visual.startColor = originalColor;
            visual.endColor = originalColor;
        }
    }

    void Die()
    {
        isAlive = false;

        // Death effect
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // Drop experience
        SpawnExperienceOrb();

        // Disable components
        if (rb != null)
            rb.simulated = false;

        // Destroy after delay
        Destroy(gameObject, 0.5f);
    }

    void SpawnExperienceOrb()
    {
        // Create orb at runtime (no prefab dependency)
        GameObject orb = new GameObject("ExperienceOrb");
        orb.transform.position = transform.position;

        // Collider (trigger) so player can attract/pick
        var col = orb.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.25f;

        // Visual: pulsing ring using LineRenderer
        var ring = new GameObject("Ring");
        ring.transform.SetParent(orb.transform, false);
        var lr = ring.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true; lr.positionCount = 28; lr.widthMultiplier = 0.05f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = new Color(0.2f, 1f, 0.6f, 1f);
        lr.endColor = lr.startColor;
        float r = 0.22f;
        for (int i = 0; i < lr.positionCount; i++)
        {
            float a = (i / (float)lr.positionCount) * Mathf.PI * 2f;
            lr.SetPosition(i, new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f));
        }

        // Optional light for glow (works with 3D light)
        var light = orb.AddComponent<Light>();
        light.type = LightType.Point; light.color = new Color(0.2f, 1f, 0.6f, 1f); light.range = 2.5f; light.intensity = 1.4f;

        var exp = orb.AddComponent<ExperienceOrb>();
        exp.SetExperienceValue(experienceValue);
    }

    void CreateCircleVisual(Color color, float size)
    {
        // İçi boş çember - Player gibi LineRenderer kullan
        GameObject visual = new GameObject("CircleVisual");
        visual.transform.SetParent(transform, false);
        
        var lr = visual.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.positionCount = 32; // Smooth circle
        lr.widthMultiplier = 0.08f; // Line thickness
        lr.startColor = color;
        lr.endColor = color;
        
        var material = new Material(Shader.Find("Sprites/Default"));
        material.renderQueue = 3000;
        lr.material = material;
        lr.sortingOrder = 40;
        
        // Circle points
        float radius = size * 0.5f;
        for (int i = 0; i < lr.positionCount; i++)
        {
            float angle = (i / (float)lr.positionCount) * Mathf.PI * 2f;
            lr.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
        }
    }

    void CreateTriangleVisual(Color color, float size)
    {
        // İçi boş üçgen - LineRenderer ile
        GameObject visual = new GameObject("TriangleVisual");
        visual.transform.SetParent(transform, false);
        
        var lr = visual.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.positionCount = 3; // Triangle has 3 points
        lr.widthMultiplier = 0.08f; // Line thickness
        lr.startColor = color;
        lr.endColor = color;
        
        var material = new Material(Shader.Find("Sprites/Default"));
        material.renderQueue = 3000;
        lr.material = material;
        lr.sortingOrder = 40;
        
        // Triangle points (pointing right)
        float radius = size * 0.5f;
        lr.SetPosition(0, new Vector3(radius, 0f, 0f));           // Right point
        lr.SetPosition(1, new Vector3(-radius * 0.5f, radius * 0.8f, 0f));  // Top left
        lr.SetPosition(2, new Vector3(-radius * 0.5f, -radius * 0.8f, 0f)); // Bottom left
    }

    void CreateSquareVisual(Color color, float size)
    {
        // İçi boş kare - LineRenderer ile
        GameObject visual = new GameObject("SquareVisual");
        visual.transform.SetParent(transform, false);
        
        var lr = visual.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.positionCount = 4; // Square has 4 points
        lr.widthMultiplier = 0.1f; // Slightly thicker for big squares
        lr.startColor = color;
        lr.endColor = color;
        
        var material = new Material(Shader.Find("Sprites/Default"));
        material.renderQueue = 3000;
        lr.material = material;
        lr.sortingOrder = 40;
        
        // Square points
        float halfSize = size * 0.5f;
        lr.SetPosition(0, new Vector3(halfSize, halfSize, 0f));     // Top right
        lr.SetPosition(1, new Vector3(-halfSize, halfSize, 0f));    // Top left
        lr.SetPosition(2, new Vector3(-halfSize, -halfSize, 0f));   // Bottom left
        lr.SetPosition(3, new Vector3(halfSize, -halfSize, 0f));    // Bottom right
    }

    public bool IsDead() => !isAlive;
    public float GetHealthPercentage() => currentHealth / maxHealth;

    float GetColliderRadius2D(Transform t)
    {
        if (t == null) return 0f;
        var circle = t.GetComponent<CircleCollider2D>();
        if (circle != null) return Mathf.Abs(circle.radius) * Mathf.Max(t.lossyScale.x, t.lossyScale.y);
        var box = t.GetComponent<BoxCollider2D>();
        if (box != null)
        {
            Vector2 size = Vector2.Scale(box.size, t.lossyScale);
            return size.magnitude * 0.25f; // approx
        }
        var capsule = t.GetComponent<CapsuleCollider2D>();
        if (capsule != null)
        {
            float r = Mathf.Max(capsule.size.x, capsule.size.y) * 0.5f;
            return r * Mathf.Max(t.lossyScale.x, t.lossyScale.y);
        }
        // Fallback using renderer bounds if available
        var sr = t.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            var ext = sr.bounds.extents;
            return new Vector2(ext.x, ext.y).magnitude * 0.5f;
        }
        return 0.5f; // default small radius
    }
}
