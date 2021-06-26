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
		Task<nint> Labeling4(
			nint queueId,
			int width,
			int height,
			OclTypedBuffer sourceBuffer,
			OclTypedBuffer labelsBuffer,
			string expression = null);

		Task<nint> Labeling8(
			nint queueId,
			int width,
			int height,
			OclTypedBuffer sourceBuffer,
			OclTypedBuffer labelsBuffer,
			string expression = null);
	}

	public class OclCCAAlgorithms : IOclCCAAlgorithms
	{
		private readonly OclFormulaAlgorithms _formulaAlgorithms = new();

		private readonly
			ConcurrentDictionary<(nint queueId, string args), (nint contextId, nint deviceId, nint programId)>
			_programs = new();

		public Task<nint> Labeling4(
			nint queueId,
			int width,
			int height,
			OclTypedBuffer sourceBuffer,
			OclTypedBuffer labelsBuffer,
			string expression = null)
		{
			return Labeling(queueId, width, height, sourceBuffer, labelsBuffer, "Labeling4", expression);
		}

		public Task<nint> Labeling8(
			nint queueId,
			int width,
			int height,
			OclTypedBuffer sourceBuffer,
			OclTypedBuffer labelsBuffer,
			string expression = null)
		{
			return Labeling(queueId, width, height, sourceBuffer, labelsBuffer, "Labeling8", expression);
		}

		private async Task<nint> Labeling(
			nint queueId,
			int width,
			int height,
			OclTypedBuffer sourceBuffer,
			OclTypedBuffer labelsBuffer,
			string kernelName,
			string expression = null)
		{
			// Mark labels in ascending order
			var markEventId = await _formulaAlgorithms.Formula(queueId, width, height, labelsBuffer);

			expression ??= "\"(a == b && a > 0)\"";
			if (!expression.StartsWith("\""))
				expression = $"\"({expression})\"";
			var args =
				$"-D TSource={sourceBuffer.NativeType} -D TResult={sourceBuffer.NativeType} -D Expression={expression}";
			nint kernelId;
			if (!_programs.TryGetValue((queueId, args), out var program))
			{
				var contextId = GetCommandQueueContext(queueId);
				var deviceId = GetCommandQueueDevice(queueId);

				var programId =
					await OclProgramRepository.Shared.GetProgram(contextId, deviceId, Kernels.Labeling, args);

				kernelId = CreateKernel(programId, kernelName);

				program = (contextId, deviceId, programId);
				_programs[(queueId, args)] = program;
			}
			else
			{
				kernelId = CreateKernel(program.programId, kernelName);
			}

			var hasChangedBufferId = OclMemoryPool.Shared.RentBuffer<int>(program.contextId, 1);

			SetKernelArg(kernelId, 0, sourceBuffer.NativeId);
			SetKernelArg(kernelId, 1, labelsBuffer.NativeId);
			SetKernelArg(kernelId, 2, hasChangedBufferId);

			nint LabelingLoop()
			{
				var hasChanged = new int[1];
				var lastEventId = markEventId;
				
				while (true)
				{
					lastEventId = EnqueueNDRangeKernel(queueId, kernelId,
						globalWorkSize: stackalloc nuint[] {(nuint) width, (nuint) height},
						waitOnEvents: stackalloc nint[] {lastEventId});
					lastEventId = EnqueueReadBuffer(queueId, hasChangedBufferId, false, 0, 1, hasChanged.AsSpan(),
						waitOnEvents: stackalloc nint[] {lastEventId});

					WaitForEvents(lastEventId);
					
					if (hasChanged[0] != 1)
						return lastEventId;

					lastEventId = EnqueueFillBuffer<int>(queueId, hasChangedBufferId, stackalloc int[] {0});
				}
			}

			var eventId = await Task.Run(LabelingLoop);

			ReleaseKernel(kernelId);
			OclMemoryPool.Shared.ReturnBuffer(hasChangedBufferId);

			return eventId;
		}
	}
}
