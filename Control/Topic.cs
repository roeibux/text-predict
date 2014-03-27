using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextPredict.Base;
using TextPredict.GUI;

namespace TextPredict.Control {
    public class Topic {
        public readonly string topic;
        public readonly Trie fringeTrie;
        public readonly Dictionary<string , Distance.Histogram> historyDicti;
        public readonly Trie allwords;
        private Distribution _topicVector;
        public Distribution topicVector {
            get {
                if (_topicVector==null) {
                    List<Node> trieWords = fringeTrie.getAll();
                    trieWords.Sort(( n1 , n2 ) => n2.getNumOfAppearances() - n1.getNumOfAppearances());
                    double appsInTopicVector=0;
                    for ( int i = 0 ; i < Configurations.TOPIC_VECTOR_SIZE ; i++ ) {
                        Node n = trieWords[i];
                        string s = n.ToString();
                        int apps = n.getNumOfAppearances();
                        appsInTopicVector += apps;
                    }
                    _topicVector = new Distribution();
                    for ( int i = 0 ; i < Configurations.TOPIC_VECTOR_SIZE ; i++ ) {
                        Node n = trieWords[i];
                        string s = n.ToString();
                        double apps = n.getNumOfAppearances();
                        _topicVector.add(s,apps/appsInTopicVector);
                    }
                }
                return _topicVector;
            }
        }
        public double similiarity;
        public double rtmax;
        public double rtmin;
        public  double rt;

        public Topic( String topic,Trie fringeTrie ,Trie allwords , Dictionary<string , Distance.Histogram> dict ) {
            this.topic = topic;
            this.fringeTrie = fringeTrie;
            this.historyDicti = dict;
            this.allwords = allwords;
        }
        public override string ToString() {
            return topic;
        }

    }
}
