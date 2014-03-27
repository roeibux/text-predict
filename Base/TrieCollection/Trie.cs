using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextEditor.Base.TrieCollection
{
    public class Trie
    {
    #region class variables
        public Node root { get; private set; }
        public string topic { get; private set; }
        public Int32 Count { get; private set; }
        public double Similarity;
        public IPrefixMatcher Matcher { get; private set; }
    #endregion

    #region public methods
        public Trie()
        {
            this.root = new Node('\0');
            this.topic = "general";
            this.Count = 0;
            this.Similarity = 0;
            this.Matcher = new PrefixMatcher(this.root);
        }

        public Trie(string topic)
        {
            this.root = new Node('\0');
            this.topic = topic.ToLower();
            this.Count = 0;
            this.Similarity = 0;
            this.Matcher = new PrefixMatcher(this.root);
        }

        public void Add(string word)
        {
            if (word == null) return;
            word = word.ToLower();

            Word newSuff = new Word(word);
            Node node = this.root;
            foreach (char c in word)
            {
                node.addSuffix(newSuff);
                node = node.addChild(c);
            }
            //add the Word to the leaf node
            node.addSuffix(newSuff);
            this.Count++;
        }

        public void Add(Word word)
        {
            if (word == null) return;
            string phrase = word.word;

            Node node = this.root;
            foreach (char c in phrase)
            {
                node.addSuffix(word);
                node = node.addChild(c);
            }
            //add the Word to the leaf node
            node.addSuffix(word);
            this.Count++;
        }
		
		public void Merge(Trie t)
        {
            if (t == null) return;

            var suffs = t.root.getSuffixes(t.Count);
            foreach (var w in suffs)
                this.Add(w);
        }

        public bool hasWord(string word)
        {
            return this.root.hasSuffix(new Word(word));
        }

        /// <summary>
        /// removes a given word from the Trie
        /// </summary>
        /// <param name="word">the word to remove</param>
        public void Remove(string word)
        {
            if (word == null) return;
            Word suf = new Word(word);
            Node node = getLeaf(suf);
            char key;

            if (node == null) return;

            while (node != root)
            {
                key = node.Key;
                node.RemoveSuffix(suf);
                node = node.Parent;
                if (node.GetChild(key).isLeaf())
                    node.RemoveChild(key);
                else
                {
                    node.RemoveSuffix(suf);
                    return;
                }
            }
            Matcher.ResetMatch();
            this.Count--;
        }

        /// <summary>
        /// calculate the trie's Similarity to a given window
        /// </summary>
        /// <param name="freq">
        ///     <para>The window to calculate similarity to.</para>
        ///     <para>every word has it's relative frequency</para>
        /// </param>
        /// <param name="winSize">window size</param>
        /// <param name="sigma">a positive input parameter for the similarity function</param>
        public void CalculateTrieSimilarity(Dictionary<string, double> freq,int winSize,double sigma)
        {
            var trieVector = new Dictionary<string, double>();
            var unionVector = new Dictionary<string,double[]>();
            double[] probs;
            var suffixes = root.getSuffixes(winSize);
            double sumOfAllProbs = 0.0, sumXY = 0.0;

            foreach (var suffix in suffixes)
            {
                sumOfAllProbs += ((double)suffix.w / (double)Count);
                trieVector.Add(suffix.word, ((double)suffix.w / (double)Count));
            }

            if (sumOfAllProbs == 0) return;

            foreach (var w in trieVector.Keys)
            {
                probs = new double[2];
                probs[0] = (trieVector[w] / sumOfAllProbs); //Xi
                probs[1] = 0.0; //Yi
                unionVector.Add(w, probs); 
            }

            foreach (var w in freq.Keys)
            {
                if (unionVector.ContainsKey(w))
                    probs = unionVector[w];
                else
                {
                    probs = new double[2];
                    probs[0] = 0.0;
                }
                probs[1] = freq[w];
            }

            //calculate the similarity
            foreach (var w in unionVector.Keys)
                sumXY += Math.Pow((unionVector[w][0]-unionVector[w][1]), 2);

            this.Similarity = Math.Pow(Math.E, -(sumXY/sigma));
        }


        public override bool Equals(Object o)
        {
            Trie t = (Trie)o;
            return (this.topic.Equals(t.topic.ToLower()));
        }

        public override int GetHashCode()
        {
            return this.topic.GetHashCode();
        }

    #endregion
    #region private methods
        private Node getLeaf(Word suf)
        {
            if (suf == null) return null;
            Node node = this.root;
            foreach (char c in suf.word)
            {
                if ((node = node.GetChild(c)) == null) return null;
            }
            return node;
        }

        private void removeWord(Node node, Word word)
        {
            if (word == null || node == null) return;

            char key;
            Node runner = node;
            if (runner.DecrementSuffix(word))
            {
                while (runner != root)
                {
                    key = runner.Key;
                    runner.RemoveSuffix(word);
                    runner = runner.Parent;
                }
            }
            Matcher.ResetMatch();
        }
    #endregion
    }//end class Trie
}//end namespace
