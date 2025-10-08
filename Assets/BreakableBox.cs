using UnityEngine;
using System.Collections;

public class BreakableBox : MonoBehaviour
{
    [Header("Box Settings")]
    public float health = 1f; // Tek vuruşta kırılır
    public int minDropCount = 2;
    public int maxDropCount = 5;
    
    [Header("Drop Chances")]
    [Range(0f, 1f)]
    public float healthDropChance = 0.4f; // %40 can drop
    [Range(0f, 1f)]
    public float xpDropChance = 0.6f; // %60 XP drop
    
    private bool isDestroyed = false;
    
    void Start()
    {
        CreateBoxVisual();
    }
    
    void CreateBoxVisual()
    {
        // Kahverengi içi dolu kare
        GameObject visual = new GameObject("BoxVisual");
        visual.transform.SetParent(transform, false);
        
        var sr = visual.AddComponent<SpriteRenderer>();
        
        // Kahverengi kare texture oluştur
        int texSize = 16;
        var tex = new Texture2D(texSize, texSize, TextureFormat.RGBA32, false);
        var cols = new Color32[texSize * texSize];
        
        Color32 brownColor = new Color32(139, 69, 19, 255); // Kahverengi
        Color32 darkBrown = new Color32(101, 50, 14, 255);  // Koyu kenar
        
        // Kare çiz
        for (int y = 0; y < texSize; y++)
        {
            for (int x = 0; x < texSize; x++)
            {
                // Kenar çizgisi
                if (x == 0 || x == texSize-1 || y == 0 || y == texSize-1)
                {
                    cols[y * texSize + x] = darkBrown;
                }
                else
                {
                    cols[y * texSize + x] = brownColor;
                }
            }
        }
        
        tex.SetPixels32(cols);
        tex.Apply();
        
        var sprite = Sprite.Create(tex, new Rect(0, 0, texSize, texSize), new Vector2(0.5f, 0.5f), 64f);
        sr.sprite = sprite;
        sr.sortingOrder = 30;
        
        // Collider ekle
        var col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
        }
        col.radius = 0.3f;
        col.isTrigger = false; // Solid, mermiler çarpabilir
    }
    
    public void TakeDamage(float damage)
    {
        if (isDestroyed) return;
        
        health -= damage;
        
        if (health <= 0)
        {
            BreakBox();
        }
    }
    
    void BreakBox()
    {
        if (isDestroyed) return;
        isDestroyed = true;
        
        // Break effect
        StartCoroutine(BreakEffect());
        
        // Drop items
        SpawnDrops();
        
        // Destroy box
        Destroy(gameObject, 0.1f);
    }
    
    IEnumerator BreakEffect()
    {
        // Simple flash effect
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            Color original = sr.color;
            sr.color = Color.white;
            yield return new WaitForSeconds(0.05f);
            sr.color = original;
        }
    }
    
    void SpawnDrops()
    {
        int dropCount = Random.Range(minDropCount, maxDropCount + 1);
        
        for (int i = 0; i < dropCount; i++)
        {
            // Random offset for drops
            Vector2 offset = Random.insideUnitCircle * 0.5f;
            Vector3 dropPos = transform.position + new Vector3(offset.x, offset.y, 0f);
            
            float rand = Random.Range(0f, 1f);
            
            if (rand < healthDropChance)
            {
                // Health drop
                SpawnHealthDrop(dropPos);
            }
            else
            {
                // XP drop
                SpawnXPDrop(dropPos);
            }
        }
    }
    
    void SpawnHealthDrop(Vector3 position)
    {
        GameObject healthDrop = new GameObject("HealthDrop");
        healthDrop.transform.position = position;
        
        // Visual - kırmızı artı işareti
        var sr = healthDrop.AddComponent<SpriteRenderer>();
        var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
        var cols = new Color32[8 * 8];
        
        Color32 redColor = new Color32(255, 0, 0, 255);
        Color32 transparent = new Color32(0, 0, 0, 0);
        
        // Artı işareti çiz
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if ((x >= 3 && x <= 4) || (y >= 3 && y <= 4))
                {
                    cols[y * 8 + x] = redColor;
                }
                else
                {
                    cols[y * 8 + x] = transparent;
                }
            }
        }
        
        tex.SetPixels32(cols);
        tex.Apply();
        
        var sprite = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 64f);
        sr.sprite = sprite;
        sr.sortingOrder = 10;
        
        // Collider
        var col = healthDrop.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.2f;
        
        // Health pickup component
        var pickup = healthDrop.AddComponent<HealthPickup>();
        pickup.healAmount = 10f;
        
        // Auto destroy after 10 seconds
        Destroy(healthDrop, 10f);
    }
    
    void SpawnXPDrop(Vector3 position)
    {
        // Use existing XP orb system
        GameObject orb = new GameObject("ExperienceOrb");
        orb.transform.position = position;

        var col = orb.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.25f;

        // Visual: smaller green ring
        var ring = new GameObject("Ring");
        ring.transform.SetParent(orb.transform, false);
        var lr = ring.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true; 
        lr.positionCount = 20; 
        lr.widthMultiplier = 0.04f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = new Color(0.2f, 1f, 0.6f, 1f);
        lr.endColor = lr.startColor;
        lr.sortingOrder = 10;
        
        float r = 0.18f;
        for (int i = 0; i < lr.positionCount; i++)
        {
            float a = (i / (float)lr.positionCount) * Mathf.PI * 2f;
            lr.SetPosition(i, new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f));
        }

        var exp = orb.AddComponent<ExperienceOrb>();
        exp.SetExperienceValue(15); // Biraz daha fazla XP
    }
}