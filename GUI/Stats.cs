using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TextEditor.Control;
using TextPredict.Base;
using TextPredict.Control;
namespace TextPredict.GUI {
    public partial class Stats : Form {
        public static Stats instance;
        private WindowHandler wHandler; 
        public Stats() {
            instance = this;
            InitializeComponent();
            wHandler = WindowHandler.getInstance();
            wHandler.onWindowReady += onWindowReady;
            
        }
        public void onWindowReady() {
            invoke(new Runnable(() => {
                    listBox1.Items.Clear();
                    listBox3.Items.Clear();
                    Trie window = wHandler.getWindow();
                    foreach ( Node s in window.getAll() )
                        listBox1.Items.Add(s.ToString() + "    X" + s.getNumOfAppearances());
                    dataGridView1.Rows.Clear();
                    
                    foreach ( Topic t in SuggestionUtils.fringeTopics.Values ) {
                        
                        listBox3.Items.Add(t.topic + " Representives:");
                        foreach (String s in getRepresentives(t))
                            listBox3.Items.Add( "    "+s );
                    }
                    
                }));
        }


        public static void showOfekSuggestions( List<Suggestion> list ) {
            invoke(new Runnable(() => {
                    instance.listBox2.Items.Clear();
                    foreach ( Suggestion s in list )
                        instance.listBox2.Items.Add(s.suggestion + " , " + String.Format("{0:0.#####}" , s.weight));
                }));
        }

        public bool onSuggestionEvent( Suggestion arg ) {
            List<Suggester> names = arg.suggesters;
            var result = String.Join(", " , names);
            invoke(() => dataGridView1.Rows.Add(arg.suggestion , result , String.Format("{0:0.#####}" , arg.weight)));
            return true; 
        }

        public static void invoke(Runnable run) {
            if ( instance.IsHandleCreated )
                instance.Invoke(run);
        }
        public static List<String> getRepresentives( Topic t ) {
            List<Node> trieWords = t.fringeTrie.getAll();
            trieWords.Sort(( n1 , n2 ) => n2.getNumOfAppearances() - n1.getNumOfAppearances());
            List<String> results = new List<String>();
            for ( int i = 0 ; i < Configurations.TOPIC_VECTOR_SIZE ; i++ ) {
                Node n = trieWords[i];
                results.Add(n.ToString() + " " + String.Format("{0:0.#####}" , ( n.getNumOfAppearances() / (double)t.fringeTrie.weight )));
            }
            return results;
        }

        public static void updateChartForTopic( string p , double r ) {
            //invoke(() => {
            //    instance.chart1.Series[0].Points.AddXY(p , WindowHandler.similarityOrly(r , 0.1));
            //    instance.chart1.Series[1].Points.AddXY(p , WindowHandler.similarityOrly(r , 1 / (double)Configurations.WINDOW_SIZE));
            //    instance.chart1.Series[2].Points.AddXY(p , WindowHandler.similarityOrly(r , Configurations.SIGMA));
            //});

        }

        public static void clearSimilarities() {
            invoke(() => {
                instance.chart1.Series[0].Points.Clear();
                instance.chart1.Series[1].Points.Clear();
                instance.chart1.Series[2].Points.Clear();
            });
        }
    }
}
