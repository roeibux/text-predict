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
    public partial class Form1 : Form {
        private static Form1 instance;
        public Form1() {
            instance = this;
            InitializeComponent();

        }

        private void dataGridView1_CellContentClick( object sender , DataGridViewCellEventArgs e ) {

        }

        public static void addStatRow(String t,String w,double xi,double yi) {
            invoke(() => instance.dataGridView1.Rows.Add(t,w , String.Format("{0:0.#####}" , xi) , String.Format("{0:0.#####}" , yi)));
        }
        public static void clear( ) {
            invoke(() => instance.dataGridView1.Rows.Clear());
        }
        public static void invoke( Runnable run ) {
            if ( instance.IsHandleCreated )
                instance.Invoke(run);
        }
    }
}
