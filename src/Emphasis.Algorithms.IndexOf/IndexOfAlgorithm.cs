using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Emphasis.Algorithms.IndexOf
{
	public class IndexOfAlgorithms
	{
		public int IndexOfGreaterThan(int[] source, int width, int height, int[] indexes, int comparand)
		{
			if (width * height > source.Length)
				throw new ArgumentOutOfRangeException(nameof(width), $"The {nameof(width)} and {nameof(height)} are out of range of {nameof(source)}.");

			var count = 0;
			var index = 0;
			var i = 0;

			for (var y = 0; y < height; y++)
			{
				for (var x = 0; x < width; x++, i++)
				{
					if (source[i] > comparand)
					{
						indexes[index] = x;
						indexes[index + 1] = y;
						index += 2;
						count++;
					}
				}
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

			if (height < levelOfParallelism)
			{
				return 0;
			}

			var count0 = 0;
			var y0 = -1;
			var slot0 = -1;

			(int, int) NextSlot()
			{
				var slot = Interlocked.Increment(ref slot0);
				return (slot * 4096, (slot + 1) * 4096);
			}

			void IndexOf()
			{
				var (di, dm) = NextSlot();
				var count = 0;
				for (var y = Interlocked.Increment(ref y0); y < height; y = Interlocked.Increment(ref y0))
				{
					var si = y * width;
					for (var x = 0; x < width; x++, si++)
					{
						if (source[si] > comparand)
						{
							if (di >= dm)
								(di, dm) = NextSlot();
							indexes[di] = x;
							indexes[di + 1] = y;
							di += 2;
							count++;
						}
					}
				}

				Interlocked.Add(ref count0, count);
			}

			var tasks = new List<Task>();
			for (var p = 0; p < levelOfParallelism - 1; p++)
			{
				var t = Task.Run(IndexOf);
				tasks.Add(t);
			}
			
			IndexOf();

			await Task.WhenAll(tasks);

			return count0;
		}
	}
}
