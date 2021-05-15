using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Emphasis.Algorithms.IndexOf
{
	public class IndexOfAlgorithms
	{
		public unsafe int IndexOfGreaterThan(int[] source, int width, int height, int[] indexes, int comparand)
		{
			var count = 0;
			fixed (int* srcPtr = &source[0])
			fixed (int* dstPtr = &indexes[0])
			{
				var src = srcPtr;
				var dst = dstPtr;
				for (var y = 0; y < height; y++)
				{
					for (var x = 0; x < width; x++, src++)
					{
						if (*src > comparand)
						{
							*dst++ = x;
							*dst++ = y;
							count++;
						}
					}
				}
			}

			return count;
		}

		private unsafe int LinearIndexOf(int[] source, int width, int height, int[] destination, ref int d, int comparand, int step, ref int i)
		{
			var count = 0;
			var size = width * height;

			fixed (int* srcPtr = &source[0])
			fixed (int* dstPtr = &destination[0])
			{
				int pm;
				do
				{
					pm = Interlocked.Add(ref i, step);
					var pi = pm - step;
					if (pm > size)
					{
						if (pm >= size + step)
							break;

						pm = size;
					}
					var y = pi / width;
					var x = pi - y * width;
					var src = srcPtr + pi;
					for (; y < height; y++)
					{
						for (; x < width && pi < pm; x++, pi++, src++)
						{
							if (*src > comparand)
							{
								var dst = dstPtr + Interlocked.Add(ref d, 2);
								*dst++ = x;
								*dst = y;
								count++;
							}
						}

						if (pi >= pm)
							break;

						x = 0;
					}
				} while (pm != size);

			}

			return count;
		}


		public async Task<int> ParallelIndexOfGreaterThan(int[] source, int width, int height, int[] indexes, int comparand, int levelOfParallelism = 0)
		{
			if (width * height > source.Length)
				throw new ArgumentOutOfRangeException(nameof(width), $"The {nameof(width)} and {nameof(height)} are out of range of {nameof(source)}.");

			if (levelOfParallelism == 0)
				levelOfParallelism = Environment.ProcessorCount;

			var size = width * height;
			var minSize = 2048;
			levelOfParallelism = Math.Max(1, Math.Min(levelOfParallelism, size / minSize));

			if (levelOfParallelism == 1)
				return IndexOfGreaterThan(source, width, height, indexes, comparand);

			var d = -2;
			var step = size / levelOfParallelism;
			var i = 0;
			
			var tasks = new List<Task<int>>();
			for (var l = 0; l < levelOfParallelism; l++)
			{
				var t = Task.Run(() => LinearIndexOf(source, width, height, indexes, ref d, comparand, step, ref i));
				tasks.Add(t);
			}
			
			var counts = await Task.WhenAll(tasks);
			var count = counts.Sum();

			return count;
		}
	}
}
