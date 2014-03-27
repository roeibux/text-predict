using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextEditor.Control;
using TextPredict.Base;
using TextPredict.Distance;
using TextPredict.GUI;

namespace TextPredict.Control {
    public class OfekSuggester : TrieSuggester {
        private Dictionary<string , int> currentDistances = new Dictionary<string , int>();
        private Dictionary<string , int> initialDistances = new Dictionary<string , int>();

        public OfekSuggester() : base(SuggestionUtils.stopWords) {
            topics = SuggestionUtils.fringeTopics.Values.ToList();
            foreach (Node n in trie.getAll()) {
                string s = n.ToString();
                Histogram histogram = new Histogram();
                foreach ( Topic t in topics ) 
                    histogram.add(t.historyDicti[s].getMedian());
                initialDistances.Add(n.ToString() , histogram.getSorted(3*histogram.n/4));
            }
        }
       
        private StringBuilder wordBuilder = new StringBuilder();
        private List<Suggestion> list;
        private int index;
        private List<Topic> topics;
        public override void resetEnviroment() {
            base.resetEnviroment();
            wordBuilder.Clear();
            foreach ( string s in initialDistances.Keys ) {
                currentDistances[s] = initialDistances[s];
            }
        }
        public override void apply( char c ) {
            base.apply(c);
            if ( isSeparator(c) ) {
                String s = wordBuilder.ToString();
                wordBuilder.Clear();
                if ( !trie.contains(s) ) return;
                var keys = trie.getAll();
                foreach ( Node key in keys ){
                    String word = key.ToString();
                    currentDistances[word] = currentDistances[word] + 1;
                }
                currentDistances[s] = 0;
            } else if ( isBack(c) ) {
                if ( wordBuilder.Length > 0 )
                    wordBuilder.Length--;
            } else
                wordBuilder.Append(c);
        }

        public override string ToString() {
            return "OfekSuggester";
        }
        protected override bool suggestNext() {
            if ( index < list.Count )
                return suggest(list[index++]);
            return false;
        }
       protected override void recomputeSuggestions() {
            if ( !isRelevant ) return;
            //Console.WriteLine("ofek starting recompute , D(the)="+currentDistances["the"]);
            index = 0;
            
            List<Node> suggestionsList = traveler.getAllPaths();
            Dictionary<string , Suggestion> suggestions = new Dictionary<string , Suggestion>();
            
            //foreach ( Node n in suggestionsList ) {
            //    string s = n.ToString();
            //    double currDistance = currentDistances[s];
            //    long allAppearances = 0;
            //    foreach ( Topic t in topics ) {
            //        Node nodeInTopicTrie = t.allwords.getNode(s);
            //        allAppearances += t.allwords.weight;
            //        double numApp = nodeInTopicTrie == null ? 0 : nodeInTopicTrie.getNumOfAppearances();
            //        double expectedDistance = t.historyDicti[s].getMedian();
            //        double distanceFactor = currDistance > expectedDistance ? expectedDistance / currDistance : currDistance / expectedDistance;
            //        double weight = distanceFactor * Math.Pow(numApp,2) * t.similiarity;
            //        Suggestion suggestion = null;
            //        if ( suggestions.TryGetValue(s , out suggestion) ) {
            //            suggestion.weight += weight;
            //        } else {
            //            suggestion = new Suggestion(s , weight , this);
            //            suggestions[s] = suggestion;
            //        }
            //    }
            //    Suggestion sggst = suggestions[s];
            //    sggst.weight /= topics.Count;
            //    sggst.weight = Math.Sqrt(sggst.weight);
            //    sggst.weight /= ((double)allAppearances/topics.Count);
            //}
            foreach ( Node n in suggestionsList ) {
                string s = n.ToString();
                double currDistance = currentDistances[s];
                //if ( s.Equals("the") ) Console.WriteLine("d(the)=" + currentDistances["the"]);
                foreach ( Topic t in topics ) {
                    Node nodeInTopicTrie = t.allwords.getNode(s);
                    double numApp = nodeInTopicTrie == null ? 0 : nodeInTopicTrie.getNumOfAppearances();
                    double distanceFactor = 1;
                    double lengthFactor = 1;
                    if (Configurations.SHOULD_CARE_FOR_STOP_WORDS_DISTANCE) {
                        Histogram h = t.historyDicti[s];
                        double expectedDistance = h.getSorted(h.n / 2);
                        double forgetDistance = h.getSorted(3 * h.n / 4);
                        if ( currDistance < expectedDistance ) {
                            distanceFactor = currDistance / expectedDistance;
                            distanceFactor = smoothingFunc(distanceFactor);
                        } else if ( currDistance > forgetDistance ) {
                            distanceFactor = forgetDistance / currDistance;
                            distanceFactor = smoothingFunc(distanceFactor);
                        } else distanceFactor = 1;
                        //if ( s.Equals("the") ) Console.WriteLine("D(the)=" + distanceFactor);
                    }
                    if ( Configurations.SHOULD_CARE_FOR_STOP_WORDS_LENGTH )
                        lengthFactor = s.Length * Configurations.LENGTH_CONSTANT_FACTOR;
                    double weight = distanceFactor * ( numApp / (double)t.allwords.weight ) * t.similiarity * lengthFactor;
                    Suggestion suggestion = null;
                    if ( suggestions.TryGetValue(s , out suggestion) ) {
                        suggestion.weight += weight;
                    } else {
                        suggestion = new Suggestion(s , weight , this);
                        suggestions[s] = suggestion;
                    }
                }
            }
            list = new List<Suggestion>(suggestions.Values);
            list.Sort(( s1 , s2 ) => {
                return Comparer<Double>.Default.Compare(s2.weight,s1.weight);
            });
            Stats.showOfekSuggestions(list);
       }

        private static double smoothingFunc(double x) {
            return ( ( Math.Log(x + 0.05) - Math.Log(0.05) ) / ( Math.Log(1.05) - Math.Log(0.05) ) ); 
        }
    }

}
