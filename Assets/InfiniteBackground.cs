using UnityEngine;

// World-space infinite scrolling background (no Canvas)
// Creates a quad behind gameplay and scrolls texture based on camera position.
public class InfiniteBackground : MonoBehaviour
{
    [Header("Appearance")]
    public Color baseColor = new Color(0.02f, 0.03f, 0.05f, 1f); // near black
    public Color lineColor = new Color(0f, 1f, 1f, 0.12f);       // cyan neon (subtle)
    public int textureSize = 256;                                // generated texture size (square)
    public int gridStep = 32;                                    // pixels between grid lines
    public int lineWidth = 2;                                    // grid line thickness in pixels
    public float parallax = 0.05f;                               // scroll speed relative to camera

    [Header("Placement")]
    public float depth = 100f;      // z position relative to camera (place far behind gameplay)
    public float coverage = 2.2f;   // how much larger than view to render to avoid edges

    Camera cam;
    Transform quad;
    Material mat;

    void Awake()
    {
        cam = Camera.main;
        if (cam == null)
        {
            cam = GetComponent<Camera>();
        }
        if (cam == null) return;

        EnsureQuad();
    }

    void LateUpdate()
    {
        if (cam == null || quad == null) return;

        // Match camera position and scale to viewport size
    var camPos = cam.transform.position;
    // Place quad along camera forward so it always sits behind gameplay in view space
    Vector3 bgPos = camPos + cam.transform.forward * depth;
    quad.position = new Vector3(bgPos.x, bgPos.y, bgPos.z);
    // Face the camera
    quad.rotation = Quaternion.LookRotation(cam.transform.forward, cam.transform.up);

        // Orthographic camera viewport in world units
        float h = cam.orthographicSize * 2f;
        float w = h * cam.aspect;

        quad.localScale = new Vector3(w * coverage, h * coverage, 1f);

        // Scroll by camera position for an infinite feel
        if (mat != null)
        {
            Vector2 off = new Vector2(camPos.x, camPos.y) * parallax;
            mat.mainTextureOffset = off;
            // Tile more on bigger screens for finer grid
            float tile = Mathf.Max(1f, (w + h) * 0.25f);
            mat.mainTextureScale = new Vector2(tile, tile);
        }
    }

    void EnsureQuad()
    {
        // Create quad child
        var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = "InfiniteBG";
        go.transform.SetParent(transform, false);
        quad = go.transform;

        // Disable collider on the generated quad
        var col = go.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Material with repeatable texture
    mat = new Material(Shader.Find("Unlit/Texture"));
        mat.mainTexture = GenerateGridTexture();
        mat.mainTexture.wrapMode = TextureWrapMode.Repeat;
        mat.color = Color.white;
    // Render as background and avoid depth occlusion
    mat.renderQueue = 1000; // Background
    if (mat.HasProperty("_ZWrite")) mat.SetInt("_ZWrite", 0);

        var mr = go.GetComponent<MeshRenderer>();
    mr.sharedMaterial = mat;
    // Ensure it renders far behind any sprites/lines
    mr.sortingOrder = -10000;
    }

    Texture2D GenerateGridTexture()
    {
        int size = Mathf.Clamp(textureSize, 64, 1024);
        int step = Mathf.Clamp(gridStep, 8, size / 2);
        int lw = Mathf.Clamp(lineWidth, 1, step / 2);

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        var pixels = new Color32[size * size];

        // Draw base
        Color32 baseC = baseColor;
        for (int i = 0; i < pixels.Length; i++) pixels[i] = baseC;

        // Neon grid lines
        Color32 lineC = lineColor;
        for (int y = 0; y < size; y++)
        {
            bool isHLine = (y % step) < lw;
            for (int x = 0; x < size; x++)
            {
                bool isVLine = (x % step) < lw;
                if (isHLine || isVLine)
                {
                    int idx = y * size + x;
                    pixels[idx] = lineC;
                }
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply(false, false);
        return tex;
    }
}
