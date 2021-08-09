using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Emphasis.OpenCL;
using static Emphasis.OpenCL.OclHelper;

namespace Emphasis.Algorithms.Resize.OpenCL
{
	public interface IOclResizeAlgorithms
	{

	}

	public class OclResizeAlgorithms : IOclResizeAlgorithms
	{
		private readonly ConcurrentDictionary<(nint queueId, string args), (nint contextId, nint deviceId, nint programId)> _resizePrograms = new();

		public async Task<nint> BilinearGray(
			nint queueId,
			int width0,
			int height0,
			int width1,
			int height1,
			OclTypedBuffer sourceBuffer,
			OclTypedBuffer resultBuffer)
		{
			var widthRatio = (float) (width0 - 1) / width1;
			var heightRatio = (float) (height0 - 1) / height1;

			var args = $"-D TSource={sourceBuffer.NativeType} -D TResult={sourceBuffer.NativeType}";
			const string kernelName = "BilinearGray";

			nint kernelId;
			if (!_resizePrograms.TryGetValue((queueId, args), out var program))
			{
				var contextId = GetCommandQueueContext(queueId);
				var deviceId = GetCommandQueueDevice(queueId);

				var programId =
					await OclProgramRepository.Shared.GetProgram(contextId, deviceId, Kernels.Resize, args);

				kernelId = CreateKernel(programId, kernelName);

				program = (contextId, deviceId, programId);
				_resizePrograms[(queueId, args)] = program;
			}
			else
			{
				kernelId = CreateKernel(program.programId, kernelName);
			}
			
			SetKernelArg(kernelId, 0, sourceBuffer.NativeId);
			SetKernelArg(kernelId, 1, resultBuffer.NativeId);
			SetKernelArg(kernelId, 2, width0);
			SetKernelArg(kernelId, 3, height0);
			SetKernelArg(kernelId, 4, widthRatio);
			SetKernelArg(kernelId, 5, heightRatio);

			var eventId = EnqueueNDRangeKernel(queueId, kernelId,
				globalWorkSize: stackalloc nuint[] { (nuint)width1, (nuint)height1});

			OnEventCompleted(eventId, () =>
			{
				ReleaseKernel(kernelId);
			});

			return eventId;
		}
	}
}
