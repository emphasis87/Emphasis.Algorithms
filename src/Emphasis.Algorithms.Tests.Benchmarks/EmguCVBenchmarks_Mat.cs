using System.Drawing;
using BenchmarkDotNet.Attributes;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Util;
using Emphasis.Algorithms.Tests.samples;

namespace Emphasis.Algorithms.Tests.Benchmarks
{
	[MarkdownExporter]
	[SimpleJob]
	public class EmguCvBenchmarks_Mat
	{
		private int _width;
		private int _height;

		private IInputOutputArray _source;
		private IInputOutputArray _gray;
		private IInputOutputArray _resized;
		private IInputOutputArray _canny;
		private IInputOutputArray _dx;
		private IInputOutputArray _dy;

		private MSER _mser;

		[GlobalSetup]
		public void Setup()
		{
			var sourceBitmap = Samples.sample13;

			_width = sourceBitmap.Width;
			_height = sourceBitmap.Height;

			_source = new Mat();

			var source = sourceBitmap.ToMat();
			source.CopyTo(_source);

			_gray = new Mat();
			_resized = new Mat();
			_canny = new Mat();
			_dx = new Mat();
			_dy = new Mat();

			Grayscale();
			Resize();
			Sobel();
			Canny();

			_mser = new MSER(
				minArea: 5, maxArea: 80, edgeBlurSize: 5);
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			_source.Dispose();
			_gray.Dispose();
			_resized.Dispose();
			_dx.Dispose();
			_dy.Dispose();
			_canny.Dispose();
			_mser.Dispose();
		}
		
		[Benchmark]
		public void Grayscale()
		{
			CvInvoke.CvtColor(_source, _gray, ColorConversion.Bgra2Gray);
		}

		[Benchmark]
		public void Resize()
		{
			CvInvoke.Resize(_gray, _resized, new Size(_width * 2, _height * 2), interpolation: Inter.Linear);
		}

		[Benchmark]
		public void Sobel()
		{
			CvInvoke.Sobel(_resized, _dx, DepthType.Cv16S, 1, 0);
			CvInvoke.Sobel(_resized, _dy, DepthType.Cv16S, 0, 1);
		}
		
		[Benchmark]
		public void Canny()
		{
			CvInvoke.Canny(_dx, _dy, _canny, 80, 40);
		}

		[Benchmark]
		public void MSER()
		{
			using var msers = new VectorOfVectorOfPoint();
			using var bboxes = new VectorOfRect();
			
			_mser.DetectRegions(_gray, msers, bboxes);
		}

		[Benchmark]
		public void MSER_large()
		{
			using var msers = new VectorOfVectorOfPoint();
			using var bboxes = new VectorOfRect();
			
			_mser.DetectRegions(_resized, msers, bboxes);
		}
	}
}
