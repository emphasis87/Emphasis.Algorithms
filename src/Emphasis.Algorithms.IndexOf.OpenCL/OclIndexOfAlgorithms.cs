using System;
using System.Threading.Tasks;
using Emphasis.OpenCL;

namespace Emphasis.Algorithms.IndexOf.OpenCL
{
	public class OclIndexOfAlgorithms
	{
		public bool IsSupported(nint deviceId)
		{
			var extensions = OclHelper.GetDeviceExtensions(deviceId);
			return 
				extensions.Contains("cl_khr_int64_base_atomics") ||
				extensions.Contains("cl_khr_global_int32_base_atomics") && extensions.Contains("cl_khr_local_int32_base_atomics ");
		}

		public async Task<int> IndexOfGreaterThan(nint queueId, int width, int height, nint sourceBufferId, nint resultBufferId, int comparand)
		{
			var contextId = OclHelper.GetCommandQueueContext(queueId);
			var deviceId = OclHelper.GetCommandQueueDevice(queueId);

			var extensions = OclHelper.GetDeviceExtensions(deviceId);
			var hasAtomic32 = extensions.Contains("cl_khr_global_int32_base_atomics") && extensions.Contains("cl_khr_local_int32_base_atomics ");
			var hasAtomic64 = extensions.Contains("cl_khr_int64_base_atomics");
			string atomicSize;
			if (hasAtomic32)
				atomicSize = "32";
			else if (hasAtomic64)
				atomicSize = "64";
			else
				throw new NotSupportedException("The device does not support atomic operations.");

			var programId = await OclProgramRepository.Shared.GetProgram(contextId, deviceId, Kernels.IndexOf, $"-DOperation=> -DAtomics_{atomicSize}");

			return (int)programId;
		}
	}
}
