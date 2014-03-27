using System;
using System.Collections.Generic;
using System.Linq;
using TextEditor.SystemProj;

namespace TextEditor.Base.TrieCollection
{ 
    /// <summary>
    /// Represents a Trie Node.
    /// </summary>
    public class Node
    {
    #region class variables
        /// <summary>
        /// <para name="Key">will hold this subtree's prefix starting character </para>
        /// <para name="Parent">this node's Parent</para>
        /// <para name="children">this node subtree</para>
        /// <para name="suffixes">
        ///     will contain this node's suffixes.
        ///     the leaf will contain only 1 suffix.
        /// </para>
        /// </summary>
        public Char Key { get; private set; }
        public Node Parent { get; private set; }
        private Dictionary<Char, Node> children;
        private List<Word> suffixes;
    #endregion

    #region public methods
        #region Constructors
        /// <summary>
        /// Create an empty node with no children and no suffixes.
        /// </summary>
        /// <param name="key">The Key of this node</param>
        public Node(Char key)
        {
            this.Key = char.ToLower(key);
            this.Parent = null;
            this.children = new Dictionary<Char, Node>();
            this.suffixes = new List<Word>();
        }
        
        /// <summary>
        /// Create an empty node with no children and no suffixes.
        /// </summary>
        /// <param name="Parent">Parent node of this node.</param>
        /// <param name="key">The Key of this node</param>
        public Node(Node parent, Char key)
        {
            this.Key = char.ToLower(key);
            this.Parent = parent;
            this.children = new Dictionary<Char, Node>();
            this.suffixes = new List<Word>();
        }

        /// <summary>
        /// Create a node from the given parameters, with no suffixes.
        /// </summary>
        /// <param name="Parent">Parent node of this node.</param>
        /// <param name="key">The Key of this node</param>
        /// <param name="children">This node children</param>
        public Node(Node parent, Char key, Dictionary<Char, Node> children)
        {
            this.Key = char.ToLower(key);
            this.Parent = parent;
            this.children = children;
            this.suffixes = new List<Word>();
        }

        /// <summary>
        /// Create a node from the given parameters.
        /// </summary>
        /// <param name="Parent">Parent node of this node.</param>
        /// <param name="key">The Key of this node</param>
        /// <param name="children">This node children</param>
        /// <param name="suffixes">This node's suffixes</param>
        public Node(Node parent, Char key, Dictionary<Char, Node> children, List<Word> suffixes)
        {
            this.Key = char.ToLower(key);
            this.Parent = parent;
            this.children = children;
            this.suffixes = suffixes;
        }
        #endregion

        public SortedList<double, List<string>> GetSuggestions(Int32 qty, bool considerSuffixLength,PrDel getWordProbability,Comparer<double> c)
        {
            if (qty < 0) return null;
            if (suffixes == null) return (new SortedList<double, List<string>>(c));
            
            if (suffixes.Count < qty) qty = suffixes.Count;
            var suggestions = new SortedList<double, List<string>>(c);
            double pr;
            List<string> li;
            foreach (var s in suffixes)
            {
                pr = getWordProbability(s.word);
                if (considerSuffixLength) pr *= (s.word.Length - GetPrefix().Length + 1); // +1 for the space (' ')
                if (suggestions.ContainsKey(pr)) {
                    li = suggestions[pr];
                    suggestions.Remove(pr);
                }
                else
                    li = new List<string>();
                if (li.Count < 5)
                    li.Add(s.word);
                suggestions.Add(pr,li);
            }
            int  count = 0;
            int amt = qty;
            foreach (var list in suggestions.Values)
            {
                amt -= list.Count;
                count++;
                if (amt <= 0) break;
            }
            var res = suggestions.Take(count);
            var ret = new SortedList<double, List<string>>(c);
            count = 0;
            foreach(var kvp in res)
            {
                count += kvp.Value.Count;
                if (count > qty)
                {
                    var range = kvp.Value.Count - (count-qty);
                    ret.Add(kvp.Key,kvp.Value.GetRange(0,range));
                    break;
                }
                else
                    ret.Add(kvp.Key,kvp.Value);
            }
            return ret;
        }
        
        public bool setSuffixes(List<Word> suffixes)
        {
            if (isLeaf() && (suffixes == null || suffixes.Count > 1))
                return false;
            this.suffixes = suffixes;
            return true;
        }
        
        public void addSuffix(Word suffix)
        {
            if (suffix == null) return;

            if (this.suffixes == null)
                this.suffixes = new List<Word>();
            addSorted(suffix);
        }
        
        public List<Word> getSuffixes(Int32 qty)
        {
            if (qty < 0) return null;
            if (this.suffixes == null) return (new List<Word>());

            if (qty > this.suffixes.Count)
                qty = this.suffixes.Count;
           
            return this.suffixes.GetRange(0, qty);
        }

        public List<string> getWords(Int32 qty)
        {
            if (qty < 0) return null; 
            var suffixes = new List<string>();
            if (this.suffixes == null) return suffixes;

            var it = this.suffixes.GetEnumerator();
            int count = 1;

            while (it.MoveNext() && count++ <= qty)
                suffixes.Add(it.Current.word);

            return suffixes;
        }
		
		public long getSuffixWeight(string suff)
        {
            return this.getSuffixWeight(new Word(suff));
        }

        public long getSuffixWeight(Word suff)
        {
            return !this.hasSuffix(suff) ? 0 : this.suffixes.Find( (w) => w.Equals(suff) ).w;
        }

        /// <summary>
        /// Get a child of this node which is associated with a Key.
        /// </summary>
        /// <param name="Key">Key associated with the child of interest.</param>
        /// <returns>The child or null if no child is associated with the given Key.</returns>
        public Node GetChild(char key)
        {
            key = char.ToLower(key);
            return children.ContainsKey(key) ? children[key] : null;
        }

        /// <summary>
        /// Get the number of children this node has.
        /// </summary>
        /// <returns>Number of children.</returns>
        public int numOfChildren()
        {
            return children.Count;
        }

        /// <summary>
        /// Check whether or not this node has any children.
        /// </summary>
        /// <returns>True if node does not have children, false otherwise.</returns>
        public bool isLeaf()
        {
            return (children.Count == 0);
        }

        /// <summary>
        /// Check whether or not one of the children of this node uses the given Key.
        /// </summary>
        /// <param name="Key">The Key to check for.</param>
        /// <returns>True if a child with given Key exists, false otherwise.</returns>
        public bool hasKey(char key)
        {
            return children.ContainsKey(char.ToLower(key));
        }

        /// <summary>
        /// Check whether or not this node has a given suffix.
        /// </summary>
        /// <param name="suff">The suffix to check for.</param>
        /// <returns>True if the suffix exists, false otherwise.</returns>
        public bool hasSuffix(Word suff)
        {
            return suffixes.Contains(suff);
        }
		
		/// <summary>
        /// Check whether or not this node has a given suffix.
        /// </summary>
        /// <param name="word">The suffix's Phrase to check for.</param>
        /// <returns>True if the suffix exists, false otherwise.</returns>
        public bool hasSuffix(string word)
        {
            return this.hasSuffix(new Word(word));
        }

        /// <summary>
        /// Add a child node associated with a Key to this node and return the node.
        /// </summary>
        /// <param name="Key">Key to associated with the child node.</param>
        /// <returns>If given Key already exists then return the existing child node, else return the new child node.</returns>
        public Node addChild(char key)
        {
            key = char.ToLower(key);
            if (children.ContainsKey(key))
                return children[key];
            else
            {
                var newChild = new Node(this, key);
                children.Add(key, newChild);
                return newChild;
            }
        }

        public void RemoveSuffix(Word suf)
        {
            suffixes.Remove(suf);
        }
        
        /// <summary>
        /// <para>decrease the weight value of a suffix from this node</para>
        /// <para>if the weight decreases to 0 then removes the suffix</para>
        /// </summary>
        /// <param name="suf">the suffix to remove</param>
        /// <returns>true - if weight decreases to 0 and suffix is removed, else false</returns>
        public bool DecrementSuffix(Word suf)
        {
            if (suffixes.Contains(suf))
            {
                suf.w = suffixes.Find( w => w.Equals(suf) ).w - 1;
                suffixes.Remove(suf);
                if (suf.w > 0)
                {
                    addSuffix(suf);
                    return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove the child of a node associated with a Key along with all its descendents.
        /// </summary>
        /// <param name="Key">The Key associated with the child to remove.</param>
        public void RemoveChild(char key)
        {
            children.Remove(key);
        }

        public string GetPrefix()
        {
            string prefix = "";
            Node node = this;
            while (node.Parent != null)
            {
                prefix = node.Key + prefix;
                node = node.Parent;
            }
            
            return prefix;
        }

    #endregion

    #region private methods

        private void addSorted(Word val)
        {
            if (this.suffixes.Count == 0)
            {
                this.suffixes.Add(val);
                return;
            }

            if (this.suffixes.Contains(val))
            {
                if (Parent == null)
                    this.suffixes.Find((w) => w.Equals(val)).w++;
            }
            else
                this.suffixes.Add(val);
            
            this.suffixes.Sort( (Word w1,Word w2) => w1.w.CompareTo(w2.w) );
            this.suffixes.Reverse();
        }
    #endregion
    }
}
