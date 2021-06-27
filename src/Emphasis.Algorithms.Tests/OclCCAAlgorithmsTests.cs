using System;
using System.Linq;
using System.Threading.Tasks;
using Emphasis.Algorithms.ConnectedComponentsAnalysis.OpenCL;
using Emphasis.OpenCL;
using FluentAssertions;
using NUnit.Framework;
using static Emphasis.OpenCL.OclHelper;

namespace Emphasis.Algorithms.Tests
{
	public class OclCCAAlgorithmsTests
	{
		[Test]
		public async Task Can_execute_labeling_n4()
		{
			// Arrange:
			var platformId = GetPlatforms().First();
			var deviceId = GetPlatformDevices(platformId).First();
			var contextId = CreateContext(platformId, new[] {deviceId});
			var queueId = CreateCommandQueue(contextId, deviceId);

			var src = new int[]
			{
				0, 1, 0, 1,
				1, 0, 1, 1,
				1, 0, 1, 0,
				1, 1, 1, 1,
			};

			var srcBuffer = CopyBuffer<int>(contextId, src);
			var labBuffer = CreateBuffer<int>(contextId, 16);

			// Act:
			var alg = new OclCCAAlgorithms();
			var eventId = await alg.Labeling4(queueId, 4, 4, new OclBuffer<int>(srcBuffer), new OclBuffer<int>(labBuffer));

			WaitForEvents(eventId);

			// Assert:
			var result = new int[16];
			var readEventId = EnqueueReadBuffer(queueId, labBuffer, true, 0, 16, result.AsSpan());

			WaitForEvents(readEventId);

			ReleaseContext(contextId);

			var expected = new int[]
			{
				0, 1, 2, 3,
				3, 5, 3, 3,
				3, 9, 3, 11,
				3, 3, 3, 3,
			};

			result.Should().Equal(expected);
		}

		[Test]
		public async Task Can_execute_labeling_n8()
		{
			// Arrange:
			var platformId = GetPlatforms().First();
			var deviceId = GetPlatformDevices(platformId).First();
			var contextId = CreateContext(platformId, new[] { deviceId });
			var queueId = CreateCommandQueue(contextId, deviceId);

			var src = new int[]
			{
				0, 1, 0, 1,
				1, 0, 1, 1,
				1, 0, 1, 0,
				1, 1, 1, 1,
			};

			var srcBuffer = CopyBuffer<int>(contextId, src);
			var labBuffer = CreateBuffer<int>(contextId, 16);

			// Act:
			var alg = new OclCCAAlgorithms();
			var eventId = await alg.Labeling8(queueId, 4, 4, new OclBuffer<int>(srcBuffer), new OclBuffer<int>(labBuffer));

			WaitForEvents(eventId);

			// Assert:
			var result = new int[16];
			var readEventId = EnqueueReadBuffer(queueId, labBuffer, true, 0, 16, result.AsSpan());

			WaitForEvents(readEventId);

			ReleaseContext(contextId);

			var expected = new int[]
			{
				0, 1, 2, 1,
				1, 5, 1, 1,
				1, 9, 1, 11,
				1, 1, 1, 1,
			};

			result.Should().Equal(expected);
		}
	}
}
