using System;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Emphasis.Algorithms.Resize.OpenCL;
using Emphasis.OpenCL;
using static Emphasis.OpenCL.OclHelper;

namespace Emphasis.Algorithms.Tests.Benchmarks
{
	[MarkdownExporter]
	[SimpleJob(invocationCount: 1000)]
	[Orderer(SummaryOrderPolicy.Method, MethodOrderPolicy.Alphabetical)]
	public class OclResizeBenchmarks
	{
		private nint _platformId;
		private nint _deviceId;
		private nint _contextId;
		private nint _queueId;
		private nint _sourceBufferId;
		private nint _resultBufferId;

		private int _width;
		private int _height;
		private int _size;

		private byte[] _source;

		private readonly OclResizeAlgorithms _resize = new();

		[GlobalSetup]
		public void Setup()
		{
			_height = 1200;
			_width = 1920;
			_size = _height * _width;

			_platformId = GetPlatforms().First();
			_deviceId = GetPlatformDevices(_platformId).First();
			_contextId = CreateContext(_platformId, new[] { _deviceId });
			_queueId = CreateCommandQueue(_contextId, _deviceId);

			_sourceBufferId = OclMemoryPool.Shared.RentBuffer<int>(_contextId, _size);
			_resultBufferId = OclMemoryPool.Shared.RentBuffer<int>(_contextId, _size * 4);

			_source = new byte[_size];

			var random = new Random(1);
			random.NextBytes(_source);
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			OclMemoryPool.Shared.ReturnBuffer(_sourceBufferId);
			OclMemoryPool.Shared.ReturnBuffer(_resultBufferId);

			ReleaseCommandQueue(_queueId);
			ReleaseContext(_contextId);
		}

		[Benchmark]
		public async Task Bilinear_Gray_2x()
		{
			var eventId = await _resize.BilinearGray(_queueId, _width, _height, _width * 2, _height * 2,
				new OclBuffer<int>(_sourceBufferId), new OclBuffer<int>(_resultBufferId));

			Flush(_queueId);
			WaitForEvents(eventId);
			ReleaseEvent(eventId);
		}
	}
}
