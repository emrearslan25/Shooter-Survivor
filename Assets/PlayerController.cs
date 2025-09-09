using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float pickupRange = 2f;

    [Header("Combat Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float damageCooldown = 1f;

    [Header("Visual")]
    public GameObject model;
    public ParticleSystem damageEffect;

    // Components
    private Rigidbody2D rb;
    private WeaponSystem weaponSystem;
    private ExperienceSystem experienceSystem;

    // State
    private Vector2 moveDirection;
    private float lastDamageTime = 0f;
    private bool isAlive = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        weaponSystem = FindObjectOfType<WeaponSystem>();
        experienceSystem = FindObjectOfType<ExperienceSystem>();

        currentHealth = maxHealth;

        // Auto-pickup system
        InvokeRepeating("AutoPickupItems", 0f, 0.1f);
    }

    void Update()
    {
        if (!isAlive) return;

        HandleMovement();
        HandleAutoAim();
    }

    void FixedUpdate()
    {
        if (!isAlive) return;

        // Apply movement (2D)
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
    }

    void HandleMovement()
    {
        // Auto-move towards nearest enemy or random direction
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            List<Enemy> enemies = gameManager.GetEnemies();

            if (enemies.Count > 0)
            {
                // Move towards center of enemies (2D)
                Vector2 centerOfEnemies = Vector2.zero;
                foreach (Enemy enemy in enemies)
                {
                    if (enemy != null)
                        centerOfEnemies += (Vector2)enemy.transform.position;
                }
                centerOfEnemies /= enemies.Count;

                moveDirection = (centerOfEnemies - (Vector2)transform.position).normalized;
            }
            else
            {
                // Random movement when no enemies
                float time = Time.time;
                moveDirection = new Vector2(
                    Mathf.Sin(time * 0.5f),
                    Mathf.Cos(time * 0.5f)
                ).normalized;
            }
        }

        // Rotate to face movement direction (2D)
        if (moveDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    void HandleAutoAim()
    {
        // Find nearest enemy for auto-aim
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            List<Enemy> enemies = gameManager.GetEnemies();
            Enemy nearestEnemy = null;
            float nearestDistance = float.MaxValue;

            foreach (Enemy enemy in enemies)
            {
                if (enemy != null && !enemy.IsDead())
                {
                    float distance = Vector2.Distance(transform.position, enemy.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestEnemy = enemy;
                    }
                }
            }

            // Aim at nearest enemy
            if (nearestEnemy != null)
            {
                Vector2 aimDirection = ((Vector2)nearestEnemy.transform.position - (Vector2)transform.position).normalized;
                // Weapon system will handle the actual firing
            }
        }
    }

    void AutoPickupItems()
    {
        // Find nearby experience orbs and pick them up (2D)
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, pickupRange);

        foreach (Collider2D collider in colliders)
        {
            ExperienceOrb orb = collider.GetComponent<ExperienceOrb>();
            if (orb != null)
            {
                orb.Pickup(this);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (Time.time - lastDamageTime < damageCooldown) return;

        currentHealth -= damage;
        lastDamageTime = Time.time;

        // Damage effect
        if (damageEffect != null)
        {
            damageEffect.Play();
        }

        // Screen shake effect (2D)
        Camera.main.transform.position += (Vector3)Random.insideUnitCircle * 0.1f;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isAlive = false;

        // Death animation
        if (model != null)
        {
            model.SetActive(false);
        }

        // Game over
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.GameOver();
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    public bool IsAlive() => isAlive;
    public float GetHealthPercentage() => currentHealth / maxHealth;
    public Vector2 GetMoveDirection() => moveDirection;
}
