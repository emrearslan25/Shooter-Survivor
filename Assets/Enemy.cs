using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float moveSpeed = 2f;
    public float maxHealth = 10f;
    public float attackDamage = 5f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;
    public int experienceValue = 10;

    [Header("Visual")]
    public GameObject model;
    public ParticleSystem deathEffect;

    // State
    private float currentHealth;
    private Transform target;
    private bool isAlive = true;
    private float lastAttackTime = 0f;

    // Components
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (!isAlive || target == null) return;

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
    // Account for collider radii so enemies can attack when touching the player's collider
    float playerRadius = GetColliderRadius2D(target);
    float myRadius = GetColliderRadius2D(transform);
    float effectiveDistance = Mathf.Max(0f, distance - (playerRadius + myRadius));

    if (effectiveDistance <= attackRange)
        {
            Attack();
            lastAttackTime = Time.time;
        }
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
        if (model != null)
        {
            // Flash red or play hit animation
            StartCoroutine(DamageFlash());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator DamageFlash()
    {
        if (model != null)
        {
            SpriteRenderer[] renderers = model.GetComponentsInChildren<SpriteRenderer>();
            Color originalColor = renderers[0].color;

            foreach (SpriteRenderer renderer in renderers)
            {
                renderer.color = Color.red;
            }

            yield return new WaitForSeconds(0.1f);

            foreach (SpriteRenderer renderer in renderers)
            {
                renderer.color = originalColor;
            }
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

        if (model != null)
            model.SetActive(false);

        // Destroy after delay
        Destroy(gameObject, 2f);
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
        exp.orbLight = light;
        exp.SetExperienceValue(experienceValue);
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
