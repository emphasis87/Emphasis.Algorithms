using System;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Emphasis.Algorithms.ConnectedComponentsAnalysis.OpenCL;
using Emphasis.OpenCL;
using static Emphasis.OpenCL.OclHelper;

namespace Emphasis.Algorithms.Tests.Benchmarks
{
	[MarkdownExporter]
	[SimpleJob]
	[Orderer(SummaryOrderPolicy.Method, MethodOrderPolicy.Alphabetical)]
	public class OclCCALabelingBenchmarks
	{
		private nint _platformId;
		private nint _deviceId;
		private nint _contextId;
		private nint _queueId;
		private nint _sourceBufferId;
		private nint _labelsBufferId;

		private int _width;
		private int _height;
		private int _size;

		private int[] _source;

		private readonly OclCCAAlgorithms _cca = new();

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
			_labelsBufferId = OclMemoryPool.Shared.RentBuffer<int>(_contextId, _size);

			_source = new int[_size];
			
			var random = new Random(1);
			for (var x = 0; x < _source.Length; x++)
			{
				_source[x] = random.Next(0, 100) < 70 ? 1 : 0;
			}
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			OclMemoryPool.Shared.ReturnBuffer(_sourceBufferId);
			OclMemoryPool.Shared.ReturnBuffer(_labelsBufferId);

			ReleaseCommandQueue(_queueId);
			ReleaseContext(_contextId);
		}

		[Benchmark]
		public async Task Labeling4()
		{
			var eventId = await _cca.Labeling4(_queueId, _width, _height,
				new OclBuffer<int>(_sourceBufferId), new OclBuffer<int>(_labelsBufferId));

			Flush(_queueId);
			WaitForEvents(eventId);
			ReleaseEvent(eventId);
		}

		[Benchmark]
		public async Task Labeling8()
		{
			var eventId = await _cca.Labeling8(_queueId, _width, _height,
				new OclBuffer<int>(_sourceBufferId), new OclBuffer<int>(_labelsBufferId));

			Flush(_queueId);
			WaitForEvents(eventId);
			ReleaseEvent(eventId);
		}
	}
}
