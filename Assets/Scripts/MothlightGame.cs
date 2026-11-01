using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Mothlight
{
    public sealed class MothlightGame : MonoBehaviour
    {
        enum State { Title, Playing, Paused, Won, Lost }
        sealed class SpriteThing { public GameObject go; public SpriteRenderer sr; public Vector2 velocity; public float phase; }
        sealed class Spark { public GameObject go; public SpriteRenderer sr; public Vector2 velocity; public bool gathered; public float phase; }
        sealed class Puff { public GameObject go; public SpriteRenderer sr; public Vector2 velocity; public float life, full; }

        const float HalfX = 7.55f, HalfY = 4.0f;
        readonly List<Spark> sparks = new List<Spark>();
        readonly List<SpriteThing> spirits = new List<SpriteThing>();
        readonly List<Puff> puffs = new List<Puff>();
        readonly List<Transform> orbiters = new List<Transform>();
        State state = State.Title;
        Camera cam;
        GameObject moth, flower, ring, garden;
        SpriteRenderer mothRenderer, flowerRenderer, ringRenderer;
        Vector2 velocity, knockback;
        float timeLeft, spawnClock, invulnerability, pulseClock, shake, titleClock;
        int delivered, hits;
        bool humming, soundOn = true;
        static Font cachedFont;
        Canvas canvas;
        Text hud, prompt, titleText, subtitle, footer, pauseTitle;
        Image hudPanel, progressFill;
        GameObject titlePanel, pausePanel, endPanel;
        Text endTitle, endStats;
        Button startButton, resumeButton, retryButton, soundButton;
        AudioSource music, sfx;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindFirstObjectByType<MothlightGame>()) return;
            var root = new GameObject("Mothlight Runtime"); DontDestroyOnLoad(root); root.AddComponent<MothlightGame>();
        }

        void Awake()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Application.targetFrameRate = -1;
#else
            Application.targetFrameRate = 60; Screen.SetResolution(1280, 800, FullScreenMode.Windowed);
#endif
            SetupCamera(); SetupWorld(); SetupAudio(); SetupUi(); ShowState(State.Title);
        }

        void SetupCamera()
        {
            cam = Camera.main;
            if (!cam) { var c = new GameObject("Garden Camera"); c.tag = "MainCamera"; cam = c.AddComponent<Camera>(); }
            cam.orthographic = true; cam.orthographicSize = 5f; cam.backgroundColor = MothPalette.Night; cam.transform.position = new Vector3(0, 0, -10);
            cam.clearFlags = CameraClearFlags.SolidColor;
        }

        GameObject SpriteObject(string name, Sprite sprite, Vector3 pos, Vector2 scale, Color tint, int order)
        {
            var go = new GameObject(name); go.transform.position = pos; go.transform.localScale = new Vector3(scale.x, scale.y, 1);
            var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = sprite; sr.color = tint; sr.sortingOrder = order; return go;
        }

        void SetupWorld()
        {
            garden = new GameObject("The Moonlit Garden");
            var bg = SpriteObject("Twilight wash", GardenArt.Background(), Vector3.zero, new Vector2(18.0f, 10.2f), Color.white, -20); bg.transform.SetParent(garden.transform);
            for (int i = 0; i < 28; i++)
            {
                float x = -7.4f + (i * 2.83f % 14.8f), y = -3.8f + ((i * 1.71f) % 7.6f);
                var leaf = SpriteObject("Moon fern", GardenArt.Get("leaf"), new Vector3(x, y, 0), new Vector2(.42f + (i % 3) * .08f, .31f), new Color(1, 1, 1, .52f), -5);
                leaf.transform.rotation = Quaternion.Euler(0, 0, (i * 37) % 360); leaf.transform.SetParent(garden.transform);
            }
            flower = SpriteObject("Moonflower", GardenArt.Get("flower"), Vector3.zero, new Vector2(1.65f, 1.65f), Color.white, 2);
            moth = SpriteObject("Moth", GardenArt.Get("moth"), new Vector3(0, -2.2f), new Vector2(.83f, .83f), Color.white, 8); mothRenderer = moth.GetComponent<SpriteRenderer>();
            ring = SpriteObject("Humming ring", GardenArt.Get("ring"), moth.transform.position, new Vector2(1, 1), Color.white, 7); ringRenderer = ring.GetComponent<SpriteRenderer>(); ring.SetActive(false);
            flowerRenderer = flower.GetComponent<SpriteRenderer>();
            for (int i = 0; i < 8; i++) SpawnSpark(true);
            SpawnSpirit(new Vector2(-5.8f, 2.6f));
        }

        void SetupAudio()
        {
            music = gameObject.AddComponent<AudioSource>(); music.loop = true; music.volume = .2f; music.clip = BuildAmbient(); music.Play();
            sfx = gameObject.AddComponent<AudioSource>(); sfx.playOnAwake = false; sfx.volume = .6f;
        }

        AudioClip BuildAmbient()
        {
            const int rate = 22050, seconds = 8; int count = rate * seconds; var data = new float[count];
            for (int i = 0; i < count; i++) { float t = i / (float)rate; float swell = .7f + .3f * Mathf.Sin(t * Mathf.PI * 2f / seconds); data[i] = swell * (.13f * Mathf.Sin(t * Mathf.PI * 2f * 110f) + .08f * Mathf.Sin(t * Mathf.PI * 2f * 164.8f) + .045f * Mathf.Sin(t * Mathf.PI * 2f * 220f)); }
            var clip = AudioClip.Create("Moon garden hush", count, 1, rate, false); clip.SetData(data, 0); return clip;
        }
        void Tone(float f0, float f1, float duration, float gain = .28f)
        {
            if (!soundOn) return; int rate = 22050, n = Mathf.CeilToInt(rate * duration); var data = new float[n];
            for (int i = 0; i < n; i++) { float x = i / (float)n, f = Mathf.Lerp(f0, f1, x), env = Mathf.Sin(Mathf.PI * x) * (1f - x * .38f); data[i] = Mathf.Sin(2 * Mathf.PI * f * x * duration) * env * gain; }
            var clip = AudioClip.Create("Mothlight chime", n, 1, rate, false); clip.SetData(data, 0); sfx.pitch = Random.Range(.96f, 1.04f); sfx.PlayOneShot(clip); Destroy(clip, duration + .15f);
        }

        void SetupUi()
        {
            var es = FindFirstObjectByType<EventSystem>(); if (!es) { var e = new GameObject("EventSystem", typeof(EventSystem)); es = e.GetComponent<EventSystem>(); }
            if (!es.GetComponent<InputSystemUIInputModule>()) { var module = es.gameObject.AddComponent<InputSystemUIInputModule>(); module.AssignDefaultActions(); }
            var c = new GameObject("Moonlit Garden UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = c.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 50;
            var scaler = c.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
#if UNITY_WEBGL && !UNITY_EDITOR
            scaler.referenceResolution = new Vector2(960, 600);
#else
            scaler.referenceResolution = new Vector2(1280, 800);
#endif
            scaler.matchWidthOrHeight = .5f;

            hudPanel = Panel("HUD", new Vector2(0, 1), new Vector2(0, 1), new Vector2(32, -30), new Vector2(500, 126), new Color(MothPalette.Night.r, MothPalette.Night.g, MothPalette.Night.b, .78f));
            hud = Label(hudPanel.transform, "HUD text", "", 25, MothPalette.Moon, TextAnchor.UpperLeft, new Vector2(22, -14), new Vector2(450, 95));
            hud.fontStyle = FontStyle.Bold;
            var bar = new GameObject("Bloom meter", typeof(RectTransform), typeof(Image)); bar.transform.SetParent(hudPanel.transform, false);
            var br = bar.GetComponent<RectTransform>(); br.anchorMin = new Vector2(0, 0); br.anchorMax = new Vector2(1, 0); br.pivot = new Vector2(0, 0); br.anchoredPosition = new Vector2(20, 18); br.sizeDelta = new Vector2(-40, 9);
            bar.GetComponent<Image>().color = new Color(MothPalette.Fern.r, MothPalette.Fern.g, MothPalette.Fern.b, .8f);
            var fill = new GameObject("Bloom light", typeof(RectTransform), typeof(Image)); fill.transform.SetParent(bar.transform, false); var fr = fill.GetComponent<RectTransform>(); fr.anchorMin = Vector2.zero; fr.anchorMax = Vector2.one; fr.offsetMin = fr.offsetMax = Vector2.zero;
            progressFill = fill.GetComponent<Image>(); progressFill.sprite = GardenArt.Get("disc"); progressFill.color = MothPalette.Pollen; progressFill.type = Image.Type.Filled; progressFill.fillMethod = Image.FillMethod.Horizontal; progressFill.fillAmount = 0;
            prompt = Label(canvas.transform, "Action hint", "", 20, MothPalette.Moon, TextAnchor.MiddleCenter, new Vector2(0, 24), new Vector2(850, 48));
            prompt.rectTransform.anchorMin = prompt.rectTransform.anchorMax = new Vector2(.5f, 0); prompt.rectTransform.pivot = new Vector2(.5f, 0); prompt.rectTransform.anchoredPosition = new Vector2(0, 22);

            titlePanel = Panel("Title screen", new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero, new Vector2(720, 560), new Color(MothPalette.Night.r, MothPalette.Night.g, MothPalette.Night.b, .97f)).gameObject;
            titleText = Label(titlePanel.transform, "Title", "MOTHLIGHT", 64, MothPalette.Pollen, TextAnchor.MiddleCenter, new Vector2(0, 145), new Vector2(660, 90)); titleText.fontStyle = FontStyle.Bold;
            subtitle = Label(titlePanel.transform, "Subtitle", "LAST BLOOM", 28, MothPalette.Moth, TextAnchor.MiddleCenter, new Vector2(0, 98), new Vector2(660, 55));
            Label(titlePanel.transform, "Story", "One little moth. One last night.\nWake the moonflower before the dark settles in.", 23, MothPalette.Moon, TextAnchor.MiddleCenter, new Vector2(0, 10), new Vector2(620, 100));
            Label(titlePanel.transform, "Instructions", "WASD / ARROWS   flutter\nHOLD SPACE / CLICK   gather fireflies\nRELEASE   feed the flower nearby; scatter thorns anywhere\nCarry up to 6 lights  •  3 hits  •  3 minutes", 16, MothPalette.Moth, TextAnchor.MiddleCenter, new Vector2(0, -92), new Vector2(650, 130));
            startButton = Button(titlePanel.transform, "Begin the night", new Vector2(0, -220), new Vector2(260, 58), StartRun);
            soundButton = Button(titlePanel.transform, "Sound: ON", new Vector2(0, -278), new Vector2(150, 32), ToggleSound, false);
            footer = Label(canvas.transform, "Version", "A small night garden · 1.0", 16, new Color(MothPalette.Moon.r, MothPalette.Moon.g, MothPalette.Moon.b, .62f), TextAnchor.LowerRight, new Vector2(-24, 18), new Vector2(330, 30)); footer.rectTransform.anchorMin = footer.rectTransform.anchorMax = new Vector2(1, 0); footer.rectTransform.pivot = Vector2.one;

            pausePanel = Panel("Pause screen", new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero, new Vector2(460, 370), new Color(MothPalette.Night.r, MothPalette.Night.g, MothPalette.Night.b, .94f)).gameObject;
            pauseTitle = Label(pausePanel.transform, "Pause title", "A BREATH IN THE DARK", 31, MothPalette.Pollen, TextAnchor.MiddleCenter, new Vector2(0, 112), new Vector2(430, 58));
            resumeButton = Button(pausePanel.transform, "Keep going", new Vector2(0, 38), new Vector2(250, 54), Resume);
            Button(pausePanel.transform, "Start over", new Vector2(0, -30), new Vector2(250, 54), StartRun);
            Button(pausePanel.transform, "Return to title", new Vector2(0, -98), new Vector2(250, 54), ToTitle);

            endPanel = Panel("End screen", new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero, new Vector2(500, 390), new Color(MothPalette.Night.r, MothPalette.Night.g, MothPalette.Night.b, .95f)).gameObject;
            endTitle = Label(endPanel.transform, "End title", "", 48, MothPalette.Pollen, TextAnchor.MiddleCenter, new Vector2(0, 110), new Vector2(460, 68)); endTitle.fontStyle = FontStyle.Bold;
            endStats = Label(endPanel.transform, "End detail", "", 23, MothPalette.Moon, TextAnchor.MiddleCenter, new Vector2(0, 35), new Vector2(450, 70));
            retryButton = Button(endPanel.transform, "Try another night", new Vector2(0, -45), new Vector2(270, 55), StartRun);
            Button(endPanel.transform, "Return to title", new Vector2(0, -112), new Vector2(250, 50), ToTitle);
            ShowState(State.Title);
        }

        Font UiFont()
        {
            if (!cachedFont) { cachedFont = Font.CreateDynamicFontFromOSFont(new[] { "Avenir Next", "Avenir" }, 24); if (!cachedFont) cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf"); }
            return cachedFont;
        }
        Image Panel(string name, Vector2 amin, Vector2 amax, Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(canvas ? canvas.transform : null, false);
            var rt = go.GetComponent<RectTransform>(); rt.anchorMin = amin; rt.anchorMax = amax; rt.pivot = (amin + amax) * .5f; rt.anchoredPosition = pos; rt.sizeDelta = size;
            go.GetComponent<Image>().color = color; return go.GetComponent<Image>();
        }
        Text Label(Transform parent, string name, string value, int size, Color color, TextAnchor anchor, Vector2 pos, Vector2 dimensions)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false); var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(.5f, .5f); rt.pivot = new Vector2(.5f, .5f); rt.anchoredPosition = pos; rt.sizeDelta = dimensions;
            var text = go.GetComponent<Text>(); text.font = UiFont(); text.text = value; text.fontSize = size; text.color = color; text.alignment = anchor; text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Overflow; text.lineSpacing = 1.5f; text.supportRichText = false; return text;
        }
        Button Button(Transform parent, string value, Vector2 pos, Vector2 dimensions, UnityEngine.Events.UnityAction action, bool primary = true)
        {
            var go = new GameObject(value + " Button", typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent, false); var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(.5f, .5f); rt.pivot = new Vector2(.5f, .5f); rt.anchoredPosition = pos; rt.sizeDelta = dimensions;
            var img = go.GetComponent<Image>(); img.color = primary ? MothPalette.Fern : new Color(MothPalette.Deep.r, MothPalette.Deep.g, MothPalette.Deep.b, .85f);
            var b = go.GetComponent<Button>(); b.targetGraphic = img; b.onClick.AddListener(action);
            var cb = b.colors; cb.normalColor = img.color; cb.highlightedColor = Color.Lerp(img.color, MothPalette.Moon, .22f); cb.pressedColor = MothPalette.Pollen; cb.selectedColor = cb.highlightedColor; b.colors = cb;
            var label = Label(go.transform, "Label", value.ToUpperInvariant(), primary ? 18 : 15, MothPalette.Moon, TextAnchor.MiddleCenter, Vector2.zero, dimensions - new Vector2(12, 4)); label.fontStyle = FontStyle.Bold;
            return b;
        }

        void ShowState(State s)
        {
            state = s; titlePanel.SetActive(s == State.Title); pausePanel.SetActive(s == State.Paused); endPanel.SetActive(s == State.Won || s == State.Lost);
            hudPanel.gameObject.SetActive(s == State.Playing || s == State.Paused); prompt.gameObject.SetActive(s == State.Playing); footer.gameObject.SetActive(s == State.Title);
            if (s == State.Title) { Time.timeScale = 1; moth.transform.position = new Vector3(0, -2.2f); }
            if (s == State.Won || s == State.Lost) {
                Time.timeScale = 1; endTitle.text = s == State.Won ? "BLOOM!" : "NIGHT FALLS"; endTitle.color = s == State.Won ? MothPalette.Pollen : MothPalette.Moth;
                int seconds = Mathf.CeilToInt(timeLeft); endStats.text = s == State.Won ? "You woke all three petals.\nThe garden remembers your light." : "You delivered " + delivered + " of 21 lights.\n" + (timeLeft <= 0 ? "The last blue hour slipped away." : "The thorns caught your little wings.");
                Tone(s == State.Won ? 523 : 185, s == State.Won ? 784 : 110, .7f, .32f);
            }
            RefreshHud();
        }

        void StartRun()
        {
            Time.timeScale = 1; delivered = hits = 0; timeLeft = MothlightRules.NightSeconds; spawnClock = 0; invulnerability = 0; velocity = knockback = Vector2.zero; humming = false;
            moth.transform.position = new Vector3(0, -2.35f); ClearOrbs(); previousHum = false;
            foreach (var s in sparks) Destroy(s.go); sparks.Clear(); foreach (var e in spirits) Destroy(e.go); spirits.Clear();
            for (int i = 0; i < 9; i++) SpawnSpark(true); SpawnSpirit(new Vector2(-5.8f, 2.6f));
            ShowState(State.Playing); Tone(440, 660, .18f);
        }
        void ToTitle() { Time.timeScale = 1; ClearOrbs(); ShowState(State.Title); }
        void Resume() { Time.timeScale = 1; ShowState(State.Playing); }
        void ToggleSound() { soundOn = !soundOn; if (music) music.volume = soundOn ? .2f : 0; SetButtonText(soundButton, "Sound: " + (soundOn ? "ON" : "OFF")); }
        void SetButtonText(Button button, string value) { if (button) button.GetComponentInChildren<Text>().text = value.ToUpperInvariant(); }
        void ClearOrbs() { foreach (var t in orbiters) if (t) Destroy(t.gameObject); orbiters.Clear(); }

        void Update()
        {
            float dt = Time.unscaledDeltaTime; titleClock += dt;
            if (state == State.Title) { moth.transform.position = new Vector3(Mathf.Sin(titleClock * .7f) * .3f, -2.2f + Mathf.Sin(titleClock * 1.4f) * .13f); moth.transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(titleClock) * 7); float idleBeat = 1f + .07f * Mathf.Sin(titleClock * 8f); moth.transform.localScale = new Vector3(.83f * idleBeat, .83f, 1); return; }
            if (KeyPressed(Key.Escape)) { if (state == State.Playing) { ShowState(State.Paused); Time.timeScale = 0; } else if (state == State.Paused) Resume(); else if (state == State.Won || state == State.Lost) ToTitle(); return; }
            if (state == State.Paused) return;
            if (state != State.Playing) return;
            if (KeyPressed(Key.R)) { StartRun(); return; }
            UpdateGame(Time.deltaTime, dt);
        }
        bool KeyPressed(Key key) { return Keyboard.current != null && Keyboard.current[key].wasPressedThisFrame; }
        bool KeyHeld(Key key) { return Keyboard.current != null && Keyboard.current[key].isPressed; }
        bool HumHeld() { return KeyHeld(Key.Space) || (Mouse.current != null && Mouse.current.leftButton.isPressed); }

        void UpdateGame(float dt, float unscaledDt)
        {
            timeLeft -= dt; spawnClock += dt; invulnerability -= dt; pulseClock += dt; shake = Mathf.Max(0, shake - unscaledDt * 5f);
            if (spawnClock > Mathf.Max(.72f, 2.2f - (MothlightRules.NightSeconds - timeLeft) / 120f)) { spawnClock = 0; if (sparks.Count < 22) SpawnSpark(false); }
            int desired = 1 + Mathf.FloorToInt((MothlightRules.NightSeconds - timeLeft) / 60f); while (spirits.Count < desired && spirits.Count < 3) SpawnSpirit(RandomEdge());
            Vector2 input = Vector2.zero;
            if (KeyHeld(Key.W) || KeyHeld(Key.UpArrow)) input.y += 1; if (KeyHeld(Key.S) || KeyHeld(Key.DownArrow)) input.y -= 1;
            if (KeyHeld(Key.D) || KeyHeld(Key.RightArrow)) input.x += 1; if (KeyHeld(Key.A) || KeyHeld(Key.LeftArrow)) input.x -= 1;
            if (input.sqrMagnitude > 1) input.Normalize();
            float speed = HumHeld() ? 3.45f : 4.6f;
            velocity = Vector2.MoveTowards(velocity, input * speed, (input.sqrMagnitude > 0 ? 32 : 15) * dt); velocity += knockback; knockback = Vector2.MoveTowards(knockback, Vector2.zero, 13 * dt);
            Vector2 pos = (Vector2)moth.transform.position + velocity * dt; pos.x = Mathf.Clamp(pos.x, -HalfX, HalfX); pos.y = Mathf.Clamp(pos.y, -HalfY, HalfY);
            velocity = MothlightRules.RemoveOutwardVelocityAtBounds(pos, velocity, HalfX, HalfY); moth.transform.position = pos;
            if (input.sqrMagnitude > .03f) moth.transform.rotation = Quaternion.Euler(0, 0, -input.x * 10);
            float wingBeat = 1f + (humming ? .13f : .055f) * Mathf.Sin(Time.time * (humming ? 24f : 8f)); moth.transform.localScale = new Vector3(.83f * wingBeat, .83f, 1);
            mothRenderer.color = invulnerability > 0 && Mathf.Sin(Time.unscaledTime * 24) > 0 ? new Color(1, 1, 1, .35f) : Color.white;
            humming = HumHeld(); UpdateSparks(dt); UpdateOrbiters(); UpdateSpirits(dt); UpdatePuffs(unscaledDt);
            HandleHumEdge();
            ring.transform.position = moth.transform.position; ring.transform.localScale = Vector3.one * (humming ? 1.45f + .09f * Mathf.Sin(Time.time * 9) : 1);
            ring.SetActive(humming); flower.transform.localScale = Vector3.one * (1.65f + .055f * Mathf.Sin(Time.time * 3.5f));
            flowerRenderer.color = Color.Lerp(MothPalette.Lavender, MothPalette.Pollen, .18f + .12f * Mathf.Sin(Time.time * 2.7f));
            cam.transform.position = new Vector3(shake > .02f ? Random.Range(-shake, shake) : 0, shake > .02f ? Random.Range(-shake, shake) : 0, -10);
            prompt.text = orbiters.Count == 0 ? (humming ? "LISTEN… the little lights are coming" : "HOLD SPACE / CLICK TO GATHER") : (Vector2.Distance(moth.transform.position, flower.transform.position) < 2.15f ? "RELEASE TO FEED THE MOONFLOWER  ·  " + orbiters.Count + "/6" : "CARRY THE LIGHT HOME  ·  RELEASE TO SCATTER THORNS");
            RefreshHud();
            if (MothlightRules.Won(delivered)) ShowState(State.Won); else if (MothlightRules.Lost(hits, timeLeft)) ShowState(State.Lost);
        }

        bool previousHum;
        void HandleHumEdge()
        {
            if (humming && !previousHum) { previousHum = true; ringRenderer.color = new Color(MothPalette.Pollen.r, MothPalette.Pollen.g, MothPalette.Pollen.b, .7f); }
            if (!humming && previousHum) { previousHum = false; ReleasePulse(); }
        }
        void RefreshHud()
        {
            if (!hud) return; int secs = Mathf.Max(0, Mathf.CeilToInt(timeLeft)); int carried = orbiters.Count;
            hud.text = string.Format("NIGHT  {0:00}:{1:00}      LIGHT  {2:00}/21\n{3}      CARRY  {4}/{5}", secs / 60, secs % 60, delivered, new string('♥', Mathf.Clamp(MothlightRules.HitsAllowed - hits, 0, 3)), carried, MothlightRules.CarryLimit);
            if (progressFill) progressFill.fillAmount = delivered / 21f;
        }

        void SpawnSpark(bool initial)
        {
            Vector2 p = RandomGardenPoint(); if (initial && Vector2.Distance(p, Vector2.zero) < 2.4f) p = RandomGardenPoint();
            var go = SpriteObject("Drifting firefly", GardenArt.Get("firefly"), p, new Vector2(.36f, .36f), Color.white, 3);
            sparks.Add(new Spark { go = go, sr = go.GetComponent<SpriteRenderer>(), velocity = Random.insideUnitCircle * .35f, phase = Random.value * 10 });
        }
        Vector2 RandomGardenPoint() => new Vector2(Random.Range(-HalfX + .3f, HalfX - .3f), Random.Range(-HalfY + .3f, HalfY - .3f));
        Vector2 RandomEdge() { int side = Random.Range(0, 4); return side == 0 ? new Vector2(-7, Random.Range(-3.5f, 3.5f)) : side == 1 ? new Vector2(7, Random.Range(-3.5f, 3.5f)) : side == 2 ? new Vector2(Random.Range(-6.8f, 6.8f), 3.7f) : new Vector2(Random.Range(-6.8f, 6.8f), -3.7f); }
        void UpdateSparks(float dt)
        {
            for (int i = sparks.Count - 1; i >= 0; i--)
            {
                var s = sparks[i]; if (!s.go) { sparks.RemoveAt(i); continue; } s.phase += dt;
                Vector2 p = s.go.transform.position; s.velocity += Random.insideUnitCircle * dt * .11f;
                if (humming && !s.gathered && orbiters.Count < MothlightRules.CarryLimit && Vector2.Distance(p, moth.transform.position) < 4.2f) s.gathered = true;
                if (s.gathered) { Vector2 d = (Vector2)moth.transform.position - p; s.velocity = Vector2.MoveTowards(s.velocity, d.normalized * 7f, dt * 20); }
                else s.velocity = Vector2.ClampMagnitude(s.velocity, .45f);
                p += s.velocity * dt; p.x = Mathf.Clamp(p.x, -HalfX, HalfX); p.y = Mathf.Clamp(p.y, -HalfY, HalfY); s.go.transform.position = p;
                if (s.gathered && Vector2.Distance(p, moth.transform.position) < .48f)
                {
                    Vector2 orbit = (Vector2)moth.transform.position + Random.insideUnitCircle.normalized * .65f; var orb = SpriteObject("Held moonlight", GardenArt.Get("firefly"), orbit, new Vector2(.32f, .32f), Color.white, 10); orbiters.Add(orb.transform);
                    Destroy(s.go); sparks.RemoveAt(i); Tone(Random.Range(700, 920), Random.Range(1050, 1400), .12f, .15f); AddPuff(p, 4, MothPalette.Pollen); continue;
                }
                s.sr.transform.localScale = Vector3.one * (.32f + Mathf.Sin(s.phase * 4) * .045f);
            }
        }
        void UpdateOrbiters()
        {
            for (int i = 0; i < orbiters.Count; i++) if (orbiters[i]) { float a = Time.time * 2.2f + i * Mathf.PI * 2f / orbiters.Count; orbiters[i].position = moth.transform.position + new Vector3(Mathf.Cos(a) * .72f, Mathf.Sin(a) * .50f, 0); }
        }
        void ReleasePulse()
        {
            if (orbiters.Count == 0) { Tone(330, 240, .09f, .08f); return; }
            int n = orbiters.Count; bool near = Vector2.Distance(moth.transform.position, flower.transform.position) < 2.15f;
            if (near) { delivered = MothlightRules.Delivered(delivered, n); AddPuff(flower.transform.position, 28, MothPalette.Pollen); flower.transform.localScale *= 1.25f; Tone(523, 784, .35f, .34f); }
            else { AddPuff(moth.transform.position, 18 + n * 3, MothPalette.Lavender); Tone(360, 520 + n * 60, .24f, .3f); shake = .12f; }
            foreach (var enemy in spirits) { Vector2 d = (Vector2)enemy.go.transform.position - (Vector2)moth.transform.position; if (d.magnitude < 3.1f) { enemy.velocity += d.normalized * (4.8f + n * .35f); AddPuff(enemy.go.transform.position, 6, MothPalette.Spirit); } }
            ClearOrbs(); pulseClock = 0;
        }
        void SpawnSpirit(Vector2 at)
        {
            var go = SpriteObject("Thorn spirit", GardenArt.Get("thorn"), at, new Vector2(.72f, .72f), Color.white, 5);
            spirits.Add(new SpriteThing { go = go, sr = go.GetComponent<SpriteRenderer>(), phase = Random.value * 5 });
        }
        void UpdateSpirits(float dt)
        {
            foreach (var e in spirits)
            {
                e.phase += dt; Vector2 p = e.go.transform.position, d = (Vector2)moth.transform.position - p; float aggression = 1 + (MothlightRules.NightSeconds - timeLeft) * .0021f;
                Vector2 chase = d.sqrMagnitude > .01f ? d.normalized * (1.08f * aggression) : Vector2.zero; e.velocity = Vector2.MoveTowards(e.velocity, chase, dt * 2.4f);
                p += e.velocity * dt; p.x = Mathf.Clamp(p.x, -HalfX, HalfX); p.y = Mathf.Clamp(p.y, -HalfY, HalfY); e.go.transform.position = p;
                e.go.transform.localScale = Vector3.one * (.72f + Mathf.Sin(e.phase * 4) * .035f); e.go.transform.Rotate(0, 0, 25 * dt);
                if (invulnerability <= 0 && Vector2.Distance(p, moth.transform.position) < .67f) { hits++; invulnerability = 1.25f; Vector2 kick = ((Vector2)moth.transform.position - p).normalized; knockback = kick * 7; e.velocity = -kick * 3; shake = .22f; AddPuff(moth.transform.position, 12, MothPalette.Spirit); Tone(180, 80, .25f, .28f); }
            }
        }
        void AddPuff(Vector2 at, int count, Color color)
        {
            for (int i = 0; i < count; i++) { float a = Random.value * Mathf.PI * 2, speed = Random.Range(.3f, 3.4f); var go = SpriteObject("Night mote", GardenArt.Get("firefly"), at, Vector2.one * Random.Range(.07f, .19f), color, 12);
                puffs.Add(new Puff { go = go, sr = go.GetComponent<SpriteRenderer>(), velocity = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * speed, life = Random.Range(.35f, .78f), full = .78f }); go.transform.rotation = Quaternion.Euler(0, 0, 0); go.GetComponent<SpriteRenderer>().color = color; go.transform.SetParent(null); }
        }
        void UpdatePuffs(float dt)
        {
            for (int i = puffs.Count - 1; i >= 0; i--) { var p = puffs[i]; p.life -= dt; if (!p.go || p.life <= 0) { if (p.go) Destroy(p.go); puffs.RemoveAt(i); continue; }
                p.go.transform.position += (Vector3)(p.velocity * dt); p.velocity *= .96f; p.sr.color = new Color(p.sr.color.r, p.sr.color.g, p.sr.color.b, Mathf.Clamp01(p.life / p.full)); }
        }
    }
}
