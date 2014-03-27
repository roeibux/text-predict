
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TextEditor.Control;
using TextEditor.SystemProj;
using TextPredict.Base;
using TextPredict.Control;
using TextPredict.Properties;

namespace TextPredict.GUI {
    public class Configurations {
        public static int TOPIC_VECTOR_SIZE = 10;
        public static int WINDOW_SIZE = 10;
        public static bool CARE_FOR_LENGTH_FRINGE = false;
        public static int SUGGESTIONS_COUNT_LIMIT = 5;
        public static int ARRANGER_SLEEP_TIME = 300;
        public static bool SHOULD_CARE_FOR_STOP_WORDS_DISTANCE = true;
        public static bool SHOULD_CARE_FOR_STOP_WORDS_LENGTH = true;
        public static double LENGTH_CONSTANT_FACTOR = 1/5.0;
        public static bool IGNORE_MANAGING_HITS = true;
        public static double WLOGW = Configurations.WINDOW_SIZE * Math.Log(Configurations.WINDOW_SIZE);
        public static double SIGMA = 1 / WLOGW;
        public static double DELTA = 1 / ( (double)WINDOW_SIZE );
        public static double GAMMA = 1 / WLOGW;
        

    }
    public class OrlySuggester : TrieSuggester {
        private double sigma;
        public Topic topic;
        private List<Node> suggestionsList;
        private int index = 0;
        public OrlySuggester( Topic topic , double sigma = 1 )
            : base(topic.fringeTrie) {
            this.sigma = sigma;
            this.topic = topic;
        }
        public override string ToString() {
            return "OrlySuggester(" + topic + ")";
        }
        protected override void recomputeSuggestions() { 
            if ( !isRelevant ) return;
            suggestionsList = traveler.getAllPaths();
            suggestionsList.Sort(( n1 , n2 ) => n2.getNumOfAppearances() - n1.getNumOfAppearances());
            index = 0;
        }
        protected override bool suggestNext() {
            if ( index >= suggestionsList.Count ) return false;
            Node s = suggestionsList[index++];
            String suggestion = s.ToString();
            double freq = (s.getNumOfAppearances() / (double)trie.weight);
            if ( Configurations.CARE_FOR_LENGTH_FRINGE )
                return suggest(new Suggestion(suggestion , topic.similiarity * freq * suggestion.Length * Configurations.LENGTH_CONSTANT_FACTOR,this)); 
            else
                return suggest(new Suggestion(suggestion , topic.similiarity * freq , this));
            //Console.WriteLine(ToString() + " suggeted " + suggestion);
        }



        //protected override double calculateSimiliarity( Trie window ) {
        //    List<Node> windowWords = window.getAll();
        //    List<Node> trieWords = trie.getAll();
        //    double sumOfFreqs = 0;
        //    int sizeOfIntersection = 0;
        //    foreach ( Node nWindow in windowWords ) {
        //        Node nTrie = trie.getNode(nWindow.ToString());
        //        sizeOfIntersection += nTrie == null ? 0 : 1;
        //        double freqInTrie = nTrie == null ? 0 : nTrie.getNumOfAppearances() / (double)trie.weight;
        //        sumOfFreqs += Math.Pow(2 , nWindow.getNumOfAppearances() / Configurations.WINDOW_SIZE) * freqInTrie;
        //    }
        //    sumOfFreqs *= Math.Pow(2 , sizeOfIntersection/Configurations.WINDOW_SIZE);
        //    return sumOfFreqs;
        //}
        
    }
}
