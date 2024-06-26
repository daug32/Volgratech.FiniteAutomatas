﻿using Grammars.Common.Grammars;
using Grammars.Common.Grammars.ValueObjects.GrammarRules;
using Grammars.Common.Grammars.ValueObjects.RuleDefinitions;
using Grammars.Common.Grammars.ValueObjects.RuleNames;
using Grammars.Common.Grammars.ValueObjects.Symbols;
using LinqExtensions;

namespace Grammars.Common.Convertors.Convertors.LeftRecursions.Implementation;

internal class LeftRecursionRemoverHandler
{
    // See https://www.youtube.com/watch?v=SV3RgUsmPcU&lc=UgwHTljtg7Pwy0xqAJF4AaABAg
    // for better algorithm description

    private readonly CommonGrammar _grammar;
    private readonly RuleNameGenerator _ruleNameGenerator;

    public LeftRecursionRemoverHandler( CommonGrammar grammar )
    {
        _grammar = grammar;
        _ruleNameGenerator = new RuleNameGenerator( grammar );
    }

    public CommonGrammar RemoveLeftRecursion()
    {
        List<RuleName> allRules = _grammar.Rules.Keys.ToList();

        int mainIterator = 0;
        while ( true )
        {
            ReplaceLeftRecursionByNewRule( allRules[mainIterator], _grammar );
            
            if ( mainIterator == allRules.Count - 1 )
            {
                break;
            }

            mainIterator++;

            for ( int secondIterator = 0; secondIterator < mainIterator; secondIterator++ )
            {
                GrammarRule mainRule = _grammar.Rules[allRules[mainIterator]];
                GrammarRule secondRule = _grammar.Rules[allRules[secondIterator]];

                _grammar.Rules[mainRule.Name] = new GrammarRule( mainRule.Name, ReplaceHeadingRules( mainRule, secondRule ) );
            }
        }

        return _grammar;
    }

    private static List<RuleDefinition> ReplaceHeadingRules( GrammarRule mainRule, GrammarRule secondRule )
    {
        var newDefinitions = new List<RuleDefinition>();
        foreach ( RuleDefinition mainRuleDefinition in mainRule.Definitions )
        {
            RuleSymbol firstSymbol = mainRuleDefinition.Symbols.First();

            if ( firstSymbol.Type != RuleSymbolType.NonTerminalSymbol || 
                 firstSymbol.RuleName != secondRule.Name )
            {
                newDefinitions.Add( mainRuleDefinition );
                continue;
            }

            // Main rule definition symbols except for heading second rule
            List<RuleSymbol> tail = mainRuleDefinition.Symbols
                .ToList()
                .WithoutFirst();

            foreach ( RuleDefinition secondRuleDefinition in secondRule.Definitions )
            {
                var newDefinitionSymbols = secondRuleDefinition.Symbols
                    .Select( symbol => symbol )
                    .ToList()
                    .WithMany( tail );

                newDefinitions.Add( new RuleDefinition( newDefinitionSymbols ) );
            }
        }

        return newDefinitions;
    }

    private void ReplaceLeftRecursionByNewRule( RuleName targetRuleName, CommonGrammar grammar )
    {
        (List<RuleDefinition> WithLeftRecursion, List<RuleDefinition> WithoutLeftRecursion) 
            groupedDefinitions = GroupRuleDefinitionsByLeftRecursionUsage( targetRuleName, grammar );

        if ( !groupedDefinitions.WithLeftRecursion.Any() )
        {
            return;
        }

        var copiedRule = new GrammarRule(
            targetRuleName,
            new List<RuleDefinition>() );
        var newRule = new GrammarRule( 
            _ruleNameGenerator.Next(), 
            new List<RuleDefinition>() );

        foreach ( RuleDefinition definition in groupedDefinitions.WithoutLeftRecursion )
        {
            copiedRule.Definitions.Add( definition.Copy() );    
            
            copiedRule.Definitions.Add( new RuleDefinition( definition.Symbols
                .ToList()
                .With( RuleSymbol.NonTerminalSymbol( newRule.Name ) ) ) );    
        }

        foreach ( RuleDefinition definition in groupedDefinitions.WithLeftRecursion )
        {
            var definitionWithoutNonTerminal = definition.Symbols.ToList().WithoutFirst();
            if ( definitionWithoutNonTerminal.Any() )
            {
                newRule.Definitions.Add( new RuleDefinition( definitionWithoutNonTerminal ) );
            }

            var definitionWithoutNonTerminalAndWithNewRule = definition.Symbols.ToList().WithoutFirst().With( RuleSymbol.NonTerminalSymbol( newRule.Name ) );
            if ( definitionWithoutNonTerminalAndWithNewRule.Any() )
            {
                newRule.Definitions.Add( new RuleDefinition( definitionWithoutNonTerminalAndWithNewRule ) );
            }
        }
        
        grammar.Rules[newRule.Name] = newRule;
        grammar.Rules[copiedRule.Name] = copiedRule;
    }

    internal 
        (List<RuleDefinition> WithLeftRecursion, List<RuleDefinition> WithoutLeftRecursion) 
        GroupRuleDefinitionsByLeftRecursionUsage( RuleName targetRuleName, CommonGrammar grammar )
    {
        var withLr = new List<RuleDefinition>();
        var withoutLr = new List<RuleDefinition>();
        
        GrammarRule targetRule = grammar.Rules[targetRuleName];

        foreach ( RuleDefinition targetRuleDefinition in targetRule.Definitions )
        {
            RuleSymbol targetRuleDefinitionStartSymbol = targetRuleDefinition.Symbols.First();
            if ( targetRuleDefinitionStartSymbol.Type == RuleSymbolType.TerminalSymbol )
            {
                withoutLr.Add( targetRuleDefinition );
                continue;
            }

            if ( targetRuleDefinitionStartSymbol.RuleName == targetRule.Name )
            {
                withLr.Add( targetRuleDefinition );
                continue;
            }
            
            withoutLr.Add( targetRuleDefinition );
        }

        return (withLr, withoutLr);
    }
}