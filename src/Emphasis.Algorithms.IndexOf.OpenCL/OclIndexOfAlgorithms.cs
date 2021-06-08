using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Emphasis.OpenCL;
using Silk.NET.OpenCL;
using static Emphasis.OpenCL.OclHelper;

namespace Emphasis.Algorithms.IndexOf.OpenCL
{
	public class OclIndexOfAlgorithms
	{
		private readonly ConcurrentDictionary<nint, bool> _isSupported = new();
		private readonly ConcurrentDictionary<nint, nint> _programs = new();

		public bool IsSupported(nint deviceId)
		{
			if (!_isSupported.TryGetValue(deviceId, out var isSupported))
			{
				var extensions = GetDeviceExtensions(deviceId);
				isSupported =
					extensions.Contains("cl_khr_int64_base_atomics") ||
					extensions.Contains("cl_khr_global_int32_base_atomics") && extensions.Contains("cl_khr_local_int32_base_atomics ");

				_isSupported[deviceId] = isSupported;
			}

			return isSupported;
		}

		public async Task<nint> IndexOfGreaterThan(nint queueId, int width, int height, nint sourceBufferId, nint resultBufferId, int comparand)
		{
			if (!_programs.TryGetValue(queueId, out var programId))
			{
				var contextId = GetCommandQueueContext(queueId);
				var deviceId = GetCommandQueueDevice(queueId);

				var extensions = GetDeviceExtensions(deviceId);
				var hasAtomic32 = extensions.Contains("cl_khr_global_int32_base_atomics") && extensions.Contains("cl_khr_local_int32_base_atomics ");
				var hasAtomic64 = extensions.Contains("cl_khr_int64_base_atomics");
				string atomicSize;
				if (hasAtomic32)
					atomicSize = "32";
				else if (hasAtomic64)
					atomicSize = "64";
				else
					throw new NotSupportedException("The device does not support atomic operations.");

				programId = await OclProgramRepository.Shared.GetProgram(contextId, deviceId, Kernels.IndexOf, $"-D Operation=> -D Atomics_{atomicSize} -D TSourceDepth=int -D TResultDepth=int");

				_programs[queueId] = programId;
			}

			var kernelId = CreateKernel(programId, "IndexOf");

			SetKernelArg(kernelId, 0, sourceBufferId);
			SetKernelArg(kernelId, 1, resultBufferId);
			SetKernelArgSize<int>(kernelId, 2, 32);
			SetKernelArg(kernelId, 3, comparand);

			var eventId = EnqueueNDRangeKernel(queueId, kernelId,
				globalWorkSize: stackalloc nuint[] {(nuint) width, (nuint) height});

			OnEventCompleted(eventId, () =>
			{
				ReleaseKernel(kernelId);
			});

			return eventId;
		}
	}
}
