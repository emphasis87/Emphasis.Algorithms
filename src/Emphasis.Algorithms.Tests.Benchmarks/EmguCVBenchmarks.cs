using System.Drawing;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emphasis.Algorithms.Tests.samples;

namespace Emphasis.Algorithms.Tests.Benchmarks
{
	[MarkdownExporter]
	[ShortRunJob]
	[Orderer(SummaryOrderPolicy.Method, MethodOrderPolicy.Alphabetical)]
	public class EmguCvBenchmarks
	{
		private UMat _source;
		private UMat _gray;
		private UMat _resized;
		private UMat _canny;
		
		private int _width;
		private int _height;

		[GlobalSetup]
		public void Setup()
		{
			var sourceBitmap = Samples.sample13;

			_width = sourceBitmap.Width;
			_height = sourceBitmap.Height;

			_source = new UMat();

			var source = sourceBitmap.ToMat();
			source.CopyTo(_source);

			_gray = new UMat();
			_resized = new UMat();
			_canny = new UMat();

			Grayscale();
			Resize_Bilinear_Gray_2x();
			Canny();
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			_source.Dispose();
			_gray.Dispose();
			_resized.Dispose();
			_canny.Dispose();
		}

		[Benchmark]
		public void Grayscale()
		{
			CvInvoke.CvtColor(_source, _gray, ColorConversion.Bgra2Gray);
		}

		[Benchmark]
		public void Resize_Bilinear_Gray_2x()
		{
			CvInvoke.Resize(_gray, _resized, new Size(_width * 2, _height * 2), interpolation: Inter.Linear);
		}

		/// <summary>
		/// Causes OutOfMemoryException
		/// </summary>
		//[Benchmark]
		public void Canny()
		{
			CvInvoke.Canny(_resized, _canny, 80, 40);
		}
	}
}
