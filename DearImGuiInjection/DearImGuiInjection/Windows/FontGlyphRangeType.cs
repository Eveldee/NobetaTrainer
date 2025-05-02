using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DearImGuiInjection.Windows;

public enum FontGlyphRangeType
{
    /// <summary>
    /// Glyph range enough for english language
    /// </summary>
    English,

    /// <summary>
    /// Glyph range enough for english and chinese simplified common language
    /// </summary>
    ChineseSimplifiedCommon,

    /// <summary>
    /// Glyph range enough for english and full chinese language
    /// </summary>
    ChineseFull,

    /// <summary>
    /// Glyph range enough for english and Japanese language
    /// </summary>
    Japanese,

    /// <summary>
    /// Glyph range enough for english and korean language
    /// </summary>
    Korean,

    /// <summary>
    /// Glyph range enough for english and Thai language
    /// </summary>
    Thai,

    /// <summary>
    /// Glyph range enough for english and Vietnamese language
    /// </summary>
    Vietnamese,

    /// <summary>
    /// Glyph range enough for english and few special chars.
    /// </summary>
    Cyrillic,
}