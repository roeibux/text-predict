using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextPredict.Base.TrieCollection {
    public class QuickArray<T> {
        internal T[] neighbours = new T[26];
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
        /// <summary>
        /// get value at the i-th cell while null cells dont count
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public T get(int i) {
            if ( i >= Size ) throw new IndexOutOfRangeException();
            if ( neighbours[indices[i]] == null ) throw new Exception("neighbours[indices[i]] = null!?");
            return neighbours[indices[i]];
        }
        public T this[int index] {
            get {
                if ( index < 0 || index >= 26 ) throw new IndexOutOfRangeException("index requested is " + index );
                return neighbours[index];
            }
            set {
                if ( index < 0 || index >= 26 ) throw new IndexOutOfRangeException("index requested is " + index);
                if ( neighbours[index] != null ) {
                    int i;
                    for ( i = 0 ; i < indices.Length && index != indices[i] ; i++ ) ;
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
}
