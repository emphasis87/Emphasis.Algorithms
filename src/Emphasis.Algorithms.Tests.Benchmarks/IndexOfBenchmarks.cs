using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Emphasis.Algorithms.IndexOf;

namespace Emphasis.Algorithms.Tests.Benchmarks
{
	[MarkdownExporter]
	[SimpleJob]
	[Orderer(SummaryOrderPolicy.Method, MethodOrderPolicy.Alphabetical)]
	public class IndexOfBenchmarks
	{
		private int[] _source;
		private int[] _indexes;
		private int _width;
		private int _height;
		private int _size;
		private IndexOfAlgorithms _indexOf;

		[GlobalSetup]
		public void Setup()
		{
			_height = 1200;
			_width = 1920;
			_size = _height * _width;
			_source = new int[_size];
			_indexes = new int[_source.Length * 2];

			var random = new Random(1);
			for (var x = 0; x < _source.Length; x++)
			{
				_source[x] = random.Next(0, 99);
			}

			_indexOf = new IndexOfAlgorithms();
		}

		[ParamsSource(nameof(SaturationSource))]
		public int Saturation { get; set; }

		public IEnumerable<int> SaturationSource => Enumerable.Range(1, 5).Select(x => x * 10);

		//[ParamsSource(nameof(LevelOfParallelismSource))]
		public int LevelOfParallelism { get; set; } = 1;

		public IEnumerable<int> LevelOfParallelismSource => Enumerable.Range(1, Environment.ProcessorCount);

		[Benchmark]
		public async Task IndexOfGreaterThan()
		{
			await _indexOf.ParallelIndexOfGreaterThan(_source, _width, _height, _indexes, 99 - Saturation, LevelOfParallelism);
		}
	}
}
