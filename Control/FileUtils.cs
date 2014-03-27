using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextEditor.Base;
using global:: System.IO;
using System.Windows.Forms;
using TextEditor.SystemProj;
using TextPredict.Base;
using System.Reflection;
using TextPredict.Properties;

namespace TextEditor.Control
{
    public delegate bool Filter<T>( T t );
    public static class FileUtils {
        public static Trie readTrie(string fileName , Filter<String> filter) {
            Trie trie=new Trie();
            StreamReader reader = null;
            try {
                reader = new StreamReader(fileName);
                string word;
                while ((word=reader.ReadLine())!=null) {
                    string[] words = word.Replace("\r" , "").Split(' ' , '\n' , ',' , '.');
                    foreach ( var w in words ) {
                        bool valid = true;
                        foreach ( char c in w )
                            valid &= Char.IsLetter(c);
                        if ( !valid ) continue;
                        if ( !filter(w) )
                            trie.add(w);
                    }
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            } finally {
                if (reader!=null) reader.Close();
            }
            return trie;
        }
        public static Trie readTrie(string fileName) {
            Trie trie=new Trie();
            StreamReader reader=null;
            try {
                reader=new StreamReader(fileName);
                string word;
                while ((word=reader.ReadLine())!=null) 
                    trie.addAll(word);
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            } finally {
                if (reader!=null) reader.Close();
            }
            return trie;

        }

       
    }
    

}
