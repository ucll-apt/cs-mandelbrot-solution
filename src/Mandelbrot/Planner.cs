using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mandelbrot
{
    public interface IPlanner
    {
        /// <summary>
        /// Number of jobs.
        /// </summary>
        public int JobCount { get; }

        /// <summary>
        /// Returns the index-th job.
        /// </summary>
        /// <param name="index">Index of job to return.</param>
        /// <returns>The index-th job</returns>
        public Action Job( int index );
    }

    public class PixelPlanner : IPlanner
    {
        private readonly IList<Mandelbrot> mandelbrots;

        public PixelPlanner( IList<Mandelbrot> mandelbrots )
        {
            this.mandelbrots = mandelbrots;
        }

        public int JobCount => mandelbrots[0].Height * mandelbrots[0].Width * mandelbrots.Count;

        public Action Job( int index )
        {
            var width = mandelbrots[0].Width;
            var height = mandelbrots[0].Height;
            var pixelCount = width * height;
            var mandelbrotIndex = index / pixelCount;
            var pixelIndex = index / pixelCount;
            var x = pixelIndex % width;
            var y = pixelIndex / width;

            return () => this.mandelbrots[mandelbrotIndex].ComputeSingle( x, y );
        }
    }

    public class RowPlanner : IPlanner
    {
        private readonly IList<Mandelbrot> mandelbrots;

        public RowPlanner( IList<Mandelbrot> mandelbrots )
        {
            this.mandelbrots = mandelbrots;
        }

        public int JobCount => mandelbrots[0].Height * mandelbrots.Count;

        public Action Job( int index )
        {
            var rowCount = mandelbrots[0].Height;
            var mandelbrotIndex = index / rowCount;
            var rowIndex = index % rowCount;

            return () => this.mandelbrots[mandelbrotIndex].ComputeRow( rowIndex );
        }
    }

    public class FramePlanner : IPlanner
    {
        private readonly IList<Mandelbrot> mandelbrots;

        public FramePlanner( IList<Mandelbrot> mandelbrots )
        {
            this.mandelbrots = mandelbrots;
        }

        public int JobCount => mandelbrots.Count;

        public Action Job( int index )
        {
            return () => this.mandelbrots[index].ComputeAll();
        }
    }

    public class MonolithPlanner : IPlanner
    {
        private readonly IList<Mandelbrot> mandelbrots;

        public MonolithPlanner( IList<Mandelbrot> mandelbrots )
        {
            this.mandelbrots = mandelbrots;
        }

        public int JobCount => 1;

        public Action Job( int index )
        {
            return () => {
                foreach ( var mandelbrot in this.mandelbrots )
                {
                    mandelbrot.ComputeAll();
                }
            };
        }
    }
}
