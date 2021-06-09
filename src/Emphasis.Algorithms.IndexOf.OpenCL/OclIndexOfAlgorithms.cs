using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Emphasis.OpenCL;
using static Emphasis.OpenCL.OclHelper;

namespace Emphasis.Algorithms.IndexOf.OpenCL
{
	public class OclIndexOfAlgorithms
	{
		private readonly ConcurrentDictionary<nint, bool> _isSupported = new();
		private readonly ConcurrentDictionary<nint, (nint contextId, nint deviceId, nint programId, string counterType, uint workGroupSize)> _indexOf = new();

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
		
		private async Task<nint> IndexOf(
			nint queueId,
			int width,
			int height,
			nint sourceBufferId,
			nint resultBufferId,
			nint counterBufferId,
			int comparand,
			string operation)
		{
			nint kernelId;
			if (!_indexOf.TryGetValue(queueId, out var indexOf))
			{
				var contextId = GetCommandQueueContext(queueId);
				var deviceId = GetCommandQueueDevice(queueId);

				var extensions = GetDeviceExtensions(deviceId);
				var hasAtomic32 = extensions.Contains("cl_khr_global_int32_base_atomics") && extensions.Contains("cl_khr_local_int32_base_atomics ");
				var hasAtomic64 = extensions.Contains("cl_khr_int64_base_atomics");
				string counterType;
				if (hasAtomic32)
					counterType = "int";
				else if (hasAtomic64)
					counterType = "long";
				else
					throw new NotSupportedException("The device does not support atomic operations.");

				var programId = await OclProgramRepository.Shared.GetProgram(contextId, deviceId, Kernels.IndexOf, $"-D Operation={operation} -D TCounter={counterType} -D TSourceDepth=int -D TResultDepth=int");

				kernelId = CreateKernel(programId, "IndexOf");
				var workGroupSize = GetKernelWorkGroupSize(kernelId, deviceId);

				indexOf = (contextId, deviceId, programId, counterType, workGroupSize);
				_indexOf[queueId] = indexOf;
			}
			else
			{
				kernelId = CreateKernel(indexOf.programId, "IndexOf");
			}
			
			SetKernelArg(kernelId, 0, sourceBufferId);
			SetKernelArg(kernelId, 1, resultBufferId);
			SetKernelArg(kernelId, 2, counterBufferId);
			SetKernelArgSize<int>(kernelId, 3, (int)(indexOf.workGroupSize * 2));
			SetKernelArg(kernelId, 4, comparand);

			var eventId = EnqueueNDRangeKernel(queueId, kernelId,
				globalWorkSize: stackalloc nuint[] { (nuint)width, (nuint)height },
				localWorkSize: stackalloc nuint[] { indexOf.workGroupSize, 1 });

			OnEventCompleted(eventId, () =>
			{
				ReleaseKernel(kernelId);
			});

			return eventId;
		}

		public Task<nint> IndexOfGreaterThan(nint queueId, int width, int height, nint sourceBufferId, nint resultBufferId, nint counterBufferId, int comparand)
		{
			return IndexOf(queueId, width, height, sourceBufferId, resultBufferId, counterBufferId, comparand, ">");
		}

		public Task<nint> IndexOfGreaterThanOrEquals(nint queueId, int width, int height, nint sourceBufferId, nint resultBufferId, nint counterBufferId, int comparand)
		{
			return IndexOf(queueId, width, height, sourceBufferId, resultBufferId, counterBufferId, comparand, ">=");
		}

		public Task<nint> IndexOfLessThan(nint queueId, int width, int height, nint sourceBufferId, nint resultBufferId, nint counterBufferId, int comparand)
		{
			return IndexOf(queueId, width, height, sourceBufferId, resultBufferId, counterBufferId, comparand, "<");
		}

		public Task<nint> IndexOfLessThanOrEquals(nint queueId, int width, int height, nint sourceBufferId, nint resultBufferId, nint counterBufferId, int comparand)
		{
			return IndexOf(queueId, width, height, sourceBufferId, resultBufferId, counterBufferId, comparand, "<=");
		}

		public Task<nint> IndexOfEquals(nint queueId, int width, int height, nint sourceBufferId, nint resultBufferId, nint counterBufferId, int comparand)
		{
			return IndexOf(queueId, width, height, sourceBufferId, resultBufferId, counterBufferId, comparand, "==");
		}
	}
}
