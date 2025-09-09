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

        if (distance <= attackRange)
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
        // Create experience orb prefab and spawn it
        GameObject orbPrefab = Resources.Load<GameObject>("ExperienceOrb");
        if (orbPrefab != null)
        {
            GameObject orb = Instantiate(orbPrefab, transform.position, Quaternion.identity);
            ExperienceOrb orbComponent = orb.GetComponent<ExperienceOrb>();
            if (orbComponent != null)
            {
                orbComponent.SetExperienceValue(experienceValue);
            }
        }
    }

    public bool IsDead() => !isAlive;
    public float GetHealthPercentage() => currentHealth / maxHealth;
}
