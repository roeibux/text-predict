using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextEditor.Control;
using TextPredict.Base;
using TextPredict.Distance;
using TextPredict.Properties;

namespace TextPredict.Control {
    public static class SuggestionUtils {
        public static readonly Trie stopWords = new Trie(Resources.stop_words);
        public static Dictionary<string , Topic> fringeTopics = new Dictionary<string , Topic>();
        private static Filter<string> stopWordsFilter = ( word ) => stopWords.contains(word);
        static SuggestionUtils()
        {
            stopWords.addAll(Resources.stopWordsConversation);
            string currentDir = Environment.CurrentDirectory;
          
            foreach ( string f in Directory.GetFiles(currentDir , "*.txt") ) {
                string topic = Path.GetFileNameWithoutExtension(f);
                if ( topic.StartsWith("stop" , StringComparison.InvariantCultureIgnoreCase) ) continue;
                fringeTopics[topic] = createTopic(topic , File.ReadAllText(f));
            }
        }


         static Topic createTopic( string topic,string text ) {
             Trie t = new Trie(text);
             t.removeAll(stopWords);
             Trie t2 = new Trie(text);
            Dictionary<string , Histogram>  dict = new Dictionary<string , Histogram>();
            foreach ( Node wordNode in stopWords.getAll() ) {
                string word = wordNode.ToString();
                Histogram h = new Histogram(word , text);
                dict.Add(word , h);
            }
            return new Topic(topic, t ,t2, dict);
        }


    }
}
