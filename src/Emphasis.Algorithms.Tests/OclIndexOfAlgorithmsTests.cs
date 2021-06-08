﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Emphasis.Algorithms.IndexOf.OpenCL;
using FluentAssertions;
using NUnit.Framework;
using static Emphasis.OpenCL.OclHelper;

namespace Emphasis.Algorithms.Tests
{
	public class OclIndexOfAlgorithmsTests
	{
		[Test]
		public async Task Should_find_indexOf_greaterThan_simple()
		{
			var source = new int[] { 0, 0, 1, 0, 1, 0 };
			var result = new int[source.Length * 2];

			var platformId = GetPlatforms().First();
			var deviceId = GetPlatformDevices(platformId).First();
			var contextId = CreateContext(platformId, new[] {deviceId});
			var queueId = CreateCommandQueue(contextId, deviceId);

			var srcBufferId = CopyBuffer(contextId, source.AsSpan());
			var dstBufferId = CreateBuffer<int>(contextId, result.Length);

			var indexOf = new OclIndexOfAlgorithms();
			var eventId = await indexOf.IndexOfGreaterThan(queueId, 3, 2, srcBufferId, dstBufferId, comparand: 0);
			
			await WaitForEventsAsync(eventId);

			var readEventId = EnqueueReadBuffer(queueId, dstBufferId, true, 0, result.Length, result.AsSpan());
			await WaitForEventsAsync(readEventId);

			ReleaseEvent(eventId);
			ReleaseEvent(readEventId);
			ReleaseMemObject(srcBufferId);
			ReleaseMemObject(dstBufferId);
			ReleaseCommandQueue(queueId);
			ReleaseContext(contextId);
			
			(result[0], result[1]).Should().Be((2, 0));
			(result[2], result[3]).Should().Be((1, 1));
			for (var i = 4; i < result.Length; i++)
			{
				result[i].Should().Be(0);
			}
		}
	}
}
