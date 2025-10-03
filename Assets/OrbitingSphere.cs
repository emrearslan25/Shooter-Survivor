using UnityEngine;

public class OrbitingSphere : MonoBehaviour
{
    public float damage = 15f;
    public float orbitSpeed = 120f; // degrees per second
    
    void Update()
    {
        // Rotate around parent
        transform.RotateAround(transform.parent.position, Vector3.forward, orbitSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && !enemy.IsDead())
        {
            enemy.TakeDamage(damage);
        }
    }
}