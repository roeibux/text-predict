using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using TextPredict.Base;
using TextPredict.Properties;

namespace TextPredict.Control {
    public class Utils {
        private static string englishABC = "abcdefghijkmnlopqrstuvwxyz";
        public static void initTrieFromString( String s , Trie t ) {
            string[] words = s.Replace("\r" , "").Split(' ' , '\n' , ',' , '.');
            foreach ( var w in words ) {
                bool valid = true;
                foreach ( char c in w )
                    valid &= Char.IsLetter(c);
                if ( !valid ) continue;
                t.add(w);
            }

        }
        public static bool isSeparator( char c ) {
            return ( Char.IsPunctuation(c) || Char.IsSymbol(c) || Char.IsWhiteSpace(c) || Char.IsNumber(c) || Char.IsSeparator(c) || c=='\r' || c=='\n' || c == (char)System.Windows.Forms.Keys.Enter );
        }
        public static bool isBack( char c ) {
            return c == (char)System.Windows.Forms.Keys.Back;
        }

        public static IEnumerable<string> iterateWords( string text ) {
            int sIndex = 0;
            int nIndex;
            while ( ( nIndex = nextWordIndex(text , sIndex , out sIndex) ) != -1 ) {
                yield return text.Substring(sIndex , nIndex - sIndex);
                sIndex = nIndex;
            }
            if ( sIndex < text.Length ) 
                yield return text.Substring(sIndex);
        }
        public static bool isRelevantKey( char p ) {
            return englishABC.IndexOf(Char.ToLower(p))>=0 || Utils.isSeparator(p) || Utils.isBack(p) || p == (char)Keys.Enter;
        }
        private static int nextWordIndex( string text , int start,out int newstart) {
            while ( start < text.Length && Utils.isSeparator(text[start]) ) start++;
            newstart = start;
            while ( start < text.Length && !Utils.isSeparator(text[start]) ) start++;
            return start >= text.Length ? -1 : start;
        }
    }
}
