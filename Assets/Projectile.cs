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

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if hit enemy
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && !enemy.IsDead())
        {
            enemy.TakeDamage(damage);

            // Hit effect
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }

            // Destroy projectile
            Destroy(gameObject);
        }
        // Check if hit wall or obstacle
        else if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            Destroy(gameObject);
        }
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
