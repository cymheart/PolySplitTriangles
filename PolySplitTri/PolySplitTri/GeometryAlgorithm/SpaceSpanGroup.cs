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
        public int type;

        
    }

    /// <summary>
    /// 空间跨距组
    /// </summary>
    public class SpaceSpanGroup
    {
        double minWalkHeight = 0;

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
                spaceSpanList = new List<SpaceSpan>();

                for (; node != null; node = node.Next)
                {
                    spaceSpan = new SpaceSpan();
                    spaceSpan.rect = voxRect;
                    spaceSpan.startPos = node.Value.endPos;

                    if (node.Next != null)
                        spaceSpan.endPos = node.Next.Value.startPos;
                    else
                        spaceSpan.endPos = spaceSpan.startPos + 100000;

                    if (spaceSpan.endPos - spaceSpan.startPos < minWalkHeight)
                        spaceSpan.type = 1;
                    else
                        spaceSpan.type = 0;

                    spaceSpanList.Add(spaceSpan);
                }

                spaceSpanDict[item.Key] = spaceSpanList;
            }
        }


        void dd()
        {
            foreach (var item in spaceSpanDict)
            {

            }
        }


    }
}
