using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

using CV = OpenCvSharp;

namespace Biscuit {
	public class misc {

		public static int AdjustAlign128(int w) { return ((w+15)/16*16); }  //	((w+15)>>4)<<4
		public static int AdjustAlign64(int w) { return ((w+7)/8*8); }      //	((w+ 7)>>3)<<3
		public static int AdjustAlign32(int w) { return ((w+3)/4*4); }      //	((w+ 3)>>2)<<2
		public static int AdjustAlign16(int w) { return ((w+1)/2*2); }      //	((w+ 1)>>1)<<1


		public static CV.Point Floor(CV.Point2d pt) {
			return new CV.Point(Math.Floor(pt.X), Math.Floor(pt.Y));
		}
		public static CV.Size Floor(CV.Size2d size) {
			return new CV.Size(Math.Floor(size.Width), Math.Floor(size.Height));
		}
		public static CV.Rect Floor(CV.Rect2d rect) {
			return new CV.Rect(Floor(rect.TopLeft), Floor(rect.Size));
		}

		public static T Clamp<T>(T value, T min, T max) where T : IComparable<T> {
			if (value.CompareTo(min) < 0)
				return min;
			if (value.CompareTo(max) > 0)
				return max;
			return value;
		}

		public static CV.Rect MakeRectFromPts(CV.Point pt1, CV.Point pt2) {
			return new CV.Rect(pt1.X, pt1.Y, pt2.X - pt1.X, pt2.Y - pt1.Y);
		}
		public static CV.Rect2d MakeRectFromPts(CV.Point2d pt1, CV.Point2d pt2) {
			return new CV.Rect2d(pt1.X, pt1.Y, pt2.X - pt1.X, pt2.Y - pt1.Y);
		}

		public static CV.Rect2d NormalizeRect(CV.Rect2d rect) {
			CV.Rect2d rect_ = rect;
			if (rect.Width < 0) {
				rect_.X += rect.Width;
				rect_.Width = -rect.Width;
			}
			if (rect.Height < 0) {
				rect_.Y += rect.Height;
				rect_.Height = -rect.Height;
			}
			return rect_;
		}
		public static CV.Rect NormalizeRect(CV.Rect rect) {
			CV.Rect rect_ = rect;
			if (rect.Width < 0) {
				rect_.X += rect.Width;
				rect_.Width = -rect.Width;
			}
			if (rect.Height < 0) {
				rect_.Y += rect.Height;
				rect_.Height = -rect.Height;
			}
			return rect_;
		}

		public static void InflateRect(ref CV.Rect2d rect, double dx, double dy) {
			rect.X -= dx;
			rect.Y -= dy;
			rect.Width += dx * 2;
			rect.Height += dy * 2;
		}
		public static void InflateRect(ref CV.Rect rect, int dx, int dy) {
			rect.X -= dx;
			rect.Y -= dy;
			rect.Width += dx * 2;
			rect.Height += dy * 2;
		}

		public static void DeflateRect(ref CV.Rect2d rect, double dx, double dy) {
			rect.X += dx;
			rect.Y += dy;
			rect.Width -= dx * 2;
			rect.Height -= dy * 2;
		}
		public static void DeflateRect(ref CV.Rect rect, int dx, int dy) {
			rect.X += dx;
			rect.Y += dy;
			rect.Width -= dx * 2;
			rect.Height -= dy * 2;
		}

		public static bool IsRectEmpty(CV.Rect rect) {
			return (rect.Width <= 0) || (rect.Height <= 0);
		}

		public static bool IsRectEmpty(CV.Rect2d rect) {
			return (rect.Width <= 0) || (rect.Height <= 0);
		}

		public static bool IsRectNull(CV.Rect2d rect) {
			return (rect.X == 0) && (rect.Y == 0) && (rect.Width == 0) && (rect.Height == 0);
		}

		public static CV.Point2d CenterPoint(CV.Rect2d rect) {
			return new CV.Point2d(rect.X + rect.Width/2.0, rect.Y + rect.Height /2.0);
		}
		public static CV.Point CenterPoint(CV.Rect rect) {
			return new CV.Point(rect.X + rect.Width/2, rect.Y + rect.Height /2);
		}

		//-----------------------------------------------------------------------------
		// ROI
		//
		public static bool IsROI_Valid(CV.Rect2d rect, CV.Size sizeImage) {
			return true
				&& (rect.Left >= 0)
				&& (rect.Top >= 0)
				&& ((sizeImage.Width < 0) || ((rect.Left < sizeImage.Width) && (rect.Right < sizeImage.Width) && (rect.Left < rect.Right)))
				&& ((sizeImage.Height < 0) || ((rect.Top < sizeImage.Height) && (rect.Bottom < sizeImage.Height) && (rect.Top < rect.Bottom)))
				;
		}
		public static bool IsROI_Valid(CV.Rect rect, CV.Size sizeImage) {
			return true
				&& (rect.Left >= 0)
				&& (rect.Top >= 0)
				&& ((sizeImage.Width < 0) || ((rect.Left < sizeImage.Width) && (rect.Right < sizeImage.Width) && (rect.Left < rect.Right)))
				&& ((sizeImage.Height < 0) || ((rect.Top < sizeImage.Height) && (rect.Bottom < sizeImage.Height) && (rect.Top < rect.Bottom)))
				;
		}

		public static bool AdjustROI(ref CV.Rect2d rect, CV.Size sizeImage) {
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
		public static bool AdjustROI(ref CV.Rect rect, CV.Size sizeImage) {
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

		public static CV.Rect GetSafeROI(CV.Rect2d rect, CV.Size sizeImage) {
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
		public static CV.Rect GetSafeROI(CV.Rect rect, CV.Size sizeImage) {
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

		public static bool PtInRect(CV.Rect rect, CV.Point pt) {
			return (pt.X >= rect.Left) && (pt.X < rect.Right) && (pt.Y >= rect.Top) && (pt.Y < rect.Bottom);
		}

		public static bool PtInRect(CV.Rect2d rect, CV.Point2d pt) {
			return (pt.X >= rect.Left) && (pt.X < rect.Right) && (pt.Y >= rect.Top) && (pt.Y < rect.Bottom);
		}

		private static void GetMatValue<T>(nint ptr, int nChannel, int index, ref CV.Scalar v) where T : unmanaged {
			unsafe {
				T* p = (T*)ptr;
				for (int i = 0; i < nChannel; ++i) {
					v[i] = p[index * nChannel + i] switch {
						byte b => b,
						char c => c,
						ushort u => u,
						short s => s,
						int i_ => i_,
						float f => f,
						double d => d,
						_ => 0.0
					};
					//v[i] = p[index * nChannel + i];
				}
			}
		}

		public static CV.Scalar GetMatValue(nint ptr, int depth, int channel, int row, int col) {
			//if ( mat.empty() or (row < 0) or (row >= mat.rows) or (col < 0) or (col >= mat.cols) )
			//	return;

			CV.Scalar v = new();
			switch (depth) {
			case CV.MatType.CV_8U:
				GetMatValue<byte>(ptr, channel, col, ref v);
				break;
			case CV.MatType.CV_8S:
				GetMatValue<char>(ptr, channel, col, ref v);
				break;
			case CV.MatType.CV_16U:
				GetMatValue<UInt16>(ptr, channel, col, ref v);
				break;
			case CV.MatType.CV_16S:
				GetMatValue<Int16>(ptr, channel, col, ref v);
				break;
			case CV.MatType.CV_32S:
				GetMatValue<Int32>(ptr, channel, col, ref v);
				break;
			case CV.MatType.CV_32F:
				GetMatValue<float>(ptr, channel, col, ref v);
				break;
			case CV.MatType.CV_64F:
				GetMatValue<double>(ptr, channel, col, ref v);
				break;
				//case CV_16F:	GetValue(uint16_t{}); break;
			}

			return v;
		}

	}
}
