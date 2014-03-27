using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TextEditor.SystemProj;
using TextEditor.Base.TrieCollection;

namespace TextEditor.GUI
{
    public partial class LoadWordFile : Form
    {
        private string WordFile;
        private string fileTopic;

        public LoadWordFile()
        {
            InitializeComponent();
            WordFile = null;
            TopicComboBox.Enabled = false;
        }

        private void LoadFile_Click(object sender, EventArgs e)
        {
            // Create an OpenFileDialog to request a file to open.
            OpenFileDialog openWordFile = new OpenFileDialog();

            // Initialize the OpenFileDialog to look for txt files.
            openWordFile.DefaultExt = "*.txt";
            openWordFile.Filter = "txt Files|*.txt";

            try
            {
                // Determine whether the user selected a file from the OpenFileDialog. 
                if (openWordFile.ShowDialog() == global:: System.Windows.Forms.DialogResult.OK &&
                   openWordFile.FileName.Length > 0)
                {
                    WordFile = String.Copy(openWordFile.FileName);
                    WordFileTextBox.Paste(WordFile);                   
                }
            }
            catch (Exception errorMsg)
            {
                MessageBox.Show(errorMsg.Message);
            }
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            if (TopictextBox.Enabled == false)
                fileTopic = TopicComboBox.SelectedItem.ToString();
            else
                fileTopic = TopictextBox.Text.ToString();

            if (WordFile == null)
            {
                MessageBox.Show("You need to upload a file");
                LoadFileButton.Focus();
                return;
            }
            if (String.IsNullOrEmpty(fileTopic) || String.IsNullOrWhiteSpace(fileTopic))
            {
                MessageBox.Show("You need to provide the file topic");
                if (TopictextBox.Enabled)
                    TopictextBox.Focus();
                else
                    TopicComboBox.Focus();
                return;
            }
            
            Trie trie;
            var temp = this.Text;
            this.Text = temp + " - Loading File...";
            this.Cursor = Cursors.WaitCursor;
            Ok.Enabled = false;
            MsgError msg = MainControl.loadFileControl.LoadFile(WordFile,out trie, fileTopic);
            if ( msg == MsgError.ERROR )
                MessageBox.Show("The file is empty");
            else
            {
                if (MsgError.OK == msg)
                {
                    MainControl.dataBase.addTopicTrie(fileTopic, trie);
                    MainControl.mainWin.setDatasetsCount(MainControl.dataBase.TriesByTopic.Count);
                    MessageBox.Show("File was uploaded successfully!");
                    TopictextBox.Clear();
                    TopicComboBox.Items.Clear();
                    topicsCheckBox.Checked = false;
                    WordFileTextBox.Clear();
                }
                else
                    MainControl.loadWordFile.Dispose();
            }
            this.Text = temp;
            this.Cursor = Cursors.Default;
            Ok.Enabled = true;
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            MainControl.loadWordFile.Dispose();
        }

        private void topicsList_CheckedChanged(object sender, EventArgs e)
        {
            if (topicsCheckBox.Checked == true)
            {
                if (MainControl.dataBase.TriesByTopic.Count != 0)
                {
                    foreach (KeyValuePair<String, Trie> entry in MainControl.dataBase.TriesByTopic)
                    {
                        TopicComboBox.Items.Add(entry.Key);
                    }
                    TopictextBox.Enabled = false;
                    TopicComboBox.Enabled = true;
                    TopicComboBox.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("No topics to present");
                    topicsCheckBox.Checked = false;
                }
            }
            else
            {
                TopictextBox.Enabled = true;
                TopicComboBox.Enabled = false;
            }
        }
    }
}
