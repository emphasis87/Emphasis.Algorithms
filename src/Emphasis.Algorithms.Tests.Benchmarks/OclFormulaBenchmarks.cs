using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Emphasis.Algorithms.Formula.OpenCL;
using Emphasis.OpenCL;
using static Emphasis.OpenCL.OclHelper;

namespace Emphasis.Algorithms.Tests.Benchmarks
{
	[MarkdownExporter]
	[SimpleJob(invocationCount: 1500)]
	[Orderer(SummaryOrderPolicy.Method, MethodOrderPolicy.Alphabetical)]
	public class OclFormulaBenchmarks
	{
		private nint _platformId;
		private nint _deviceId;
		private nint _contextId;
		private nint _queueId;
		private nint _sourceBufferId;

		private int _width;
		private int _height;
		private int _size;

		private OclFormulaAlgorithms _algorithms;

		[GlobalSetup]
		public void Setup()
		{
			_height = 1200;
			_width = 1920;
			_size = _height * _width;

			_platformId = GetPlatforms().First();
			_deviceId = GetPlatformDevices(_platformId).First();
			_contextId = CreateContext(_platformId, new[] {_deviceId});
			_queueId = CreateCommandQueue(_contextId, _deviceId);

			_sourceBufferId = OclMemoryPool.Shared.RentBuffer<int>(_contextId, _size);
			
			_algorithms = new OclFormulaAlgorithms();
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			OclMemoryPool.Shared.ReturnBuffer(_sourceBufferId);

			ReleaseCommandQueue(_queueId);
			ReleaseContext(_contextId);
		}
		
		[Benchmark]
		public async Task Formula()
		{
			await _algorithms.Formula(_queueId, _width, _height,
				new OclBuffer<int>(_sourceBufferId));

			Finish(_queueId);
		}
	}
}
