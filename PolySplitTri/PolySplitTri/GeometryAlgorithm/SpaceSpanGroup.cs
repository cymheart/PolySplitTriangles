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
    public struct SpaceSpan
    {
        public Vector3d[] rect;
        public double startPos;
        public double endPos;
    }

    /// <summary>
    /// 空间跨距组
    /// </summary>
    public class SpaceSpanGroup
    {
        VoxSpace voxSpace;
        Dictionary<int, List<SpaceSpan>> spaceSpanDict = new Dictionary<int, List<SpaceSpan>>();

        public SpaceSpanGroup(VoxSpace voxSpace)
        {
            this.voxSpace = voxSpace;
        }

        public void CreateSpaceSpanGroup(SolidSpanGroup solidSpanGroup)
        {
            Dictionary<int, LinkedList<SolidSpan>> soildSpanDict = solidSpanGroup.soildSpanDict;
            LinkedList<SolidSpan> solidSpanList;
            SpaceSpan spaceSpan;
            int[] cellIdxs;
            Vector3d[] voxRect;
            List<SpaceSpan> spaceSpanList;

            foreach (var item in soildSpanDict)
            {
                cellIdxs = solidSpanGroup.GetCellIdxs(item.Key);
                voxRect = voxSpace.GetFloorGridCellRect(cellIdxs[0], cellIdxs[1]);

                solidSpanList = item.Value;

                var node = solidSpanList.First;
                double spaceEndPos = -1;

                spaceSpanList = new List<SpaceSpan>();

                for (; node != null; node = node.Next)
                {
                    if (node.Next != null)
                        spaceEndPos = node.Next.Value.startPos;
                    else
                        spaceEndPos = node.Value.endPos + 100000;

                    spaceSpan = new SpaceSpan()
                    {
                        rect = voxRect,
                        startPos = node.Value.endPos,
                        endPos = spaceEndPos
                    };

                    spaceSpanList.Add(spaceSpan);
                }

                spaceSpanDict[item.Key] = spaceSpanList;
            }
        }


    }
}
