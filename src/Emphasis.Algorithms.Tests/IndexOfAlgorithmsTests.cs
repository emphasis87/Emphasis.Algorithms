﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emphasis.Algorithms.IndexOf;
using FluentAssertions;
using NUnit.Framework;

namespace Emphasis.Algorithms.Tests
{
	public class IndexOfAlgorithmsTests
	{
		private int[] _source;
		private int _width;
		private int _height;
		private int _size;
		private IndexOfAlgorithms _indexOf;

		[OneTimeSetUp]
		public void GlobalSetup()
		{
			_height = 1200;
			_width = 1920;
			_size = _height * _width;
			_source = new int[_size];

			for (var x = 0; x < _source.Length; x++)
			{
				_source[x] = 1;
			}

			_indexOf = new IndexOfAlgorithms();
		}

		[Test]
		public void Should_find_indexOf_greaterThan_simple()
		{
			var source = new int[] {0, 0, 1, 0, 1, 0};
			var result = new int[source.Length * 2];
			var count = _indexOf.IndexOfGreaterThan(3, 2, source, result, comparand: 0);

			count.Should().Be(2);
			(result[0], result[1]).Should().Be((2, 0));
			(result[2], result[3]).Should().Be((1, 1));
		}

		private static IEnumerable<int> LevelOfParallelismSource => Enumerable.Range(1, Environment.ProcessorCount);

		[TestCaseSource(nameof(LevelOfParallelismSource))]
		public async Task Should_find_parallel_indexOf_greaterThan(int levelOfParallelism)
		{
			var source = new int[_width * _height];
			for (var y = 0; y < _height; y++)
			{
				source[y * _width + 1] = 1;
				source[y * _width + 9] = 1;
			}
			var result = new int[source.Length * 2];
			var count = await _indexOf.ParallelIndexOfGreaterThan(_width, _height, source, result, comparand: 0, levelOfParallelism: levelOfParallelism);

			count.Should().Be(2 * _height);
			var results = new HashSet<(int, int)>();
			for (var i = 0; i < count * 2; i += 2)
			{
				results.Add((result[i], result[i + 1]));
			}
			for (var y = 0; y < _height; y++)
			{
				results.Should().Contain((1, y));
				results.Should().Contain((9, y));
			}
		}

		[TestCaseSource(nameof(LevelOfParallelismSource))]
		public async Task Should_find_parallel_indexOf_greaterThan_full_saturation(int levelOfParallelism)
		{
			var result = new int[_source.Length * 2];
			var count = await _indexOf.ParallelIndexOfGreaterThan(_width, _height, _source, result, comparand: 0, levelOfParallelism: levelOfParallelism);

			count.Should().Be(_width * _height);
			var results = new HashSet<(int, int)>();
			for (var i = 0; i < count * 2; i += 2)
			{
				results.Add((result[i], result[i + 1]));
			}
			for (var y = 0; y < _height; y++)
			{
				for (var x = 0; x < _height; x++)
				{
					results.Should().Contain((x, y));
				}
			}
		}
	}
}
