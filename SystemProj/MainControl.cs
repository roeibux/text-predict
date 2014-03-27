using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TextEditor.GUI;
using TextEditor.Control;
using TextEditor.Base.TrieCollection;
using TextPredict.GUI;

namespace TextEditor.SystemProj
{
    public enum MsgError
    {
        OK,
        ERROR,
        NOT_VALID
    }
    public delegate double PrDel(string word);

    public static class MainControl
    {
    #region Application Variables
        public static MainWindow mainWin;
        public static ConfigurationWindow configurationWin;
        public static LoadWordFile loadWordFile;
        public static LoadFileControl loadFileControl;
        public static DataBase dataBase;
        public static Demonstration demo;
        public static List<String> LastWordsWindow = new List<String>();
        public static List<Trie> MostRelevntTries = new List<Trie>();
    #endregion

    #region User Settings
        public static bool ConsiderSuffixLength = true;
        public static double Sigma = 1;
        public static int numberOfMostRelevntTries;
        public static int demoIntervals = 100;
        public static int numberOfFringe = 5;
        public static int numberOfStop = 5;
        private static Stats stats;
        public static int WINDOW_SIZE = 100;
        public static int WINDOW_SIZE1 = 100;
    #endregion


        [STAThread]
        public static void Main()
        {
            try
            {
                dataBase = new DataBase(Properties.Settings.Default.appDataConnectionString);
                new Form1().Show();
                new Form3().Show();
                new Form2().Show();
                stats = new Stats();
                mainWin = new MainWindow();
                loadWordFile = new LoadWordFile();
                loadFileControl = new LoadFileControl();
                numberOfMostRelevntTries = ( MostRelevntTries.Count >= numberOfMostRelevntTries ) ? numberOfMostRelevntTries : MostRelevntTries.Count;
                mainWin.setDatasetsCount(dataBase.TriesByTopic.Count);

                mainWin.Show();
                stats.Show();
                
                Application.Run();
            }
            catch (Exception errorMsg)
            {
                MessageBox.Show(string.Format("{0}\nS.T:\n{1}", errorMsg.Message, errorMsg.StackTrace));

            }
        }


        public static void Exit()
        {
            mainWin.Text += " - Saving Database...";
            mainWin.Cursor = Cursors.WaitCursor;
            mainWin.ToggleDemo(false);
            dataBase.SaveAllTries();
            mainWin.ToggleDemo(false);
            mainWin.Cursor = Cursors.Default;
            mainWin.Dispose();
            Application.Exit();
        }

        public static void ClearDb()
        {
            dataBase.Clear();
            MostRelevntTries = new List<Trie>();
            dataBase = new DataBase(Properties.Settings.Default.appDataConnectionString);
        }

		public static double GetProbabilityFor(string word)
        {
            double pr = 0.0;

		    if (dataBase.isStopWord(word))
            {
                pr = (double)dataBase.StopWordsTrie.root.getSuffixWeight(word);
                pr /= dataBase.StopWordsTrie.Count;
                return pr;
            }

            foreach (var t in MostRelevntTries)
            {
                //calculate word relative frequency
                double ww;
                if ((ww = (double)t.root.getSuffixWeight(word)) == 0) continue;
                ww /= (double)t.Count;

                //add to word probability 
                ww *= t.Similarity;
                pr += ww;
            }
            return pr;
        }
        
        /// <summary>
        /// <para>Update each trie's Similarity to the current Topic, using the last typed words window.</para>
        /// <para>When method finishes The most relevent tries list will be updated</para>
        /// </summary>
        public static void UpdateSimilarity()
        {
            var winFreq = new Dictionary<string, double>();
            var temp = new Dictionary<string, double>();
            var allTries = new List<Trie>();

            if (dataBase.TriesByTopic.Count == 0)
                return;
            //calculate each word's number of appearance
            foreach (string w in LastWordsWindow)
            {
                var numOfApp = 1.0;
                if (winFreq.ContainsKey(w))
                {
                    numOfApp = winFreq[w];
                    numOfApp++;
                    winFreq.Remove(w);
                }
                winFreq.Add(w, numOfApp);
            }

            //calculate each word's relative frequancy
            foreach (var w in winFreq.Keys)
                temp[w] = winFreq[w] / WINDOW_SIZE;
            
            winFreq = temp;

            foreach (Trie t in dataBase.TriesByTopic.Values)
            {
                t.CalculateTrieSimilarity(winFreq, WINDOW_SIZE, Sigma);
                allTries.Add(t);
            }
            //sort tries by similarity
            allTries.Sort( (Trie t1, Trie t2) => t1.Similarity.CompareTo(t2.Similarity) );
            allTries.Reverse();
            //update MostRelevntTries
            int range = numberOfMostRelevntTries;
            if (range >= allTries.Count)
                range = allTries.Count;
            MostRelevntTries = allTries.GetRange(0, range);
        }

        public static bool StartDemo(RichTextBox rtb,ListBox lb,string file)
        {
            demo = new Demonstration(rtb,lb);
            return demo.StartDemo(file);
        }

        public static void StopDemo()
        {
            demo.StopDemo();
        }

        public class ReverseDoubleComparer : Comparer<double>
        {
            public override int Compare(double x, double y)
            {
                return -Comparer<double>.Default.Compare(x, y);
            }
        }

    }
}
