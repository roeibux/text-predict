using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TextPredict.Control {
    public delegate void Runnable();
public abstract class BaseThread
{
  private Thread _thread;

  protected BaseThread(String name) { 
        _thread = new Thread(new ThreadStart(this.run));
        _thread.Name = name;
  }

  // Thread methods / properties
  public void start() { _thread.Start(); }
  public void join() { _thread.Join(); }
  public bool isAlive { get { return _thread.IsAlive; } }
  public void interrupt() { _thread.Interrupt(); }
  // Override in base class
  public abstract void run();
}


public class Worker : BaseThread {
    
	private Queue<Runnable> runq = new Queue<Runnable>();
	private SpecialSemaphore mSemaphore = new SpecialSemaphore(0);
	private bool mTerminated;

    public Worker( string name ) :base(name) {
        start();
    }


	public override void run() {
		while (!mTerminated)
			getTask()();
	}

    //public void execute(Runnable r) {
    //    lock (mSemaphore) {
    //        runq.Enqueue(r);
    //        mSemaphore.signal();
    //    }
    //}

	private Runnable getTask() {
		lock (mSemaphore) {
			mSemaphore.await();
			return runq.Dequeue();
		}
	}

	public void clearTasks() {
		lock (mSemaphore) {
			while (runq.Dequeue() != null)
				mSemaphore.decrement();
		}
	}

	public void queue(Runnable r) {
		lock (mSemaphore) {
            runq.Enqueue(r);
		    mSemaphore.signal();
		}
	}

	public bool isIdle() {
		lock (mSemaphore) {
			return mSemaphore.count() == 1;
		}
	}

	/**
	 * if worker is in the middle of execution, will terminate after its done.
	 */
	public void Terminate() {
		mTerminated = true;
	}


	/**
	 * make this worker forget all he had to do, and interrupt it from whatever
	 * it does.
	 */
	public void reappoint() {
		lock (mSemaphore) {
			clearTasks();
		}
	}

	private class SpecialSemaphore {
		private int i;

		public SpecialSemaphore(int init) {
			i = init;
		}

		public void await() {
			if (--i >= 0)
				return;
            Monitor.Wait(this);
		}

		/**
		 * decrement without waiting
		 */
		public void decrement() {
			--i;

		}

		public void signal() {
			if (++i > 0)
				return;
            Monitor.Pulse(this);

		}

		public int count() {
			return -i;

		}

	}
}
}
