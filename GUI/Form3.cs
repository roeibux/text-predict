using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TextPredict.Control;

namespace TextPredict.GUI {
    public partial class Form3 : Form {
        private static Form3 instance;
        public Form3() {
            instance = this;
            InitializeComponent();
        }



        public static void clearSimilarities() {
            invoke(() => {
                instance.chart1.Series[0].Points.Clear();
                instance.chart2.Series[0].Points.Clear();
                instance.chart3.Series[0].Points.Clear();
                instance.chart4.Series[0].Points.Clear();
            });
        }
        public static void invoke( Runnable run ) {
            if ( instance.IsHandleCreated )
                instance.Invoke(run);
        }

        internal static void updateChartForTopic( string p , double frt ) {
            invoke(() => {
                instance.chart1.Series[0].Points.AddXY(p , WindowHandler.similiarity1(frt));
                instance.chart2.Series[0].Points.AddXY(p , WindowHandler.similiarity2(frt));
                //instance.chart3.Series[0].Points.AddXY(p , WindowHandler.similarityOrly(r , Configurations.SIGMA));
                //instance.chart4.Series[0].Points.AddXY(p , WindowHandler.similarityOfek(r , sumXMax + sumYMax));
            });
        }
    }


}
