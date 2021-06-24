using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emphasis.Algorithms.Formula.OpenCL;
using Emphasis.OpenCL;
using FluentAssertions;
using NUnit.Framework;
using static Emphasis.OpenCL.OclHelper;

namespace Emphasis.Algorithms.Tests
{
	public class OclFormulaAlgorithmsTests
	{
		[Test]
		public async Task Can_execute_default_formula()
		{
			// Arrange:
			var platformId = GetPlatforms().First();
			var deviceId = GetPlatformDevices(platformId).First();
			var contextId = CreateContext(platformId, new[] { deviceId });
			var queueId = CreateCommandQueue(contextId, deviceId);

			var srcBuffer = CreateBuffer<int>(contextId, 16);

			// Act:
			var alg = new OclFormulaAlgorithms();
			var eventId = await alg.Formula(queueId, 4, 4, new OclBuffer<int>(srcBuffer));

			WaitForEvents(eventId);

			// Assert:
			var result = new int[16];
			var readEventId = EnqueueReadBuffer(queueId, srcBuffer, true, 0, 16, result.AsSpan());

			WaitForEvents(readEventId);

			ReleaseContext(contextId);

			for (var i = 0; i < result.Length; i++)
			{
				result[i].Should().Be(i);
			}
		}
	}
}
