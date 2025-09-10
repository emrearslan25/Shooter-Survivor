using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Layout")]
    public bool autoLayout = true;
    public float margin = 16f;
    public Vector2 healthBarSize = new Vector2(300, 24);
    public Vector2 xpBarSize = new Vector2(420, 18);
    public int baseFontSize = 24;
    [Header("Panels Layout")]
    public Vector2 upgradePanelSize = new Vector2(520, 420);
    public Vector2 gameOverPanelSize = new Vector2(520, 320);
    public Vector2 upgradeButtonSize = new Vector2(440, 60);
    public float upgradeButtonSpacing = 12f;
    public float panelPadding = 24f;
    [Header("XP Layout")]
    public bool xpTextAboveBar = true;
    public float xpTextSpacing = 8f;
    public float xpTextExtraOffset = 4f;
    [Header("Font & Scaling")]
    public bool useAutoSizing = false;
    public int minAutoFont = 18;
    public int maxAutoFont = 36;
    public bool configureCanvasScaler = true;
    public Vector2 referenceResolution = new Vector2(1920, 1080);
    [Range(0f,1f)] public float matchWidthOrHeight = 0.5f;

    [Header("Player UI")]
    public Slider healthBar;
    public TMP_Text healthText;
    public TMP_Text levelText;
    public TMP_Text timerText;

    [Header("Game UI")]
    public TMP_Text enemyCountText;
    public TMP_Text scoreText;

    [Header("Panels")]
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public GameObject upgradePanel;
    [Header("Upgrade Panel UI (optional)")]
    public TMP_Text upgradeTitleText;      // Optional: Title inside UpgradePanel
    public TMP_Text upgradeDescriptionText; // Optional: Description inside UpgradePanel
    public float upgradeTitleHeight = 60f;
    public float upgradeDescHeight = 120f;

    // References
    private PlayerController player;
    private GameManager gameManager;
    private ExperienceSystem experienceSystem;
    private Image damageOverlay;
    private Coroutine damageFlashCo;

    [Header("UX Effects")]
    public bool enableXPPulse = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        gameManager = FindObjectOfType<GameManager>();
        experienceSystem = FindObjectOfType<ExperienceSystem>();

        // Hide panels initially
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (upgradePanel != null) upgradePanel.SetActive(false);

        if (autoLayout)
        {
            if (configureCanvasScaler)
            {
                ConfigureCanvasScaler();
            }
            ApplyUILayout();
        }

    EnsureCanvasGroups();
    EnsureDamageOverlay();
    }

    void Update()
    {
        UpdatePlayerUI();
        UpdateGameUI();
    }

    void UpdatePlayerUI()
    {
        if (player != null)
        {
            // Health
            if (healthBar != null)
                healthBar.value = player.GetHealthPercentage();

            if (healthText != null)
                healthText.text = $"{Mathf.CeilToInt(player.currentHealth)} / {player.maxHealth}";
        }

        // Level
        if (experienceSystem != null && levelText != null)
            levelText.text = $"Level {experienceSystem.GetCurrentLevel()}";

        // Timer
        if (gameManager != null && timerText != null)
        {
            float time = gameManager.GetGameTime();
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    void UpdateGameUI()
    {
        if (gameManager != null)
        {
            // Enemy count
            if (enemyCountText != null)
                enemyCountText.text = $"Enemies: {gameManager.GetEnemies().Count}";

            // Score (based on time survived + enemies killed)
            if (scoreText != null)
            {
                int score = Mathf.FloorToInt(gameManager.GetGameTime() * 10);
                scoreText.text = $"Score: {score}";
            }
        }
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            ShowPanelAnimated(gameOverPanel);
    }

    public void ShowPauseMenu()
    {
        if (pausePanel != null)
        {
            ShowPanelAnimated(pausePanel);
            Time.timeScale = 0f;
        }
    }

    public void HidePauseMenu()
    {
        if (pausePanel != null)
        {
            HidePanelAnimated(pausePanel);
            Time.timeScale = 1f;
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void OnGUI()
    {
        // Simple pause menu toggle
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 1f)
                ShowPauseMenu();
            else
                HidePauseMenu();
        }
    }

    // ---- Layout helpers ----
    void ApplyUILayout()
    {
        // Top-Left: Health bar and health text
        SetRect(healthBar ? healthBar.GetComponent<RectTransform>() : null,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(margin, -margin), healthBarSize);

        SetTMP(healthText, TextAlignmentOptions.Left, baseFontSize);
        SetRect(healthText ? healthText.GetComponent<RectTransform>() : null,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(margin, -margin - (healthBarSize.y + 8f)), Vector2.zero);

        // Top-Center: Level text
        SetTMP(levelText, TextAlignmentOptions.Center, baseFontSize + 4);
        SetRect(levelText ? levelText.GetComponent<RectTransform>() : null,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -margin), Vector2.zero);

        // Top-Right: Timer text
        SetTMP(timerText, TextAlignmentOptions.Right, baseFontSize);
        SetRect(timerText ? timerText.GetComponent<RectTransform>() : null,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-margin, -margin), Vector2.zero);

        // Bottom-Left: Enemy count
        SetTMP(enemyCountText, TextAlignmentOptions.Left, baseFontSize);
        SetRect(enemyCountText ? enemyCountText.GetComponent<RectTransform>() : null,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(margin, margin), Vector2.zero);

        // Bottom-Right: Score
        SetTMP(scoreText, TextAlignmentOptions.Right, baseFontSize);
        SetRect(scoreText ? scoreText.GetComponent<RectTransform>() : null,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-margin, margin), Vector2.zero);

    // Bottom-Center: XP bar and text (from ExperienceSystem)
        if (experienceSystem != null)
        {
            if (experienceSystem.xpBar != null)
            {
                var xpBarRt = experienceSystem.xpBar.GetComponent<RectTransform>();
                SetRect(xpBarRt,
                    new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, margin), xpBarSize);
            }

            if (experienceSystem.xpText != null)
            {
                SetTMP(experienceSystem.xpText, TextAlignmentOptions.Center, baseFontSize);
                var xpTextRt = experienceSystem.xpText.GetComponent<RectTransform>();
                float barH = xpBarSize.y;
                if (experienceSystem.xpBar != null)
                {
                    var rt = experienceSystem.xpBar.GetComponent<RectTransform>();
                    if (rt != null && rt.rect.height > 0)
                        barH = rt.rect.height;
                }
                float y = xpTextAboveBar ? (margin + barH + xpTextSpacing + xpTextExtraOffset) : margin;
        SetRect(xpTextRt,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, y), Vector2.zero);
            }

            EnsureXPSiblingOrder();
        }

        // Centered panels
        if (upgradePanel != null)
        {
            LayoutPanelCentered(upgradePanel, upgradePanelSize);
            LayoutUpgradePanelTexts(upgradePanel);
            LayoutUpgradeButtons(upgradePanel);
        }

        if (gameOverPanel != null)
        {
            LayoutPanelCentered(gameOverPanel, gameOverPanelSize);
            LayoutPanelTextsCentered(gameOverPanel);
        }
    }

    void SetRect(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 anchoredPos, Vector2 size)
    {
        if (rt == null) return;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        if (size != Vector2.zero)
            rt.sizeDelta = size;
    }

    void SetTMP(TMP_Text tmp, TextAlignmentOptions align, int fontSize)
    {
        if (tmp == null) return;
        tmp.alignment = align;
        if (useAutoSizing)
        {
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = minAutoFont;
            tmp.fontSizeMax = maxAutoFont;
        }
        else
        {
            if (fontSize > 0) tmp.fontSize = fontSize;
            tmp.enableAutoSizing = false;
        }
        tmp.overflowMode = TextOverflowModes.Truncate;
    }

    void LayoutPanelCentered(GameObject panel, Vector2 size)
    {
        if (panel == null) return;
        var rt = panel.GetComponent<RectTransform>();
        if (rt == null) return;
        SetRect(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
    }

    void LayoutUpgradeButtons(GameObject panel)
    {
        if (panel == null) return;
        Button[] buttons = panel.GetComponentsInChildren<Button>(true);
        if (buttons == null || buttons.Length == 0) return;

        // Offset by header/description if present
        float topOffset = panelPadding;
        if (TryResolveUpgradeTitle(panel) != null) topOffset += upgradeTitleHeight + panelPadding;
        if (TryResolveUpgradeDescription(panel) != null) topOffset += upgradeDescHeight + panelPadding;

        float availableHeight = upgradePanelSize.y - topOffset - panelPadding; // bottom padding
        float totalHeight = (buttons.Length * upgradeButtonSize.y) + ((buttons.Length - 1) * upgradeButtonSpacing);
        totalHeight = Mathf.Min(totalHeight, availableHeight);
        float startY = (availableHeight / 2f) - (upgradeButtonSize.y / 2f);

        for (int i = 0; i < buttons.Length; i++)
        {
            var brt = buttons[i].GetComponent<RectTransform>();
            if (brt == null) continue;
            // center within panel
            SetRect(brt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, startY - i * (upgradeButtonSize.y + upgradeButtonSpacing)), upgradeButtonSize);

            // Try to adjust TMP child label if any
            var tmp = buttons[i].GetComponentInChildren<TMP_Text>(true);
            if (tmp != null)
            {
                SetTMP(tmp, TextAlignmentOptions.Center, baseFontSize);
            }
        }
    }

    void LayoutUpgradePanelTexts(GameObject panel)
    {
        var title = TryResolveUpgradeTitle(panel);
        var desc = TryResolveUpgradeDescription(panel);

        float y = (upgradePanelSize.y / 2f) - panelPadding - (upgradeTitleHeight / 2f);
        if (title != null)
        {
            SetTMP(title, TextAlignmentOptions.Center, baseFontSize + 6);
            var rt = title.GetComponent<RectTransform>();
            SetRect(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(upgradePanelSize.x - panelPadding * 2f, upgradeTitleHeight));
        }

        if (desc != null)
        {
            float descY = y - (upgradeTitleHeight / 2f) - panelPadding - (upgradeDescHeight / 2f);
            SetTMP(desc, TextAlignmentOptions.Center, baseFontSize);
            desc.enableWordWrapping = true;
            var rt = desc.GetComponent<RectTransform>();
            SetRect(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, descY), new Vector2(upgradePanelSize.x - panelPadding * 2f, upgradeDescHeight));
        }
    }

    TMP_Text TryResolveUpgradeTitle(GameObject panel)
    {
        if (upgradeTitleText != null) return upgradeTitleText;
        return FindChildTMPByNameContains(panel, "title");
    }

    TMP_Text TryResolveUpgradeDescription(GameObject panel)
    {
        if (upgradeDescriptionText != null) return upgradeDescriptionText;
        var t = FindChildTMPByNameContains(panel, "desc");
        if (t != null) return t;
        return FindChildTMPByNameContains(panel, "description");
    }

    TMP_Text FindChildTMPByNameContains(GameObject root, string token)
    {
        if (root == null) return null;
        token = token.ToLowerInvariant();
        var tmps = root.GetComponentsInChildren<TMP_Text>(true);
        foreach (var tmp in tmps)
        {
            if (tmp.name.ToLowerInvariant().Contains(token))
                return tmp;
        }
        return null;
    }

    void ConfigureCanvasScaler()
    {
        var scaler = GetComponentInParent<CanvasScaler>();
        if (scaler == null) scaler = FindObjectOfType<CanvasScaler>();
        if (scaler == null) return;
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.matchWidthOrHeight = matchWidthOrHeight;
    }

    void LayoutPanelTextsCentered(GameObject panel)
    {
        if (panel == null) return;
        // Try to place first two TMP texts as title + subtitle
        var tmps = panel.GetComponentsInChildren<TMP_Text>(true);
        if (tmps == null || tmps.Length == 0) return;

        float titleH = 64f;
        float subtitleH = 40f;
        float spacing = 12f;

        // Title
        var title = tmps[0];
        SetTMP(title, TextAlignmentOptions.Center, baseFontSize + 10);
        var rtTitle = title.GetComponent<RectTransform>();
        SetRect(rtTitle, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, subtitleH/2f + spacing), new Vector2(gameOverPanelSize.x - panelPadding*2f, titleH));

        // Subtitle if exists
        if (tmps.Length > 1)
        {
            var sub = tmps[1];
            SetTMP(sub, TextAlignmentOptions.Center, baseFontSize + 2);
            var rtSub = sub.GetComponent<RectTransform>();
            SetRect(rtSub, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -(subtitleH/2f + spacing)), new Vector2(gameOverPanelSize.x - panelPadding*2f, subtitleH));
        }
    }

    void EnsureXPSiblingOrder()
    {
        if (experienceSystem == null) return;
        if (experienceSystem.xpBar == null || experienceSystem.xpText == null) return;
        var bar = experienceSystem.xpBar.transform as RectTransform;
        var text = experienceSystem.xpText.transform as RectTransform;
        if (bar == null || text == null) return;
        if (text.parent != bar.parent) return; // they should be siblings under same Canvas/container

        if (xpTextAboveBar)
        {
            // Make sure text renders after bar (higher sibling index)
            int topIndex = Mathf.Max(bar.GetSiblingIndex(), text.GetSiblingIndex());
            text.SetSiblingIndex(topIndex);
        }
        else
        {
            // Text under bar if overlay desired
            int idx = Mathf.Min(bar.GetSiblingIndex(), text.GetSiblingIndex());
            text.SetSiblingIndex(idx);
        }
    }

    // ---------- Visual Polish Helpers ----------
    void EnsureCanvasGroups()
    {
        AddCanvasGroupIfMissing(upgradePanel);
        AddCanvasGroupIfMissing(gameOverPanel);
        AddCanvasGroupIfMissing(pausePanel);
    }

    void AddCanvasGroupIfMissing(GameObject go)
    {
        if (go == null) return;
        var cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        cg.alpha = go.activeSelf ? 1f : 0f;
        cg.interactable = go.activeSelf;
        cg.blocksRaycasts = go.activeSelf;
    }

    public void ShowUpgradePanel()
    {
    if (upgradePanel == null) return;
    // Ensure layout, headers and buttons
    LayoutPanelCentered(upgradePanel, upgradePanelSize);
    EnsureUpgradeHeaderContent();
    EnsureUpgradeButtons(3);
    LayoutUpgradePanelTexts(upgradePanel);
    LayoutUpgradeButtons(upgradePanel);

    ShowPanelAnimated(upgradePanel);
    Time.timeScale = 0f;
    }

    public void HideUpgradePanel()
    {
        if (upgradePanel == null) return;
        HidePanelAnimated(upgradePanel);
        Time.timeScale = 1f;
    }

    // Ensure the upgrade panel has proper header texts and content
    void EnsureUpgradeHeaderContent()
    {
        if (upgradePanel == null) return;

        // Resolve or create title/description TMP texts
        if (upgradeTitleText == null)
        {
            upgradeTitleText = TryResolveUpgradeTitle(upgradePanel);
            if (upgradeTitleText == null)
            {
                var go = new GameObject("UpgradeTitle");
                go.transform.SetParent(upgradePanel.transform, false);
                upgradeTitleText = go.AddComponent<TextMeshProUGUI>();
                // Basic TMP settings
                upgradeTitleText.fontSize = baseFontSize + 8;
                upgradeTitleText.alignment = TextAlignmentOptions.Center;
                upgradeTitleText.enableAutoSizing = useAutoSizing;
                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(upgradePanelSize.x - panelPadding * 2f, upgradeTitleHeight);
            }
        }

        if (upgradeDescriptionText == null)
        {
            upgradeDescriptionText = TryResolveUpgradeDescription(upgradePanel);
            if (upgradeDescriptionText == null)
            {
                var go = new GameObject("UpgradeSubtitle");
                go.transform.SetParent(upgradePanel.transform, false);
                upgradeDescriptionText = go.AddComponent<TextMeshProUGUI>();
                upgradeDescriptionText.fontSize = baseFontSize + 2;
                upgradeDescriptionText.alignment = TextAlignmentOptions.Center;
                upgradeDescriptionText.enableAutoSizing = useAutoSizing;
                upgradeDescriptionText.enableWordWrapping = true;
                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(upgradePanelSize.x - panelPadding * 2f, upgradeDescHeight);
            }
        }

        // Set text content: Level and subtitle
        if (experienceSystem == null) experienceSystem = FindObjectOfType<ExperienceSystem>();
        int lvl = experienceSystem != null ? experienceSystem.GetCurrentLevel() : 1;
        if (upgradeTitleText != null) upgradeTitleText.text = $"Seviye {lvl}";
        if (upgradeDescriptionText != null) upgradeDescriptionText.text = "Geliştirmeler";
    }

    void EnsureUpgradeButtons(int targetCount)
    {
        if (upgradePanel == null) return;
        var existing = upgradePanel.GetComponentsInChildren<Button>(true);
        int need = Mathf.Max(0, targetCount - (existing != null ? existing.Length : 0));
        for (int i = 0; i < need; i++)
        {
            var btnGO = new GameObject($"UpgradeButton_{(existing.Length + i + 1)}");
            btnGO.transform.SetParent(upgradePanel.transform, false);
            var rt = btnGO.AddComponent<RectTransform>();
            rt.sizeDelta = upgradeButtonSize;
            var img = btnGO.AddComponent<Image>();
            img.color = new Color(0.15f, 0.2f, 0.25f, 0.9f);
            var btn = btnGO.AddComponent<Button>();
            var colors = btn.colors; colors.normalColor = img.color; colors.highlightedColor = new Color(0.22f,0.28f,0.35f,1f); colors.pressedColor = new Color(0.12f,0.16f,0.2f,1f); btn.colors = colors;
            // Label
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(btnGO.transform, false);
            var lrt = labelGO.AddComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0,0); lrt.anchorMax = new Vector2(1,1); lrt.offsetMin = new Vector2(12,8); lrt.offsetMax = new Vector2(-12,-8);
            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center; tmp.fontSize = baseFontSize; tmp.text = "Seçenek";
        }
    }

    void ShowPanelAnimated(GameObject panel)
    {
        if (panel == null) return;
        panel.SetActive(true);
        var cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
        var rt = panel.GetComponent<RectTransform>();
        StartCoroutine(FadeAndScale(cg, rt, true));
    }

    void HidePanelAnimated(GameObject panel)
    {
        if (panel == null) return;
        var cg = panel.GetComponent<CanvasGroup>();
        var rt = panel.GetComponent<RectTransform>();
        StartCoroutine(FadeAndScale(cg, rt, false, () => panel.SetActive(false)));
    }

    System.Collections.IEnumerator FadeAndScale(CanvasGroup cg, RectTransform rt, bool show, System.Action onDone = null)
    {
        float t = 0f;
        float dur = 0.18f;
        Vector3 start = show ? Vector3.one * 0.9f : Vector3.one;
        Vector3 end = show ? Vector3.one : Vector3.one * 0.9f;
        float a0 = show ? 0f : 1f;
        float a1 = show ? 1f : 0f;
        if (rt != null) rt.localScale = start;
        if (cg != null) cg.alpha = a0;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / dur);
            if (rt != null) rt.localScale = Vector3.LerpUnclamped(start, end, k);
            if (cg != null) cg.alpha = Mathf.LerpUnclamped(a0, a1, k);
            yield return null;
        }
        if (rt != null) rt.localScale = end;
        if (cg != null) cg.alpha = a1;
        onDone?.Invoke();
    }

    void AddTextEffects(TMP_Text tmp, Color32 outline, Color32 shadow, Vector2 shadowDist)
    {
        if (tmp == null) return;
        var outlineComp = tmp.GetComponent<Outline>();
        if (outlineComp == null) outlineComp = tmp.gameObject.AddComponent<Outline>();
        outlineComp.effectColor = outline;
        outlineComp.effectDistance = new Vector2(1.2f, -1.2f);

        var shadowComp = tmp.GetComponent<Shadow>();
        if (shadowComp == null) shadowComp = tmp.gameObject.AddComponent<Shadow>();
        shadowComp.effectColor = shadow;
        shadowComp.effectDistance = shadowDist;
    }

    void EnsureDamageOverlay()
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        if (damageOverlay != null) return;
        var go = new GameObject("DamageOverlay");
        go.transform.SetParent(canvas.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.pivot = new Vector2(0.5f, 0.5f); rt.anchoredPosition = Vector2.zero; rt.sizeDelta = Vector2.zero;
        damageOverlay = go.AddComponent<Image>();
        damageOverlay.color = new Color(1f, 0f, 0f, 0f);
        go.SetActive(true);
    }

    public void FlashDamage(float maxAlpha = 0.35f)
    {
        if (damageOverlay == null) return;
        if (damageFlashCo != null) StopCoroutine(damageFlashCo);
        damageFlashCo = StartCoroutine(DamageFlashRoutine(maxAlpha));
    }

    System.Collections.IEnumerator DamageFlashRoutine(float maxAlpha)
    {
        float up = 0.06f;
        float down = 0.25f;
        float t = 0f;
        while (t < up)
        {
            t += Time.unscaledDeltaTime;
            float k = t / up;
            var c = damageOverlay.color; c.a = Mathf.Lerp(0f, maxAlpha, k); damageOverlay.color = c; yield return null;
        }
        t = 0f;
        while (t < down)
        {
            t += Time.unscaledDeltaTime;
            float k = t / down;
            var c = damageOverlay.color; c.a = Mathf.Lerp(maxAlpha, 0f, k); damageOverlay.color = c; yield return null;
        }
        var c2 = damageOverlay.color; c2.a = 0f; damageOverlay.color = c2;
    }

    public void PulseXP()
    {
    if (!enableXPPulse) return;
    if (experienceSystem == null || experienceSystem.xpText == null) return;
        StartCoroutine(Pulse(experienceSystem.xpText.rectTransform));
    }

    System.Collections.IEnumerator Pulse(RectTransform rt)
    {
        if (rt == null) yield break;
        Vector3 start = rt.localScale;
        Vector3 big = start * 1.12f;
        float up = 0.08f, down = 0.12f; float t = 0f;
        while (t < up)
        {
            t += Time.unscaledDeltaTime; float k = t / up; rt.localScale = Vector3.LerpUnclamped(start, big, k); yield return null;
        }
        t = 0f;
        while (t < down)
        {
            t += Time.unscaledDeltaTime; float k = t / down; rt.localScale = Vector3.LerpUnclamped(big, start, k); yield return null;
        }
        rt.localScale = start;
    }

    // (Background-related code removed by request)
}
