using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextEditor.Base.TrieCollection;
using global:: System.IO;
using System.Windows.Forms;
using TextEditor.SystemProj;

namespace TextEditor.Control
{
    public class LoadFileControl
    {
        public Trie LoadStopWordFile(string fileName )
        {
            Trie trie = new Trie();
            bool isFileEmpty = true;

            try
            {
                using (var reader = new StreamReader(fileName))
                {
                    string word;
                    while ((word = reader.ReadLine()) != null)
                    {   
                        trie.Add(word);
                        isFileEmpty = false;              
                    }
                }
            }
            catch (Exception errorMsg ) { MessageBox.Show(errorMsg.Message); }

            if (isFileEmpty == false)
                return trie;
            else
                return null;
        }

        public MsgError LoadFile( string fileName, out Trie trie, string topic )
        {
            bool isFileEmpty = true, isTrieEmptyOfFringeWord = true;
            trie = new Trie(topic);

            try
            {
                using (var reader = new StreamReader(fileName))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] words = line.Split(' ','?','!','.','(',')','"',',',':');
                        foreach (string word in words)
                        {
                            //check if the word is not a stop word
                            if (String.IsNullOrEmpty(word)) continue;
                            if (false == MainControl.dataBase.isStopWord(word))
                            {
                                trie.Add(word);
                                isTrieEmptyOfFringeWord = false;
                            }
                            else
                            {
                                MainControl.dataBase.StopWordsTrie.Add(word);
                            }
                            isFileEmpty = false;
                        }
                    }
                }
            }
            catch (Exception errorMsg) { MessageBox.Show(errorMsg.Message); }

            if (isFileEmpty == false && isTrieEmptyOfFringeWord == false)
                return MsgError.OK;
            else
                if (isFileEmpty == false)
                    return MsgError.NOT_VALID;
                else
                    return MsgError.ERROR;
        }
    }
}
