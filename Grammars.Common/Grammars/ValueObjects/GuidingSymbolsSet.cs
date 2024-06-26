﻿using System.Collections.Immutable;
using Grammars.Common.Grammars.ValueObjects.RuleNames;
using Grammars.Common.Grammars.ValueObjects.Symbols;

namespace Grammars.Common.Grammars.ValueObjects;

public class GuidingSymbolsSet
{
    public readonly RuleName Rule;
    public readonly IImmutableSet<RuleSymbol> GuidingSymbols;

    public GuidingSymbolsSet( RuleName rule, IEnumerable<RuleSymbol> guidingSymbols )
    {
        Rule = rule;
        GuidingSymbols = guidingSymbols.ToImmutableHashSet();
    }

    public GuidingSymbolsSet Exclude( RuleSymbol ruleSymbol ) => new( Rule, GuidingSymbols.Where( x => !x.Equals( ruleSymbol ) ) );

    public bool Has( RuleSymbol symbol ) => GuidingSymbols.Contains( symbol );

    public bool Has( TerminalSymbolType terminalSymbolType ) => GuidingSymbols.Any( s => 
        s.Type == RuleSymbolType.TerminalSymbol && 
        s.Symbol!.Type == terminalSymbolType );

    public bool HasIntersections( GuidingSymbolsSet other ) => GuidingSymbols.Intersect( other.GuidingSymbols ).Any();

    public bool HasIntersections( HashSet<RuleSymbol> unitableGroupHeadings ) => GuidingSymbols.Intersect( unitableGroupHeadings ).Any();
}