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
        public abstract xPoint2d Trans(xPoint2d pt);

        /// <summary>
        /// Inverse Transforms point
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public abstract xPoint2d TransI(xPoint2d pt);


        public bool PtInRect(xPoint2d ptLeftBottom, xPoint2d ptRightTop, xPoint2d pt) {
            return (pt.x >= ptLeftBottom.x) && (pt.x <= ptRightTop.x)
                && (pt.y >= ptLeftBottom.y) && (pt.y <= ptRightTop.y);
        }

        /// <summary>
        /// Transforms pt
        /// </summary>
        /// <param name="ptsSrc">source [2,2] enclosing Rectangle.</param>
        /// <param name="ptsDest">dest [2,2] enclosing Rectangle.</param>
        /// <param name="pt">Point</param>
        /// <returns>Transformed pt</returns>
        protected static xPoint2d Transform2dLinear(xPoint2d[,] ptsSrc, xPoint2d[,] ptsDest, xPoint2d pt) {
            Func<xPoint2d, double> TERM1 = p => p.x;
            Func<xPoint2d, double> TERM2 = p => p.y;
            Func<xPoint2d, double> TERM3 = p => p.x * p.y;
            Func<xPoint2d, double> TERM4 = p => 1.0;

            xPoint2d[] pts = new xPoint2d[4];
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

            xPoint2d ptTrans = new xPoint2d(pt);
            CV.Mat m1 = CV.Mat.Zeros(4, 1, CV.MatType.CV_64FC1);
            var m1_ = m1.GetGenericIndexer<double>();

            // x
            m1_[0, 0] = ptsDest[0, 0].x;
            m1_[1, 0] = ptsDest[1, 0].x;
            m1_[2, 0] = ptsDest[1, 1].x;
            m1_[3, 0] = ptsDest[0, 1].x;

            CV.Mat m3;
            m3 = m2i * m1;
            var m3_ = m3.GetGenericIndexer<double>();
            ptTrans.x = m3_[0, 0] * TERM1(pt) + m3_[1, 0] * TERM2(pt) + m3_[2, 0] * TERM3(pt) + m3_[3, 0] * TERM4(pt);

            m1_[0, 0] = ptsDest[0, 0].y;
            m1_[1, 0] = ptsDest[1, 0].y;
            m1_[2, 0] = ptsDest[1, 1].y;
            m1_[3, 0] = ptsDest[0, 1].y;

            m3 = m2i * m1;
            m3_ = m3.GetGenericIndexer<double>();
            ptTrans.y = m3_[0, 0] * TERM1(pt) + m3_[1, 0] * TERM2(pt) + m3_[2, 0] * TERM3(pt) + m3_[3, 0] * TERM4(pt);

            return ptTrans;
        }
    }

    public class xCoordTrans2d : ICoordTrans {
        // ptNew = m_mat(pt - ptShift) + ptOffset;

        public CV.Mat<double> m_mat;
        public xPoint2d m_ptShift;
        public xPoint2d m_ptOffset;

        public xCoordTrans2d() {
            m_mat = new CV.Mat<double>(2, 2);
			var m = m_mat.GetIndexer();
			m[0, 0] = 1.0;
            m[0, 1] = 0.0;
            m[1, 0] = 0.0;
			m[1, 1] = 1.0;

			m_ptShift = new xPoint2d(0, 0);
            m_ptOffset = new xPoint2d(0, 0);
        }
        public xCoordTrans2d(double dAngleRad) {
            m_mat = GetRotationMatrix(dAngleRad);
            m_ptShift = new xPoint2d(0, 0);
            m_ptOffset = new xPoint2d(0, 0);
        }

        public static CV.Mat<double> GetRotationMatrix(double dAngleRad) {
            var mat = new CV.Mat<double>(2, 2);
            double c = Math.Cos(dAngleRad);
            double s = Math.Sin(dAngleRad);
            var m = mat.GetIndexer();
            m[0, 0] = c;
            m[0, 1] = -s;
            m[1, 0] = s;
            m[1, 1] = c;
            return mat;
        }

        public override xPoint2d Trans(xPoint2d pt) {
            xPoint2d pt2 = new xPoint2d(pt - m_ptShift);
            var m = m_mat.GetIndexer();
            return new xPoint2d(m[0, 0] * pt2.x + m[0, 1] * pt2.y + m_ptOffset.x,
                m[1, 0] * pt2.x + m[1, 1] * pt2.y + m_ptOffset.y);
        }
        public override xPoint2d TransI(xPoint2d pt) {
            bool bOK = false;
            CV.Mat mat = m_mat.Inv();// (0.0, ref bOK);
            if (mat == null || CV.Cv2.Determinant(mat) == 0.0)
                throw new Exception("No Inverse xMatrix.");

            xPoint2d pt2 = new xPoint2d(pt - m_ptOffset);
            var m = mat.GetGenericIndexer<double>();
            return new xPoint2d(m[0, 0] * pt2.x + m[0, 1] * pt2.y + m_ptShift.x,
				m[1, 0] * pt2.x + m[1, 1] * pt2.y + m_ptShift.y);
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

        public override xPoint2d Trans(xPoint2d pt) {
			try
			{
                pt = Trans(m_src, m_dest, pt);
			}
			catch {}
            return pt;
        }

        public override xPoint2d TransI(xPoint2d pt) {
            return Trans(m_dest, m_src, pt);
        }

        public bool IsInBound_Trans(xPoint2d pt)
        {
            int iy = 0, ix = 0;
            return m_src.FindEnclosingPTS(pt, ref iy, ref ix);            
        }

        public bool IsInBound_Transl(xPoint2d pt)
        {
            int iy = 0, ix = 0;
            return m_dest.FindEnclosingPTS(pt, ref iy, ref ix);
        }

        protected static xPoint2d Trans(xMeshTable src, xMeshTable dest, xPoint2d pt) {
            if ((src == null) || (dest == null))
                return pt;

            int iy = 0, ix = 0;
            if (!src.FindEnclosingPTS(pt, ref iy, ref ix))
			{
                throw new Exception("OutBound");
                //return pt;
			}

            xPoint2d[,] ptsSrc = new xPoint2d[2, 2];
            xPoint2d[,] ptsDest = new xPoint2d[2, 2];
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
