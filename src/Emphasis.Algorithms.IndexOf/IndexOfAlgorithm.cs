using System;
using System.Threading.Tasks;

namespace Emphasis.Algorithms.IndexOf
{
	public class IndexOfAlgorithms
	{
		public int IndexOfGreaterThan(int[] source, int width, int height, int[] indexes, int comparand)
		{
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
	}
}
