using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Emphasis.OpenCL;

namespace Emphasis.Algorithms.Resize.OpenCL
{
	public interface IOclResizeAlgorithms
	{

	}

	public class OclResizeAlgorithms : IOclResizeAlgorithms
	{
		private readonly ConcurrentDictionary<(nint queueId, string args), (nint contextId, nint deviceId, nint programId)> _programs = new();

		public async Task<nint> BiLinear(
			nint queueId,
			int width0,
			int height0,
			int width1,
			int height1,
			OclTypedBuffer sourceBuffer,
			OclTypedBuffer resultBuffer)
		{
			var x_ratio = (float) (width0 - 1) / width1;
			var y_ratio = (float) (height0 - 1) / height1;
			
		}
	}
}
