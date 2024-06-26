﻿using FiniteAutomatas.RegularExpressions.Implementation.Models;

namespace FiniteAutomatas.RegularExpressions.Implementation.Extensions;

internal static class RegexSymbolTypeExtensions
{
    public static string ToSymbol( this RegexSymbolType type ) => type switch
    {
        RegexSymbolType.And => "&",
        RegexSymbolType.Or => "|",
        RegexSymbolType.ZeroOrMore => "*",
        RegexSymbolType.OneOrMore => "+",
        RegexSymbolType.OpenBrace => "(",
        RegexSymbolType.CloseBrace => ")",
        _ => throw new ArgumentOutOfRangeException( nameof( type ), type, null )
    };
}