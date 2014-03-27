using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace TextEditor.SystemProj
{
    /// <summary>
    /// This represents the Demonstration.
    /// The demonstration runs as a seperate thread.
    /// </summary>
    public class Demonstration
    {
        private BackgroundWorker bw;
        private readonly RichTextBox rtb;
        private readonly ListBox suggestions;
        private string[] fileLines;
        private int wordsInFile;
        private int wordsWritten;

        //Class delegates
        private delegate string StringVoid();
        private delegate bool BoolVoid();
        private delegate bool BoolString(string text);
        private delegate bool BoolInt(int index);
        private delegate bool BoolIntInt(int start, int end);
        private delegate int IntString(string text);
        private delegate bool BoolColor(Color c);
        private delegate int IntVoid();

        public Demonstration(RichTextBox textBox, ListBox list)
        {
            if (textBox == null || list == null) 
                throw new ArgumentNullException("Can't Input null arguments to Demonstration");
            rtb = textBox;
            suggestions = list;
            wordsWritten = 0;
        }

        public bool StartDemo(string filePath)
        {
            try 
            {
                if (!File.Exists(filePath)) return false;
                fileLines = File.ReadAllLines(filePath);
            }
            catch (Exception) { return false; }
             
            wordsInFile = getNumOfWords();
            bw = new BackgroundWorker {WorkerReportsProgress = true, WorkerSupportsCancellation = true};

            bw.DoWork += runDemo;
            bw.ProgressChanged += updateChange;
            bw.RunWorkerCompleted += demoComplete;

            bw.RunWorkerAsync();
            return true;
        }

        public void StopDemo()
        {
            if (bw.WorkerSupportsCancellation)
                bw.CancelAsync();
        }

        private void runDemo(object sender, DoWorkEventArgs e)
        {
            //setText("");
            for (var i = 0; i < fileLines.Length; i++)
            {
                
                if (i != 0)
                    appendText("\n");
                var words = fileLines[i].Split();
                foreach (var w in words)
                {
                    handleWord(w.Trim());
                    if (bw.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }
        }

        private void updateChange(object o, ProgressChangedEventArgs args)
        {
            MainControl.mainWin.updateProgressbar(args.ProgressPercentage);
        }

        private void demoComplete(object o, RunWorkerCompletedEventArgs args)
        {
            MainControl.mainWin.finishDemo();
        }

        #region Operation
        private void handleWord(string word)
        {
            if (string.IsNullOrEmpty(word)) return;
            bool breaked = false;
            int start = getSelectionStart();
            int end = start;
            foreach(var c in word)
            {
                if (setListbox(word))
                {
                    breaked = true;
                    break;
                }
                appendText(c.ToString());
                end = getSelectionStart();
                Thread.Sleep(MainControl.demoIntervals);
                
            }
            if (!breaked)
            {
                appendText(" ");
            }
            colorNotCompleted(start, end);
            wordsWritten++;
            double progress = (double)((double) wordsWritten/(double)wordsInFile);
            bw.ReportProgress((int)(progress * 100.0));
        }
        
        private bool setListbox(string word)
        {
            if (!isListboxVisible())
                return false;
            int index = getWordIndex(word);
            if (index == ListBox.NoMatches)
                return false;
            selectIndex(index);
            return true;
        }

        private int getNumOfWords()
        {
            return fileLines.Select(line => line.Split()).Select(words => words.Count()).Sum();
        }

        #endregion
        #region Thread Methods

        private bool setText(string text)
        {
            if (rtb.InvokeRequired)
            {
                var d = new BoolString(setText);
                return (bool)MainControl.mainWin.Invoke(d, new object[] { text });
            }
            rtb.Clear();
            rtb.AppendText(text);
            return true;
        }

        private bool appendText(string text)
        {
            if (rtb.InvokeRequired)
            {
                var d = new BoolString(appendText);
                return (bool)MainControl.mainWin.Invoke(d, new object[] { text });
            }
            rtb.AppendText(text);
            //if (text == "\n")
            //    rtb.ScrollToCaret();
            return true;
        }

        private int getSelectionStart()
        {
            if (rtb.InvokeRequired)
            {
                var d = new IntVoid(getSelectionStart);
                return (int) MainControl.mainWin.Invoke(d);
            }
            return rtb.SelectionStart;
        }

        private bool colorNotCompleted(int start,int end)
        {
            if (rtb.InvokeRequired)
            {
                var d = new BoolIntInt(colorNotCompleted);
                return (bool) MainControl.mainWin.Invoke(d, new object[] { start,end });
            }
            int temp = rtb.SelectionStart;
            rtb.Select(start, end-start);
            rtb.SelectionColor = Color.Red;
            rtb.Select(temp,0);
            rtb.SelectionColor = rtb.ForeColor;
            return true;
        }

        private bool setTextColor(Color c)
        {
            if (rtb.InvokeRequired)
            {
                var d = new BoolColor(setTextColor);
                return (bool) MainControl.mainWin.Invoke(d, new object[] { c });
            }
            else
            {
                rtb.SelectionLength = 0;
                rtb.SelectionColor = c;
                return true;
            }
        }
        
        private bool selectIndex(int index)
        {
            if (!suggestions.InvokeRequired)
            {
                suggestions.SetSelected(index, true);
                return true;
            }
            var d = new BoolInt(selectIndex);
            return (bool) MainControl.mainWin.Invoke(d, new object[] { index });
        }

        private int getWordIndex(string word)
        {
            if (!suggestions.InvokeRequired)
                return suggestions.FindStringExact(word);
            var d = new IntString(getWordIndex);
            return (int) MainControl.mainWin.Invoke(d, new object[] {word});
        }

        private bool isListboxVisible()
        {
            if (suggestions.InvokeRequired)
            {
                var d = new BoolVoid(isListboxVisible);
                return (bool) MainControl.mainWin.Invoke(d);
            }
            return suggestions.Visible;
        }
    #endregion
    }
}
