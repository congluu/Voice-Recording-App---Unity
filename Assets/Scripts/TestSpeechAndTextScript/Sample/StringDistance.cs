using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StringDistance 
{
    #region Levenshtein Distance
    public static int LevenshteinDistance(string voiceResultText, string textInResultArrayToCompare)
    {
        int lengthA = voiceResultText.Length;
        int lengthB = textInResultArrayToCompare.Length;
        int[,] distances = new int[lengthA + 1, lengthB + 1];

        // Step 1
        if (lengthA == 0)
        {
            return lengthB;
        }

        if (lengthB == 0)
        {
            return lengthA;
        }

        // Step 2
        for (int i = 0; i <= lengthA; distances[i, 0] = i++)
        {
        }

        for (int j = 0; j <= lengthB; distances[0, j] = j++)
        {
        }

        // Step 3
        for (int i = 1; i <= lengthA; i++)
        {
            //Step 4
            for (int j = 1; j <= lengthB; j++)
            {
                // Step 5
                int cost = (textInResultArrayToCompare[j - 1] == voiceResultText[i - 1]) ? 0 : 1;

                // Step 6
                distances[i, j] = Math.Min(
                    Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                    distances[i - 1, j - 1] + cost);
            }
        }
        // Step 7
        return distances[lengthA, lengthB];
    }
    #endregion

    #region Using Levenshtein Distance to calculate percentage similarity of two strings
    /// <summary>
    /// Calculate percentage similarity of two strings
    /// <param name="source">Source String to Compare with</param>
    /// <param name="target">Targeted String to Compare</param>
    /// <returns>Return Similarity between two strings from 0 to 1.0</returns>
    /// </summary>
    public static double CalculateSimilarity(string source, string target)
    {
        if ((source == null) || (target == null)) return 0.0;
        if ((source.Length == 0) || (target.Length == 0)) return 0.0;
        if (source == target) return 1.0;

        int stepsToSame = LevenshteinDistance(source, target);
        return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
    }

    #endregion
}
