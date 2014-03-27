using System;
using System.Windows.Forms;
using TextEditor.SystemProj;

namespace TextEditor.GUI
{
    public partial class ConfigurationWindow : Form
    {

        public ConfigurationWindow()
        {
            InitializeComponent();

            comboBox1.SelectedIndex = MainControl.WINDOW_SIZE - 1;
            comboBox2.SelectedIndex = MainControl.numberOfFringe;
            comboBox3.SelectedIndex = MainControl.numberOfStop;
            NumOfTriesCombo.Items.Clear();
            var size = MainControl.dataBase.TriesByTopic.Count;
            for (int i = 1; i <= size; i++)
                NumOfTriesCombo.Items.Add(i);
            if (size == 0)
            {
                NumOfTriesCombo.Items.Add(0);
            }
            NumOfTriesCombo.SelectedIndex = (MainControl.numberOfMostRelevntTries <= 1) ? 0 : MainControl.numberOfMostRelevntTries - 1;
            checkBox2.Checked = MainControl.ConsiderSuffixLength;
            numericSigma.Value = Convert.ToDecimal(MainControl.Sigma);
            miliCounter.Value = Convert.ToDecimal(MainControl.demoIntervals / 1000.0);
        }


        private void button1_Click(object sender, EventArgs e) 
        {
            if (((Button)sender).Text.Equals("submit", StringComparison.CurrentCultureIgnoreCase))
            {
                MainControl.WINDOW_SIZE = Convert.ToInt32(comboBox1.SelectedItem);
                MainControl.numberOfFringe = Convert.ToInt32(comboBox2.SelectedItem);
                MainControl.numberOfStop = Convert.ToInt32(comboBox3.SelectedItem);
                MainControl.numberOfMostRelevntTries = Convert.ToInt32(NumOfTriesCombo.SelectedItem);
                MainControl.ConsiderSuffixLength = checkBox2.Checked;
                MainControl.Sigma = Convert.ToDouble(numericSigma.Value);
                MainControl.demoIntervals = Convert.ToInt32(miliCounter.Value*1000);
            }
            this.Visible = false;
            this.Dispose();
        }
    }
}
