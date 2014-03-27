using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TextEditor.Control;
using TextEditor.SystemProj;
using TextPredict.Control;
using TextPredict.GUI;
using TextPredict.Properties;

namespace TextEditor.GUI
{
    public partial class MainWindow : Form
    {
        private string FileName;
        private bool isFileChanged;
        private string currentWord;
        private int sizeOfCurrentWindow = 0;
        private int index = 0;
        private Point pt = new Point(0, 27);
        private int numberOfKeysInTextBox = 0;
        private char[] letter;
        private bool isWordCompletion;
        private bool demoIsRunning;
        private bool[] isCurrentWordHasMatcher;
        private bool isStopTrieHasMatcher;
        private bool isBackspace;
        public long _NumberOfKeys = 0;
        //setting labels as well
        public long NumberOfKeys
        {
            set
            {
                _NumberOfKeys = value;
            }
            get { return _NumberOfKeys; }
        }

        private long _NumberOfKeysTyped = 0;
        private SuggestionManager suggestionManager;
        private Worker bw;
        private Demonstrator demonstrator;
        private WindowHandler windowHandler;
        //setting labels as well
        public long NumberOfKeysTyped
        {
            set
            {
                _NumberOfKeysTyped = value;
            }
            get { return _NumberOfKeysTyped; }
        }
      
        public MainWindow()
        {
            InitializeComponent();
            demoIsRunning = false;
            FileName = null;
            isFileChanged = false;
            
            letter = new char[1];
            isWordCompletion = false;
            isBackspace = false;
            isCurrentWordHasMatcher = new bool[MainControl.numberOfMostRelevntTries];
            isStopTrieHasMatcher = true;
            listBox1.VisibleChanged += ( o , e ) => {
                if ( listBox1.Visible ) {
                    if ( listBox1.Items.Count > 0 ) listBox1.SelectedIndex = 0;
                    updateSuggestions();
                }
                
            };
            for (int i = 0; i < MainControl.numberOfMostRelevntTries; i++)
                isCurrentWordHasMatcher[i] = true;
            //this.richTextBox.TextChanged+=new System.EventHandler(this.onTextChanged);
            this.richTextBox.KeyDown+=new System.Windows.Forms.KeyEventHandler(this.KeyDownHandler);
            this.richTextBox.KeyPress+=new System.Windows.Forms.KeyPressEventHandler(this.KeyPressHandler);
            this.richTextBox.SelectionChanged += onPositionChanged;
            this.suggestionManager = new SuggestionArranger(5);
            foreach ( Topic t in SuggestionUtils.fringeTopics.Values )
                suggestionManager.addSuggester(new OrlySuggester(t));
            suggestionManager.addSuggester(new OfekSuggester());
            this.suggestionManager.SuggestionEventHandler += onSuggestionEvent;
            this.suggestionManager.SuggestionEventHandler += Stats.instance.onSuggestionEvent;
            demonstrator = new Demonstrator(richTextBox,listBox1,listbox1_ItemSelected,this);
            demonstrator.setText(Resources.bicycle);
            windowHandler=WindowHandler.getInstance();
            windowHandler.onWindowReady += suggestionManager.runSuggester;
            updateStats();
        }
        public SuggestionManager getSuggestionManager() {
            return suggestionManager;
        }

        public int numberOfCharsEntered;
        public int numberOfCharsSaved;
        public int numberOfManagingHits;
        private int totalKeys;
        private void onPositionChanged( object sender , EventArgs e ) {
            this.richTextBox.SelectionChanged -= onPositionChanged;
            updateSuggestions();  
            this.richTextBox.SelectionChanged += onPositionChanged;
        }

        private void updateStats() {
            lblPercent.Text = ( totalKeys == 0 )
                      ? String.Format("Keystroke Saved: 0%")
                      : String.Format("Keystroke Saved: {0:P1}" , numberOfCharsSaved / (double)( totalKeys + (Configurations.IGNORE_MANAGING_HITS ? 0 : numberOfManagingHits) ));
            toolStripStatusLabel2.Text = String.Format("Characters Typed: {0}" , numberOfCharsEntered);
            toolStripStatusLabel4.Text = String.Format("Total: {0}" , totalKeys);
            toolStripStatusLabel1.Text = String.Format("Managing Hits: {0}" , numberOfManagingHits);
        }
     
        private void updateSuggestions() {
            if ( !listBox1.Visible ) return;
            listBox1.Location = getLBLocation(richTextBox.SelectionStart);
            listBox1.Items.Clear();
            suggestionManager.resetEnviroment();
            for ( int i = 0 ; i < richTextBox.SelectionStart  ; i++ )
                if ( Utils.isRelevantKey(richTextBox.Text[i]) ) 
                    suggestionManager.apply(Char.ToLower(richTextBox.Text[i]));
            windowHandler.calculateWindow(richTextBox.SelectionStart , richTextBox.Text);
        }

        private void KeyPressHandler( object sender , KeyPressEventArgs e ) {
            if ( !Utils.isRelevantKey(e.KeyChar) ) return;
            if ( e.KeyChar == (char)Keys.Space && ( ModifierKeys == Keys.Control ) ) {
                e.Handled = true;
            }
        }

        


        public bool onSuggestionEvent(Suggestion suggestion) {
            return (bool)this.Invoke(new Func<bool>(()=>{
                listBox1.Items.Add(suggestion.suggestion);
                if ( listBox1.Items.Count == 1 ) listBox1.SelectedIndex = 0;
                return listBox1.Items.Count < Configurations.SUGGESTIONS_COUNT_LIMIT;
            }));
        }

        private void listbox1_ItemSelected() {
            this.richTextBox.SelectionChanged -= onPositionChanged;
            string selectedWord = (string)listBox1.SelectedItem;
            int i,j;
            for ( i = richTextBox.SelectionStart - 1 ; i >= 0 && !Suggester.isSeparator(richTextBox.Text[i]) ; i-- ) ;
            for ( j = richTextBox.SelectionStart ; j < richTextBox.Text.Length && !Suggester.isSeparator(richTextBox.Text[j]) ; j++ ) ;
            selectedWord += ( j < richTextBox.Text.Length && Suggester.isSeparator(richTextBox.Text[j]) ? "" : " " );
            int length = 0;
            if ( ++i < richTextBox.Text.Length ) {
                richTextBox.SelectionStart=i;
                length = richTextBox.SelectionLength=j-i;
                richTextBox.SelectedText="";
            }
            richTextBox.AppendText(selectedWord);
            length = selectedWord.Length - ( j - i );
            richTextBox.SelectionStart = i + selectedWord.Length - length;
            richTextBox.SelectionLength = length;
            richTextBox.SelectionColor = Color.BlueViolet;
            
            
            richTextBox.SelectionStart = i + selectedWord.Length + 1;
            richTextBox.SelectionLength = 0;
            richTextBox.SelectionColor = Color.Black;
            updateSuggestions();
            updateStats();
            this.richTextBox.SelectionChanged += onPositionChanged;
        }



        private void KeyDownHandler( object sender , KeyEventArgs e ) {
            if ( ( e.KeyCode == Keys.Down ||
                  e.KeyCode == Keys.Left ||
                  e.KeyCode == Keys.Up ||
                  e.KeyCode == Keys.Right ) && !listBox1.Visible ) return;
            if ( e.KeyCode == Keys.Space && ( e.Control ) ) {
                listBox1.Visible = !listBox1.Visible;
                e.Handled = true;
            }
            if ( e.Control && e.KeyCode==Keys.D ) {

            }
           switch ( e.KeyCode ) {
                    case ://demo
                        e.Handled = true;
                        if ( !demonstrator.isPaused ) demonstrator.pause();
                        else demonstrator.proceed();
                        break;
                    case Keys.Down:
                        listBox1.SelectedIndex = Math.Min(listBox1.SelectedIndex + 1 , listBox1.Items.Count - 1);
                        e.Handled = true;
                        break;
                    case Keys.Up:
                        listBox1.SelectedIndex = Math.Max(listBox1.SelectedIndex - 1 , 0);
                        e.Handled = true;
                        break;
                    case Keys.Right:
                        if ( listBox1.SelectedItem != null && listBox1.Visible ) listbox1_ItemSelected();
                        e.Handled = true;
                        break;
                    case Keys.Left:
                        if ( listBox1.SelectedItem != null ) listBox1.ClearSelected();
                        else listBox1.Visible = false;
                        e.Handled = true;
                        break;
                }
            }
            
      







        private void SetAllMatachers(char[] word,bool reset = true)
        {
            int trieIndex;

            if (reset)
            {
                isCurrentWordHasMatcher = new bool[MainControl.numberOfMostRelevntTries];
                for (trieIndex = 0; trieIndex < isCurrentWordHasMatcher.Length; trieIndex++)
                    isCurrentWordHasMatcher[trieIndex] = true;

                //reset the Matcheres
                MainControl.dataBase.StopWordsTrie.Matcher.ResetMatch();
                isStopTrieHasMatcher = true;
                foreach (var trie in MainControl.dataBase.TriesByTopic.Values)
                    trie.Matcher.ResetMatch();

                if (word == null) return;
            }
            trieIndex = 0;
            //set all machers to the current new word
            for (int k = 0; k < word.Length; k++)
            {
                if (isStopTrieHasMatcher)
                    isStopTrieHasMatcher = MainControl.dataBase.StopWordsTrie.Matcher.NextMatch(word[k]);
                foreach (var trie in MainControl.MostRelevntTries)
                {
                    if (isCurrentWordHasMatcher[trieIndex])
                            isCurrentWordHasMatcher[trieIndex] = trie.Matcher.NextMatch(word[k]);
                    trieIndex++;
                }
            }
        }

        //private void backspacePressed()
        //{
        //    isBackspace = true;
        //    listBox1.Visible = false;
        //    if (String.IsNullOrEmpty(currentWord))
        //    {
        //        if (richTextBox.TextLength == 0) return;
        //        var lineNum = richTextBox.GetLineFromCharIndex(richTextBox.SelectionStart);
        //        string curLine = richTextBox.Lines[lineNum];
        //        if (curLine.Length == 0)
        //            curLine = richTextBox.Lines[lineNum - 1];
        //        else
        //            curLine = curLine.Substring(0, curLine.Length - 1);
        //        currentWord = curLine.Contains(' ') ? curLine.Substring(curLine.LastIndexOf(" ", StringComparison.Ordinal) + 1) : curLine;
        //        index = currentWord.Length;
        //        SetAllMatachers(currentWord.ToCharArray());
        //    }
        //    else
        //    {
        //        currentWord = currentWord.Substring(0, currentWord.Length - 1);
        //        index--;
        //        SetAllMatachers(currentWord.ToCharArray());
        //    }
        //}

        //if key space was pressed
        private void KeySpacePressed()
        {
            if (currentWord == null) 
            {
                printReleventSuffixes();
                return;
            }

            if (!MainControl.dataBase.isStopWord(currentWord))
            {
                if (sizeOfCurrentWindow < MainControl.WINDOW_SIZE)
                    sizeOfCurrentWindow++;
                else
                    MainControl.LastWordsWindow.RemoveAt(0);

                MainControl.LastWordsWindow.Add(currentWord);
                if (sizeOfCurrentWindow == MainControl.WINDOW_SIZE)
                    MainControl.UpdateSimilarity();
            }

            currentWord = null;
            index = 0;
            SetAllMatachers(null);
            printReleventSuffixes();
        }
        
        //Regular key was pressed 
        private void KeyIsPressed(char[] sequence)
        {
            currentWord = currentWord == null ? new string(sequence) : currentWord.Insert(index, sequence[0].ToString());
            index++;

            //print all the relevnt Suffixes in the list box
            SetAllMatachers(sequence, false);
            printReleventSuffixes();
        }

        //print all the relevnt Suffixes in the list box
        private void printReleventSuffixes()
        {
            listBox1.Items.Clear();
            printReleventFringeSuffixes();
            printReleventStopSuffixes();
            if ( listBox1.Items.Count > 0 ) {
                //set listbox new location
                listBox1.Location = getLBLocation(richTextBox.SelectionStart);
                listBox1.Visible = true;
            }
        }

        public void printReleventStopSuffixes()
        {
            if (!isStopTrieHasMatcher) return;
            
            //print the stop words suffixes to the listbox
            //get the list of suffixes 
            var Suffixes = MainControl.dataBase.StopWordsTrie.Matcher.GetPrefixSuggestions(MainControl.numberOfStop, MainControl.ConsiderSuffixLength, MainControl.GetProbabilityFor, new MainControl.ReverseDoubleComparer());
            var i = 0;
            while (i < MainControl.numberOfStop && Suffixes.Count > 0)
            {
                List<string> list = Suffixes.First().Value;
                while (i < MainControl.numberOfStop && list.Count > 0)
                {
                    listBox1.Items.Add(list.First());
                    list.RemoveAt(0);
                    i++;
                }
                Suffixes.RemoveAt(0);
            }
        }

        public void printReleventFringeSuffixes()
        {
            var fringeWordSuffixes = new SortedList<double, List<string>>(new MainControl.ReverseDoubleComparer());
            int trieIndex = 0;

            //print the fringe words suffixes to the listbox
            foreach (var trie in MainControl.MostRelevntTries)
            {
                if (isCurrentWordHasMatcher[trieIndex])
                {
                    //get list of matches
                    var tempList = trie.Matcher.GetPrefixSuggestions(MainControl.numberOfFringe, MainControl.ConsiderSuffixLength, MainControl.GetProbabilityFor,new MainControl.ReverseDoubleComparer());
                    addToMainList(fringeWordSuffixes, tempList);
                }
                trieIndex++;
            }

            //add to listbox only the words with the most highest Weight
            int i = 0;
            while (i < MainControl.numberOfFringe && fringeWordSuffixes.Count > 0)
            {
                List<string> list = fringeWordSuffixes.First().Value;
                while (list.Count > 0 && i < MainControl.numberOfFringe)
                {
                    listBox1.Items.Add(list.First());
                    list.RemoveAt(0);
                    i++;
                }
                fringeWordSuffixes.RemoveAt(0);
            }
        }

        private static void addToMainList(SortedList<double, List<string>> main, SortedList<double, List<string>> toAdd)
        {
            if (toAdd == null || toAdd.Count == 0) return;
            foreach(var key in toAdd.Keys)
            {
                List<string> li;
                if (main.ContainsKey(key))
                {
                    li = main[key];
                    main.Remove(key);
                }
                else
                    li = new List<string>();
                li = new List<string>(li.Concat(toAdd[key]));
                main.Add(key,li);
            }
        }



        private void listboxClick(object sender, MouseEventArgs e)
        {
            if (listBox1.SelectedIndex < 0 || !listBox1.GetItemRectangle(listBox1.SelectedIndex).Contains(e.Location))
                return;
            listbox1_ItemSelected();
        }

        #region Form Events

        private void addWordFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainControl.loadWordFile = new LoadWordFile { Visible = true };
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainControl.configurationWin = new ConfigurationWindow { Visible = true };
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Create an OpenFileDialog to request a file to open.
            var openFile = new OpenFileDialog();

            // Initialize the OpenFileDialog to look for RTF files.
            openFile.DefaultExt = "*.rtf";
            openFile.Filter = "RTF Files|*.rtf";

            try
            {
                // Determine whether the user selected a file from the OpenFileDialog. 
                if (openFile.ShowDialog() == global:: System.Windows.Forms.DialogResult.OK &&
                    openFile.FileName.Length > 0)
                {
                    // Load the contents of the file into the RichTextBox.
                    richTextBox.LoadFile(openFile.FileName);
                    FileName = String.Copy(openFile.FileName);
                }
            }
            catch (Exception errorMsg)
            {
                MessageBox.Show(errorMsg.Message);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            save();
        }

        private void save()
        {
            //checkes if the Command "Save As" was executed
            if (FileName != null)
            {
                // Save the contents of the RichTextBox into the file.
                richTextBox.SaveFile(FileName);
            }
            else
            {
                saveAs();
            }
            //indicate if the file was changed and not saved yet 
            isFileChanged = true;
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveAs();
            //indicate if the file was changed and not saved yet 
            isFileChanged = true;
        }

        private void saveAs()
        {
            // Create a SaveFileDialog to request a path and file name to save to.
            var saveFile = new SaveFileDialog();

            // Initialize the SaveFileDialog to specify the RTF extention for the file.
            saveFile.DefaultExt = "*.rtf";
            saveFile.Filter = "RTF Files|*.rtf";

            try
            {
                // Determine whether the user selected a file name from the saveFileDialog.
                if (saveFile.ShowDialog() == global:: System.Windows.Forms.DialogResult.OK &&
                    saveFile.FileName.Length > 0)
                {
                    // Save the contents of the RichTextBox into the file.
                    richTextBox.SaveFile(saveFile.FileName);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Text Editor",
                                MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                MessageBox.Show(exception.StackTrace, "Text Editor",
                                MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void exit()
        {
            if (isFileChanged)
            {
                DialogResult dialogResult = MessageBox.Show("Do you want to save the changes? ", "", MessageBoxButtons.YesNoCancel);
                if (dialogResult == DialogResult.Cancel)
                    return;
                if (dialogResult == DialogResult.Yes)
                    save();
            }
            MainControl.Exit();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exit();
        }

        private void exitPressed(object sender, FormClosingEventArgs e)
        {
            exit();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isFileChanged == true)
            {
                DialogResult dialogResult = MessageBox.Show("Do you want to save the changes? ", "", MessageBoxButtons.YesNoCancel);
                if (dialogResult == DialogResult.Cancel)
                    return;
                if (dialogResult == DialogResult.Yes)
                    save();

            }
            FileName = null;
            richTextBox.Clear();
            numberOfKeysInTextBox = 0;
            NumberOfKeysTyped = 0;
            NumberOfKeys = 0;
            MainControl.LastWordsWindow = new List<string>();
            sizeOfCurrentWindow = 0;
            letter = new char[1];
            isWordCompletion = false;
            isBackspace = false;
            isCurrentWordHasMatcher = new bool[MainControl.numberOfMostRelevntTries];
            isStopTrieHasMatcher = true;
            for (int i = 0; i < MainControl.numberOfMostRelevntTries; i++)
                isCurrentWordHasMatcher[i] = true;
            currentWord = null;
            index = 0;
        }

        private void ClearDB(object sender, EventArgs e)
        {
            var ret = MessageBox.Show("Are You sure you wan't to clear all the data?", "Confirmation", MessageBoxButtons.YesNo);
            if (ret == DialogResult.No) return;
            var temp = this.Text.ToString();
            this.Text = this.Text + " - Clearing...";
            MainControl.ClearDb();
            setDatasetsCount(0);
            this.Text = temp;
            MessageBox.Show("Database Cleared");
        }



      

        private void ListBox_indexChanged(object sender, EventArgs e)
        {
            listbox1_ItemSelected();
        }

        #endregion

        #region Demonstration

        public void ToggleDemo(bool demo = true)
        {
            progressBarDemo.Visible = !progressBarDemo.Visible;
            if (demo)
            {
                richTextBox.Enabled = !richTextBox.Enabled;
                richTextBox.ForeColor = Color.Black;
                labelDemo.Visible = !labelDemo.Visible;
                stopDemoButton.Visible = !stopDemoButton.Visible;
                fileToolStripMenuItem.Enabled = !fileToolStripMenuItem.Enabled;
                toolsToolStripMenuItem.Enabled = !toolsToolStripMenuItem.Enabled;
                demoIsRunning = !demoIsRunning;
            }
        }
        
        private void startDemo(object sender, EventArgs e)
        {
            // Create an OpenFileDialog to request a file to open.
            var openFile = new OpenFileDialog();

            // Initialize the OpenFileDialog to look for RTF files.
            openFile.DefaultExt = "*.txt";
            openFile.Filter = "Text Files|*.txt";

            try
            {
                // Determine whether the user selected a file from the OpenFileDialog. 
                if (openFile.ShowDialog() != DialogResult.OK || openFile.FileName.Length == 0)
                    return;
                if (!MainControl.StartDemo(richTextBox, listBox1, openFile.FileName))
                {
                    MessageBox.Show(String.Format("Can't Open file {0} for demonstration", openFile.FileName));
                    return;
                }
                this.listBox1.SelectedIndexChanged += new EventHandler(ListBox_indexChanged);
                ToggleDemo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Error in starting demonstration\n{0}", ex.Message));
            }
        }


        private void stopDemoButton_Click(object sender, EventArgs e)
        {
            stopDemoButton.Enabled = false;
            MainControl.StopDemo();
            stopDemoButton.Enabled = true;
        }

        public void finishDemo()
        {
            ToggleDemo();
            this.listBox1.SelectedIndexChanged -= new EventHandler(ListBox_indexChanged);
            MessageBox.Show("Demo is Finished");
        }

        public void updateProgressbar(int percent)
        {
            progressBarDemo.Value = percent;
        }

        
        #endregion

        public void setDatasetsCount(int count)
        {
            lblDBCount.Text = String.Format("Datasets Count: {0}", count);
        }
        
        private Point getLBLocation(int caret)
        {
            //get position in relation to the start of the rich text box
            Point p = richTextBox.GetPositionFromCharIndex(caret);

            //add to p the x and y of the rich text box
            p.X += richTextBox.Location.X;
            p.Y += richTextBox.Location.Y;

            //add space from the caret
            p.X += 5;
            p.Y += 18;
            return p;
        }
        
        public void ListboxSelected()
        {
            listbox1_ItemSelected();
        }


    }
}