using Mathd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geometry_Algorithm
{

    /// <summary>
    /// 空间跨距
    /// </summary>
    public class SpaceSpan
    {
        public Vector3d[] rect;
        public double startPos;
        public double endPos;
        public int type;

        public SpaceSpan[] connectSpan = 
            new SpaceSpan[] { null, null, null, null };
}

    /// <summary>
    /// 空间跨距组
    /// </summary>
    public class SpaceSpanGroup
    {
        /// <summary>
        /// 最小角色行走高度
        /// </summary>
        double minWalkHeight = 1f;

        /// <summary>
        /// 最小跨步高度
        /// </summary>
        double minStepHeight = 0.1f;

        /// <summary>
        /// 角色行走半径
        /// </summary>
        double walkRadius = 0.4f;
        int walkRadiusVoxCount = 0;

        static int[] relativeDirMap = { 2, 3, 0, 1 };

        VoxSpace voxSpace;
        Dictionary<int, List<SpaceSpan>> spaceSpanDict = new Dictionary<int, List<SpaceSpan>>();

        public SpaceSpanGroup(VoxSpace voxSpace)
        {
            this.voxSpace = voxSpace;
            walkRadiusVoxCount = CalWalkRadiusVoxCount();
        }


        int CalWalkRadiusVoxCount()
        {
            double n = walkRadius / voxSpace.cellSize;
            int count = (int)(Math.Ceiling(n));
            return count;
        }

        public void CreateSpaceSpanGroup(SolidSpanGroup solidSpanGroup)
        {
            _CreateSpaceSpanGroup(solidSpanGroup);
            CreateSpansConnectRelation();
        }


        public void _CreateSpaceSpanGroup(SolidSpanGroup solidSpanGroup)
        {
            Dictionary<int, LinkedList<SolidSpan>> soildSpanDict = solidSpanGroup.soildSpanDict;
            LinkedList<SolidSpan> solidSpanList;
            SpaceSpan spaceSpan;
            int[] cellIdxs;
            Vector3d[] voxRect;
            List<SpaceSpan> spaceSpanList;
            double startpos;
            double endpos;

            foreach (var item in soildSpanDict)
            {
                cellIdxs = solidSpanGroup.GetCellIdxs(item.Key);
                voxRect = voxSpace.GetFloorGridCellRect(cellIdxs[0], cellIdxs[1]);

                solidSpanList = item.Value;

                var node = solidSpanList.First;
                spaceSpanList = new List<SpaceSpan>();

                for (; node != null; node = node.Next)
                {
                    startpos = node.Value.endPos;
                    if (node.Next != null)
                        endpos = node.Next.Value.startPos;
                    else
                        endpos = startpos + 1000000;

                    if (endpos - startpos >= minWalkHeight)
                    {
                        spaceSpan = new SpaceSpan();
                        spaceSpan.rect = voxRect;
                        spaceSpan.startPos = startpos;
                        spaceSpan.endPos = endpos;
                        spaceSpan.connectSpan = new SpaceSpan[] { null, null, null, null };
                        spaceSpanList.Add(spaceSpan);
                    }
                }

                spaceSpanDict[item.Key] = spaceSpanList;
            }
        }

        /// <summary>
        /// 生成span间的连接关系
        /// </summary>
        void CreateSpansConnectRelation()
        {
            List<SpaceSpan> spaceSpanList;
            List<SpaceSpan> neiSpaceList;
            SpaceSpan span;
            SpaceSpan neiSpan;
            int[] spanIdx;
            int neiKey;
            bool isObstacleDir = false;
            double realStepHeight;
            double realWalkHeightA, realWalkHeightB;

            foreach (var item in spaceSpanDict)
            {
                spanIdx = GetCellIdxs(item.Key);
                spaceSpanList = item.Value;

                for (int neiDirIdx = 0; neiDirIdx < 4; neiDirIdx++)
                {          
                    neiKey = GetNeiKey(spanIdx, neiDirIdx);
                    if (neiKey != -1)
                        neiSpaceList = spaceSpanDict[neiKey];
                    else
                        neiSpaceList = null;

                    for (int i = 0; i < spaceSpanList.Count; i++)
                    {
                        span = spaceSpanList[i];
                        isObstacleDir = true;

                        if (span.connectSpan[neiDirIdx] != null)
                            continue;

                        for (int j = 0; neiSpaceList != null && j < neiSpaceList.Count; j++)
                        {
                            neiSpan = neiSpaceList[j];

                            //判断邻接span是否在当前检测span的高度范围内
                            if (neiSpan.startPos > span.endPos ||
                                neiSpan.endPos < span.startPos)
                                continue;


                            realStepHeight = Math.Abs(neiSpan.startPos - span.startPos);

                            if (realStepHeight > minStepHeight)
                            {
                                continue;
                            }
                            else
                            {
                                realWalkHeightA = neiSpan.endPos - span.startPos;
                                realWalkHeightB = span.endPos - neiSpan.startPos;

                                if (realWalkHeightA < minWalkHeight ||
                                    realWalkHeightB < minWalkHeight)
                                {
                                    continue;
                                }
                                else
                                {
                                    isObstacleDir = false;

                                    if (neiSpan.type == 0)
                                    {
                                        span.connectSpan[neiDirIdx] = neiSpan;
                                        neiSpan.connectSpan[relativeDirMap[neiDirIdx]] = span;
                                    }

                                    break;
                                }
                            }
                        }


                        //判断是否为此方向有障碍阻止移动
                        if (isObstacleDir)
                        {
                            SetNotWalkSpansByObstacleDir(spanIdx, span, neiDirIdx);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 根据障碍方向，和角色行走半径，给span的半径范围内不可移动的spans作标记
        /// </summary>
        /// <param name="spanIdx"></param>
        /// <param name="span"></param>
        /// <param name="obstacleDir"></param>
        void SetNotWalkSpansByObstacleDir(int[] spanIdx, SpaceSpan span, int obstacleDir)
        {
            int x, y;
            int n = 1;
            if (obstacleDir == 2 || obstacleDir == 3)
                n = -1;

            switch(obstacleDir)
            {
                case 0:
                case 2:
                    {
                        for (int i = 0; i < walkRadiusVoxCount; i++)
                        {
                            x = spanIdx[0] + i * n;
                            SetNotWalkSpans(x, spanIdx[1], span.startPos, span.endPos);

                            for (int j = 0; j <= i + 1; j++)
                            {
                                SetNotWalkSpans(x, spanIdx[1] - j, span.startPos, span.endPos);
                                SetNotWalkSpans(x, spanIdx[1] + j, span.startPos, span.endPos);
                            }
                        }

                    }
                    break;

                case 1:
                case 3:
                    {
                        for (int i = 0; i < walkRadiusVoxCount; i++)
                        {
                            y = spanIdx[0] + i * n;
                            SetNotWalkSpans(spanIdx[0], y, span.startPos, span.endPos);

                            for (int j = 1; j <= i + 1; j++)
                            {
                                SetNotWalkSpans(spanIdx[0] - j, y, span.startPos, span.endPos);
                                SetNotWalkSpans(spanIdx[0] + j, y, span.startPos, span.endPos);
                            }
                        }
                    }
                    break;
            }
        }

        void SetNotWalkSpans(
            int spanCellX, int spanCellZ, 
            double spanStartposY, double spanEndposY)
        {
            int key = GetKey(spanCellX, spanCellZ);
            List<SpaceSpan> spaceSpanList = spaceSpanDict[key];
            SpaceSpan span, neiSpan;

            for(int i=0; i<spaceSpanList.Count; i++)
            {
                span = spaceSpanList[i];

                if(span.type == 1)
                    continue;

                if (span.startPos > spanEndposY ||            
                    span.endPos < spanStartposY)
                    continue;

                span.type = 1;

                for (int j = 0; j < 4; j++)
                {
                    neiSpan = span.connectSpan[j];
                    span.connectSpan[j] = null;
                    neiSpan.connectSpan[relativeDirMap[j]] = null;
                }
            }
        }
        
        int GetRelativeDir(int dir)
        {
            return relativeDirMap[dir];
        }

        int GetNeiKey(int[] spanIdx, int dirIdx)
        {
            switch(dirIdx)
            {
                case 0: return GetKey(spanIdx[0] - 1, spanIdx[1]);
                case 1: return GetKey(spanIdx[0], spanIdx[1] + 1);
                case 2: return GetKey(spanIdx[0] + 1, spanIdx[1]);
                case 3: return GetKey(spanIdx[0], spanIdx[1] - 1);

            }

            return -1;
        }

        public int GetKey(int cellx, int cellz)
        {
            if (cellx < 0 || cellz < 0)
                return -1;

            return (cellx << 14) | cellz;
        }

        public int[] GetCellIdxs(int key)
        {
            int[] idxs = new int[] { key >> 14, 0x3FFF & key };
            return idxs;
        }


    }
}
