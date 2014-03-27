using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextPredict.Control {
    public class Distribution {
        private Dictionary<object , int> distributionMap = new Dictionary<object , int>();
        private List<double> distribution = new List<double>();
        private List<double> sortdistribution = new List<double>();
        public readonly List<object> keys = new List<object>();
        private double sum;
        public int count {
            get {
                return distribution.Count;
            }
        }
        public Distribution() {
        }
        public void add(object o, double d) {
            if ( (int)sum + d > 1 || d < 0.0 ) throw new Exception("not distribution" + (sum + d));
            else {
                sum += d;
                int index;
                keys.Add(o);
                if ( distributionMap.TryGetValue(o , out index) ) {
                    distribution[index] += d;
                    sortdistribution = new List<double>(distribution);
                    sortdistribution.Sort();
                    sortdistribution.Reverse();
                } else {
                    distributionMap.Add(o , distribution.Count);
                    distribution.Add(d);
                    sortdistribution.Add(d);
                    sortdistribution.Sort();
                    sortdistribution.Reverse();
                }
                
            }
        }
        
        //public double KLDistance( Distribution q ) {
        //    return KLDistance(this,q);
        //}

        public double summation( Func<double,double> f ) {
            double summation = 0;
            foreach ( double d in distribution ) summation+=f(d);
            return summation;
        }
        public double this[object o] {
            get {
                int index;
                return distributionMap.TryGetValue(o , out index) ? this[index]: 0; 
            }
        }
        public double this[int i] {
            get {
                return distribution.Count > i ? sortdistribution[i] : 0;
            }
        }
        //public static double KLDistance( Distribution p , Distribution q ) {
        //    List<double> pd = p.distribution;
        //    List<double> qd = q.distribution;
        //    double summation = 0;
        //    if (pd.Count>qd.Count) throw new Exception("undefined : exists q_i=0 where p_i!=0");
        //    for ( int i = 0 ; i < qd.Count ; i++ ) {
        //        if ( qd[i] == 0 && pd[i] != 0 ) throw new Exception("undefined : exists q_i=0 where p_i!=0");
        //        summation += Math.Log(pd[i] / qd[i]) * pd[i];
        //    }
        //    return summation;
        //}
    }

    public class UniformDistribution<T> : Distribution {
        public UniformDistribution( List<T> list , int n ) {
            for ( int i = 0 ; i < n ; i++ ) 
                add(list[i],1/(double)n);
        }
    }
}
