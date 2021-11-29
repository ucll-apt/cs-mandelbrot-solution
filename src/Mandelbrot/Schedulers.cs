using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mandelbrot
{
    public interface IScheduler
    {
        /// <summary>
        /// Run all jobs.
        /// </summary>
        /// <param name="planner">Planner that provides all jobs.</param>
        void Schedule( IPlanner planner );
    }

    public class SingleThreadScheduler : IScheduler
    {
        public void Schedule( IPlanner planner )
        {
            for ( var i = 0; i != planner.JobCount; ++i )
            {
                planner.Job( i )();
            }
        }
    }

    public class ManualThreadingScheduler : IScheduler
    {
        public void Schedule( IPlanner planner )
        {
            var nthreads = Environment.ProcessorCount;
            var threads = new List<Thread>();
            var row = 0;

            for ( var i = 0; i != nthreads; ++i )
            {
                var thread = new Thread( () =>
                {
                    while ( true )
                    {
                        var jobIndex = Interlocked.Increment( ref row );

                        if ( jobIndex >= planner.JobCount )
                        {
                            break;
                        }

                        planner.Job( jobIndex )();
                    }
                } );

                thread.Start();
                threads.Add( thread );
            }

            foreach ( var thread in threads )
            {
                thread.Join();
            }
        }
    }

    public class ParallelScheduler : IScheduler
    {
        public void Schedule( IPlanner planner )
        {
            var result = Parallel.For( 0, planner.JobCount, jobIndex =>
            {
                planner.Job( jobIndex )();
            } );
        }
    }

    public class EnumerableScheduler : IScheduler
    {
        public void Schedule( IPlanner planner )
        {
            Enumerable.Range( 0, planner.JobCount ).AsParallel().ForAll( jobIndex =>
            {
                planner.Job( jobIndex )();
            } );
        }
    }

    public class TaskScheduler : IScheduler
    {
        public void Schedule( IPlanner planner )
        {
            var tasks = Enumerable.Range( 0, planner.JobCount ).Select( jobIndex =>
            {
                return new Task( () =>
                {
                    planner.Job( jobIndex )();
                } );
            } );

            Task.WhenAll( tasks ).Wait();
        }
    }

    public class ThreadproolScheduler : IScheduler
    {
        public void Schedule( IPlanner planner )
        {
            var jobsDone = 0;
            var lck = new object();

            for ( var jobIndex = 0; jobIndex != planner.JobCount; ++jobIndex )
            {
                var i = jobIndex;

                ThreadPool.QueueUserWorkItem( _ =>
                {
                    planner.Job( i )();

                    Interlocked.Increment( ref jobsDone );

                    lock ( lck )
                    {
                        Monitor.Pulse( lck );
                    }
                } );
            }

            while ( jobsDone < planner.JobCount )
            {
                lock ( lck )
                {
                    Monitor.Wait( lck );
                }
            }
        }
    }
}
