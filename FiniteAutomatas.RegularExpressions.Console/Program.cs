﻿using FiniteAutomatas.Domain.Convertors;
using FiniteAutomatas.Domain.Convertors.Convertors.Minimization;
using FiniteAutomatas.Domain.Convertors.Convertors.NfaToDfas;
using FiniteAutomatas.Domain.Models.Automatas;
using FiniteAutomatas.Visualizations;

namespace FiniteAutomatas.RegularExpressions.Console;

public class Program
{
    public static async Task Main()
    {
        while ( true )
        {
            System.Console.Write( "Write a regex: " );
            string regex = System.Console.ReadLine()!;
            
            try
            {
                System.Console.WriteLine( "Creating a DFA..." );
                DeterminedFiniteAutomata<char> dfa = new RegexToNfaParser()
                    .Parse( regex )
                    .Convert( new NfaToDfaConvertor<char>() )
                    .Convert( new DfaMinimizationConvertor<char>() );

                System.Console.WriteLine( "Printing DFA..." );
                await dfa
                    .PrintToConsole<DeterminedFiniteAutomata<char>, char>( new VisualizationOptions()
                    {
                        DrawErrorState = false
                    } )
                    .PrintToImageAsync<DeterminedFiniteAutomata<char>, char>( @".\dfa3Minimized.png", new VisualizationOptions()
                    {
                        DrawErrorState = false,
                        TimeoutInMilliseconds = 15_000
                    } );

                System.Console.WriteLine( "Success" );
            }
            catch ( Exception ex )
            {
                System.Console.WriteLine( $"Couldn't create an finite automata for regex. Regex: {regex}" );
                System.Console.WriteLine( ex );
            }
        }
// ReSharper disable once FunctionNeverReturns
    }
}