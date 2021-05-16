using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
			//var minSize = 2048;
			//levelOfParallelism = Math.Max(1, Math.Min(levelOfParallelism, size / minSize));

			var count = 0;
			var position = 0;

			unsafe int IndexOf(int start, int length)
			{
				var c = 0;
				var src = source.AsSpan(start, length);
				var y = start / width;
				var x = start - y * width;
				var i = 0;

				var tmpSize = 4096;
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
							c++;
						}
					}

					x = 0;
				}

				if (ti > 0)
				{
					var dm = Interlocked.Add(ref position, ti);
					var di = dm - ti;
					tmp.CopyTo(indexes.AsSpan(di));
				}

				return c;
			}

			var step = size / levelOfParallelism;
			var tasks = new List<Task<int>>();
			for (var l = 0; l < levelOfParallelism-1; l++)
			{
				var start = l * step;
				var length = step;
				tasks.Add(Task.Run(() => IndexOf(start, length)));
			}

			var st = (levelOfParallelism - 1) * step;
			var cn = IndexOf(st, size - st);
			var cc = await Task.WhenAll(tasks);
			count = cc.Sum() + cn;

			
			/*
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
			*/

			return count;
		}
	}
}
