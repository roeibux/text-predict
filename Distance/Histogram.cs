using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TextPredict.Control;

namespace TextPredict.Distance
{
    public class Histogram {

        public static readonly int MAX = 100;
        private int[] histogram = new int[MAX];

        private int median = -1;
        public int n { get; private set; }


        public Histogram() {
            Array.Clear(histogram , 0 , histogram.Length);
        }
        
        public Histogram( string word,string from ) {
            int i = 0;
            foreach ( string w in Utils.iterateWords(from) ) {
                if ( w.Equals(word , StringComparison.OrdinalIgnoreCase) ) {
                    add(i);
                    i = 1;
                } else i++;
            }
        }
        

        
        public void add(int value) {
            value = Math.Min(value , MAX - 1);
            histogram[value]++;
            n++;
        }

  

        //private void updateMedian( int value ) {
        //    if ( !inited ) {
        //        inited = true;
        //        median = i = value;
        //    }
        //    if ( isorted < n / 2 ) {
        //        ihist++;
        //        if ( ihist >= histogram[i] ) {
        //            for ( i++ ; i < MAX && histogram[i] == 0 ; i++ ) ;
        //            ihist = 0;
        //        }
        //        isorted++;
        //    } else if ( isorted > n / 2 ) {
        //        ihist--;
        //        if ( ihist < 0 ) {
        //            for ( i-- ; i < MAX && histogram[i] == 0 ; i-- ) ;
        //            ihist = histogram[i] - 1;
        //        }
        //        isorted--;
        //    }
        //    if ( i == MAX ) { median = MAX; return; }
        //    median = i;
        //    if ( n % 2 == 0 ) {
        //        if ( ihist+1 >= histogram[i] ) {
        //            int j;
        //            for ( j = i+1 ; j < MAX && histogram[j] == 0 ; j++ );
        //            median = ( median + j ) / 2;
        //        } 
        //    }
        //}

        public int getMedian() {
            return getSorted(n / 2);
        }

        public int getSorted( int p ) {
            int sum = 0;
            for ( int i = 0 ; i < MAX ; i++ ) {
                sum += histogram[i];
                if ( sum >= p ) {
                    return i;
                }
            }
            return MAX;
        }

    }
}
