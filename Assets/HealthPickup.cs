using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public float healAmount = 10f;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            // Heal player
            player.Heal(healAmount);
            
            // Visual effect (optional)
            // TODO: Add heal effect
            
            // Destroy pickup
            Destroy(gameObject);
        }
    }
}