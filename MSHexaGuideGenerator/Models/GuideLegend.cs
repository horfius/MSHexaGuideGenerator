using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSHexaGuideGenerator.Models
{
    [Flags]
    public enum LegendAnchor
    {
        Left = 0b0001,
        Right = 0b0010,
        Top = 0b0100,
        Bottom = 0b1000,
        BottomLeft = Bottom | Left,
        BottomRight = Bottom | Right,
        TopRight = Top | Right,
        TopLeft = Top | Left,
        Invalid = 0
    }

    public static class LegendAnchorExtensions
    {
        public static LegendAnchor ToLegendAnchor(this string text) => text switch
        {
            // Only bottom corner placements are valid right now
            "BottomLeft" => LegendAnchor.BottomLeft,
            "BottomRight" => LegendAnchor.BottomRight,
            _ => LegendAnchor.Invalid
        };

        public static bool IsTopAnchored(this LegendAnchor anchor) => (anchor & LegendAnchor.Top) != LegendAnchor.Invalid;

        public static bool IsBottomAnchored(this LegendAnchor anchor) => (anchor & LegendAnchor.Bottom) != LegendAnchor.Invalid;

        public static bool IsLeftAnchored(this LegendAnchor anchor) => (anchor & LegendAnchor.Left) != LegendAnchor.Invalid;

        public static bool IsRightAnchored(this LegendAnchor anchor) => (anchor & LegendAnchor.Right) != LegendAnchor.Invalid;
    }

    public record GuideLegendContents(string Icon, string Text) { }

    public record GuideLegend(string Header, string Anchor, GuideLegendContents[] Contents) { }
}
