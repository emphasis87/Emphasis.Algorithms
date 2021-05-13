﻿using System;
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
		private IndexOfAlgorithms _indexOf;

		[GlobalSetup]
		public void Setup()
		{
			_height = 1200;
			_width = 1920;
			_source = new int[_width * _height];
			_indexes = new int[_source.Length * 2];

			var random = new Random();
			for (var x = 0; x < _source.Length; x++)
			{
				_source[x] = random.Next(0, 1);
			}

			_indexOf = new IndexOfAlgorithms();
		}
		
		[Benchmark]
		public void IndexOfGreaterThan()
		{
			_indexOf.IndexOfGreaterThan(_source, _width, _height, _indexes, 0);
		}
	}
}