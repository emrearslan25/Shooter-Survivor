using UnityEngine;

// Inspector-assigned sprite infinite tiling background system
// Creates tiles around player that move as player moves for seamless infinite background
public class InfiniteBackground : MonoBehaviour
{
    [Header("Background Settings")]
    public Sprite backgroundSprite;
    public float tileSize = 10f; // Size of each background tile
    public int gridSize = 3; // 3x3 grid around player
    public string sortingLayerName = "Background";
    public int sortingOrder = -10;

    [Header("References")]
    public Transform player;
    public Camera mainCamera;

    private GameObject[,] backgroundTiles;
    private Vector2 lastPlayerGridPos;
    private Vector2 spriteSize;

    void Start()
    {
        // Auto-find references if not assigned
        if (player == null)
        {
            var playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
                player = playerController.transform;
        }

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (backgroundSprite == null)
        {
            Debug.LogWarning("InfiniteBackground: No background sprite assigned!");
            return;
        }

        // Calculate sprite size based on tileSize
        spriteSize = new Vector2(tileSize, tileSize);

        // Initialize the tile grid
        InitializeTileGrid();

        // Set initial position
        if (player != null)
        {
            lastPlayerGridPos = GetGridPosition(player.position);
            UpdateTilePositions();
        }
    }

    void Update()
    {
        if (player == null || backgroundSprite == null) return;

        Vector2 currentPlayerGridPos = GetGridPosition(player.position);

        // Check if player moved to a different grid cell
        if (currentPlayerGridPos != lastPlayerGridPos)
        {
            UpdateTilePositions();
            lastPlayerGridPos = currentPlayerGridPos;
        }
    }

    void InitializeTileGrid()
    {
        backgroundTiles = new GameObject[gridSize, gridSize];

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                GameObject tile = CreateBackgroundTile();
                tile.name = $"BackgroundTile_{x}_{y}";
                backgroundTiles[x, y] = tile;
            }
        }
    }

    GameObject CreateBackgroundTile()
    {
        GameObject tile = new GameObject("BackgroundTile");
        
        SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
        sr.sprite = backgroundSprite;
        sr.sortingLayerName = sortingLayerName;
        sr.sortingOrder = sortingOrder;

        // Scale sprite to match tileSize
        if (backgroundSprite != null)
        {
            Vector2 spriteOriginalSize = backgroundSprite.bounds.size;
            Vector2 scale = new Vector2(
                tileSize / spriteOriginalSize.x,
                tileSize / spriteOriginalSize.y
            );
            tile.transform.localScale = scale;
        }

        return tile;
    }

    void UpdateTilePositions()
    {
        if (player == null) return;

        Vector2 playerGridPos = GetGridPosition(player.position);
        int halfGrid = gridSize / 2;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (backgroundTiles[x, y] != null)
                {
                    // Calculate world position for this tile
                    Vector2 tileGridPos = new Vector2(
                        playerGridPos.x - halfGrid + x,
                        playerGridPos.y - halfGrid + y
                    );

                    Vector3 worldPos = new Vector3(
                        tileGridPos.x * tileSize,
                        tileGridPos.y * tileSize,
                        0f
                    );

                    backgroundTiles[x, y].transform.position = worldPos;
                }
            }
        }
    }

    Vector2 GetGridPosition(Vector3 worldPos)
    {
        return new Vector2(
            Mathf.FloorToInt(worldPos.x / tileSize),
            Mathf.FloorToInt(worldPos.y / tileSize)
        );
    }

    // Method to change background sprite at runtime
    public void SetBackgroundSprite(Sprite newSprite)
    {
        backgroundSprite = newSprite;
        
        if (backgroundTiles != null)
        {
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    if (backgroundTiles[x, y] != null)
                    {
                        SpriteRenderer sr = backgroundTiles[x, y].GetComponent<SpriteRenderer>();
                        if (sr != null)
                        {
                            sr.sprite = newSprite;
                            
                            // Update scale for new sprite
                            if (newSprite != null)
                            {
                                Vector2 spriteOriginalSize = newSprite.bounds.size;
                                Vector2 scale = new Vector2(
                                    tileSize / spriteOriginalSize.x,
                                    tileSize / spriteOriginalSize.y
                                );
                                backgroundTiles[x, y].transform.localScale = scale;
                            }
                        }
                    }
                }
            }
        }
    }

    void OnValidate()
    {
        // Update tile size when changed in inspector
        if (Application.isPlaying && backgroundTiles != null)
        {
            spriteSize = new Vector2(tileSize, tileSize);
            UpdateTilePositions();
            
            // Update scale of existing tiles
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    if (backgroundTiles[x, y] != null && backgroundSprite != null)
                    {
                        Vector2 spriteOriginalSize = backgroundSprite.bounds.size;
                        Vector2 scale = new Vector2(
                            tileSize / spriteOriginalSize.x,
                            tileSize / spriteOriginalSize.y
                        );
                        backgroundTiles[x, y].transform.localScale = scale;
                    }
                }
            }
        }
    }
}
