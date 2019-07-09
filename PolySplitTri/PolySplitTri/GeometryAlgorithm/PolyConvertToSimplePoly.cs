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
            /// <summary>
            /// 多边形边
            /// </summary>
            public PolySide side;

            /// <summary>
            /// 边所在环编号
            /// </summary>
            public int ringIdx;

            /// <summary>
            /// 边所在环的边编号位置
            /// </summary>
            public int sidesIdx;

            /// <summary>
            /// 此边与ab线段的交点与开始点a的距离
            /// </summary>
            public double dist;
        }

        public struct IgnoreSideInfo
        {
            public int ringIdx;
            public int sideIdx1;
            public int sideIdx2;
        }


        CrossSideInfoComparer cmp;

        public PolyConvertToSimplePoly(GeometryAlgorithm geoAlgor)
        {
            this.geoAlgor = geoAlgor;
            cmp = new CrossSideInfoComparer(this);
        }


        /// <summary>     
        /// 切割为简单多边形
        /// 1.如果当前多边形存在内环(ring)，从第一个内环m的第一个顶点开始，往外环第一个顶点作连线ab，执行算法第2步。
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
            //临时存放和线段ab相交边的信息
            List<CrossSideInfo> crossSideInfoList = new List<CrossSideInfo>();

            while (true)
            {
                //判断是否存在内环多边形,当不存在内环时,结束算法，返回最后生成的多边形
                if (poly.vertexsList.Count <= 1)
                    return poly;

                int selectInRingIdx = 1;
                int startVertexIdx = 0;
                int endVertexIdx = 0;

                //线段ab起始点
                Vector3d startVertA = poly.vertexsList[selectInRingIdx][0];

                //线段ab结束点
                Vector3d outVertB = poly.vertexsList[0][0];

                //ab线段
                PolySide sideAB = geoAlgor.CreatePolySide(startVertA, outVertB);
                int result;
                IgnoreSideInfo[] ignoreSideInfos = CreateIgnoreSideInfos(poly, new Vector3d[] { startVertA, outVertB });
                //交点
                Vector3d pt;

                while (true)
                {
                    for (int i = 0; i < poly.sidesList.Count; i++)
                    {
                        PolySide[] sides = poly.sidesList[i];

                        for (int j = 0; j < sides.Length; j++)
                        {
                            if (InIgnoreSides(i,j, ignoreSideInfos))
                                continue;
            
                            //求解ab线段和多边形边sides[j]的交点
                            result = geoAlgor.SolvePolySideCrossPoint2D(sides[j], sideAB, out pt);
                            if (result == 1)  //存在交点
                            {
                                Vector3d distVect = pt - startVertA;

                                CrossSideInfo crossSideInfo = new CrossSideInfo()
                                {
                                    side = sides[j],
                                    ringIdx = i,
                                    sidesIdx = j,
                                    dist = distVect.sqrMagnitude
                                };

                                crossSideInfoList.Add(crossSideInfo);
                            }
                        }
                    }

                    //存在和ab线段相交的边cd集合
                    if (crossSideInfoList.Count > 0)
                    {
                        crossSideInfoList.Sort(cmp);

                        //如果边线cd存在，并且就是外环边线，将ab连接线段的末端点修改为和这条外边的首端点连接
                        if (crossSideInfoList[0].ringIdx == 0)
                        {
                            //线段ab结束点修改为这条边的首端点
                            outVertB = crossSideInfoList[0].side.startpos;
                            endVertexIdx = crossSideInfoList[0].sidesIdx;

                            ignoreSideInfos = CreateIgnoreSideInfos(poly, new Vector3d[] { startVertA, outVertB });

                            //ab线段
                            sideAB = geoAlgor.CreatePolySide(startVertA, outVertB);
                        }

                        //否则如果边线cd存在，并且是内环边线，将把ab连接线段首端点修改为最远相交内环边的首端点
                        else
                        {
                            for (int i = crossSideInfoList.Count - 1; i >= 0; i--)
                            {
                                if (crossSideInfoList[i].ringIdx == 0)
                                    continue;

                                selectInRingIdx = crossSideInfoList[i].ringIdx;

                                //线段ab开始点修改为这条边的首端点
                                startVertA = crossSideInfoList[i].side.startpos;
                                startVertexIdx = crossSideInfoList[i].sidesIdx;

                                ignoreSideInfos = CreateIgnoreSideInfos(poly, new Vector3d[] { startVertA, outVertB });

                                //ab线段
                                sideAB = geoAlgor.CreatePolySide(startVertA, outVertB);
                            }
                        }

                        crossSideInfoList.Clear();
                    }
                    else
                    {
                        poly = CreatePolyByRemoveRing(poly, selectInRingIdx, startVertexIdx, endVertexIdx, sideAB);
                        break;
                    }
                }
            }
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
            for (int i = 0; i < outSplitVertIdx; i++)
                resultPolySideList.Add(poly.sidesList[0][i]);


            PolySide linkSide = geoAlgor.CreatePolySide(outVert, startVert);
            resultPolySideList.Add(linkSide);

            for (int i = ringSplitVertIdx; i < poly.sidesList[ringIdx].Length; i++)
                resultPolySideList.Add(poly.sidesList[ringIdx][i]);
            for (int i = 0; i < ringSplitVertIdx; i++)
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

            resultPoly.faceNormal = poly.faceNormal;
            return resultPoly;
        }


        IgnoreSideInfo[] CreateIgnoreSideInfos(Poly poly, Vector3d[] verts)
        {
            List<IgnoreSideInfo> ignoreSideInfos = new List<IgnoreSideInfo>();
            IgnoreSideInfo ignoreSideInfo;
            Vector3d[] vertexs;
            int j2;

            for (int n = 0; n < verts.Length; n++)
            {
                for (int i = 0; i < poly.vertexsList.Count; i++)
                {
                    vertexs = poly.vertexsList[i];
                    for (int j = 0; j < vertexs.Length; j++)
                    {
                        if (geoAlgor.IsEqual(vertexs[j], verts[n]) == false)
                            continue;

                        if (j == 0) { j2 = poly.sidesList[i].Length - 1; }
                        else { j2 = j - 1; }

                        ignoreSideInfo = new IgnoreSideInfo()
                        {
                            ringIdx = i,
                            sideIdx1 = j,
                            sideIdx2 = j2
                        };

                        ignoreSideInfos.Add(ignoreSideInfo);
                    }
                }
            }

            return ignoreSideInfos.ToArray();
        }


        /// <summary>
        /// 判断指定环边是否在忽略边列表中
        /// </summary>
        /// <param name="ringIdx"></param>
        /// <param name="sideIdx"></param>
        /// <param name="ignoreSideInfos"></param>
        /// <returns></returns>
        bool InIgnoreSides(int ringIdx, int sideIdx, IgnoreSideInfo[] ignoreSideInfos)
        {
            for(int i=0; i<ignoreSideInfos.Length; i++)
            {
                if (ringIdx == ignoreSideInfos[i].ringIdx &&
                    (sideIdx == ignoreSideInfos[i].sideIdx1 ||
                    sideIdx == ignoreSideInfos[i].sideIdx2))
                    return true;
            }

            return false;

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
