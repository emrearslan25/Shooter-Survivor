using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Light-weight, inspector-driven UI styling helper similar to the sample you shared.
// Attach this to your Canvas, assign references, and click Play (or keep ExecuteInEditMode on) to style texts, buttons, and panels.
[ExecuteInEditMode]
public class GameUIStyler : MonoBehaviour
{
    [Header("Apply Mode")]
    public bool applyInEditMode = true;
    public bool applyOnStart = true;
    public bool autoDiscover = true;
    public Transform root; // optional; defaults to this.transform

    [Header("Text Styling")]
    public TMP_Text[] headlineTexts;       // e.g., Level, Timer
    public TMP_Text[] bodyTexts;           // e.g., HealthText, EnemyCount, Score, xpText
    public bool addTextOutline = true;
    public Color32 outlineColor = new Color32(0, 0, 0, 180);
    public Vector2 outlineDistance = new Vector2(1.2f, -1.2f);
    public bool addTextShadow = true;
    public Color32 shadowColor = new Color32(0, 0, 0, 140);
    public Vector2 shadowDistance = new Vector2(2f, -2f);
    public bool useAutoSizing = true;
    public int minAutoSize = 18;
    public int maxAutoSize = 36;

    [Header("Button Styling")]
    public Button[] buttons;               // e.g., Upgrade buttons, pause menu buttons
    public Color normalColor = new Color(1f, 1f, 1f, 0.08f);
    public Color highlightedColor = new Color(1f, 1f, 1f, 0.18f);
    public Color pressedColor = new Color(1f, 1f, 1f, 0.32f);
    public Color selectedColor = new Color(1f, 1f, 1f, 0.18f);
    public Color disabledColor = new Color(1f, 1f, 1f, 0.04f);
    public float buttonTextSize = 24f;

    [Header("Panel Styling")]
    public Image[] panelBackgrounds;       // Panels to tint (UpgradePanel, GameOverPanel, PausePanel)
    public Color panelColor = new Color(0.05f, 0.08f, 0.12f, 0.85f);
    public bool addPanelOutline = true;
    public Color panelOutlineColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);

    [Header("Slider Styling")]
    public bool styleSliders = true;
    public Color sliderFillColor = new Color(0.3f, 0.85f, 1f, 0.95f);
    public Color sliderBgColor = new Color(1f, 1f, 1f, 0.12f);
    public Slider[] sliders; // optional explicit assignment

    void OnEnable()
    {
        if (!Application.isPlaying && !applyInEditMode) return;
        ApplyTheme();
    }

    void Start()
    {
        if (Application.isPlaying && applyOnStart)
            ApplyTheme();
    }

    public void ApplyTheme()
    {
        if (root == null) root = this.transform;
        if (autoDiscover)
        {
            AutoDiscover();
        }
        StyleTexts(headlineTexts, true);
        StyleTexts(bodyTexts, false);
        StyleButtons();
        StylePanels();
        if (styleSliders) StyleSliders();
    }

    void StyleTexts(TMP_Text[] texts, bool isHeadline)
    {
        if (texts == null) return;
        foreach (var t in texts)
        {
            if (t == null) continue;
            // Auto sizing
            if (useAutoSizing)
            {
                t.enableAutoSizing = true;
                t.fontSizeMin = Mathf.Max(10, minAutoSize);
                t.fontSizeMax = Mathf.Max(minAutoSize + 1, maxAutoSize + (isHeadline ? 6 : 0));
            }
            // Outline
            if (addTextOutline)
            {
                var o = t.GetComponent<Outline>();
                if (o == null) o = t.gameObject.AddComponent<Outline>();
                o.effectColor = outlineColor;
                o.effectDistance = outlineDistance;
            }
            // Shadow
            if (addTextShadow)
            {
                var s = t.GetComponent<Shadow>();
                if (s == null) s = t.gameObject.AddComponent<Shadow>();
                s.effectColor = shadowColor;
                s.effectDistance = shadowDistance;
            }
        }
    }

    void StyleButtons()
    {
        if (buttons == null) return;
        foreach (var btn in buttons)
        {
            if (btn == null) continue;
            // Ensure an Image exists to show states
            var img = btn.GetComponent<Image>();
            if (img == null) img = btn.gameObject.AddComponent<Image>();
            img.color = normalColor;

            // Ensure a TMP child exists
            var tmp = btn.GetComponentInChildren<TMP_Text>();
            if (tmp == null)
            {
                var textObj = new GameObject("ButtonText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                textObj.transform.SetParent(btn.transform, false);
                var rt = textObj.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
                tmp = textObj.GetComponent<TMP_Text>();
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.text = btn.name;
            }
            if (!useAutoSizing)
            {
                tmp.enableAutoSizing = false;
                tmp.fontSize = buttonTextSize;
            }

            // Add subtle outline/shadow to button text
            if (addTextOutline)
            {
                var o = tmp.GetComponent<Outline>();
                if (o == null) o = tmp.gameObject.AddComponent<Outline>();
                o.effectColor = outlineColor;
                o.effectDistance = outlineDistance;
            }
            if (addTextShadow)
            {
                var s = tmp.GetComponent<Shadow>();
                if (s == null) s = tmp.gameObject.AddComponent<Shadow>();
                s.effectColor = shadowColor;
                s.effectDistance = shadowDistance;
            }

            // ColorBlock for interactivity
            var colors = btn.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = highlightedColor;
            colors.pressedColor = pressedColor;
            colors.selectedColor = selectedColor;
            colors.disabledColor = disabledColor;
            btn.colors = colors;
        }
    }

    void StylePanels()
    {
        if (panelBackgrounds == null) return;
        foreach (var img in panelBackgrounds)
        {
            if (img == null) continue;
            img.color = panelColor;
            if (addPanelOutline)
            {
                var o = img.GetComponent<Outline>();
                if (o == null) o = img.gameObject.AddComponent<Outline>();
                o.effectColor = panelOutlineColor;
                o.effectDistance = new Vector2(2f, -2f);
            }
        }
    }

    void StyleSliders()
    {
        Slider[] targetSliders = sliders != null && sliders.Length > 0 ? sliders : root.GetComponentsInChildren<Slider>(true);
        foreach (var s in targetSliders)
        {
            if (s == null) continue;
            // Background image
            var bg = s.transform.Find("Background")?.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = sliderBgColor;
            }
            // Fill image (common paths: Fill Area/Fill or Fill)
            Image fill = s.transform.Find("Fill Area/Fill")?.GetComponent<Image>();
            if (fill == null) fill = s.transform.Find("Fill")?.GetComponent<Image>();
            if (fill != null)
            {
                fill.color = sliderFillColor;
            }
            // Handle optional tint
            var handle = s.transform.Find("Handle Slide Area/Handle")?.GetComponent<Image>();
            if (handle != null)
            {
                var c = sliderFillColor; c.a = 0.9f; handle.color = c;
            }
        }
    }

    void AutoDiscover()
    {
        // Texts
        var tmps = root.GetComponentsInChildren<TMP_Text>(true);
        var heads = new System.Collections.Generic.List<TMP_Text>();
        var bodies = new System.Collections.Generic.List<TMP_Text>();
        foreach (var t in tmps)
        {
            if (t == null) continue;
            string n = t.name.ToLowerInvariant();
            if (n.Contains("level") || n.Contains("timer") || n.Contains("title") || n.Contains("header")) heads.Add(t);
            else bodies.Add(t);
        }
        if (headlineTexts == null || headlineTexts.Length == 0) headlineTexts = heads.ToArray();
        if (bodyTexts == null || bodyTexts.Length == 0) bodyTexts = bodies.ToArray();

        // Buttons
        if (buttons == null || buttons.Length == 0)
        {
            buttons = root.GetComponentsInChildren<Button>(true);
        }

        // Panels
        if (panelBackgrounds == null || panelBackgrounds.Length == 0)
        {
            var imgs = root.GetComponentsInChildren<Image>(true);
            var list = new System.Collections.Generic.List<Image>();
            foreach (var img in imgs)
            {
                if (img == null) continue;
                string n = img.name.ToLowerInvariant();
                if (n.Contains("panel")) list.Add(img);
                if (n.Contains("upgradepanel") || n.Contains("gameoverpanel") || n.Contains("pausepanel")) list.Add(img);
            }
            panelBackgrounds = list.ToArray();
        }

        // Sliders
        if (sliders == null || sliders.Length == 0)
        {
            sliders = root.GetComponentsInChildren<Slider>(true);
        }
    }
}
