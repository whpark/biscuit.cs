//=============================================================================
// 
// CoordSystem (Point, Size, Rect ...)
// PWH. 2021.08.06. ==> C#...
//
//

using System;
using System.Collections.Generic;
using System.Text;

namespace Biscuit {

    public struct xPoint2d {
        public double x, y;
        public xPoint2d(double x_, double y_) => (x, y) = (x_, y_);
        public xPoint2d(xPoint2d pt) => (x, y) = (pt.x, pt.y);

        public override string ToString() => $"({x}, {y})";

        // 4칙연산 with double
        public static xPoint2d operator +(xPoint2d a) => a;
        public static xPoint2d operator -(xPoint2d a) {
            return new xPoint2d(-a.x, -a.y);
        }
        public static xPoint2d operator +(xPoint2d a, xPoint2d b) {
            return new xPoint2d(a.x + b.x, a.y + b.y);
        }
        public static xPoint2d operator -(xPoint2d a, xPoint2d b) {
            return new xPoint2d(a.x - b.x, a.y - b.y);
        }

        public static xPoint2d operator *(xPoint2d a, double b) {
            return new xPoint2d(a.x * b, a.y * b);
        }
        public static xPoint2d operator *(double b, xPoint2d a) {
            return new xPoint2d(b * a.x, b * a.y);
        }
        public static xPoint2d operator /(xPoint2d a, double b) {
            return new xPoint2d(a.x / b, a.y / b);
        }

        double[] ToArray() {
            return new double[2] { x, y };
        }

    }

    public struct xPoint3d {
        public double x, y, z;
        public xPoint3d(double x_, double y_, double z_) => (x, y, z) = (x_, y_, z_);
        public xPoint3d(xPoint3d pt) => (x, y, z) = (pt.x, pt.y, pt.z);

        public override string ToString() => $"({x}, {y}, {z})";

        // 4칙연산 with double
        public static xPoint3d operator +(xPoint3d a) => a;
        public static xPoint3d operator -(xPoint3d a) {
            return new xPoint3d(-a.x, -a.y, -a.z);
        }
        public static xPoint3d operator +(xPoint3d a, xPoint3d b) {
            return new xPoint3d(a.x + b.x, a.y + b.y, a.z + b.z);
        }
        public static xPoint3d operator -(xPoint3d a, xPoint3d b) {
            return new xPoint3d(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static xPoint3d operator *(xPoint3d a, double b) {
            return new xPoint3d(a.x * b, a.y * b, a.z * b);
        }
        public static xPoint3d operator *(double b, xPoint3d a) {
            return new xPoint3d(b * a.x, b * a.y, b * a.z);
        }
        public static xPoint3d operator /(xPoint3d a, double b) {
            return new xPoint3d(a.x / b, a.y / b, a.z / b);
        }

        double[] ToArray() {
            return new double[3] { x, y, z };
        }

    }
}
