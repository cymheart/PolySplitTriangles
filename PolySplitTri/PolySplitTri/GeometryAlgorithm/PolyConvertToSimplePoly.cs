using Mathd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geometry_Algorithm
{
    public class PolyConvertToSimplePoly
    {
        GeometryAlgorithm geoAlgor;

        public struct CrossSideInfo
        {
            public PolySide side;
            public int hoopIdx;
            public int sidesIdx;
            public double dist;
        }

        CrossSideInfoComparer cmp;

        public PolyConvertToSimplePoly(GeometryAlgorithm geoAlgor)
        {
            this.geoAlgor = geoAlgor;
            cmp = new CrossSideInfoComparer(this);
        }


        /// <summary>     
        /// 切割为简单多边形
        /// 1.如果当前多边形存在内环，从第一个内环m的第一个顶点开始，往外环第一个顶点作连线ab，执行算法第2步。
        ///   否则如果当前多边形不存在内环,结束整个算法.
        /// 2.记录线段ab和所有有与之有相交交点的边线，排序这些交点边线(以交点距离线段ab首端点距离排序)， 找到离端点a最近的交点边线cd
        ///   如果边线cd不存在，将直接以ab线段作为分割线，分割此内环m，形成新的去除这个内环的多边形作为当前多边形，然后回到第1步.
        ///   否则如果边线cd存在，并且就是外环边线，将ab连接线段的末端点修改为和这条边的首端点连接，重复执行第2步。
        ///   否则如果边线cd存在，并且是内环边线，将把ab连接线段首端点修改为这条内环边的首端点，重复执行第2步。  
        /// </summary>
        /// <param name="poly"></param>
        /// <returns></returns>
        public Poly ConvertToSimplePoly2D(Poly poly)
        {
            List<CrossSideInfo> crossSideInfoList = new List<CrossSideInfo>();
            List<Vector3d> resultPolyVertexList = new List<Vector3d>();
            List<PolySide> resultPolySideList = new List<PolySide>();



            if (poly.vertexsList.Count <= 1)
                return poly;

            int selectInHoopIdx = 1;
            int[] excludeInHoopSideIdx = new int[] { 0, poly.sidesList[selectInHoopIdx].Length - 1 };
            int startVertexIdx = 0;
            int endVertexIdx = 0;

            Vector3d startVert;
            Vector3d outVert = poly.vertexsList[0][0];

            startVert = poly.vertexsList[selectInHoopIdx][0];


            PolySide side = geoAlgor.CreatePolySide(startVert, outVert);
            int result;
            Vector3d pt;

            for (int i = 1; i < poly.sidesList.Count; i++)
            {
                PolySide[] sides = poly.sidesList[i];

                for (int j = 0; j < sides.Length; j++)
                {
                    if (selectInHoopIdx == i &&
                        (j == excludeInHoopSideIdx[0] ||
                         j == excludeInHoopSideIdx[1]))
                    {
                        continue;
                    }

                    result = geoAlgor.SolvePolySideCrossPoint2D(sides[j], side, out pt);
                    if (result == 1)
                    {
                        Vector3d distVect = pt - startVert;

                        CrossSideInfo crossSideInfo = new CrossSideInfo()
                        {
                            side = sides[j],
                            hoopIdx = i,
                            sidesIdx = j,
                            dist = distVect.sqrMagnitude
                        };

                        crossSideInfoList.Add(crossSideInfo);
                    }
                }
            }


            if (crossSideInfoList.Count > 0)
            {
                crossSideInfoList.Sort(cmp);

                if(crossSideInfoList[0].hoopIdx == 0)
                {

                }

            }
            else
            {
                poly = CreatePolyByRemoveRing(poly, selectInHoopIdx, startVertexIdx, endVertexIdx, side);
            }

            return null;


        }


        /// <summary>
        /// 生成去除一个内环后的多边形
        /// </summary>
        /// <param name="poly">原始多边形</param>
        /// <param name="ringIdx">内环在原始多边形中的编号</param>
        /// <param name="ringVertexIdx">内环分切点编号</param>
        /// <param name="outVertexIdx">外环分切点编号</param>
        /// <param name="endLinkSide">连接线段</param>
        /// <returns></returns>
        Poly CreatePolyByRemoveRing(
            Poly poly, 
            int ringIdx, 
            int ringSplitVertIdx, 
            int outSplitVertIdx,
            PolySide endLinkSide)
        {
            List<Vector3d> resultPolyVertexList = new List<Vector3d>();
            List<PolySide> resultPolySideList = new List<PolySide>();
            Vector3d outVert = poly.vertexsList[0][outSplitVertIdx];
            Vector3d startVert = poly.vertexsList[ringIdx][ringSplitVertIdx];

            for (int i = outSplitVertIdx; i < poly.vertexsList[0].Length; i++)
                resultPolyVertexList.Add(poly.vertexsList[0][i]);
            for (int i = 0; i <= outSplitVertIdx; i++)
                resultPolyVertexList.Add(poly.vertexsList[0][i]);

            for (int i = ringSplitVertIdx; i < poly.vertexsList[ringIdx].Length; i++)
                resultPolyVertexList.Add(poly.vertexsList[ringIdx][i]);
            for (int i = 0; i <= ringSplitVertIdx; i++)
                resultPolyVertexList.Add(poly.vertexsList[ringIdx][i]);


            //   
            for (int i = outSplitVertIdx; i < poly.sidesList[0].Length; i++)
                resultPolySideList.Add(poly.sidesList[0][i]);
            for (int i = 0; i <= outSplitVertIdx; i++)
                resultPolySideList.Add(poly.sidesList[0][i]);


            PolySide linkSide = geoAlgor.CreatePolySide(outVert, startVert);
            resultPolySideList.Add(linkSide);

            for (int i = ringSplitVertIdx; i < poly.sidesList[ringIdx].Length; i++)
                resultPolySideList.Add(poly.sidesList[ringIdx][i]);
            for (int i = 0; i <= ringSplitVertIdx; i++)
                resultPolySideList.Add(poly.sidesList[ringIdx][i]);

            resultPolySideList.Add(endLinkSide);


            //
            Poly resultPoly = new Poly();
            resultPoly.sidesList.Add(resultPolySideList.ToArray());

            for (int i = 1; i < poly.sidesList.Count; i++)
            {
                if (i == ringIdx)
                    continue;

                resultPoly.sidesList.Add(poly.sidesList[i]);
            }


            //
            resultPoly.vertexsList.Add(resultPolyVertexList.ToArray());

            for (int i = 1; i < poly.vertexsList.Count; i++)
            {
                if (i == ringIdx)
                    continue;

                resultPoly.vertexsList.Add(poly.vertexsList[i]);
            }

            return resultPoly;
        }


        class CrossSideInfoComparer : IComparer<CrossSideInfo>
        {
            GeometryAlgorithm geoAlgor;
            public CrossSideInfoComparer(PolyConvertToSimplePoly convert)
            {
                geoAlgor = convert.geoAlgor;
            }
            public int Compare(CrossSideInfo left, CrossSideInfo right)
            {
                if (geoAlgor.IsZero(left.dist - right.dist))
                    return 0;
                else if (left.dist > right.dist)
                    return 1;
                else
                    return -1;
            }
        }
    }

}
