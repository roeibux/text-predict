using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using TextPredict.Control;

namespace TextPredict.GUI {
    public partial class Form2 : Form {
        private double LOGW =  Math.Log(Configurations.WINDOW_SIZE);
        private double WLOGW = Configurations.WINDOW_SIZE * Math.Log(Configurations.WINDOW_SIZE);
        private static Form2 instance;
        public Form2() {
            instance = this;
            InitializeComponent();
            
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            double n = 1000;
            for ( int i = 0 ; i < n ; i++ ) {
                double d = ( i / n ) * ( Configurations.DELTA + Configurations.GAMMA + 0.05 );
                chart1.Series[0].Points.AddXY(d , WindowHandler.similarity3(d , 0.1));
                chart1.Series[1].Points.AddXY(d , WindowHandler.similarityOrly(d , 1 / (double)Configurations.WINDOW_SIZE));
                chart1.Series[2].Points.AddXY(d , WindowHandler.similarityOrly(d,Configurations.SIGMA));
            }
        }
        public static void invoke( Runnable run ) {
            if ( instance.IsHandleCreated )
                instance.Invoke(run);
        }
        public static  void showPlots(double max) {
           
            invoke(() => {
                double n = 1000;
                instance.chart1.Series[0].Points.Clear();
                instance.chart1.Series[1].Points.Clear();
                instance.chart1.Series[2].Points.Clear();
                instance.chart1.Series[3].Points.Clear();
                for ( int i = 0 ; i < n ; i++ ) {
                    double d = ( i / n ) * ( max );
                    instance.chart1.Series[0].Points.AddXY(d , WindowHandler.similarity3(d , max));
                    instance.chart1.Series[1].Points.AddXY(d , WindowHandler.similarityOrly(d , 1 / (double)Configurations.WINDOW_SIZE));
                    instance.chart1.Series[2].Points.AddXY(d , WindowHandler.similarityOrly(d , Configurations.SIGMA));
                    instance.chart1.Series[3].Points.AddXY(d , WindowHandler.similarityOfek(d,max));
                }
            });
        }


    }
}
