using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Emphasis.Algorithms.IndexOf
{
	public class IndexOfAlgorithms
	{
		public async Task<int> IndexOfGreaterThan(int[] source, int width, int height, int[] indexes, int comparand, int levelOfParallelism = 0)
		{
			if (width * height > source.Length)
				throw new ArgumentOutOfRangeException(nameof(width), $"The {nameof(width)} and {nameof(height)} are out of range of {nameof(source)}.");
			
			if (levelOfParallelism == 0)
				levelOfParallelism = Environment.ProcessorCount;

			var size = width * height;
			var minSize = 2048;
			levelOfParallelism = Math.Max(1, Math.Min(levelOfParallelism, size / minSize));
			
			var srcHandle = GCHandle.Alloc(source, GCHandleType.Pinned);
			using var srcDisposable = Disposable.Create(() => srcHandle.Free());
			
			var dstHandle = GCHandle.Alloc(indexes, GCHandleType.Pinned);
			using var dstDisposable = Disposable.Create(() => dstHandle.Free());

			var count0 = 0;
			var d0 = -2;
			
			var pStep = size / (levelOfParallelism * 8);
			var p0 = -pStep;

			void LinearIndexOf()
			{
				unsafe
				{
					var count = 0;
					var src = (int*)srcHandle.AddrOfPinnedObject();
					var dst = (int*)dstHandle.AddrOfPinnedObject();

					while (true)
					{
						var p = Interlocked.Add(ref p0, pStep);
						var pm = Math.Min(size, p + pStep);
						var y = p / width;
						var x = p - y * width;
						var s = src + y * width + x;
						for (; y < height; y++)
						{
							for (; x < width && p < pm; x++, p++, s++)
							{
								if (*s > comparand)
								{
									var di = Interlocked.Add(ref d0, 2);
									*(dst + di) = x;
									*(dst + (di + 1)) = y;
									count++;
								}
							}
							if (p >= pm)
								break;
							x = 0;
						}
						if (pm >= size)
							break;
					}

					Interlocked.Add(ref count0, count);
				}
			}
			
			Action indexOf = LinearIndexOf;

			var tasks = new List<Task>();
			for (var l = 0; l < levelOfParallelism - 1; l++)
			{
				var t = Task.Run(indexOf);
				tasks.Add(t);
			}

			indexOf();

			await Task.WhenAll(tasks);

			return count0;
		}
	}
}
