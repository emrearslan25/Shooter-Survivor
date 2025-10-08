using System.Collections;
using UnityEngine;

public class CriticalHitEffect : MonoBehaviour
{
    public static void CreateCriticalHitEffect(Vector3 position)
    {
        GameObject effect = new GameObject("CriticalHitEffect");
        effect.transform.position = position;
        
        // Create critical hit visual
        var sr = effect.AddComponent<SpriteRenderer>();
        var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        var cols = new Color32[16 * 16];
        
        // Create star pattern
        Color32 yellowColor = new Color32(255, 255, 0, 255);
        Color32 redColor = new Color32(255, 150, 0, 255);
        Color32 transparent = new Color32(0, 0, 0, 0);
        
        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                int index = y * 16 + x;
                
                // Create star shape
                bool isStar = false;
                
                // Center lines (horizontal and vertical)
                if ((x == 8 && (y >= 2 && y <= 14)) || (y == 8 && (x >= 2 && x <= 14)))
                {
                    isStar = true;
                }
                
                // Diagonal lines
                if ((x - y == 0 && x >= 4 && x <= 12) || (x + y == 16 && x >= 4 && x <= 12))
                {
                    isStar = true;
                }
                
                if (isStar)
                {
                    cols[index] = yellowColor;
                }
                else
                {
                    cols[index] = transparent;
                }
            }
        }
        
        tex.SetPixels32(cols);
        tex.Apply();
        
        var sprite = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 64f);
        sr.sprite = sprite;
        sr.sortingOrder = 20;
        
        // Add animation
        var critEffect = effect.AddComponent<CriticalHitEffect>();
        critEffect.StartCoroutine(critEffect.AnimateEffect(effect));
    }
    
    System.Collections.IEnumerator AnimateEffect(GameObject effect)
    {
        float duration = 0.8f;
        float timer = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one * 1.5f;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            
            // Scale animation
            if (progress < 0.3f)
            {
                float scaleProgress = progress / 0.3f;
                effect.transform.localScale = Vector3.Lerp(startScale, endScale, scaleProgress);
            }
            else
            {
                float fadeProgress = (progress - 0.3f) / 0.7f;
                var sr = effect.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Color color = sr.color;
                    color.a = 1f - fadeProgress;
                    sr.color = color;
                }
                
                // Continue scaling down
                effect.transform.localScale = Vector3.Lerp(endScale, Vector3.zero, fadeProgress);
            }
            
            yield return null;
        }
        
        Destroy(effect);
    }
}