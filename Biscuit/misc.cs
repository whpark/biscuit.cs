using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CV = OpenCvSharp;

namespace Biscuit
{
	public class misc
	{

		public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
		{
			if (value.CompareTo(min) < 0) return min;
			if (value.CompareTo(max) > 0) return max;
			return value;
		}

		public static CV.Rect2d NormalizeRect(CV.Rect2d rect)
		{
			CV.Rect2d rect_ = rect;
			if (rect.Width < 0)
			{
				rect.X += rect.Width;
				rect.Width = -rect.Width;
			}
			if (rect.Height < 0)
			{
				rect.Y += rect.Height;
				rect.Height = -rect.Height;
			}
			return rect_;
		}

		public static void DeflateRect(ref CV.Rect2d rect, double dx, double dy)
		{
			rect.X += dx;
			rect.Y += dy;
			rect.Width -= dx * 2;
			rect.Height -= dy * 2;
		}
	}
}
