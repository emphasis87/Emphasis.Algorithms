using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Emphasis.OpenCL;
using static Emphasis.OpenCL.OclHelper;

namespace Emphasis.Algorithms.IndexOf.OpenCL
{
	public interface IOclIndexOfAlgorithms
	{
		Task<nint> IndexOfGreaterThan<T>(nint queueId, int width, int height, OclTypedBuffer sourceBuffer, OclTypedBuffer resultBuffer, OclTypedBuffer counterBuffer, T comparand)
			where T : unmanaged;

		Task<nint> IndexOfGreaterThanOrEquals<T>(nint queueId, int width, int height, OclTypedBuffer sourceBuffer, OclTypedBuffer resultBuffer, OclTypedBuffer counterBuffer, T comparand)
			where T : unmanaged;

		Task<nint> IndexOfLessThan<T>(nint queueId, int width, int height, OclTypedBuffer sourceBuffer, OclTypedBuffer resultBuffer, OclTypedBuffer counterBuffer, T comparand)
			where T : unmanaged;

		Task<nint> IndexOfLessThanOrEquals<T>(nint queueId, int width, int height, OclTypedBuffer sourceBuffer, OclTypedBuffer resultBuffer, OclTypedBuffer counterBuffer, T comparand)
			where T : unmanaged;

		Task<nint> IndexOfEquals<T>(nint queueId, int width, int height, OclTypedBuffer sourceBuffer, OclTypedBuffer resultBuffer, OclTypedBuffer counterBuffer, T comparand)
			where T : unmanaged;
	}

	public class OclIndexOfAlgorithms : IOclIndexOfAlgorithms
	{
		private readonly ConcurrentDictionary<(nint deviceId, string counterType), bool> _isSupported = new();
		private readonly ConcurrentDictionary<(nint queueId, string args), (nint contextId, nint deviceId, nint programId, string counterType, uint workGroupSize)> _programs = new();

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
			var args = $"-D Operation={operation} -D TCounter={counterBuffer.NativeType} -D TSource={sourceBuffer.NativeType} -D TResult={resultBuffer.NativeType}";
			const string kernelName = "IndexOf";

			nint kernelId;
			if (!_programs.TryGetValue((queueId, args), out var program))
			{
				var contextId = GetCommandQueueContext(queueId);
				var deviceId = GetCommandQueueDevice(queueId);
				var programId = await OclProgramRepository.Shared.GetProgram(contextId, deviceId, Kernels.IndexOf, args);

				kernelId = CreateKernel(programId, kernelName);
				var workGroupSize = GetKernelWorkGroupSize(kernelId, deviceId);

				program = (contextId, deviceId, programId, counterBuffer.NativeType, workGroupSize);
				_programs[(queueId, args)] = program;
			}
			else
			{
				kernelId = CreateKernel(program.programId, kernelName);
			}
			
			SetKernelArg(kernelId, 0, sourceBuffer.NativeId);
			SetKernelArg(kernelId, 1, resultBuffer.NativeId);
			SetKernelArg(kernelId, 2, counterBuffer.NativeId);
			SetKernelArg(kernelId, 3, comparand);

			var eventId = EnqueueNDRangeKernel(queueId, kernelId,
				globalWorkSize: stackalloc nuint[] {(nuint) width, (nuint) height});

			OnEventCompleted(eventId, () =>
			{
				ReleaseKernel(kernelId);
			});

			return eventId;
		}

		public Task<nint> IndexOfGreaterThan<T>(nint queueId, int width, int height, OclTypedBuffer sourceBuffer, OclTypedBuffer resultBuffer, OclTypedBuffer counterBuffer, T comparand)
			where T : unmanaged
		{
			return IndexOf(queueId, width, height, sourceBuffer, resultBuffer, counterBuffer, comparand, ">");
		}

		public Task<nint> IndexOfGreaterThanOrEquals<T>(nint queueId, int width, int height, OclTypedBuffer sourceBuffer, OclTypedBuffer resultBuffer, OclTypedBuffer counterBuffer, T comparand)
			where T : unmanaged
		{
			return IndexOf(queueId, width, height, sourceBuffer, resultBuffer, counterBuffer, comparand, ">=");
		}

		public Task<nint> IndexOfLessThan<T>(nint queueId, int width, int height, OclTypedBuffer sourceBuffer, OclTypedBuffer resultBuffer, OclTypedBuffer counterBuffer, T comparand)
			where T : unmanaged
		{
			return IndexOf(queueId, width, height, sourceBuffer, resultBuffer, counterBuffer, comparand, "<");
		}

		public Task<nint> IndexOfLessThanOrEquals<T>(nint queueId, int width, int height, OclTypedBuffer sourceBuffer, OclTypedBuffer resultBuffer, OclTypedBuffer counterBuffer, T comparand)
			where T : unmanaged
		{
			return IndexOf(queueId, width, height, sourceBuffer, resultBuffer, counterBuffer, comparand, "<=");
		}

		public Task<nint> IndexOfEquals<T>(nint queueId, int width, int height, OclTypedBuffer sourceBuffer, OclTypedBuffer resultBuffer, OclTypedBuffer counterBuffer, T comparand)
			where T : unmanaged
		{
			return IndexOf(queueId, width, height, sourceBuffer, resultBuffer, counterBuffer, comparand, "==");
		}
	}
}
