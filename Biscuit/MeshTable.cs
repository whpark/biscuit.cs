//=============================================================================
// 
// Mesh Table
// PWH. 2021.08.06. ==> C#...
//
//

using System;
using System.Collections.Generic;
using System.Text;

namespace Biscuit {
    public class xMeshTable {
        public xPoint2d[,] m_pts;
        public int rows => m_pts.GetLength(0);
        public int cols => m_pts.GetLength(1);
        public xMeshTable(int rows, int cols) {
            m_pts = new xPoint2d[rows, cols];
        }
        public xMeshTable(int rows, int cols, xPoint2d value) {
            m_pts = new xPoint2d[rows, cols];
            for (int y = 0; y < rows; y++) {
                for (int x = 0; x < cols; x++)
                    m_pts[y, x] = value;
            }
        }

        public xMeshTable(xMeshTable m) {
            m_pts = m.m_pts.Clone() as xPoint2d[,];
        }

        public ref xPoint2d At(int row, int col) {
            return ref m_pts[row, col];
        }

        public xMeshTable SetAll(xPoint2d pt) {
            for (int y = 0; y < rows; y++) {
                for (int x = 0; x < cols; x++)
                    At(y, x) = pt;
            }
            return this;
        }

        public xMeshTable ForAll(Func<xPoint2d[,], int, int, xPoint2d> func) {
            for (int y = 0; y < rows; y++) {
                for (int x = 0; x < cols; x++) {
                    m_pts[y, x] = func(m_pts, y, x);
                }
            }
            return this;
        }

        public bool FindEnclosingPTS(xPoint2d pt, ref int iy, ref int ix) {
            iy = -1;
            ix = -1;
            for (int y = 0; y < rows; y++) {
                if (pt.y > m_pts[y, 0].y)
                    continue;
                iy = y;
                for (int x = 0; x < cols; x++) {
                    if (pt.x > m_pts[y, x].x)
                        continue;
                    ix = x;
                    break;
                }
                break;
            }

            if ((iy < 1) || (ix < 1) || (iy >= rows) || (ix >= cols))
                return false;

            return true;
        }

    }
}
