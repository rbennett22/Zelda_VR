namespace Zigfu.Utility
{
    public class ThreadedJob
    {
        object m_Handle = new object();
        System.Threading.Thread m_Thread = null;


        bool m_IsDone = false;
        public bool IsDone {
            get {
                bool tmp;
                lock (m_Handle) { tmp = m_IsDone; }
                return tmp;
            }
            set {
                lock (m_Handle) { m_IsDone = value; }
            }
        }

        public virtual void Start()
        {
            m_Thread = new System.Threading.Thread(Run);
            m_Thread.Start();
        }
        public virtual void Abort()
        {
            m_Thread.Abort();
        }

        
        public virtual bool Update()
        {
            if (IsDone) { OnFinished(); }
            return IsDone;
        }
        private void Run()
        {
            ThreadFunction();
            IsDone = true;
        }


        // --- Virtual ---

        protected virtual void ThreadFunction() { }
        protected virtual void OnFinished() { }
    }
}
