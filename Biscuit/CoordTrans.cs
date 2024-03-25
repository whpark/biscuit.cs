//=============================================================================
// 
// CoordTrans (Transforms points to points)
//      Affine Transform, Mesh Transform ...
// PWH. 2021.08.06. ==> C#...
//
//

using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Text;

using CV = OpenCvSharp;

namespace Biscuit {

	public abstract class ICoordTrans {
		/// <summary>
		/// Transforms point to new coordinate
		/// </summary>
		/// <param name="pt"></param>
		/// <returns></returns>
		public abstract CV.Point2d Trans(CV.Point2d pt);

		/// <summary>
		/// Inverse Transforms point
		/// </summary>
		/// <param name="pt"></param>
		/// <returns></returns>
		public abstract CV.Point2d TransI(CV.Point2d pt);


		public bool PtInRect(CV.Point2d ptLeftBottom, CV.Point2d ptRightTop, CV.Point2d pt) {
			return (pt.X >= ptLeftBottom.X) && (pt.X <= ptRightTop.X)
				&& (pt.Y >= ptLeftBottom.Y) && (pt.Y <= ptRightTop.Y);
		}

		/// <summary>
		/// Transforms pt
		/// </summary>
		/// <param name="ptsSrc">source [2,2] enclosing Rectangle.</param>
		/// <param name="ptsDest">dest [2,2] enclosing Rectangle.</param>
		/// <param name="pt">Point</param>
		/// <returns>Transformed pt</returns>
		protected static CV.Point2d Transform2dLinear(CV.Point2d[,] ptsSrc, CV.Point2d[,] ptsDest, CV.Point2d pt) {
			Func<CV.Point2d, double> TERM1 = p => p.X;
			Func<CV.Point2d, double> TERM2 = p => p.Y;
			Func<CV.Point2d, double> TERM3 = p => p.X * p.Y;
			Func<CV.Point2d, double> TERM4 = p => 1.0;

			CV.Point2d[] pts = new CV.Point2d[4];
			pts[0] = ptsSrc[0, 0];
			pts[1] = ptsSrc[1, 0];
			pts[2] = ptsSrc[1, 1];
			pts[3] = ptsSrc[0, 1];

			CV.Mat m2 = new CV.Mat(4, 4, CV.MatType.CV_64FC1);
			var m2_ = m2.GetGenericIndexer<double>();
			for (int i = 0; i < 4; i++) {
				m2_[i, 0] = TERM1(pts[i]);
				m2_[i, 1] = TERM2(pts[i]);
				m2_[i, 2] = TERM3(pts[i]);
				m2_[i, 3] = TERM4(pts[i]);
			}

			CV.Mat m2i = m2.Inv();
			if (m2i == null || CV.Cv2.Determinant(m2i) == 0.0)
				throw new Exception("Wrong xMatrix");

			CV.Point2d ptTrans = pt;
			CV.Mat m1 = CV.Mat.Zeros(4, 1, CV.MatType.CV_64FC1);
			var m1_ = m1.GetGenericIndexer<double>();

			// x
			m1_[0, 0] = ptsDest[0, 0].X;
			m1_[1, 0] = ptsDest[1, 0].X;
			m1_[2, 0] = ptsDest[1, 1].X;
			m1_[3, 0] = ptsDest[0, 1].X;

			CV.Mat m3;
			m3 = m2i * m1;
			var m3_ = m3.GetGenericIndexer<double>();
			ptTrans.X = m3_[0, 0] * TERM1(pt) + m3_[1, 0] * TERM2(pt) + m3_[2, 0] * TERM3(pt) + m3_[3, 0] * TERM4(pt);

			m1_[0, 0] = ptsDest[0, 0].Y;
			m1_[1, 0] = ptsDest[1, 0].Y;
			m1_[2, 0] = ptsDest[1, 1].Y;
			m1_[3, 0] = ptsDest[0, 1].Y;

			m3 = m2i * m1;
			m3_ = m3.GetGenericIndexer<double>();
			ptTrans.Y = m3_[0, 0] * TERM1(pt) + m3_[1, 0] * TERM2(pt) + m3_[2, 0] * TERM3(pt) + m3_[3, 0] * TERM4(pt);

			return ptTrans;
		}
	}

	public class xCoordTransScaleShift : ICoordTrans {
		// ptNew = m_mat(pt - ptShift) + ptOffset;

		public double m_scale = 1.0;
		public CV.Point2d m_origin;
		public CV.Point2d m_offset;

		public xCoordTransScaleShift() {
			m_scale = 1.0;
			m_origin = new CV.Point2d(0, 0);
			m_offset = new CV.Point2d(0, 0);
		}

		public override CV.Point2d Trans(CV.Point2d pt) {
			return new CV.Point2d(m_scale * (pt.X - m_origin.X) + m_offset.X, m_scale * (pt.Y - m_origin.Y) + m_offset.Y);
		}
		public override CV.Point2d TransI(CV.Point2d pt) {
			if (m_scale == 0.0)
				throw new Exception("No Inverse");

			return new CV.Point2d((pt.X - m_offset.X) / m_scale + m_origin.X, (pt.Y - m_offset.Y) / m_scale + m_origin.Y);
		}
	}

	public class xCoordTrans2d : ICoordTrans {
		// ptNew = m_mat(pt - ptShift) + ptOffset;

		public CV.Mat m_mat;
		public CV.Point2d m_origin;
		public CV.Point2d m_offset;

		public xCoordTrans2d() {
			m_mat = new CV.Mat(2, 2, CV.MatType.CV_64FC1);
			var m = m_mat.GetGenericIndexer<double>();
			m[0, 0] = 1.0;
			m[0, 1] = 0.0;
			m[1, 0] = 0.0;
			m[1, 1] = 1.0;

			m_origin = new CV.Point2d(0, 0);
			m_offset = new CV.Point2d(0, 0);
		}
		public xCoordTrans2d(double dAngleRad) {
			m_mat = GetRotationMatrix(dAngleRad);
			m_origin = new CV.Point2d(0, 0);
			m_offset = new CV.Point2d(0, 0);
		}

		public static CV.Mat GetRotationMatrix(double dAngleRad) {
			var mat = new CV.Mat(2, 2, CV.MatType.CV_64FC1);
			double c = Math.Cos(dAngleRad);
			double s = Math.Sin(dAngleRad);
			var m = mat.GetGenericIndexer<double>();
			m[0, 0] = c;
			m[0, 1] = -s;
			m[1, 0] = s;
			m[1, 1] = c;
			return mat;
		}
		public xCoordTrans2d(xCoordTrans2d B) {
			m_mat = new CV.Mat(B.m_mat);
			m_origin = B.m_offset;
			m_offset = B.m_origin;
		}

		public override CV.Point2d Trans(CV.Point2d pt) {
			CV.Point2d pt2 = pt - m_origin;
			var m = m_mat.GetGenericIndexer<double>();
			return new CV.Point2d(m[0, 0] * pt2.X + m[0, 1] * pt2.Y + m_offset.X,
				m[1, 0] * pt2.X + m[1, 1] * pt2.Y + m_offset.Y);
		}
		public override CV.Point2d TransI(CV.Point2d pt) {
			bool bOK = false;
			CV.Mat mat = m_mat.Inv();// (0.0, ref bOK);
			if (mat == null || CV.Cv2.Determinant(mat) == 0.0)
				throw new Exception("No Inverse xMatrix.");

			CV.Point2d pt2 = pt - m_offset;
			var m = mat.GetGenericIndexer<double>();
			return new CV.Point2d(m[0, 0] * pt2.X + m[0, 1] * pt2.Y + m_origin.X,
				m[1, 0] * pt2.X + m[1, 1] * pt2.Y + m_origin.Y);
		}
	}

	public class xCoordTrans2dMesh : ICoordTrans {
		private xMeshTable m_src;
		private xMeshTable m_dest;

		public xCoordTrans2dMesh(xMeshTable src, xMeshTable dest) {
			if (src.rows != dest.rows || src.cols != dest.cols)
				throw new Exception("src.size != dest.size");
			m_src = new xMeshTable(src);
			m_dest = new xMeshTable(dest);
		}

		public override CV.Point2d Trans(CV.Point2d pt) {
			try {
				pt = Trans(m_src, m_dest, pt);
			}
			catch { }
			return pt;
		}

		public override CV.Point2d TransI(CV.Point2d pt) {
			return Trans(m_dest, m_src, pt);
		}

		public bool IsInBound_Trans(CV.Point2d pt) {
			int iy = 0, ix = 0;
			return m_src.FindEnclosingPTS(pt, ref iy, ref ix);
		}

		public bool IsInBound_Transl(CV.Point2d pt) {
			int iy = 0, ix = 0;
			return m_dest.FindEnclosingPTS(pt, ref iy, ref ix);
		}

		protected static CV.Point2d Trans(xMeshTable src, xMeshTable dest, CV.Point2d pt) {
			if ((src == null) || (dest == null))
				return pt;

			int iy = 0, ix = 0;
			if (!src.FindEnclosingPTS(pt, ref iy, ref ix)) {
				throw new Exception("OutBound");
				//return pt;
			}

			CV.Point2d[,] ptsSrc = new CV.Point2d[2, 2];
			CV.Point2d[,] ptsDest = new CV.Point2d[2, 2];
			ptsSrc[0, 0] = src.At(iy-1, ix-1);
			ptsSrc[0, 1] = src.At(iy-1, ix);
			ptsSrc[1, 0] = src.At(iy, ix-1);
			ptsSrc[1, 1] = src.At(iy, ix);
			ptsDest[0, 0] = dest.At(iy-1, ix-1);
			ptsDest[0, 1] = dest.At(iy-1, ix);
			ptsDest[1, 0] = dest.At(iy, ix-1);
			ptsDest[1, 1] = dest.At(iy, ix);
			return Transform2dLinear(ptsSrc, ptsDest, pt);
		}
	}

}
