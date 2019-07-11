using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Geometry_Algorithm
{

    public class VoxHeightFieldInfo
    {
        Dictionary<int, LinkedList<VoxHeightSpan>> heightSpanDict = new Dictionary<int, LinkedList<VoxHeightSpan>>();

        public void AddVoxBox(VoxBox voxBox)
        {
            int key = GetKey(voxBox.floorCellIdxX, voxBox.floorCellIdxZ);
            LinkedList<VoxHeightSpan> cellSpanList;

            if (heightSpanDict.TryGetValue(key, out cellSpanList) == false)
            {
                cellSpanList = new LinkedList<VoxHeightSpan>();
                heightSpanDict[key] = cellSpanList;
            }

            AppendVoxBoxToSpanHeightList(cellSpanList, voxBox);
        }


        void AppendVoxBoxToSpanHeightList(LinkedList<VoxHeightSpan> cellSpanList, VoxBox voxBox)
        {
            int voxStartIdx = voxBox.heightCellStartIdx;
            int voxEndIdx = voxBox.heightCellStartIdx;
            float yPosStart = voxBox.yPosRange[0];
            float yPosEnd = voxBox.yPosRange[1];    

            LinkedListNode<VoxHeightSpan> startNode = null;
            LinkedListNode<VoxHeightSpan> endNode = null;

            var node = cellSpanList.First;
            for (; node != null; node = node.Next)
            {
                if(node.Value.startCellIdx > voxStartIdx && startNode == null)
                {
                    startNode = node;
                }
                else if (voxStartIdx >= node.Value.startCellIdx  &&
                    voxStartIdx <= node.Value.endCellIdx)
                {
                    yPosStart = node.Value.startPos;
                    voxStartIdx = node.Value.startCellIdx;
                    startNode = node;
                }

                if (node.Value.startCellIdx > voxEndIdx && endNode == null)
                {
                    endNode = node.Previous;
                    break;
                }
                else if (voxEndIdx >= node.Value.startCellIdx && 
                    voxEndIdx <= node.Value.endCellIdx)
                {
                    yPosEnd = node.Value.endPos;
                    voxEndIdx = node.Value.endCellIdx;
                    endNode = node;
                    break;
                }
            }

            if(startNode != null && endNode == null)
                endNode = cellSpanList.Last;

            VoxHeightSpan voxSpan = new VoxHeightSpan()
            {
                startPos = yPosStart,
                endPos = yPosEnd,
                startCellIdx = voxStartIdx,
                endCellIdx = voxEndIdx
            };

            if (startNode == null && endNode == null)
            {
                if(node == cellSpanList.First)
                    cellSpanList.AddFirst(voxSpan);
                else
                    cellSpanList.AddLast(voxSpan);
            }     
            else
            {
                var prevNode = startNode.Previous;
                var mnode = startNode;
                LinkedListNode<VoxHeightSpan> tmpNode;
                bool flag = true;

                while(flag)
                {
                    if (mnode == endNode)
                        flag = false;

                    tmpNode = mnode.Next;
                    cellSpanList.Remove(mnode);
                    mnode = tmpNode;
                }

                if(prevNode == null)
                    cellSpanList.AddFirst(voxSpan);
                else
                    cellSpanList.AddAfter(prevNode, voxSpan);
            }
        }



        int GetKey(int cellx, int cellz)
        {
            return (cellx << 14) | cellz; 
        }

    }
}
