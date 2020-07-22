using System;

namespace WpfUtilities
{
    /// <summary>
    /// Use this Base class for Disposing
    /// </summary>
    public class Disposable : IDisposable
    {
        // Flag: Has Dispose already been called?
        private bool disposed = false;

        public void Dispose()
        {
            //Performs the actual work of releasing resources
            this.Dispose(true);

            //The call to the SuppressFinalize method prevents the garbage collector from running the finalizer.
            // If the type has no finalizer, the call to GC.SuppressFinalize has no effect
            GC.SuppressFinalize(this);
        }
                
        protected virtual void Dispose(bool disposing)
        {
            //1. Check if its already disposed
            if (this.disposed)
            {
                return;
            }

            //2. Release managed resources
            if (disposing)
            {                
            }

            //3. Release unmanaged resources

            //4. Set dispose flag to true
            this.disposed = true;
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~Disposable()
        {
            this.Dispose(false);
        }
    }

    /*******************DO NOT USE BELOW CLASSES.  THEY ARE EXAMPLES*******************/

    //How to use the Base dispose class.  

    //This is a sample object that would wrap an unmanaged object (assume there is an unmanaged object)
    public class DotNetWrapperAroundUnmanagedObject : IDisposable
    {
        public event EventHandler<string> Started;

        public void Start() => this.OnStarted("started");

        private void OnStarted(string msg)
        {
            EventHandler<string> ev = Started;
            ev?.Invoke(this, msg);
        }

        public void Dispose() { }
    }

    public class DerivedClassUsingDisposable : Disposable
    {
        // Flag: Has Dispose already been called?
        private bool disposed = false;

        public DotNetWrapperAroundUnmanagedObject WrappedUnmanaged = new DotNetWrapperAroundUnmanagedObject();

        public DerivedClassUsingDisposable()
        {
            this.WrappedUnmanaged.Started += this.WrappedUnmanaged_Started;
        }

        private void WrappedUnmanaged_Started(object sender, string e)
        {            
        }

        protected override void Dispose(bool disposing)
        {
            //1. Check if its already disposed
            if(this.disposed)
            {
                return;
            }

            //2. Release managed resources
            if(disposing)
            {
                this.WrappedUnmanaged.Started -= this.WrappedUnmanaged_Started;
            }

            //3. Release unmanaged resources
            this.WrappedUnmanaged.Dispose();

            //4. Set dispose flag to true
            this.disposed = true;

            //5. Call base dispose
            base.Dispose(disposing);
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~DerivedClassUsingDisposable()
        {
            this.Dispose(false);
        }
    }

    /*******************GARBAGE COLLECTION  *******************/

    /* GC=Garbage collection or Collector
    
    A managed object is any object where the .net runtime manages the memory
    GC does not allocate or release unmanaged memory like file handles
    GC cannot reclaim unmanaged objects

    Disposing is to free unmanaged resources, perform general cleanup, and to tell finalizer, if one is present, not to run
    If you call dispose, then you tell GC not to run finalizer with GC.SuppressFinalize
    Freeing the actual memory associated with a managed object is the job of the GC

    If you have an unmanaged resource, you need to release these resources.
    This can be done deterministically(you decide) or undeterministically(when the runtime decides)

    To release the resource, you implement the dispose method, preferably using the dispose pattern
    The garbage collector doesn't call the dipose method so you have to call it.
    If you think there is a possibility of forgetting, a backup is to have a finalizer.  
    The GC calls the finalizer if there is one.  You cannot call it.
    Do not implement a finalizer unless there are unmanaged resources that need to be released
    Having a finalizer means it gets added to the finalization queue
    An empty finalizer just causes a needless loss of performance.

    Implementing the Dispose pattern
    It has 2 sections, release managed resources (dispose(true)) and release unmanaged resources
    dispose(true)

    If the method call comes from a finalizer (dispose(false)), only the code that frees unmanaged resources 
    should execute. The implementer is responsible for ensuring the the false path doesn't interact with 
    managed objects that may have been reclaimed. This is important because the order in which the garbage collector 
    destroys managed objects during finalization is non-deterministic.

    Pointers
    If you have objects that implement IDisposable, you should implement IDisposable
    Most of the time when an object has a close method, it calls dispose automatically
    You should unsubscribe from events in the dispose
    You can even set them to null to eliminate the possibility of firing during or after disposal
    If your object holds high value secrets like encryption keys, clear them during disposal
    to avoid discovery by less priviledged assemblies or malware

    System.Timers must be disposed since the .NET framework holds a reference to them and they wont be disposed so you must call dispose

    Disposal vs GC
    Disposing releases resources so you dont have to wait for GC to do it.  Deterministic
    Disposal releases file handles, locks, and OS resources while GC releases memory
        
    You never deallocate the array yourself. GC does it for you
    Array is allocated on the managed memory heap referenced by the variable myArray stored on 
    the local variable stack.  When it becomes out of scope, myArray variable pops off stack and 
    nothing is left to reference the array on the heap, so it becomes an orphan. Orphans become eligible
    to be reclaimed by GC
    public void Test()
    {
        byte[] myArray = new byte[1000]
    }

    Using is syntactic shortcut for calling Dispose
    using(FileStream fs = new FileStream(...))
    {}

    translates to
    FileStream fs = new FileStream(...)
    try{}
    finally
    {
        if(fs!=null) ((IDisposable)fs).Dispose();
    }

    ************Event handler memory leaks************

    class Host
    {
        public event EventHandler Click
    }

    class Client
    {
        Host _host;
        public Client(Host host)
        {
            _host = host;
            _host.click += this.HostClicked;
        }
        void HostClicked(...)
    }

    class Test
    {
        static Host _host = new Host();
        public static void CreateClients()
        {
            Client[] clients = new Clients[100];
            for(i=0 to 100)
            {
                clients[i] = new Client(_host);
            }
        }
        At this point since CreateClients() finishes executing you think the 100 clients are eligible for collection but since _host has an event that holds references
        to event handlers on the client objects, the clients stay alive since _host is alive
    }

    To fix, in Client class add public void Dispose() { _host.click -= HostClicked; } and in Test class, when you are done with them add Array.ForEach(clients, c => c.Dispose());


    ************Event handler memory leaks END************

    How GC works
    GC starts with a root object and walks (traces) the object graph on the managed heap marking objects that are reachable
    Objects that are not reachable are orphans are eligible for collection.  The rest are considered live
    If no finalizer, they are removed
    If they have finalizers, they are enqueued on finalization queue, but the object isn't removed, 
    since the finalization queue has a reference to the object.  It is not an orphan
    Then when the finalizer thread runs, the finalizers are processed and now and the objects become orphans
    Then when the GC runs again, they are discarded.  This whole process took 3 runs

    GC divides the managed heap into 3 generations
    Newly allocated objects are put in gen0.  If an object survives the GC collection cycle(when it walks down the object graph and is seen as live or reachable)
    they are moved to gen1.  All other objects are gen2. New objects>= 85kb are put in gen2 right away
    Once an object survives, live objects are fragmented since random elements are now orphans so a memory compaction runs.
    This allows the GC to add objects to end of the heap instead of having a service keep track of where every object is
    Memory compaction doesn't happen in Gen2 since moving large blocks of memory is expensive.
    Consequence of this is allocations are slower in Gen2 since now you cant just add to end of heap, you also look for gaps in the memory maintained by a linked list
    Memory is also fragmented
    Gen0 is small (few mb) and when it fills up, a collection runs
    Gen0 collection runs most frequently and takes around 1ms, and less freqently as generations go up.
    a gen collection will collect gens below it so gen1 collection will run gen0 collection.
    Gen2 is a full collection and may take around 100ms
    Gen2 is also called the large object heap(loh)
    New objects 85kb and up go straight to loh, which avoids excessive Gen0 collections
    GC blocks execution threads during gen0 and 1 collections but not really during gen2 since it runs longer

    The frequency of GC running is based on differnt factors like roral memory load on the machine.  If your app allocates
    unmanaged memory, the runtime will get an unrealistic view of mem usage since it only knows about managed memory.
    To mitigate this, you can tell CLR to assume a certain amount of unmanaged memory has been allocated by calling
    GC.AddMemoryPressure

    

     

     */

}
