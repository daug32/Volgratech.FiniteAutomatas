using FiniteAutomatas.Domain.Convertors;
using FiniteAutomatas.Domain.Convertors.Convertors.Minimization;
using FiniteAutomatas.Domain.Convertors.Convertors.NfaToDfas;
using FiniteAutomatas.Domain.Models.Automatas;
using FiniteAutomatas.Domain.Models.ValueObjects;
using FiniteAutomatas.RegularExpressions;

namespace FiniteAutomatas.Tests;

public class MinimizationTests
{
    // I used this website to calculate minimal number of states
    // I also increased total number of states because this website doesn't count error state as a different state
    // https://cyberzhg.github.io/toolbox/min_dfa 
    private static readonly object[][] _testData = 
    {
        new object[] { "abcdefgh|1-abcdefgh|2-abcdefgh", 12 },
        new object[] { "abc", 5 },
        new object[] { "a*b*c*", 4 },
        new object[] { "(x|y)*(ab|ac*)*|(x|y)*(a*b*c)*|(x|y)(ab)*", 11 }
    };
    
    [TestCaseSource( nameof( _testData ) )]
    public void Test( string regex, int expectedNumberOfStates )
    {
        // Arrange
        DeterminedFiniteAutomata regexFa = new RegexToNfaParser()
            .Parse( regex )
            .Convert( new NfaToDfaConvertor() );
        
        // Act
        DeterminedFiniteAutomata minimizedFa = regexFa.Convert( new DfaMinimizationConvertor() );

        // Assert
        
        // Number of states must be equal to expected number
        Assert.That( minimizedFa.AllStates.Count, Is.EqualTo( expectedNumberOfStates ) );
        
        // All states must contain transitions via all possible arguments
        bool allStatesHasTransitions = true;
        foreach ( State state in minimizedFa.AllStates )
        {
            bool hasTransitionsForAllArguments = true;
            foreach ( Argument argument in minimizedFa.Alphabet )
            {
                bool hasTransitionForArgument = false;
                foreach ( Transition transition in minimizedFa.Transitions )
                {
                    if ( !transition.From.Equals( state ) || !transition.Argument.Equals( argument ))
                    {
                        continue;
                    }

                    hasTransitionForArgument = true;
                    break;
                }

                hasTransitionsForAllArguments &= hasTransitionForArgument; 
            }

            allStatesHasTransitions &= hasTransitionsForAllArguments;
        }

        Assert.That( allStatesHasTransitions, Is.True );
    }
}