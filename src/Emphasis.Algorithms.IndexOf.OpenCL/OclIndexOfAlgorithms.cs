using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Emphasis.OpenCL;
using static Emphasis.OpenCL.OclHelper;

namespace Emphasis.Algorithms.IndexOf.OpenCL
{
	public class OclIndexOfAlgorithms
	{
		private readonly ConcurrentDictionary<(nint deviceId, string counterType), bool> _isSupported = new();
		private readonly ConcurrentDictionary<nint, (nint contextId, nint deviceId, nint programId, string counterType, uint workGroupSize)> _indexOf = new();

		public bool IsSupported(nint deviceId, OclTypedBuffer counterBuffer)
		{
			var nativeType = counterBuffer.NativeType;
			if (!_isSupported.TryGetValue((deviceId, nativeType), out var isSupported))
			{
				var extensions = GetDeviceExtensions(deviceId);
				var supportsInt32 = extensions.Contains("cl_khr_global_int32_base_atomics") && extensions.Contains("cl_khr_local_int32_base_atomics ");
				var supportsInt64 = extensions.Contains("cl_khr_int64_base_atomics");
				isSupported = nativeType switch
				{
					"int" => supportsInt32,
					"uint" => supportsInt32,
					"long" => supportsInt64,
					"ulong" => supportsInt64,
					_ => false
				};

				_isSupported[(deviceId, nativeType)] = isSupported;
			}

			return isSupported;
		}
		
		private async Task<nint> IndexOf<T>(
			nint queueId,
			int width,
			int height,
			OclTypedBuffer sourceBuffer,
			OclTypedBuffer resultBuffer,
			OclTypedBuffer counterBuffer,
			T comparand,
			string operation)
			where T : unmanaged
		{
			nint kernelId;
			if (!_indexOf.TryGetValue(queueId, out var indexOf))
			{
				var contextId = GetCommandQueueContext(queueId);
				var deviceId = GetCommandQueueDevice(queueId);
				var programId = await OclProgramRepository.Shared.GetProgram(contextId, deviceId, Kernels.IndexOf,
					$"-D Operation={operation} -D TCounter={counterBuffer.NativeType} -D TSource={sourceBuffer.NativeType} -D TResult={resultBuffer.NativeType}");

				kernelId = CreateKernel(programId, "IndexOf");
				var workGroupSize = GetKernelWorkGroupSize(kernelId, deviceId);

				indexOf = (contextId, deviceId, programId, counterBuffer.NativeType, workGroupSize);
				_indexOf[queueId] = indexOf;
			}
			else
			{
				kernelId = CreateKernel(indexOf.programId, "IndexOf");
			}
			
			SetKernelArg(kernelId, 0, sourceBuffer.NativeId);
			SetKernelArg(kernelId, 1, resultBuffer.NativeId);
			SetKernelArg(kernelId, 2, counterBuffer.NativeId);
			//SetKernelArgSize<int>(kernelId, 3, (int)(indexOf.workGroupSize * 2));
			SetKernelArg(kernelId, 3, comparand);

			var eventId = EnqueueNDRangeKernel(queueId, kernelId,
				globalWorkSize: stackalloc nuint[] { (nuint)width, (nuint)height },
				localWorkSize: stackalloc nuint[] { indexOf.workGroupSize, 1 });

			OnEventCompleted(eventId, () =>
			{
				ReleaseKernel(kernelId);
			});

			return eventId;
		}

		public Task<nint> IndexOfGreaterThan(nint queueId, int width, int height, OclTypedBuffer sourceBuffer, OclTypedBuffer resultBuffer, OclTypedBuffer counterBuffer, int comparand)
		{
			return IndexOf(queueId, width, height, sourceBuffer, resultBuffer, counterBuffer, comparand, ">");
		}

		public Task<nint> IndexOfGreaterThanOrEquals(nint queueId, int width, int height, OclTypedBuffer sourceBuffer, OclTypedBuffer resultBuffer, OclTypedBuffer counterBuffer, int comparand)
		{
			return IndexOf(queueId, width, height, sourceBuffer, resultBuffer, counterBuffer, comparand, ">=");
		}

		public Task<nint> IndexOfLessThan(nint queueId, int width, int height, OclTypedBuffer sourceBuffer, OclTypedBuffer resultBuffer, OclTypedBuffer counterBuffer, int comparand)
		{
			return IndexOf(queueId, width, height, sourceBuffer, resultBuffer, counterBuffer, comparand, "<");
		}

		public Task<nint> IndexOfLessThanOrEquals(nint queueId, int width, int height, OclTypedBuffer sourceBuffer, OclTypedBuffer resultBuffer, OclTypedBuffer counterBuffer, int comparand)
		{
			return IndexOf(queueId, width, height, sourceBuffer, resultBuffer, counterBuffer, comparand, "<=");
		}

		public Task<nint> IndexOfEquals(nint queueId, int width, int height, OclTypedBuffer sourceBuffer, OclTypedBuffer resultBuffer, OclTypedBuffer counterBuffer, int comparand)
		{
			return IndexOf(queueId, width, height, sourceBuffer, resultBuffer, counterBuffer, comparand, "==");
		}
	}
}
