using System;

namespace TroveTools.NET.Framework
{
    public class BoyerMooreByteSearch
    {
        private const int ALPHABET_SIZE = 256;

        private byte[] pattern;
        private int[] last, match, suffix;

        public BoyerMooreByteSearch(byte[] pattern)
        {
            this.pattern = pattern;
            last = new int[ALPHABET_SIZE];
            match = new int[pattern.Length];
            suffix = new int[pattern.Length];

            // Preprocessing
            ComputeLast();
            ComputeMatch();
        }

        /// <summary>Searches the pattern in the text. Returns the position of the first occurrence, if found and -1 otherwise.</summary>
        public int Match(byte[] text)
        {
            // Searching
            int i = pattern.Length - 1;
            int j = pattern.Length - 1;
            while (i < text.Length)
            {
                if (pattern[j] == text[i])
                {
                    if (j == 0) return i;
                    j--;
                    i--;
                }
                else
                {
                    i += pattern.Length - j - 1 + Math.Max(j - last[text[i]], match[j]);
                    j = pattern.Length - 1;
                }
            }
            return -1;
        }

        /// <summary>Searches the pattern in the text starting at offset with the specified length. Returns the position of the first occurrence, if found and -1 otherwise.</summary>
        public int Match(byte[] text, int offset, int length)
        {
            // Searching
            int i = offset + pattern.Length - 1;
            int j = pattern.Length - 1;
            while (i < offset + length)
            {
                if (pattern[j] == text[i])
                {
                    if (j == 0) return i;
                    j--;
                    i--;
                }
                else
                {
                    i += pattern.Length - j - 1 + Math.Max(j - last[text[i]], match[j]);
                    j = pattern.Length - 1;
                }
            }
            return -1;
        }

        /// <summary>
        /// Computes the function last and stores its values in the array last.
        /// last(Char ch) = the index of the right-most occurrence of the character ch in the pattern;
        /// -1 if ch does not occur in the pattern.
        /// </summary>
        private void ComputeLast()
        {
            for (int k = 0; k < last.Length; k++)
            {
                last[k] = -1;
            }
            for (int j = pattern.Length - 1; j >= 0; j--)
            {
                if (last[pattern[j]] < 0) last[pattern[j]] = j;
            }
        }

        /// <summary>
        /// Computes the function match and stores its values in the array match.
        /// </summary>
        private void ComputeMatch()
        {
            /* Phase 1 */
            for (int j = 0; j < match.Length; j++)
            {
                match[j] = match.Length;
            } //O(m) 

            ComputeSuffix(); //O(m)

            /* Phase 2 */
            //Uses an auxiliary array, backwards version of the KMP failure function.
            //suffix[i] = the smallest j > i s.t. p[j..m-1] is a prefix of p[i..m-1],
            //if there is no such j, suffix[i] = m

            //Compute the smallest shift s, such that 0 < s <= j and
            //p[j-s]!=p[j] and p[j-s+1..m-s-1] is suffix of p[j+1..m-1] or j == m-1}, if such s exists,
            for (int i = 0; i < match.Length - 1; i++)
            {
                int j = suffix[i + 1] - 1; // suffix[i+1] <= suffix[i] + 1
                if (suffix[i] > j)
                {
                    // therefore pattern[i] != pattern[j]
                    match[j] = j - i;
                }
                else
                {
                    // j == suffix[i]
                    match[j] = Math.Min(j - i + match[i], match[j]);
                }
            }

            /* Phase 3 */
            //Uses the suffix array to compute each shift s such that
            //p[0..m-s-1] is a suffix of p[j+1..m-1] with j < s < m
            //and stores the minimum of this shift and the previously computed one.
            if (suffix[0] < pattern.Length)
            {
                for (int j = suffix[0] - 1; j >= 0; j--)
                {
                    if (suffix[0] < match[j]) { match[j] = suffix[0]; }
                }
                {
                    int j = suffix[0];
                    for (int k = suffix[j]; k < pattern.Length; k = suffix[k])
                    {
                        while (j < k)
                        {
                            if (match[j] > k) match[j] = k;
                            j++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Computes the values of suffix, which is an auxiliary array, backwards version of the KMP failure function.
        /// suffix[i] = the smallest j > i s.t. p[j..m-1] is a prefix of p[i..m-1], if there is no such j, suffix[i] = m, i.e. 
        /// p[suffix[i]..m-1] is the longest prefix of p[i..m-1], if suffix[i] < m.
        /// </summary>
        private void ComputeSuffix()
        {
            suffix[suffix.Length - 1] = suffix.Length;
            int j = suffix.Length - 1;
            for (int i = suffix.Length - 2; i >= 0; i--)
            {
                while (j < suffix.Length - 1 && !pattern[j].Equals(pattern[i]))
                {
                    j = suffix[j + 1] - 1;
                }
                if (pattern[j] == pattern[i]) j--;
                suffix[i] = j + 1;
            }
        }
    }
}