using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TextEditor.Control;
using TextEditor.GUI;
using TextPredict.Control;

namespace TextPredict.GUI {
    public class Demonstrator {
        private Worker worker = new Worker("DemonstratorThread");
        private RichTextBox richTextBox;
        private ListBox listBox;
        public bool isPaused;
        private object pauseLock = new object();
        private string text;
  
        private Action<Suggester> onDone;
        public Demonstrator( RichTextBox rtb , ListBox list , Runnable select , MainWindow window ) {
            richTextBox = rtb;
            listBox = list;
            selectItem = select;
            mainWindow = window;
            manager = window.getSuggestionManager();
            isPaused = true;
            onDone = ( dontCare ) => worker.queue(() => next());
            manager.OnDone += onDone;
        }
        public void setText(String text) {
            this.text = text;
            wordlist = Utils.iterateWords(text).GetEnumerator();
            if (!wordlist.MoveNext()) throw new Exception("empty string");
            charlist = wordlist.Current.GetEnumerator();
        }
        public void start( ) {
            running = true;
            run(() => listBox.Visible = true);
        }

        private void next() {
            while ( isPaused )
                lock ( pauseLock ) {
                    manager.OnDone -= onDone;
                    Monitor.Wait(pauseLock);
                    manager.OnDone += onDone;
                }
            if ( wordlist.Current == null ) return;
            string word = wordlist.Current.ToLower();
            int i = 0;
            if ( charlist.MoveNext() ) {
                i++;
                char c = charlist.Current;
                if ( runForResult(new Func<bool>(() => {
                    int index = listBox.Items.IndexOf(word);
                    if ( index >= 0 && index <= 5 && (Configurations.IGNORE_MANAGING_HITS || ( word.Length - i ) >= index + 1)) {
                        listBox.SelectedIndex = index;
                        return true;
                    } else {
                        richTextBox.AppendText(c + "");
                        return false;
                    }
                })) ) {
                    Thread.Sleep(DEMONSTRATION_INTERVAL);
                    run(() => selectItem());
                    Thread.Sleep(DEMONSTRATION_INTERVAL);
                    if ( !wordlist.MoveNext() ) return;
                    charlist = wordlist.Current.GetEnumerator();
                }
            } else {
                runSync(() => {
                    richTextBox.AppendText(" ");
                });
                if ( !wordlist.MoveNext() ) return;
                charlist = wordlist.Current.GetEnumerator();
            }
        }
        private void run( Runnable f ) {
            if ( richTextBox.IsHandleCreated )
                richTextBox.Invoke(new Runnable(f));
        }
        private void runSync( Runnable f ) {
            if ( richTextBox.IsHandleCreated )
                richTextBox.Invoke(new Func<object>(() => { f(); return null; }));
        }
        private T runForResult<T>( Func<T> f ) {
            if ( richTextBox.IsHandleCreated )
                return (T)richTextBox.Invoke(new Func<T>(() => { return f(); }));
            else return default(T);
        }
        public void pause() {
            isPaused = true;
        }
        public void proceed() {
            if ( !running ) start();
            isPaused = false;
            lock ( pauseLock )
                Monitor.Pulse(pauseLock);
        }
        private Runnable selectItem;
        private MainWindow mainWindow;


        public static int DEMONSTRATION_INTERVAL = 200;
        private TextEditor.Control.SuggestionManager manager;
        private IEnumerator<string> wordlist;
        private CharEnumerator charlist;
        public bool running { get; private set; }
    }
}
