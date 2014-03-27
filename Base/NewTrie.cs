using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextPredict.Base {
    public class Trie {
        #region class variables
        internal Node root;
        public int weight {
            get {
                return root.weight;
            }
        }
        public int count { get; private set; }
        private TrieTraveler traveler;
        #endregion

        #region public methods
        public Trie() {
            root = new Node();
            traveler = new TrieTraveler(this);
        }
        public Trie( String s ) {
            root = new Node();
            traveler = new TrieTraveler(this);
            addAll(s);
        }
        public Trie( string[] strings ) {
            root = new Node();
            traveler = new TrieTraveler(this);
            foreach (string s in strings) 
                add(s);
        }

        public bool add( string word ) {
            char[] a = word.ToLower().ToCharArray();
            if ( word.Equals("") ) return false;
            if ( !contains(word) )
                count++;
            Node curr = root;
            curr.weight++;
            for ( int i = 0 ; i < a.Length ; i++ ) {
                Node next = curr.neighbours[a[i]];
                if ( next == null )
                    next = curr.neighbours[a[i]] = new Node(curr , a[i]);
                next.weight++;
                curr = next;
            }
            return true;
        }


        public bool contains( string word ) {
            traveler.rewind();
            return traveler.travel(word) && traveler.getCurrent().isLeaf();
        }

        /// <summary>
        /// substracts an instance of the given word from the Trie, if the number of appearances of this word is 1 before subtraction, then it removes it completely
        /// </summary>
        /// <param name="word">the word to remove</param>
        /// <returns>true if succeded false else</returns>
        public bool subtract( string word ) {
            //if ( !contains(word) ) return false;?
            char[] a = word.ToLower().ToCharArray();
            Node curr = root;
            int i;
            for ( i = 0 ; i < a.Length ; i++ ) {
                Node next = curr.neighbours[a[i]];
                if ( next == null )
                    return false;
                curr = next;
            }
            do {
                int w = --curr.weight;
                char c = curr.c;
                curr = curr.parent;
                if ( curr != null && w <= 0 ) curr.neighbours[c] = null;
            } while ( curr != null );

            if ( !contains(word) )
                count--;
            return true;
        }
        /// <summary>
        /// remove all instances of the given word from the Trie
        /// </summary>
        /// <param name="word">the word to remove</param>
        /// <returns>true if succeded false else</returns>
        public bool remove( string word ) {
            char[] a = word.ToLower().ToCharArray();
            Node curr = root;
            int i;
            for ( i = 0 ; i < a.Length ; i++ ) {
                Node next = curr.neighbours[a[i]];
                if ( next == null )
                    return false;
                curr = next;
            }
            int appearances = curr.getNumOfAppearances();
            do {
                int w = curr.weight -= appearances;
                char c = curr.c;
                curr = curr.parent;
                if ( curr != null && w <= 0 ) curr.neighbours[c] = null;
            } while ( curr != null );
            count--;
            return true;
        }

        public List<Node> getStrings( String startsWith ) {
            traveler.rewind();
            if ( !traveler.travel(startsWith) ) return new List<Node>();
            return traveler.getAllPaths();
        }
        #endregion


        public List<Node> getAll() {
            traveler.rewind();
            return traveler.getAllPaths();
        }



        public void addAll( string s ) {
            string[] words = s.Replace("\r" , "").Split(' ' , '\n' , ',' , '.');
            foreach ( var w in words ) {
                bool valid = true;
                foreach ( char c in w )
                    valid &= Char.IsLetter(c);
                if ( !valid ) continue;
                add(w);
            }
        }

        public void clear() {
            root = new Node();
            traveler = new TrieTraveler(this);
            this.count = 0;
        }

        public void removeAll( Trie t ) {
            foreach ( Node n in t.getAll() ) {
                remove(n.ToString());
            }
        }

        public Node getNode( string p ) {
            traveler.rewind();
            if ( traveler.travel(p) )
                return traveler.getCurrent();
            return null;
        }
    }//end class Trie
    public class Neighberhood {
        internal Node[] neighbours = new Node[26];
        internal int[] indices = new int[26];
        public int Capacity {
            get {
                return neighbours.Length;
            }
        }
        public int Size {
            get;
            private set;
        }
        public Node this[int i] {
            get {
                if ( i >= Size ) throw new IndexOutOfRangeException();
                if ( neighbours[indices[i]]==null ) throw new Exception("neighbours[indices[i]] = null!?");
                return neighbours[indices[i]];
            }
        }
        public Node this[char c] {
            get {
                int index = c - 'a';
                if ( index < 0 || index >= 26 ) throw new IndexOutOfRangeException("index requested is " + index + " char "+c);
                return neighbours[index];
            }
            set {
                int index = c - 'a';
                if ( index < 0 || index >= 26 ) throw new IndexOutOfRangeException("index requested is " + index + " char " + c);
                if ( neighbours[index] != null ) {
                    int i;
                    for (  i = 0 ; i < indices.Length && index != indices[i] ; i++ );
                    if ( i < 0 || i >= 26 ) throw new IndexOutOfRangeException("something bad heppend");
                    indices[i] = indices[Size - 1];
                    Size--;
                }
                   
                neighbours[index] = value;
                if ( value != null ) {
                    indices[Size] = index;
                    Size++;
                }

            }
        }

    }
    public class Node {
        #region class variables

        internal char c { get; private set; }

        internal Node parent { get; private set; }

        internal Neighberhood neighbours = new Neighberhood();

        internal object extra;
        /// <summary>
        /// the sum of weights over all the subtrees
        /// </summary>
        internal int weight = 0;

        #endregion



        /// <summary>
        /// Create an empty node with no children and no suffixes.
        /// </summary>
        /// <param name="key">The Key of this node</param>
        public Node( char c ) {
            this.c = c;
            this.parent = null;
        }

        public Node( Node parent , char c ) {
            this.parent = parent;
            this.c = c;
        }

        public Node() {
        }

        public bool isLeaf() {
            return getNumOfAppearances() > 0;
        }
        public override String ToString() {
            return ToString(new StringBuilder()).ToString();
        }
        public int getNumOfAppearances() {
            int sumWeights = 0;
            for ( int i = 0 ; i < neighbours.Size ; i++ )
                sumWeights += neighbours[i].weight;
            return weight - sumWeights;
        }
        private StringBuilder ToString( StringBuilder sb ) {
            if ( parent == null ) return sb;
            else return parent.ToString(sb).Append(c);
        }
    }

    public class TrieTraveler {
        private Node current;
        public int currentPosition { get; private set; }
        private Trie trie;
        private StringBuilder sb;
        public TrieTraveler( Trie t ) {
            trie = t;
            current = trie.root;
            sb = new StringBuilder();
        }
        public TrieTraveler( TrieTraveler trieTraveler ) {
            current = trieTraveler.current;
            currentPosition = trieTraveler.currentPosition;
            trie = trieTraveler.trie;
            sb = new StringBuilder(trieTraveler.sb.ToString());
        }
        /// <summary>
        /// checks if underlying trie has entry suitable for the next char c for the traveler to travel
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool hasEntry( char c ) {
            return current.neighbours[c] != null;
        }
        /// <summary>
        /// Tries to apply travesal to the entry suitable for char c, if no such entry, then returns false and back to root. otherwise travels and returns true.
        /// </summary>
        /// <param name="c"></param>
        public bool travel( char c ) {
            current = current.neighbours[c];
            if ( current == null ) {
                rewind();
                return false;
            } else currentPosition++;
            sb.Append(c);
            return true;
        }

        /// <summary>
        /// Tries to apply travesal to the entry suitable for char c, if no such entry, then returns false and back to root.
        /// </summary>
        /// <param name="c"></param>
        /// <returns>true if succeded false otherwise</returns>
        public bool travel( String word ) {
            char[] a = word.ToLower().ToCharArray();
            for ( int i = 0 ; i < a.Length ; i++ ) {
                if ( !travel(a[i]) )
                    return false;
            }
            return true;
        }

        public String getPath() { return sb.ToString(); }

        public bool back() {
            current = current.parent;
            bool fail = current == null;
            if ( fail ) {
                currentPosition = 0;
                current = trie.root;
            } else {
                currentPosition--;
                sb.Length--;
            }
            return !fail;
        }
        /// <summary>
        /// travels back by n.  
        /// </summary>
        /// <param name="n"></param>
        /// <returns>how many steps back were actually taken, and how many were not, by simply returnning currentPosition at the time this was invoked minus n v</returns>
        public int back( int n ) {
            int res = currentPosition - n;
            while ( n-- > 0 && back() ) ;
            return res;
        }
        /// <summary>
        /// travels back to the root of the trie
        /// </summary>
        public void rewind() {
            currentPosition = 0;
            current = trie.root;
            sb.Clear();
        }

        public Neighberhood getNeighberhood() {
            return current.neighbours;
        }

        public List<Node> getAllPaths() {
            List<Node> results = new List<Node>();
            recoursiveTravel(current , results);
            return results;
        }
        public ICollection<Node> getAllPaths( ICollection<Node> collection ) {
            List<Node> results = new List<Node>();
            recoursiveTravel(current , collection);
            return results;
        }
        private void recoursiveTravel( Node node , ICollection<Node> results ) {
            if ( node == null ) return;
            Neighberhood n = node.neighbours;
            for ( int i = 0 ; i < n.Size ; i++ ) {
                Node temp = n[i];
                recoursiveTravel(temp , results);
                if ( temp.isLeaf() )
                    results.Add(temp);
            }
        }

        public Node getCurrent() {
            return current;
        }
    }
}
