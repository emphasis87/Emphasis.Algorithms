using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emphasis.Algorithms.IndexOf.OpenCL;
using Emphasis.Algorithms.Tests.samples;
using Emphasis.OpenCL;
using FluentAssertions;
using NUnit.Framework;
using static Emphasis.OpenCL.OclHelper;

namespace Emphasis.Algorithms.Tests
{
	public class OclIndexOfAlgorithmsTests
	{
		private int[] _source;
		private int _width;
		private int _height;
		private int _size;

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
		}

		[Test]
		public async Task IndexOf_greaterThan_simple()
		{
			// Arrange:
			var source = new int[] { 0, 0, 1, 1, 1, 0 };
			var result = new int[source.Length * 2];
			var count = new int[1] {0};
			var w = 3;
			var h = 2;

			var platformId = GetPlatforms().First();
			var deviceId = GetPlatformDevices(platformId).First();
			var contextId = CreateContext(platformId, new[] {deviceId});
			var queueId = CreateCommandQueue(contextId, deviceId);

			var srcBufferId = CopyBuffer(contextId, source.AsSpan());
			var dstBufferId = CreateBuffer<int>(contextId, result.Length);
			var cntBufferId = CreateBuffer<int>(contextId, 1);

			// Act:
			var indexOf = new OclIndexOfAlgorithms();
			var eventId = await indexOf.IndexOfGreaterThan(queueId, w, h, 
				new OclBuffer<int>(srcBufferId), new OclBuffer<int>(dstBufferId), new OclBuffer<int>(cntBufferId), comparand: 0);
			
			// Assert:
			WaitForEvents(eventId);

			var readDstId = EnqueueReadBuffer(queueId, dstBufferId, false, 0, result.Length, result.AsSpan());
			var readCntId = EnqueueReadBuffer(queueId, cntBufferId, false, 0, 1, count.AsSpan());
			WaitForEvents(readDstId, readCntId);
			
			ReleaseContext(contextId);

			var cnt = count[0];
			cnt.Should().Be(3);

			var indexes = new HashSet<(int, int)>();
			for (var i = 0; i < result.Length; i+=2)
			{
				indexes.Add((result[i], result[i + 1]));
			}

			indexes.Should().BeEquivalentTo((2, 0), (0, 1), (1, 1), (0, 0));

			var expected = new int[6];
			for (var i = 0; i < cnt * 2; i += 2)
			{
				var x = result[i];
				var y = result[i + 1];
				expected[y * w + x] = 1;
			}

			source.Should().Equal(expected);
		}
		
		[Test]
		public async Task IndexOf_greaterThan()
		{
			var w = 100;
			var h = 8;
			var source = new int[w * h];
			for (var y = 0; y < h; y++)
			{
				source[y * w + 1] = 1;
				source[y * w + 9] = 1;
			}
			var result = new int[source.Length * 2];
			var count = new int[1] { 0 };

			var platformId = GetPlatforms().First();
			var deviceId = GetPlatformDevices(platformId).First();
			var contextId = CreateContext(platformId, new[] { deviceId });
			var queueId = CreateCommandQueue(contextId, deviceId);

			var srcBufferId = CopyBuffer(contextId, source.AsSpan());
			var dstBufferId = CreateBuffer<int>(contextId, result.Length);
			var cntBufferId = CreateBuffer<int>(contextId, 1);

			// Act:
			var indexOf = new OclIndexOfAlgorithms();
			var eventId = await indexOf.IndexOfGreaterThan(queueId, w, h, 
				new OclBuffer<int>(srcBufferId), new OclBuffer<int>(dstBufferId), new OclBuffer<int>(cntBufferId), comparand: 0);

			// Assert:
			WaitForEvents(eventId);

			var readDstId = EnqueueReadBuffer(queueId, dstBufferId, false, 0, result.Length, result.AsSpan());
			var readCntId = EnqueueReadBuffer(queueId, cntBufferId, false, 0, 1, count.AsSpan());
			WaitForEvents(readDstId, readCntId);
			
			ReleaseContext(contextId);

			count[0].Should().Be(2 * h);

			var results = new HashSet<(int, int)>();
			for (var i = 0; i < count[0] * 2; i += 2)
			{
				results.Add((result[i], result[i + 1]));
			}
			for (var y = 0; y < h; y++)
			{
				results.Should().Contain((1, y));
				results.Should().Contain((9, y));
			}
		}
		
		[Test]
		public async Task IndexOf_greaterThan_full_saturation()
		{
			var source = _source;
			var result = new int[source.Length * 2];
			var count = new int[1] { 0 };

			var platformId = GetPlatforms().First();
			var deviceId = GetPlatformDevices(platformId).First();
			var contextId = CreateContext(platformId, new[] { deviceId });
			var queueId = CreateCommandQueue(contextId, deviceId);

			var srcBufferId = CopyBuffer(contextId, source.AsSpan());
			var dstBufferId = CreateBuffer<int>(contextId, result.Length);
			var cntBufferId = CreateBuffer<int>(contextId, 1);

			// Act:
			var indexOf = new OclIndexOfAlgorithms();
			var eventId = await indexOf.IndexOfGreaterThan(queueId, _width, _height, 
				new OclBuffer<int>(srcBufferId), new OclBuffer<int>(dstBufferId), new OclBuffer<int>(cntBufferId), comparand: 0);

			// Assert:
			WaitForEvents(eventId);

			var readDstId = EnqueueReadBuffer(queueId, dstBufferId, false, 0, result.Length, result.AsSpan());
			var readCntId = EnqueueReadBuffer(queueId, cntBufferId, false, 0, 1, count.AsSpan());
			WaitForEvents(readDstId, readCntId);
			
			ReleaseContext(contextId);

			count[0].Should().Be(_width * _height);

			var results = new HashSet<(int, int)>();
			for (var i = 0; i < count[0] * 2; i += 2)
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
