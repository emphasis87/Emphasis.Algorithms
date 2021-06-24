using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using Emphasis.OpenCL;
using static Emphasis.OpenCL.OclHelper;

namespace Emphasis.Algorithms.Formula.OpenCL
{
	public interface IOclFormulaAlgorithms
	{
		Task<nint> Formula(
			nint queueId,
			int width,
			int height,
			OclTypedBuffer sourceBuffer,
			string formula);
	}

	public class OclFormulaAlgorithms : IOclFormulaAlgorithms
	{
		private readonly ConcurrentDictionary<nint, (nint contextId, nint deviceId, nint programId)> _programs = new();

		public async Task<nint> Formula(
			nint queueId,
			int width,
			int height,
			OclTypedBuffer sourceBuffer,
			string formula = null)
		{
			nint kernelId;
			if (!_programs.TryGetValue(queueId, out var program))
			{
				var contextId = GetCommandQueueContext(queueId);
				var deviceId = GetCommandQueueDevice(queueId);
				
				var args = new StringBuilder();
				args.Append($" -D TSource={sourceBuffer.NativeType}");
				if (!string.IsNullOrWhiteSpace(formula))
					args.Append($" -D Expression={formula}");

				var programId = await OclProgramRepository.Shared.GetProgram(contextId, deviceId, Kernels.Formula, args.ToString());
				
				kernelId = CreateKernel(programId, "Formula");

				program = (contextId, deviceId, programId);
				_programs[queueId] = program;
			}
			else
			{
				kernelId = CreateKernel(program.programId, "Formula");
			}

			SetKernelArg(kernelId, 0, sourceBuffer.NativeId);

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
