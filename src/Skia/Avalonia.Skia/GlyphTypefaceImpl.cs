﻿using System;
using System.Runtime.InteropServices;
using Avalonia.Media;
using HarfBuzzSharp;
using SkiaSharp;

namespace Avalonia.Skia
{
    internal class GlyphTypefaceImpl : IGlyphTypeface
    {
        private bool _isDisposed;

        public GlyphTypefaceImpl(SKTypeface typeface, FontSimulations fontSimulations)
        {
            Typeface = typeface ?? throw new ArgumentNullException(nameof(typeface));

            Face = new Face(GetTable)
            {
                UnitsPerEm = Typeface.UnitsPerEm
            };

            Font = new Font(Face);

            Font.SetFunctionsOpenType();

            var metrics = Typeface.ToFont().Metrics;

            const double defaultFontRenderingEmSize = 12.0;

            Font.OpenTypeMetrics.TryGetPosition(OpenTypeMetricsTag.HorizontalAscender, out var ascent);
            Font.OpenTypeMetrics.TryGetPosition(OpenTypeMetricsTag.HorizontalDescender, out var descent);
            Font.OpenTypeMetrics.TryGetPosition(OpenTypeMetricsTag.HorizontalLineGap, out var lineGap);
            Font.OpenTypeMetrics.TryGetPosition(OpenTypeMetricsTag.StrikeoutOffset, out var strikethroughOffset);
            Font.OpenTypeMetrics.TryGetPosition(OpenTypeMetricsTag.StrikeoutSize, out var strikethroughSize);
            Font.OpenTypeMetrics.TryGetPosition(OpenTypeMetricsTag.UnderlineOffset, out var underlineOffset);
            Font.OpenTypeMetrics.TryGetPosition(OpenTypeMetricsTag.UnderlineSize, out var underlineSize);
            
            Metrics = new FontMetrics
            {
                DesignEmHeight = (short)Typeface.UnitsPerEm,
                Ascent = -ascent,
                Descent = -descent,
                LineGap = lineGap,
                UnderlinePosition = -underlineOffset,
                UnderlineThickness = underlineSize,
                StrikethroughPosition = -strikethroughOffset,
                StrikethroughThickness = strikethroughSize,
                IsFixedPitch = Typeface.IsFixedPitch
            };

            GlyphCount = Typeface.GlyphCount;

            FontSimulations = fontSimulations;

            Weight = (FontWeight)Typeface.FontWeight;

            Style = Typeface.FontSlant.ToAvalonia();

            Stretch = (FontStretch)Typeface.FontStyle.Width;
        }

        public Face Face { get; }

        public Font Font { get; }

        public SKTypeface Typeface { get; }

        public FontSimulations FontSimulations { get; }

        public int ReplacementCodepoint { get; }

        public FontMetrics Metrics { get; }

        public int GlyphCount { get; }

        public string FamilyName => Typeface.FamilyName;

        public FontWeight Weight { get; }

        public FontStyle Style { get; }

        public FontStretch Stretch { get; }

        public bool TryGetGlyphMetrics(ushort glyph, out GlyphMetrics metrics)
        {
            metrics = default;

            if (!Font.TryGetGlyphExtents(glyph, out var extents))
            {
                return false;
            }
            
            metrics = new GlyphMetrics
            {
                XBearing = extents.XBearing,
                YBearing = extents.YBearing,
                Width = extents.Width,
                Height = extents.Height
            };
                
            return true;
        }

        /// <inheritdoc cref="IGlyphTypeface"/>
        public ushort GetGlyph(uint codepoint)
        {
            if (Font.TryGetGlyph(codepoint, out var glyph))
            {
                return (ushort)glyph;
            }

            return 0;
        }

        public bool TryGetGlyph(uint codepoint, out ushort glyph)
        {
            glyph = GetGlyph(codepoint);

            return glyph != 0;
        }

        /// <inheritdoc cref="IGlyphTypeface"/>
        public ushort[] GetGlyphs(ReadOnlySpan<uint> codepoints)
        {
            var glyphs = new ushort[codepoints.Length];

            for (var i = 0; i < codepoints.Length; i++)
            {
                if (Font.TryGetGlyph(codepoints[i], out var glyph))
                {
                    glyphs[i] = (ushort)glyph;
                }
            }

            return glyphs;
        }

        /// <inheritdoc cref="IGlyphTypeface"/>
        public int GetGlyphAdvance(ushort glyph)
        {
            return Font.GetHorizontalGlyphAdvance(glyph);
        }

        /// <inheritdoc cref="IGlyphTypeface"/>
        public int[] GetGlyphAdvances(ReadOnlySpan<ushort> glyphs)
        {
            var glyphIndices = new uint[glyphs.Length];

            for (var i = 0; i < glyphs.Length; i++)
            {
                glyphIndices[i] = glyphs[i];
            }

            return Font.GetHorizontalGlyphAdvances(glyphIndices);
        }

        private Blob? GetTable(Face face, Tag tag)
        {
            var size = Typeface.GetTableSize(tag);

            var data = Marshal.AllocCoTaskMem(size);

            var releaseDelegate = new ReleaseDelegate(() => Marshal.FreeCoTaskMem(data));

            return Typeface.TryGetTableData(tag, 0, size, data) ?
                new Blob(data, size, MemoryMode.ReadOnly, releaseDelegate) : null;
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            if (!disposing)
            {
                return;
            }

            Font.Dispose();
            Face.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool TryGetTable(uint tag, out byte[] table)
        {
            return Typeface.TryGetTableData(tag, out table);
        }
    }
}
