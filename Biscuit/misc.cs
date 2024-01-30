using OpenCvSharp;
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

		public static bool IsRectEmpty(CV.Rect rect)
		{
			return (rect.Width <= 0) || (rect.Height <= 0);
		}

		public static bool IsRectEmpty(CV.Rect2d rect)
		{
			return (rect.Width <= 0) || (rect.Height <= 0);
		}

		public static bool IsRectNull(CV.Rect2d rect)
		{
			return (rect.X == 0) && (rect.Y == 0) && (rect.Width == 0) && (rect.Height == 0);
		}

		//-----------------------------------------------------------------------------
		// ROI
		//
		public static bool IsROI_Valid(CV.Rect2d rect, CV.Size sizeImage)
		{
			return true
				&& (rect.Left >= 0)
				&& (rect.Top >= 0)
				&& ((sizeImage.Width < 0) || ((rect.Left < sizeImage.Width) && (rect.Right < sizeImage.Width) && (rect.Left < rect.Right)))
				&& ((sizeImage.Height < 0) || ((rect.Top < sizeImage.Height) && (rect.Bottom < sizeImage.Height) && (rect.Top < rect.Bottom)))
				;
		}

		public static bool AdjustROI(ref CV.Rect2d rect, CV.Size sizeImage)
		{
			rect = NormalizeRect(rect);

			if (rect.Left < 0)
				rect.Left = 0;
			if (rect.Top < 0)
				rect.Top = 0;
			if ((sizeImage.Width > 0) && (rect.Right > sizeImage.Width))
				rect.Width = sizeImage.Width - rect.Left;
			if ((sizeImage.Height > 0) && (rect.Bottom > sizeImage.Height))
				rect.Height = sizeImage.Height - rect.Top;

			return !IsRectEmpty(rect);
		}

		public static CV.Rect GetSafeROI(CV.Rect2d rect, CV.Size sizeImage)
		{
			rect = NormalizeRect(rect);
			CV.Rect r = new CV.Rect((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);

			if (r.Left < 0)
				r.Left = 0;
			if (r.Top < 0)
				r.Top = 0;
			if ((sizeImage.Width > 0) && (r.Right > sizeImage.Width))
				r.Width = sizeImage.Width - r.Left;
			if ((sizeImage.Height > 0) && (r.Bottom > sizeImage.Height))
				r.Height = sizeImage.Height - r.Top;

			return r;
		}

	}
}
