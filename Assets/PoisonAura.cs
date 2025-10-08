using System.Collections;
using UnityEngine;

public class PoisonAura : MonoBehaviour
{
    public float poisonDamage = 15f;
    public float poisonInterval = 1f; // Her 1 saniyede hasar
    public float radius = 3f;
    
    private float lastPoisonTime = 0f;
    
    void Update()
    {
        if (Time.time - lastPoisonTime >= poisonInterval)
        {
            ApplyPoisonDamage();
            lastPoisonTime = Time.time;
        }
    }
    
    void ApplyPoisonDamage()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        float damage = player != null ? player.poisonDamage : 15f;
        float currentRadius = player != null ? player.poisonRadius : 3f;
        
        // Find all enemies in range
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, currentRadius);
        
        foreach (var col in enemies)
        {
            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy != null && !enemy.IsDead())
            {
                enemy.TakeDamage(damage);
                
                // Visual poison effect on enemy
                StartCoroutine(PoisonEffect(enemy.transform));
            }
        }
    }
    
    System.Collections.IEnumerator PoisonEffect(Transform target)
    {
        // Create temporary green particle effect
        GameObject effect = new GameObject("PoisonEffect");
        effect.transform.position = target.position;
        
        var sr = effect.AddComponent<SpriteRenderer>();
        var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        var cols = new Color32[4 * 4];
        Color32 greenColor = new Color32(0, 255, 0, 200);
        
        for (int i = 0; i < cols.Length; i++)
        {
            cols[i] = greenColor;
        }
        
        tex.SetPixels32(cols);
        tex.Apply();
        
        var sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 64f);
        sr.sprite = sprite;
        sr.sortingOrder = 15;
        
        // Animate upward
        Vector3 startPos = effect.transform.position;
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            effect.transform.position = startPos + Vector3.up * progress * 0.5f;
            sr.color = new Color(0f, 1f, 0f, 1f - progress);
            
            yield return null;
        }
        
        Destroy(effect);
    }
}