﻿using ConsoleTables;
using FiniteAutomatas.Domain.Models.Automatas;
using FiniteAutomatas.Domain.Models.ValueObjects;

namespace FiniteAutomatas.Visualizations.Implementation;

internal class FiniteAutomataConsoleVisualizer
{
    private readonly FiniteAutomata _automata;

    public FiniteAutomataConsoleVisualizer( FiniteAutomata automata )
    {
        _automata = automata;
    }

    public void Print()
    {
        // Create columns
        string[] columns = BuildColumns().ToArray();

        // Create rows
        var rows = BuildRows( columns ).ToArray();

        var table = new ConsoleTable( columns );
        foreach ( var row in rows )
        {
            table.AddRow( row.ToArray() );
        }

        table.Write();
    }

    private IEnumerable<List<string>> BuildRows( string[] columns )
    {
        foreach ( State state in _automata.AllStates.OrderBy( x => Int32.TryParse( x.Name, out int value )
                     ? value
                     : -1 ) )
        {
            var items = new List<string>();

            foreach ( string column in columns )
            {
                if ( column == "Id" )
                {
                    items.Add( state.Name );
                    continue;
                }

                if ( column == "IsStart" )
                {
                    items.Add( state.IsStart.ToString() );
                    continue;
                }

                if ( column == "IsEnd" )
                {
                    items.Add( state.IsEnd.ToString() );
                    continue;
                }

                if ( column == "IsError" )
                {
                    items.Add( state.IsError.ToString() );
                    continue;
                }

                var transitions = _automata.Transitions
                    .Where( x =>
                        x.From.Equals( state ) && x.Argument == new Argument( column ) )
                    .Select( x =>
                    {
                        string transitionLabel = x.To.Name; 
                        if ( x.AdditionalData != null )
                        {
                            transitionLabel = $"{transitionLabel}/{x.AdditionalData}";
                        }

                        return transitionLabel;
                    } )
                    .ToList();

                items.Add( String.Join( ",", transitions ) );
            }

            yield return items;
        }
    }

    private IEnumerable<string> BuildColumns()
    {
        var result = new List<string>();
        result.Add( "Id" );
        result.Add( "IsStart" );
        result.Add( "IsEnd" );
        result.Add( "IsError" );
        result.AddRange( _automata.Alphabet.Select( x => x.Value ) );

        return result;
    }
}