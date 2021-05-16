using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Emphasis.Algorithms.IndexOf
{
	public class IndexOfAlgorithms
	{
		public unsafe int IndexOfGreaterThan(int width, int height, int[] source, int[] indexes, int comparand)
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

		private unsafe int IndexOfGreaterThan(int width, int height, int[] source, int[] indexes, int comparand, ref int position, int start, int length)
		{
			var count = 0;
			var src = source.AsSpan(start, length);
			var y = start / width;
			var x = start - y * width;
			var i = 0;

			const int tmpSize = 4096;
			Span<int> tmp = stackalloc int[tmpSize];
			var ti = 0;

			for (; y < height; y++)
			{
				for (; x < width && i < length; x++, i++)
				{
					if (src[i] > comparand)
					{
						tmp[ti++] = x;
						tmp[ti++] = y;
						if (ti >= tmpSize)
						{
							var dm = Interlocked.Add(ref position, tmpSize);
							var di = dm - tmpSize;
							tmp.CopyTo(indexes.AsSpan(di));
							ti = 0;
						}
						count++;
					}
				}

				x = 0;
			}

			if (ti > 0)
			{
				var dm = Interlocked.Add(ref position, ti);
				var di = dm - ti;
				tmp[..ti].CopyTo(indexes.AsSpan(di));
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

			var position = 0;
			var step = size / levelOfParallelism;
			var tasks = new List<Task<int>>();
			for (var l = 0; l < levelOfParallelism-1; l++)
			{
				var start = l * step;
				var length = step;
				tasks.Add(Task.Run(() => IndexOfGreaterThan(width, height, source, indexes, comparand, ref position, start, length)));
			}

			var start0 = (levelOfParallelism - 1) * step;
			var count0 = IndexOfGreaterThan(width, height, source, indexes, comparand, ref position, start0, size - start0);
			var counts = await Task.WhenAll(tasks);
			var count = counts.Sum() + count0;

			return count;
		}
	}
}
