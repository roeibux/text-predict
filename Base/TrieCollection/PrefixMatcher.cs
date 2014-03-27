using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextEditor.SystemProj;

namespace TextEditor.Base.TrieCollection
{
    public class PrefixMatcher : IPrefixMatcher
    {
        private Node root;
        private Node current;
        public string Prefix { get; private set; }

        public PrefixMatcher(Node root)
        {
            this.root = this.current = root;
            this.Prefix = "";
        }

        /// <summary>
        /// Clear the currently entered prefix.
        /// </summary>
        public void ResetMatch()
        {
            this.current = this.root;
            this.Prefix = "";
        }

        /// <summary>
        /// Remove the last character of the currently entered prefix.
        /// </summary>
        public void BackMatch()
        {
            if (current != root)
            {
                current = current.Parent;
                Prefix = Prefix.Substring(0, Prefix.Length - 1);
            }
        }

        /// <summary>
        /// Get the last character of the currently entered prefix.
        /// </summary>
        /// <returns></returns>
        public char LastMatch()
        {
            return current.Key;
        }

        /// <summary>
        /// advance the Matcher to next match
        /// </summary>
        /// <param name="next">the next key to find</param>
        /// <returns>True if the current node has the key, False otherwise</returns>
        public bool NextMatch(char next)
        {
            if (current.hasKey(next))
            {
                current = current.GetChild(next);
                Prefix += next;
                return true;
            }
            return false;
        }

        /// <summary>
        /// advance the Matcher to next match
        /// </summary>
        /// <param name="next">the next word to find</param>
        /// <returns>True if the current node has the word, False otherwise</returns>
        public bool NextMatch(string next)
        {
            if (next == null) return false;

            foreach (char c in next)
                if (!NextMatch(c)) return false;
            return true;
        }

        /// <summary>
        /// Check if the currently entered prefix is an existing string in the trie.
        /// </summary>
        /// <returns>True if the currently entered prefix is an existing string, false otherwise.</returns>
        public bool IsExactMatch()
        {
            return current.isLeaf();
        }

        /// <summary>
        /// Get the value mapped by the currently entered prefix.
        /// </summary>
        /// <returns>The value mapped by the currently entered prefix or null if current prefix does not map to any value.</returns>
        public string GetExactMatch()
        {
            return IsExactMatch() ? current.getSuffixes(1)[0].word : null;
        }

       
        public List<Word> GetPrefixMatch(int qty)
        {
            return current.getSuffixes(qty);
        }

        /// <summary>
        /// get suggestions from current node
        /// </summary>
        /// <param name="qty">how many suggestions</param>
        /// <param name="considerSuffixLength">consider or not Suffixes length</param>
        /// <param name="getWordProbability">a method delegate for computing a word's probability</param>
        /// <returns>a sorted list of the suggestions according to their probabilities</returns>
        public SortedList<double, List<string>> GetPrefixSuggestions(int qty,bool considerSuffixLength,PrDel getWordProbability, Comparer<double> c)
        {
            return current.GetSuggestions(qty, considerSuffixLength, getWordProbability,c);
        }
    }
}
