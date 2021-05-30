using System;
using Emphasis.OpenCL;

namespace Emphasis.Algorithms.IndexOf.OpenCL
{
	public class OclIndexOfAlgorithms
	{
		public bool IsSupported(nint deviceId)
		{
			var extensions = OclHelper.GetDeviceExtensions(deviceId);
			return 
				extensions.Contains("cl_khr_int64_base_atomics") ||
				extensions.Contains("cl_khr_global_int32_base_atomics") && extensions.Contains("cl_khr_local_int32_base_atomics ");
		}

		public int IndexOfGreaterThan(int width, int height, nint sourceBufferId, nint indexesBufferId, int comparand)
		{

		}
	}
}
