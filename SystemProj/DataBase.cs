using System;
using System.Collections.Generic;
using System.Data;
using TextEditor.Base.TrieCollection;
using System.Data.OleDb;
using TextEditor.Base;
using System.Windows.Forms;
using TextEditor.Properties;
using System.Data.SqlServerCe;
using System.Data.SqlClient;
using TextPredict.Properties;
using TextPredict.Control;

namespace TextEditor.SystemProj
{
    public class DataBase
    {
        /// <summary>
        /// A Dictionary of all tries
        /// </summary>
        public Dictionary<String, Trie> TriesByTopic;
        public Trie StopWordsTrie;
        public TextPredict.Base.Trie OfekTrie;
        private readonly SqlCeConnection conn;
        public static TextPredict.Base.Trie globalTrie;

        public DataBase(string connStr)
        {
            StopWordsTrie = new Trie();
            TriesByTopic = new Dictionary<string, Trie>();
            conn = new SqlCeConnection(connStr);//new SqlConnection(connStr);
            globalTrie = new TextPredict.Base.Trie();
            //Utils.initTrieFromString(Resources.worms , globalTrie);
            //Utils.initTrieFromString(Resources.stop_words , globalTrie);
            //Utils.initTrieFromString(Resources.stopWordsConversation , globalTrie);
            //Utils.initTrieFromString(Resources.bicycle , globalTrie);
            //Utils.initTrieFromString(Resources.science , globalTrie);
            //Utils.initTrieFromString(Resources.how_to_play_fotbool , globalTrie);
            //Utils.initTrieFromString(Resources.how_to_play_basketball , globalTrie);
            //Utils.initTrieFromString(Resources.Cardiovascular_disease , globalTrie);
            //Utils.initTrieFromString(Resources.calories , globalTrie);
            try
            {
                if (!load())
                    Initialize();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                if (conn.State == ConnectionState.Open) conn.Close();
            }
        }

        public void Clear()
        {
            conn.Open();
            var cmd = new SqlCeCommand("DELETE FROM Words", conn);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        //Check if the 
        public bool isStopWord(string word)
        {
            return StopWordsTrie.root.hasSuffix(word);
        }

        //Add new topic trie
        public void addTopicTrie(string topic,Trie trie)
        {
            if (TriesByTopic.ContainsKey(topic))
                TriesByTopic[topic].Merge(trie);
            else
                TriesByTopic.Add(topic, trie);
        }

        public void Initialize()
        {
           //load word file
            //string path = "stop-words.txt";
            //StopWordsTrie = MainControl.loadFileControl.LoadStopWordFile(path); 
            var words = Resources.stop_words.Replace("\r","").Split('\n');
            foreach (var w in words)
                StopWordsTrie.Add(w);
        }
        
       public void SaveAllTries()
       {
           /*Saving Application settings*/
           Settings.Default.WindowSize = MainControl.WINDOW_SIZE;
           Settings.Default.numOfFringe = MainControl.numberOfFringe;
           Settings.Default.numOfStop = MainControl.numberOfStop;
           Settings.Default.numOfMostRelevant = MainControl.numberOfMostRelevntTries;
           Settings.Default.ConsiderSuffixLength = MainControl.ConsiderSuffixLength;
           Settings.Default.Sigma = MainControl.Sigma;
           Settings.Default.demoIntervals = MainControl.demoIntervals;
           Settings.Default.Save();
           
            SqlCeCommand cmd;
            conn.Open();
            List<Word> li;
            List<Word>.Enumerator it;
            cmd = new SqlCeCommand("DELETE FROM Words", conn) {CommandType = CommandType.Text};
           cmd.ExecuteNonQuery();
            var progress = TriesByTopic.Count + 1;
            var cnt = 1;
            MainControl.mainWin.updateProgressbar((int)(100/progress));
            foreach (Trie t in TriesByTopic.Values)
            {
                if (null == (li = t.root.getSuffixes(t.Count))) continue;
                if (li.Count == 0) continue;

                it =  li.GetEnumerator();
                while (it.MoveNext())
                {
                    try
                    {
                        cmd.CommandText = String.Format("INSERT INTO Words (Word,Weight,Topic) VALUES ('{0}',{1},'{2}');", it.Current.word.Replace("'", "''"), it.Current.w, t.topic);
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception e) { MessageBox.Show(e.Message); }
                }
                var temp = ((double)cnt++ / progress) * 100.0;
                MainControl.mainWin.updateProgressbar((int)temp);
            }

            //insert stop words trie
            li = StopWordsTrie.root.getSuffixes(StopWordsTrie.Count);
            if (li != null && li.Count > 0)
            {
                it = li.GetEnumerator();
                while (it.MoveNext())
                {
                    try
                    {
                        cmd.CommandText = String.Format("INSERT INTO Words (Word,Weight,Topic) VALUES ('{0}',{1},'{2}');", it.Current.word.Replace("'", "''"), it.Current.w, StopWordsTrie.topic);
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception) { }
                }
            }
            MainControl.mainWin.updateProgressbar(100);
            conn.Close();
        }
        private bool load()
        {
            /*Load Application settings*/
            MainControl.WINDOW_SIZE = Settings.Default.WindowSize;
            MainControl.numberOfFringe = Settings.Default.numOfFringe;
            MainControl.numberOfStop = Settings.Default.numOfStop;
            MainControl.numberOfMostRelevntTries = Settings.Default.numOfMostRelevant;
            MainControl.ConsiderSuffixLength = Settings.Default.ConsiderSuffixLength;
            MainControl.Sigma = Settings.Default.Sigma;
            MainControl.demoIntervals = Settings.Default.demoIntervals;

            Word word;
            bool dbHasRows = false;
            Trie tempTrie = null;
            string curTopic = "general";
            conn.Open();
            var cmd = new SqlCeCommand("SELECT * FROM Words WHERE Topic='general' ORDER BY Word", conn);
            var dr = cmd.ExecuteReader();

            while (dr.Read()) //add words to stop words trie
            { 
                word = new Word(dr["Word"].ToString(),long.Parse(dr["Weight"].ToString()));
                StopWordsTrie.Add(word);
            }

            dr.Close();
            cmd.CommandText = "SELECT * FROM Words WHERE Topic<>'general' ORDER BY Topic,Word";
            dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                if (!curTopic.Equals(dr["Topic"].ToString()))
                {
                    try
                    {
                        if (tempTrie != null) TriesByTopic.Add(curTopic, tempTrie);
                        curTopic = dr["Topic"].ToString();
                        tempTrie = new Trie(curTopic);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                }
                word = new Word(dr["Word"].ToString(), long.Parse(dr["Weight"].ToString()));
                tempTrie.Add(word);
            }
            if (tempTrie != null) TriesByTopic.Add(curTopic, tempTrie);

            dr.Close();
            conn.Close();
            return (StopWordsTrie.Count > 0) || (TriesByTopic.Count > 0);
        }
    }
}
