using System.Collections.Generic;
using System.Drawing;
using BenchmarkDotNet.Attributes;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Util;
using Emphasis.Algorithms.Tests.samples;

namespace Emphasis.Algorithms.Tests.Benchmarks
{
	/// <summary>
	/// Disposes of allocated memory only after each iteration (with custom number of invocations).
	/// This requires large enough GPU memory.
	/// </summary>
	[MarkdownExporter]
	[SimpleJob]
	public class EmguCvBenchmarks_UMat_no_dispose
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

			_source = new UMat();
			_gray = new UMat();
			_resized = new UMat();
			_canny = new UMat();
			_dx = new UMat();
			_dy = new UMat();

			var source = sourceBitmap.ToMat();
			source.CopyTo(_source);

			CvInvoke.CvtColor(_source, _gray, ColorConversion.Bgra2Gray);
			CvInvoke.Resize(_gray, _resized, new Size(_width * 2, _height * 2), interpolation: Inter.Linear);
			CvInvoke.Sobel(_resized, _dx, DepthType.Cv16S, 1, 0);
			CvInvoke.Sobel(_resized, _dy, DepthType.Cv16S, 0, 1);
			CvInvoke.Canny(_dx, _dy, _canny, 80, 40);

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

		private readonly List<UMat> _grayList = new();
		private readonly List<UMat> _resizedList = new();
		private readonly List<UMat> _dxList = new();
		private readonly List<UMat> _dyList = new();
		private readonly List<UMat> _cannyList = new();

		[IterationCleanup]
		public void IterationCleanup()
		{
			_grayList.ForEach(x => x.Dispose());
			_grayList.Clear();
			_resizedList.ForEach(x => x.Dispose());
			_resizedList.Clear();
			_dxList.ForEach(x => x.Dispose());
			_dxList.Clear();
			_dyList.ForEach(x => x.Dispose());
			_dyList.Clear();
			_cannyList.ForEach(x => x.Dispose());
			_cannyList.Clear();
		}

		[Benchmark]
		[InvocationCount(512, 16)]
		public void Grayscale()
		{
			var gray = new UMat();
			_grayList.Add(gray);
			CvInvoke.CvtColor(_source, gray, ColorConversion.Bgra2Gray);
		}

		[Benchmark]
		[InvocationCount(256, 16)]
		public void Resize()
		{
			var resized = new UMat();
			_resizedList.Add(resized);
			CvInvoke.Resize(_gray, resized, new Size(_width * 2, _height * 2), interpolation: Inter.Linear);
		}

		[Benchmark]
		[InvocationCount(64, 16)]
		public void Sobel()
		{
			var dx = new UMat();
			var dy = new UMat();
			_dxList.Add(dx);
			_dyList.Add(dy);
			CvInvoke.Sobel(_resized, dx, DepthType.Cv16S, 1, 0);
			CvInvoke.Sobel(_resized, dy, DepthType.Cv16S, 0, 1);
		}

		[Benchmark]
		[InvocationCount(16, 16)]
		public void Canny()
		{
			var canny = new UMat();
			_cannyList.Add(canny);
			CvInvoke.Canny(_dx, _dy, canny, 80, 40);
		}

		[Benchmark]
		[InvocationCount(4, 4)]
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
