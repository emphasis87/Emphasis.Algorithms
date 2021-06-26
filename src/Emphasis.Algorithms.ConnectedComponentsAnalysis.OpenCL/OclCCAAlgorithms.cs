using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Emphasis.Algorithms.Formula.OpenCL;
using Emphasis.OpenCL;
using static Emphasis.OpenCL.OclHelper;

namespace Emphasis.Algorithms.ConnectedComponentsAnalysis.OpenCL
{
	public interface IOclCCAAlgorithms
	{

	}

	public class OclCCAAlgorithms : IOclCCAAlgorithms
	{
		private readonly OclFormulaAlgorithms _formulaAlgorithms = new();
		private readonly ConcurrentDictionary<(nint queueId, string args), (nint contextId, nint deviceId, nint programId)> _programs = new();

		public async Task<nint> Labeling4(
			nint queueId,
			int width,
			int height,
			OclTypedBuffer sourceBuffer,
			OclTypedBuffer labelsBuffer)
		{
			// Mark labels in ascending order
			var markEventId = await _formulaAlgorithms.Formula(queueId, width, height, labelsBuffer);

			var args = $"-D TSource={sourceBuffer.NativeType} -D TResult={sourceBuffer.NativeType}";
			nint kernelId;
			if (!_programs.TryGetValue((queueId, args), out var program))
			{
				var contextId = GetCommandQueueContext(queueId);
				var deviceId = GetCommandQueueDevice(queueId);

				var programId = await OclProgramRepository.Shared.GetProgram(contextId, deviceId, Kernels.Labeling, args);

				kernelId = CreateKernel(programId, "Labeling4");

				program = (contextId, deviceId, programId);
				_programs[(queueId, args)] = program;
			}
			else
			{
				kernelId = CreateKernel(program.programId, "Labeling4");
			}

			var hasChangedBuffer = OclMemoryPool.Shared.RentBuffer<int>(program.contextId, 1);

			SetKernelArg(kernelId, 0, sourceBuffer.NativeId);
			SetKernelArg(kernelId, 1, labelsBuffer.NativeId);
			SetKernelArg(kernelId, 2, hasChangedBuffer);

			var eventId = EnqueueNDRangeKernel(queueId, kernelId,
				globalWorkSize: stackalloc nuint[] {(nuint) width, (nuint) height},
				waitOnEvents: stackalloc nint[] {markEventId});

			OnEventCompleted(eventId, () =>
			{
				ReleaseKernel(kernelId);

				OclMemoryPool.Shared.ReturnBuffer(hasChangedBuffer);
			});

			return eventId;
		}

		public async Task<nint> Labeling8(
			nint queueId,
			int width,
			int height,
			OclTypedBuffer sourceBuffer,
			OclTypedBuffer labelsBuffer)
		{
			var args = $"-D TSource={sourceBuffer.NativeType} -D TResult={sourceBuffer.NativeType}";
			nint kernelId;
			if (!_programs.TryGetValue((queueId, args), out var program))
			{
				var contextId = GetCommandQueueContext(queueId);
				var deviceId = GetCommandQueueDevice(queueId);

				var programId = await OclProgramRepository.Shared.GetProgram(contextId, deviceId, Kernels.Labeling, args);

				kernelId = CreateKernel(programId, "Labeling8");

				program = (contextId, deviceId, programId);
				_programs[(queueId, args)] = program;
			}
			else
			{
				kernelId = CreateKernel(program.programId, "Labeling8");
			}

			SetKernelArg(kernelId, 0, sourceBuffer.NativeId);
			SetKernelArg(kernelId, 1, labelsBuffer.NativeId);

			var eventId = EnqueueNDRangeKernel(queueId, kernelId,
				globalWorkSize: stackalloc nuint[] { (nuint)width, (nuint)height });

			OnEventCompleted(eventId, () =>
			{
				ReleaseKernel(kernelId);
			});

			return eventId;
		}
	}
}
