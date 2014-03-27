using System.Collections.Generic;
using TextEditor.SystemProj;

namespace TextEditor.Base.TrieCollection
{
    public interface IPrefixMatcher
    {
        void ResetMatch();
        void BackMatch();
        char LastMatch();
        bool NextMatch(char next);
        bool IsExactMatch();
        string GetExactMatch();
        List<Word> GetPrefixMatch(int qty);
        SortedList<double, List<string>> GetPrefixSuggestions(int qty, bool considerSuffixLength, PrDel getWordProbability, Comparer<double> c);
    }
}