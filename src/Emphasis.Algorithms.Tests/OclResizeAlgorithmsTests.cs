using System;
using System.Linq;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emphasis.Algorithms.Resize.OpenCL;
using Emphasis.Algorithms.Tests.samples;
using Emphasis.OpenCL;
using NUnit.Framework;
using static Emphasis.OpenCL.OclHelper;
using static Emphasis.Algorithms.Tests.TestHelper;

namespace Emphasis.Algorithms.Tests
{
	public class OclResizeAlgorithmsTests
	{
		private OclResizeAlgorithms _resizeAlgorithms;

		[OneTimeSetUp]
		public void GlobalSetup()
		{
			_resizeAlgorithms = new OclResizeAlgorithms();
		}

		[Explicit]
		public async Task Can_resize_bilinear_gray()
		{
			var platformId = GetPlatforms().First();
			var deviceId = GetPlatformDevices(platformId).First();
			var contextId = CreateContext(platformId, new[] { deviceId });
			var queueId = CreateCommandQueue(contextId, deviceId);

			var src = new UMat();
			var sample = Samples.sample00;
			var w = sample.Width;
			var h = sample.Height;
			using var bitmap = sample.ToMat();
			bitmap.CopyTo(src);

			using var gray = new UMat();
			CvInvoke.CvtColor(src, gray, ColorConversion.Bgra2Gray);

			var grayArray = new byte[w * h];
			gray.CopyTo(grayArray);

			var grayBufferId = CopyBuffer(contextId, grayArray.AsSpan());
			var resultBufferId = OclMemoryPool.Shared.RentBuffer<byte>(contextId, w * 2 * h * 2);

			var eventId = await _resizeAlgorithms.BilinearGray(queueId, w, h, w * 2, h * 2, new OclBuffer<byte>(grayBufferId), new OclBuffer<byte>(resultBufferId));

			var resultArray = new byte[w * 2 * h * 2];
			EnqueueReadBuffer(queueId, resultBufferId, false, 0, w * 2 * h * 2, resultArray.AsSpan(), stackalloc nint[] {eventId});

			Finish(queueId);

			ReleaseContext(contextId);

			using var result = resultArray.ToBitmap(w * 2, h * 2);
			Run(result, "bilinear_gray");

			using var g = grayArray.ToBitmap(w, h);
			Run(g, "gray");
		}
	}
}
