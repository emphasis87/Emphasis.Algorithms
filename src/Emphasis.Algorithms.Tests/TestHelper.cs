using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Emphasis.Algorithms.Tests
{
	public static class TestHelper
	{
		public static void Run(Bitmap bitmap, string name)
		{
			if (!Path.HasExtension(name))
				name = $"{name}.png";

			bitmap.Save(name);
			Run(name);
		}

		public static void Run(string path, string arguments = null)
		{
			if (!Path.IsPathRooted(path) && Path.HasExtension(path))
				path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, path));

			var info = new ProcessStartInfo(path)
			{
				UseShellExecute = true
			};
			if (arguments != null)
			{
				info.Arguments = arguments;
			}

			Process.Start(info);
		}

		public static Bitmap ToBitmap(this byte[] data, int width, int height, int channels = 1)
		{
			var bitmap = new Bitmap(width, height);
			var bounds = new System.Drawing.Rectangle(0, 0, width, height);

			var bitmapData = bitmap.LockBits(bounds, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			var bitmapPointer = bitmapData.Scan0;

			if (channels == 1)
			{
				var p = 0;
				var source = new byte[height * width * 4];
				for (var i = 0; i < height * width; i++)
				{
					var value = data[i];
					source[p++] = value;
					source[p++] = value;
					source[p++] = value;
					source[p++] = 255;
				}

				data = source;
			}

			var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			var dataPointer = dataHandle.AddrOfPinnedObject();

			for (var y = 0; y < height; y++)
			{
				SharpDX.Utilities.CopyMemory(bitmapPointer, dataPointer, width * 4);

				dataPointer = IntPtr.Add(dataPointer, width * 4);
				bitmapPointer = IntPtr.Add(bitmapPointer, width * 4);
			}

			bitmap.UnlockBits(bitmapData);

			dataHandle.Free();

			return bitmap;
		}
	}
}
