using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mothlight
{
    public static class GardenArt
    {
        static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        static Sprite Make(string id, int w, int h, Action<Texture2D> paint)
        {
            if (sprites.TryGetValue(id, out var found)) return found;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear; tex.wrapMode = TextureWrapMode.Clamp;
            var clear = new Color32[w * h]; tex.SetPixels32(clear); paint(tex); tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(.5f, .5f), 64f);
            sprite.name = id; sprites[id] = sprite; return sprite;
        }
        static void Dot(Texture2D t, int x, int y, Color c)
        { if (x >= 0 && y >= 0 && x < t.width && y < t.height) t.SetPixel(x, y, c); }
        static void Ellipse(Texture2D t, float cx, float cy, float rx, float ry, Color c)
        {
            for (int y = Mathf.FloorToInt(cy - ry); y <= Mathf.CeilToInt(cy + ry); y++)
                for (int x = Mathf.FloorToInt(cx - rx); x <= Mathf.CeilToInt(cx + rx); x++)
                    if (Mathf.Pow((x - cx) / rx, 2) + Mathf.Pow((y - cy) / ry, 2) <= 1f) Dot(t, x, y, c);
        }
        static void Line(Texture2D t, Vector2 a, Vector2 b, int thick, Color c)
        {
            int n = Mathf.CeilToInt(Vector2.Distance(a, b) * 2f);
            for (int i = 0; i <= n; i++) { Vector2 p = Vector2.Lerp(a, b, n == 0 ? 0 : (float)i / n);
                for (int y = -thick; y <= thick; y++) for (int x = -thick; x <= thick; x++) Dot(t, Mathf.RoundToInt(p.x) + x, Mathf.RoundToInt(p.y) + y, c); }
        }
        public static Sprite Get(string id)
        {
            if (id == "moth") return Make(id, 64, 64, t => {
                Ellipse(t, 22, 39, 14, 11, MothPalette.Lavender); Ellipse(t, 42, 39, 14, 11, MothPalette.Moth);
                Ellipse(t, 22, 39, 8, 6, MothPalette.Moth); Ellipse(t, 42, 39, 8, 6, MothPalette.Moon);
                Ellipse(t, 32, 28, 6, 14, MothPalette.Ink); Ellipse(t, 32, 44, 7, 7, MothPalette.Moth);
                Ellipse(t, 29, 45, 1.3f, 1.3f, MothPalette.Ink); Ellipse(t, 35, 45, 1.3f, 1.3f, MothPalette.Ink);
                Line(t, new Vector2(30, 50), new Vector2(26, 57), 0, MothPalette.Ink); Line(t, new Vector2(34, 50), new Vector2(38, 57), 0, MothPalette.Ink);
                Ellipse(t, 18, 37, 2, 2, Color.white); Ellipse(t, 46, 37, 2, 2, Color.white);
            });
            if (id == "firefly") return Make(id, 32, 32, t => { Ellipse(t, 16, 16, 15, 15, new Color(MothPalette.Pollen.r, MothPalette.Pollen.g, MothPalette.Pollen.b, .18f)); Ellipse(t, 16, 16, 7, 7, MothPalette.Pollen); Ellipse(t, 14, 18, 2, 2, Color.white); });
            if (id == "thorn") return Make(id, 64, 64, t => {
                for (int i = 0; i < 10; i++) { float a = i * Mathf.PI * 2f / 10f; float r = (i % 2 == 0) ? 27 : 18; Ellipse(t, 32 + Mathf.Cos(a) * r, 32 + Mathf.Sin(a) * r, 7, 7, MothPalette.Spirit); }
                Ellipse(t, 32, 32, 18, 17, MothPalette.Lavender); Ellipse(t, 25, 35, 3, 4, MothPalette.Ink); Ellipse(t, 39, 35, 3, 4, MothPalette.Ink);
                Ellipse(t, 24, 43, 4, 2, MothPalette.Moon); Ellipse(t, 40, 43, 4, 2, MothPalette.Moon);
            });
            if (id == "flower") return Make(id, 128, 128, t => {
                for (int i = 0; i < 8; i++) { float a = i * Mathf.PI * 2f / 8f; Ellipse(t, 64 + Mathf.Cos(a) * 27, 64 + Mathf.Sin(a) * 27, 17, 25, MothPalette.Lavender); }
                Ellipse(t, 64, 64, 24, 24, MothPalette.Pollen); Ellipse(t, 58, 70, 5, 5, Color.white); Ellipse(t, 74, 70, 5, 5, Color.white);
                Ellipse(t, 58, 70, 2, 3, MothPalette.Ink); Ellipse(t, 74, 70, 2, 3, MothPalette.Ink);
                for (int i = -2; i <= 2; i++) Ellipse(t, 64 + i * 9, 52, 2.5f, 3, MothPalette.Ink);
            });
            if (id == "leaf") return Make(id, 64, 64, t => { Ellipse(t, 32, 32, 23, 12, MothPalette.Fern); Line(t, new Vector2(12, 28), new Vector2(50, 36), 1, MothPalette.FernLight); });
            if (id == "ring") return Make(id, 128, 128, t => {
                for (int y = 0; y < 128; y++) for (int x = 0; x < 128; x++) { float d = Vector2.Distance(new Vector2(x, y), new Vector2(63.5f, 63.5f)); if (d > 55 && d < 60) { float a = Mathf.Clamp01((60 - d) / 5f) * .55f; Dot(t, x, y, new Color(MothPalette.Pollen.r, MothPalette.Pollen.g, MothPalette.Pollen.b, a)); } }
            });
            return Make(id, 32, 32, t => Ellipse(t, 16, 16, 15, 15, Color.white));
        }
        public static Sprite Background()
        {
            return Make("background", 512, 288, t => {
                for (int y = 0; y < t.height; y++) for (int x = 0; x < t.width; x++) {
                    float edge = Mathf.InverseLerp(0, 380, Vector2.Distance(new Vector2(x, y), new Vector2(250, 165)));
                    Color c = Color.Lerp(MothPalette.Deep, MothPalette.Night, edge);
                    float haze = Mathf.Exp(-((x - 255) * (x - 255) + (y - 100) * (y - 100)) / 42000f) * .24f;
                    c = Color.Lerp(c, MothPalette.Fern, haze); t.SetPixel(x, y, c);
                }
                var rng = new System.Random(211);
                for (int i = 0; i < 145; i++) { int x = rng.Next(512), y = rng.Next(288); Color star = MothPalette.Moon; star.a = (float)(rng.NextDouble() * .32 + .08); Dot(t, x, y, star); if (i % 11 == 0) Dot(t, x + 1, y, star); }
                for (int i = 0; i < 13; i++) { int x = 20 + i * 39; Ellipse(t, x, 35 + (i % 4) * 8, 9, 2, new Color(MothPalette.FernLight.r, MothPalette.FernLight.g, MothPalette.FernLight.b, .14f)); }
            });
        }
    }
}
