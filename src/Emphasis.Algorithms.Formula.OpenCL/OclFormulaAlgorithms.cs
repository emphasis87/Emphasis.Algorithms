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
		private readonly ConcurrentDictionary<(nint queueId, string args), (nint contextId, nint deviceId, nint programId)> _programs = new();

		public async Task<nint> Formula(
			nint queueId,
			int width,
			int height,
			OclTypedBuffer sourceBuffer,
			string formula = null)
		{
			formula = formula?.Trim() ?? "d";
			var args = $" -D TSource={sourceBuffer.NativeType}  -D Expression={formula}";
			nint kernelId;
			if (!_programs.TryGetValue((queueId, args), out var program))
			{
				var contextId = GetCommandQueueContext(queueId);
				var deviceId = GetCommandQueueDevice(queueId);
				
				var programId = await OclProgramRepository.Shared.GetProgram(contextId, deviceId, Kernels.Formula, args);
				
				kernelId = CreateKernel(programId, "Formula");

				program = (contextId, deviceId, programId);
				_programs[(queueId, args)] = program;
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
