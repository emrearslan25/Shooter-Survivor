using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float damage = 3f;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Debug: Log what we're colliding with
        Debug.Log($"EnemyProjectile collided with: {other.name}");
        
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            Debug.Log($"Dealing {damage} damage to player");
            player.TakeDamage(damage);
            // Destroy projectile on hit
            Destroy(gameObject);
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // Additional collision check for continuous collision
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}