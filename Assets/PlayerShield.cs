using UnityEngine;

public class PlayerShield : MonoBehaviour
{
    private PlayerController playerController;

    public void Initialize(PlayerController player)
    {
        playerController = player;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // Damage enemies that are touching the shield
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && !enemy.IsDead() && playerController != null)
        {
            playerController.DamageEnemiesInShield();
        }
    }
}