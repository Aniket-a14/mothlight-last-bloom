using UnityEngine;

namespace Mothlight
{
    public static class MothPalette
    {
        public static readonly Color Night = Hex("11162E");
        public static readonly Color Deep = Hex("252C52");
        public static readonly Color Fern = Hex("315C58");
        public static readonly Color FernLight = Hex("638578");
        public static readonly Color Moon = Hex("E9D9AA");
        public static readonly Color Moth = Hex("E9C5D1");
        public static readonly Color Pollen = Hex("FFD66D");
        public static readonly Color Spirit = Hex("FF7B83");
        public static readonly Color Lavender = Hex("BBA7DA");
        public static readonly Color Ink = Hex("20233E");
        static Color Hex(string value) { ColorUtility.TryParseHtmlString("#" + value, out var c); return c; }
    }
}
