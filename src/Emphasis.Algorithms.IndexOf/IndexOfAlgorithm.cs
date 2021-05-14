using System;
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

		public int ParallelIndexOfGreaterThan(int[] source, int width, int height, int[] indexes, int comparand, int levelOfParallelism = 0)
		{
			if (width * height > source.Length)
				throw new ArgumentOutOfRangeException(nameof(width), $"The {nameof(width)} and {nameof(height)} are out of range of {nameof(source)}.");
			
			if (levelOfParallelism == 0)
				levelOfParallelism = Environment.ProcessorCount;

			if (levelOfParallelism == 1)
				return IndexOfGreaterThan(source, width, height, indexes, comparand);

			var size = width * height;
			if (size < 4096)
				return IndexOfGreaterThan(source, width, height, indexes, comparand);

			int w0, h0;

			
			var count = 0;
			var index = 0;
			var i = 0;

			void IndexOf(int x0, int x1, int y0, int y1)
			{
				int x = x0;
				for (; y0 <= y1; y0++)
				{
					for (; x < width; x++, i++)
					{
						if (source[i] > comparand)
						{
							indexes[index] = x;
							indexes[index + 1] = y;
							index += 2;
							count++;
						}
					}

					x = 0;
				}
			}

			Task.Run()


			

			return count;
		}
	}
}
