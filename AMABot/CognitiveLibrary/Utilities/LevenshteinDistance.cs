using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CognitiveLibrary.Utilities
{
    public class LevenshteinDistance
    {
        private static int Compute(string source, string target)
        {
            int sourceLength = source.Length;
            int targetLength = target.Length;
            int[,] matrix = new int[sourceLength + 1, targetLength + 1];

            // First calculation, if one entry is empty return full length
            if (sourceLength == 0)
                return targetLength;

            if (targetLength == 0)
                return sourceLength;

            // Initialization of matrix with row size sourceLength and columns size targetLength
            for (int i = 0; i <= sourceLength; matrix[i, 0] = i++) { }
            for (int j = 0; j <= targetLength; matrix[0, j] = j++) { }

            // Calculate rows and columns distances
            for (int i = 1; i <= sourceLength; i++)
            {
                for (int j = 1; j <= targetLength; j++)
                {
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            // return result
            return matrix[sourceLength, targetLength];
        }

        public static string GetClosest(List<string> stringsToCheck, string stringtoMatch)
        {
            Dictionary<string, int> resultset = new Dictionary<string, int>();
            foreach (string s in stringsToCheck)
            {
                resultset.Add(s, LevenshteinDistance.Compute(stringtoMatch,s));
            }
            
            //get the minimum number of modifications needed to arrive at the basestring
            int minimumModifications = resultset.Min(c => c.Value);

            //gives you a list with all strings that need a minimum of modifications to become the
            //same as the stringtoMatch
            var closestlist = resultset.Where(c => c.Value == minimumModifications).FirstOrDefault();

            return closestlist.Key;
        }
    }
}
