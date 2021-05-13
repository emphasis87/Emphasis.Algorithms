using BenchmarkDotNet.Running;

namespace Emphasis.Algorithms.Tests.Benchmarks
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
		}
	}
}
