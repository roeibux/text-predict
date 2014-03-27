using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;
using TextEditor.Base;
using TextPredict.Base;
using TextPredict.Control;
using TextPredict.GUI;

namespace TextEditor.Control {
    public class Semaphore {
        private int i;
        private Object mutex = new Object();
        public Semaphore( int init ) {
            i = init;
        }
        public void wait() {
            lock ( mutex ) {
                if ( --i >= 0 ) return;
                //Console.WriteLine(s + " goes to sleep");
                Monitor.Wait(mutex);
                //Console.WriteLine(s + " woke up");
            }
        }

        public void signal() {
            lock ( mutex ) {
                if ( ++i > 0 ) return;
                Monitor.Pulse(mutex);
            }
        }
        public void release() {
            while ( i < 0 ) signal();
        }
    }
    public abstract class Suggester {
        public event Func<Suggestion , bool> SuggestionEventHandler;
        public event Action<Suggester> OnDone;
        private Boolean isStopped;
        protected Worker worker;

        public Suggester() {
            worker = new Worker(ToString());
        }

        protected virtual void onDone() {
            if (OnDone!=null) OnDone(this);
        }
        /// <summary>
        /// call suggest once you have a suggestion ready, return true if succeded, false if ran out of suggestions.
        /// </summary>
        protected abstract bool suggestNext();
        /// <summary>
        /// recompute all your suggestions here.
        /// </summary>
        protected abstract void recomputeSuggestions();

        public abstract void apply( char c );
        public abstract void back();
        public abstract void back( int n );
        public abstract void rewind();

        public static bool isSeparator( char c ) {
            return Utils.isSeparator(c);
        }
        public static bool isBack( char c ) {
            return Utils.isBack(c);
        }
        protected bool suggest( Suggestion suggestion ) {
            return SuggestionEventHandler(suggestion);
        }
        /// <summary>
        /// when called you should reset suggester's enviroment
        /// </summary>
        public abstract void resetEnviroment();


        public virtual void runSuggester() {
            isStopped = false;
            worker.queue(() => {
                recomputeSuggestions();
                while ( !isStopped && suggestNext() ) ;
                onDone();
            });
        }

        public virtual void stop() {
            isStopped = true;
        }



    }


    public abstract class TrieSuggester : Suggester {
        protected TrieTraveler traveler;
        protected Trie trie;
        protected bool isRelevant = true;
        public TrieSuggester( Trie trie ) {
            this.trie = trie;
            traveler = new TrieTraveler(trie);
        }
        public override void apply( char c ) {
            if ( isSeparator(c) ) rewind();
            else if ( !isRelevant ) return;
            else if ( isBack(c) ) traveler.back();
            else isRelevant = traveler.travel(c);
        }

        public override void back() {
            traveler.back();
        }
        public override void back( int n ) {
            traveler.back(n);
        }
        public override void rewind() {
            traveler.rewind();
            isRelevant = true;
        }
        public override void resetEnviroment() {
            rewind();
        }


    }

    public abstract class SuggestionManager : Suggester {
        protected List<Suggester> suggesters = new List<Suggester>();
        private int doneCount = 0;
        private Object mutex = new Object();
        public void addSuggester( Suggester s ) {
            s.SuggestionEventHandler += onSuggesterSuggestion;
            s.OnDone += ( suggester ) => {
                lock ( mutex ) {
                    doneCount++;
                    if ( doneCount == suggesters.Count ) 
                        onSuggestersDone();
                }
               
            };
            suggesters.Add(s);
        }

        protected abstract void onSuggestersDone();


        public void removeSuggester( Suggester s ) {
            suggesters.Remove(s);
        }

        public override void apply( char c ) {

                foreach ( Suggester suggester in suggesters ) {
                    suggester.apply(c);
                }

        }
        public override void resetEnviroment() {
                foreach ( Suggester suggester in suggesters ) {
                    suggester.resetEnviroment();
                }
        }

        protected override void recomputeSuggestions() {
            lock ( mutex ) {
                doneCount = 0;
            }
        }
        public override void back() {
          
                foreach ( Suggester suggester in suggesters ) {
                    suggester.back();
                }
          
        }
        public override void back( int n ) {

                foreach ( Suggester suggester in suggesters ) {
                    suggester.back(n);
                }
  
        }

        public override void stop() {

                base.stop();
                foreach ( Suggester suggester in suggesters ) {
                    suggester.stop();
                }

        }
        /// <summary>
        /// recieve the suggestion and let the suggester know if it should get more suggestions by returnning true.
        /// if this method returnhs false the suggester will be stalled untill next time its runSuggester is called.
        /// </summary>
        /// <param name="suggestion"></param>
        /// <returns></returns>
        public abstract bool onSuggesterSuggestion( Suggestion suggestion );

        public override void rewind() {

                foreach ( Suggester suggester in suggesters ) {
                    suggester.rewind();
                }
        
        }
    }

    public class Suggestion {
        public readonly String suggestion;
        public double weight;
        public List<Suggester> suggesters = new List<Suggester>(1);
        public Suggestion( string suggestion , double weight ) {
            this.suggestion = suggestion;
            this.weight = weight;
        }
        public Suggestion( string suggestion , double weight , params Suggester[] s ) {
            this.suggestion = suggestion;
            this.weight = weight;
            suggesters.InsertRange(0 , s);
        }
        public Suggestion( string suggestion , double weight , Suggester s ) {
            this.suggestion = suggestion;
            this.weight = weight;
            suggesters.Add(s);
        }
        public Suggestion( string suggestion , double weight , List<Suggester> s ) {
            this.suggestion = suggestion;
            this.weight = weight;
            suggesters.InsertRange(0 , s);
        }
        public static Suggestion operator +( Suggestion s1 , Suggestion s2 ) {
            if ( !s1.suggestion.Equals(s2.suggestion) ) throw new Exception("invalid operation - cant add suggestions that dont contain the same word");
            Suggestion res = new Suggestion(s1.suggestion , s1.weight + s2.weight);
            res.suggesters.AddRange(s1.suggesters);
            res.suggesters.AddRange(s2.suggesters);
            return res;
        }
    }

    public class SuggestionArranger : SuggestionManager {
        private Dictionary<String , Suggestion> suggestions = new Dictionary<String , Suggestion>();
        private int limit;
        private bool running;
        private int index;
        private List<Suggestion> list;
        private Object waitLock = new Object();

        public SuggestionArranger( int limit ) {
            this.limit = limit;
        }

        public List<Suggester> getSuggesters() {
            return suggesters;
        }

        protected override void recomputeSuggestions() {
            running = true;
            base.recomputeSuggestions();
            suggestions.Clear(); 
            index = 0;
            //Console.WriteLine("--------");
            foreach ( Suggester suggester in suggesters ) {
                suggester.runSuggester();
            }
            lock ( waitLock ) { Monitor.Wait(waitLock , Configurations.ARRANGER_SLEEP_TIME); }
            lock ( suggestions ) {
                if ( !running ) return;
                list = new List<Suggestion>(suggestions.Values);
                list.Sort(( s1 , s2 ) => {
                    return Comparer<Double>.Default.Compare(s2.weight , s1.weight);
                });
                running = false;
            }
        }
        protected override void onSuggestersDone() {
            //Console.WriteLine("onSuggestersDone()");
            lock ( waitLock ) { Monitor.Pulse(waitLock); }
        }

        protected override bool suggestNext() {
            if ( index < limit && index < list.Count )
                return suggest(list[index++]);
            return false;
        }

        public override bool onSuggesterSuggestion( Suggestion s ) {
            lock ( suggestions ) {
                if ( !running ) return false;
                Suggestion suggestion = null;
                if ( suggestions.TryGetValue(s.suggestion , out suggestion) ) {
                    suggestion += s;
                } else {
                    suggestion = s;
                    suggestions[s.suggestion] = suggestion;
                }
                return true;
            }
        }


    }
}
