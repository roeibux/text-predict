using System;
using global::System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextEditor.Base
{
    public class Word
    {
        public String word { get; private set; }
        public long w { get; set; }
        private static Dictionary<String, Word> stringToWord = new Dictionary<string, Word>();
        public Word(String suffix)
        {
            this.word = suffix.ToLower();
            this.w = 1;
        }
        
        public Word(String suffix, long weight)
        {
            this.word = suffix;
            this.w = weight;
        }

        public override bool Equals(Object s)
        {
            Word suf = (Word)s;
            return (this.word.Equals(suf.word.ToLower()));
        }

        public override int GetHashCode()
        {
            return (word.GetHashCode() ^ w.GetHashCode());
        }

        public static Word getWordFor(String s) {
            Word output;
            if (stringToWord.TryGetValue(s, out output) ) 
                return output;
            stringToWord.Add(s, output = new Word(s));
            return output;
        }
    }
}
