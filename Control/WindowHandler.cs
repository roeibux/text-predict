using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextPredict.Base;
using TextPredict.GUI;

namespace TextPredict.Control {
    public class WindowHandler {
        private StringBuilder wordBuilder = new StringBuilder();
        private Trie window = new Trie();

        private Queue<String> windowQueue = new Queue<String>();
        private Trie stopWords;
        private static WindowHandler instance;
        private Worker bw;
        public event Runnable onWindowReady;
        public event Action<Topic> onTopicReady;
        public event Runnable onWindowInvalid;

        public static WindowHandler getInstance() {
            if ( instance == null )
                instance = new WindowHandler();
            return instance;
        }
        private WindowHandler() {

            stopWords = SuggestionUtils.stopWords;
            bw = new Worker("WindowHandlerThread");
        }

        private void apply( char c ) {
            if ( Utils.isSeparator(c) ) {
                String word = null;
                String s = wordBuilder.ToString().ToLower();
                wordBuilder.Clear();
                if ( s.Length == 0 || stopWords.contains(s) ) return;
                if (windowQueue.Count >= Configurations.WINDOW_SIZE) {
                    word = windowQueue.Dequeue();
                    if (word != null) {
                         lock (window) window.subtract(word);
                    }
                }
                windowQueue.Enqueue(s);
                lock (window) window.add(s);
                if (window.weight != windowQueue.Count) throw new Exception("window trie count doesn't match windowqueue count");
            } else if ( Utils.isBack(c) ) {
                if ( wordBuilder.Length > 0 )
                    wordBuilder.Length--;
            } else
                wordBuilder.Append(c);
        }


        public void rewind() {
            window.clear();
            wordBuilder.Clear();
            windowQueue.Clear();
        }
        public void back() {
            if ( wordBuilder.Length > 0 )
                wordBuilder.Length--;
        }

        public Trie getWindow() {
            return window;
        }


        public void calculateWindow( int selectionStart , string text ) {

            bw.queue(() => {
                if ( onWindowInvalid != null )
                    onWindowInvalid();
                rewind();
                int i;
                for ( i = 0 ; i < text.Length ; i++ )
                    apply(text[i]);
                Form1.clear();
                Form3.clearSimilarities();
               Stats.clearSimilarities();
                calculateSimiliarity(window);   
                if ( onWindowReady != null ) onWindowReady();
            });
        }

        private void calculateSimiliarity( Trie window ) {
            List<Node> windowWords = window.getAll();
            List<string> windowVector = new List<string>();
            Distribution X = new Distribution();
            double xmax = 0;
            foreach ( Node n in windowWords ) {
                string s = n.ToString();
                windowVector.Add(s);
                double x = window.weight > 0 ? n.getNumOfAppearances() / (double)window.weight : 0;
                X.add(s , x );
                xmax += x * x;
            }
            double rmin = 0;
            foreach ( Topic t in SuggestionUtils.fringeTopics.Values ) {
                Distribution Y = t.topicVector;
                t.rtmin = 0;
                for ( int i = 0 ; i < Math.Max(Y.count , X.count) ; i++ )
                    t.rtmin += Math.Pow(Y[i] - X[i] , 2);
                rmin = Math.Min(rmin,t.rtmin);
                t.rtmax = xmax;
                for ( int i = 0 ; i < Y.count ; i++ ) 
                    t.rtmax += Math.Pow(Y[i], 2);
                List<string> union = new List<string>(windowVector);
                foreach (object o in Y.keys ) 
                    if (union.Contains((string)o)) 
                        union.Add((string)o);
                t.rt = 0;
                for ( int i = 0 ; i < union.Count ; i++ ) {
                    string s = union[i];
                    double xs = X[s];
                    double ys = Y[s];
                    t.rt += Math.Pow(xs - ys , 2);
                } 
            }
            foreach ( Topic t in SuggestionUtils.fringeTopics.Values ) {
                double frt = ( t.rt - rmin ) / ( t.rtmax - rmin );
                t.similiarity = similiarity1(frt);
                if ( onTopicReady != null ) onTopicReady(t);
                Form3.updateChartForTopic(t.topic , frt);
                Stats.updateChartForTopic(t.topic , frt);
            }
            Form2.showPlots(1);

        }

        public static double similiarity1( double frt ) {
            double s = 0.125;
            return Math.Exp(-Math.Pow(frt , 2) / ( 2 * s * s ));
        }
        public static double similiarity2( double frt ) {
            return Math.Exp(-frt*4);
        }


        //private double calculateSimiliarity( Topic t, Trie window ) {
        //    Trie fringe = t.fringeTrie;
        //    List<Node> windowWords = window.getAll();
        //    List<Node> trieWords = fringe.getAll();
        //    if ( windowWords.Count == 0 ) return 1;
        //    if ( fringe.weight == 0 ) return 0;
        //    double appearanceSquares = 0;
        //    foreach ( Node nWindow in windowWords ) {
        //        Node nTrie = fringe.getNode(nWindow.ToString());
        //        appearanceSquares += Math.Pow(nWindow.getNumOfAppearances() , 2) * Math.Pow(( nTrie == null ? 0 : nTrie.getNumOfAppearances() ) , 2);
        //    }
        //    appearanceSquares /= fringe.weight;
        //    return appearanceSquares;
        //}
        //private double calculateSimiliarity( Topic t, Trie window ) {
        //    Trie fringe = t.fringeTrie;
        //    List<Node> windowWords = window.getAll();
        //    List<Node> trieWords = fringe.getAll();
        //    if ( windowWords.Count == 0 ) return 1;
        //    if ( fringe.weight == 0 ) return 0;
        //    double appearanceSquares = 0;
        //    foreach ( Node nWindow in windowWords ) {
        //        Node nTrie = fringe.getNode(nWindow.ToString());
        //        appearanceSquares += nWindow.getNumOfAppearances() * ( ( nTrie == null ? 0 : nTrie.getNumOfAppearances() ) / (double)fringe.weight );
        //    }
        //    return appearanceSquares;
        //}

        //private double calculateSimiliarity( Trie window , Trie fringe ) {
        //    List<Node> windowWords = window.getAll();
        //    List<Node> trieWords = fringe.getAll();
        //    trieWords.Sort(( n1 , n2 ) => n2.getNumOfAppearances() - n1.getNumOfAppearances());
        //    double sumXY = 0;
        //    double sumFreqInTrie = 0;
        //    for ( int i = 0 ; i < Configurations.TRIE_TOPIC_REPRESENTIVE_COUNT ; i++ ) {
        //        Node n = trieWords[i];
        //        sumFreqInTrie += n.getNumOfAppearances() / (double)fringe.weight;
        //    }
        //    for ( int i = 0 ; i < Configurations.TRIE_TOPIC_REPRESENTIVE_COUNT ; i++ ) {
        //        Node n = trieWords[i];
        //        Node node = window.getNode(n.ToString());
        //        double freqInWindow = ( node == null ? 0 : node.getNumOfAppearances() ) / (double)window.weight;
        //        double freqInTrie = n.getNumOfAppearances() / (double)fringe.weight;
        //        sumXY += Math.Pow(( freqInTrie / sumFreqInTrie ) - freqInWindow , 2);
        //    }
        //    return Math.Pow(Math.E , -( sumXY / 1 ));
        //}

        //private double calculateSimiliarity( Topic t , Trie window ) {
        //    Trie fringe = t.fringeTrie;
        //    List<Node> windowWords = window.getAll();
        //    List<Node> trieWords = fringe.getAll();
        //    List<String> topicVector = new List<String>();
        //    List<String> union = new List<String>();
        //    trieWords.Sort(( n1 , n2 ) => n2.getNumOfAppearances() - n1.getNumOfAppearances());
        //    foreach ( Node n in windowWords ) {
        //        String s = n.ToString();
        //        if ( !union.Contains(s) )
        //            union.Add(s);
        //    }
        //    double sumFreqInTopicVector = 0 , sumXY = 0;
        //    for ( int i = 0 ; i < Configurations.TRIE_TOPIC_REPRESENTIVE_COUNT ; i++ ) {
        //        Node n = trieWords[i];
        //        String s = n.ToString();
        //        topicVector.Add(s);
        //        sumFreqInTopicVector += n.getNumOfAppearances() / (double)fringe.weight;
        //        if ( !union.Contains(s) )
        //            union.Add(s);
        //    }
        //    foreach ( String s in union ) {
        //        Node nodeInTopicVector = fringe.getNode(s);
        //        Node nodeInWindow = window.getNode(s);
        //        double freqInWindow = ( nodeInWindow == null ? 0 : nodeInWindow.getNumOfAppearances() ) / (double)window.weight;
        //        double freqInTrie = 0;
        //        if ( topicVector.Contains(s) ) {
        //            freqInTrie = ( nodeInTopicVector == null ? 0 : nodeInTopicVector.getNumOfAppearances() ) / (double)fringe.weight;
        //        }
        //        Form1.addStatRow(t.topic , s , freqInTrie , freqInWindow);
        //        sumXY += Math.Pow(( freqInTrie / sumFreqInTopicVector ) - freqInWindow , 2);
        //    }
        //    return Math.Pow(Math.E , -( sumXY / Configurations.WINDOW_SIZE * Math.Log(Configurations.WINDOW_SIZE) ));
        //}
        
        //private double calculateSimiliarity( Topic t , Trie window ) {
        //    Trie fringe = t.fringeTrie;
        //    List<Node> windowWords = window.getAll();
        //    List<Node> trieWords = fringe.getAll();
        //    List<string> union = new List<string>();
        //    List<string> topicVector = new List<string>();
        //    List<string> windowVector = new List<string>();
        //    Distribution Y = new Distribution();
        //    Distribution X = new Distribution();
        //    int appsInTopicVector = 0;
        //    double r=0;
        //    trieWords.Sort(( n1 , n2 ) => n2.getNumOfAppearances() - n1.getNumOfAppearances());
        //    for ( int i = 0 ; i < Configurations.TOPIC_VECTOR_SIZE ; i++ ) {
        //        Node n = trieWords[i];
        //        string s = n.ToString();
        //        if ( union.Contains(s) ) throw new Exception("something is weird");
        //        union.Add(s);
        //        topicVector.Add(s);
        //        int apps = n.getNumOfAppearances();
        //        appsInTopicVector += apps;
        //    }
        //    for ( int i = 0 ; i < Configurations.TOPIC_VECTOR_SIZE ; i++ )
        //        Y.add(trieWords[i].ToString(),appsInTopicVector > 0 ? trieWords[i].getNumOfAppearances() / (double)appsInTopicVector : 0);
        //    foreach ( Node n in windowWords ) {
        //        string s = n.ToString();
        //        if (!union.Contains(s)) 
        //            union.Add(s);
        //        windowVector.Add(s);
        //        X.add(s , window.weight > 0 ? n.getNumOfAppearances() / (double)window.weight : 0);
        //    }
        //    for ( int i = 0 ; i < union.Count ; i++ ) {
        //        string s_i = union[i];
        //        double x_i = X[s_i];
        //        double y_i = Y[s_i];
        //        //Form1.addStatRow(t.topic , s_i , x_i , y_i);
        //        r += Math.Pow(x_i - y_i , 2);
        //    }
        //    Func<double,double> squareX = ( x ) => Math.Pow(x , 2);
        //    double sumXMax = X.summation(squareX);
        //    double sumYMax = Y.summation(squareX);
        //    Form1.addStatRow(t.topic , "Max(sumX^2,sumY^2)" , sumXMax , sumYMax);
        //    UniformDistribution<string> UW = new UniformDistribution<string>(windowVector , windowVector.Count);
        //    UniformDistribution<string> UT = new UniformDistribution<string>(topicVector , topicVector.Count);
        //    Form1.addStatRow(t.topic , "D_KL(X||U,Y||U)" , X.KLDistance(UW) , Y.KLDistance(UT));
        //    Form1.addStatRow(t.topic , "r" , r , 0);
        //    Form2.showPlots(sumXMax + sumYMax);
        //    Form3.updateChartForTopic(t.topic , r,sumXMax, sumYMax);
        //    Stats.updateChartForTopic(t.topic,r);
        //    return similarityOfek(r , sumXMax + sumYMax);
        //}
        //private double calculateSimiliarity( Topic t , Trie window ) {
        //    Trie fringe = t.fringeTrie;
        //    List<Node> windowWords = window.getAll();
        //    List<Node> trieWords = fringe.getAll();
        //    List<string> union = new List<string>();
        //    List<string> topicVector = new List<string>();
        //    List<string> windowVector = new List<string>();
        //    Distribution Y = new Distribution();
        //    Distribution X = new Distribution();
        //    int appsInTopicVector = 0;
        //    double r = 0;
        //    trieWords.Sort(( n1 , n2 ) => n2.getNumOfAppearances() - n1.getNumOfAppearances());
        //    for ( int i = 0 ; i < Configurations.TOPIC_VECTOR_SIZE ; i++ ) {
        //        Node n = trieWords[i];
        //        string s = n.ToString();
        //        if ( union.Contains(s) ) throw new Exception("something is weird");
        //        union.Add(s);
        //        topicVector.Add(s);
        //        int apps = n.getNumOfAppearances();
        //        appsInTopicVector += apps;
        //    }
        //    for ( int i = 0 ; i < Configurations.TOPIC_VECTOR_SIZE ; i++ )
        //        Y.add(trieWords[i].ToString() , appsInTopicVector > 0 ? trieWords[i].getNumOfAppearances() / (double)appsInTopicVector : 0);
        //    foreach ( Node n in windowWords ) {
        //        string s = n.ToString();
        //        if ( !union.Contains(s) )
        //            union.Add(s);
        //        windowVector.Add(s);
        //        X.add(s , window.weight > 0 ? n.getNumOfAppearances() / (double)window.weight : 0);
        //    }
        //    for ( int i = 0 ; i < union.Count ; i++ ) {
        //        string s_i = union[i];
        //        double x_i = X[s_i];
        //        double y_i = Y[s_i];
        //        //Form1.addStatRow(t.topic , s_i , x_i , y_i);
        //        r += Math.Pow(x_i - y_i , 2);
        //    }
        //    Func<double , double> squareX = ( x ) => Math.Pow(x , 2);
        //    double sumXMax = X.summation(squareX);
        //    double sumYMax = Y.summation(squareX);

        //    Form1.addStatRow(t.topic , "Max(sumX^2,sumY^2)" , sumXMax , sumYMax);
        //    UniformDistribution<string> UW = new UniformDistribution<string>(windowVector , windowVector.Count);
        //    UniformDistribution<string> UT = new UniformDistribution<string>(topicVector , topicVector.Count);
        //    Form1.addStatRow(t.topic , "D_KL(X||U,Y||U)" , X.KLDistance(UW) , Y.KLDistance(UT));
        //    Form1.addStatRow(t.topic , "r" , r , 0);
        //    Form2.showPlots(sumXMax + sumYMax);
        //    Form3.updateChartForTopic(t.topic , r , sumXMax , sumYMax);
        //    Stats.updateChartForTopic(t.topic , r);
        //    return similarityOfek(r , sumXMax + sumYMax);
        //}
        private static double WLOGW = Configurations.WLOGW;
        private static double gamma = Configurations.GAMMA;
        private static double sigma = Configurations.SIGMA;

        private static double func1_end;
        public static double similarityOfek( double r , double max ) {
            double s = max/8;
            return Math.Exp(-Math.Pow(r , 2) / ( 2 * s * s ));

        }

        public static double similarityBest( double r , double max ) {
            double s = max / 8;
            return Math.Exp(-Math.Pow(r , 2) / ( 2 * s * s ));

        }

        //public static double similarityOfek( double r , double max ) {
        //    double delta = max/2;
        //    func1_end = 1 / 2.0;
        //    if ( r >= 0 && r < delta )
        //        return func1(r , func1_end,delta , max);
        //    else
        //        return similarity3(( r - delta ) / ( 1 - delta ) , max) * func1_end;
        //}
        public static double similarityOrly( double r ) {
            return Math.Pow(Math.E , -( r / sigma ));
        }
        public static double func1( double r , double y , double delta, double max ) {
            double a = r / ( delta );
            double result = 1 - Math.Pow(Math.E , a - 1);
            result /= ( 1 - ( 1 / Math.E ) );
            result += 0.6 * ( a - 1 );
            result *= 2.5;
            result *= ( 1 - y );
            return result + y;
        }
        //public static double func2( double r ) {
        //    double result = Math.Pow(Math.E , -r / sigma) - 1 / Math.Pow(Math.E , 1 / sigma);
        //    result /= ( Math.Pow(Math.E , -delta * sigma) - 1 / Math.Pow(Math.E , 1 / sigma) );
        //    result *= func1_end;
        //    return result;
        //}

        public static double similarityOrly( double r , double sigma ) {
            return Math.Pow(Math.E , -( r / sigma ));
        }

        public static double similarity3( double r , double sigma ) {
            double y = 10;
            return ( Math.Pow(Math.E , -( y * r / sigma )) - Math.Pow(Math.E , -y) ) / ( 1 - Math.Pow(Math.E , -y) );
            
        }



    }

}
