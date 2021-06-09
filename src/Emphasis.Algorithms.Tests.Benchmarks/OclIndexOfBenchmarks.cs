using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Emphasis.Algorithms.IndexOf;
using Emphasis.Algorithms.IndexOf.OpenCL;
using Emphasis.OpenCL;
using static Emphasis.OpenCL.OclHelper;

namespace Emphasis.Algorithms.Tests.Benchmarks
{
	[MarkdownExporter]
	[SimpleJob(invocationCount: 1000)]
	[Orderer(SummaryOrderPolicy.Method, MethodOrderPolicy.Alphabetical)]
	public class OclIndexOfBenchmarks
	{
		private nint _platformId;
		private nint _deviceId;
		private nint _contextId;
		private nint _queueId;
		private nint _sourceBufferId;
		private nint _resultBufferId;
		private nint _counterBufferId;

		private int[] _source;
		private int[] _indexes;

		private int _width;
		private int _height;
		private int _size;

		private OclIndexOfAlgorithms _indexOf;

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
			_resultBufferId = OclMemoryPool.Shared.RentBuffer<int>(_contextId, _size * 2);
			_counterBufferId = OclMemoryPool.Shared.RentBuffer<int>(_contextId, 1);

			_source = new int[_size];
			_indexes = new int[_source.Length * 2];

			var random = new Random(1);
			for (var x = 0; x < _source.Length; x++)
			{
				_source[x] = random.Next(0, 99);
			}
			
			_indexOf = new OclIndexOfAlgorithms();
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			OclMemoryPool.Shared.ReturnBuffer(_sourceBufferId);
			OclMemoryPool.Shared.ReturnBuffer(_resultBufferId);
			OclMemoryPool.Shared.ReturnBuffer(_counterBufferId);

			ReleaseCommandQueue(_queueId);
			ReleaseContext(_contextId);
		}

		[ParamsSource(nameof(SaturationSource))]
		public int Saturation { get; set; } = 10;

		public IEnumerable<int> SaturationSource => Enumerable.Range(1, 5).Select(x => x * 10);

		[Benchmark]
		public async Task IndexOfGreaterThan()
		{
			var comparand = 99 - Saturation;
			var eventId = await _indexOf.IndexOfGreaterThan(_queueId, _width, _height, _sourceBufferId, _resultBufferId, _counterBufferId, comparand);
			await WaitForEventsAsync(eventId);
			ReleaseEvent(eventId);
		}
	}
}
