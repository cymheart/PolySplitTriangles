using Geometry_Algorithm;
using Mathd;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PolySplitTri
{

    public partial class SplitTriDemo : Form
    {
        PolySplitTriangles polySplitTris;
        PolyConvertToSimplePoly convSimplePoly;
        GeometryAlgorithm geoAlgor = new GeometryAlgorithm();
        Graphics g;
        Pen pen = new Pen(Color.Red);
        int state = 1;
        int inPointIdx = -1;
        int opMode = 0;

        List<Point> ptList = new List<Point>();
        List<List<Point>> ptLists = new List<List<Point>>();

        Point? movePoint = new Point(0,0);
        List<Vector3d[]> tris = null;
        List<Color> trisFillColor = new List<Color>();



        public SplitTriDemo()
        {
            InitializeComponent();
            polySplitTris = new PolySplitTriangles(geoAlgor);
            convSimplePoly = new PolyConvertToSimplePoly(geoAlgor); 

            this.DoubleBuffered = true;

        }

        //private void canvas_MouseDown(object sender, MouseEventArgs e)
        //{
        //    if (state == 1)
        //    {
        //        Point mousePt = GetMousePos();
        //        inPointIdx = InRegionIdx(mousePt);

        //        if (inPointIdx != -1)
        //            opMode = 1;
        //        return;
        //    }

        //    if (e.Button == MouseButtons.Left)
        //    {
        //        Point pt = GetMousePos();
        //        ptList.Add(pt);
        //        canvas.Refresh();
        //    }
        //    else
        //    {
        //        state = 1;
        //    }

        //}

        //private void canvas_MouseUp(object sender, MouseEventArgs e)
        //{
        //    if (opMode == 1)
        //        opMode = 2;
        //    else
        //        opMode = 0;
        //}

        //private void canvas_MouseMove(object sender, MouseEventArgs e)
        //{
        //    if (opMode == 0)
        //    {
        //        movePoint = GetMousePos();
        //        canvas.Refresh();
        //    }
        //    else if (opMode == 1)
        //    {
        //        ptList[inPointIdx] = GetMousePos();

        //        if (tris != null)
        //            CreateSplitTris();

        //        canvas.Refresh();
        //    }
        //}


        private void canvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                switch(state)
                {
                    case 0:
                        {
                            Point pt = GetMousePos();    
                            ptList.Add(pt);
                            canvas.Refresh();
                        }
                        break;

                    case 1:
                        {
                            ptList = new List<Point>();
                            ptLists.Add(ptList);
                            Point pt = GetMousePos();
                            movePoint = pt;
                            ptList.Add(pt);         
                            state = 0;
                            canvas.Refresh();
                        }
                        break;
                }
            }
        }

        private void canvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (state == 0)
                {
                    state = 1;
                    movePoint = null;
                    canvas.Refresh();
                }
            }           
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if(state == 0)
            {
                movePoint = GetMousePos();
                canvas.Refresh();
            }
        }


        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.Clear(Color.White);

            if (tris != null)
            {
                Point[] pts = new Point[3];
                for (int i = 0; i < tris.Count; i++)
                {
                    for (int j = 0; j < tris[i].Length; j++)
                        pts[j] = new Point((int)tris[i][j].x, (int)tris[i][j].z);

                    Brush brush = new SolidBrush(trisFillColor[i]);
                    g.FillPolygon(brush, pts);
                }
            }

            //
            Point pt;
            for (int i = 0; i < ptLists.Count - 1; i++)
            {
                List<Point> tmpPtList = ptLists[i];
                for (int j = 1; j < tmpPtList.Count; j++)
                {
                    pt = tmpPtList[j];
                    g.DrawLine(Pens.DarkBlue, pt, tmpPtList[j - 1]);
                }

                if (tmpPtList.Count >= 3)
                {
                    Point lastpt = tmpPtList[tmpPtList.Count - 1];
                    g.DrawLine(Pens.DarkBlue, lastpt, tmpPtList[0]);
                }
            }

            for (int i = 1; i < ptList.Count; i++)
            {
                pt = ptList[i];
                g.DrawLine(Pens.DarkBlue, pt, ptList[i - 1]);
            }


            switch (state)
            {
                case 0:
                    {                  
                        if (ptList.Count > 0 && movePoint != null)
                        {                           
                            Point lastpt = ptList[ptList.Count - 1];
                            g.DrawLine(Pens.DarkBlue, lastpt, movePoint.Value);
                        }
                    }
                    break;

                case 1:
                    {
                        if (ptList.Count >= 3)
                        {
                            Point lastpt = ptList[ptList.Count - 1];
                            g.DrawLine(Pens.DarkBlue, lastpt, ptList[0]);
                        }
                    }
                    break;
            }


            //
            Font font = new Font("微软雅黑", 7);

            for (int i = 0; i < ptLists.Count; i++)
            {
                List<Point> tmpPtList = ptLists[i];
                for (int j = 0; j < tmpPtList.Count; j++)
                {
                    pt = tmpPtList[j];
                    int w = 10, h = 10;
                    int x = pt.X - w / 2;
                    int y = pt.Y - h / 2;
                    Rectangle rect = new Rectangle(x, y, w, h);
                    g.FillEllipse(Brushes.Red, rect);



                    g.DrawString("(" + pt.X + "," + pt.Y + ")", font, Brushes.DarkRed, pt);
                }
            }
        }

        int InRegionIdx(Point pt)
        {
            Rectangle rect;
            Point centerPt;

            for (int i = 0; i < ptList.Count; i++)
            {
                centerPt = ptList[i];
                int w = 10, h = 10;
                int x = centerPt.X - w / 2;
                int y = centerPt.Y - h / 2;
                rect = new Rectangle(x, y, w, h);

                if (rect.Contains(pt))
                {
                    return i;
                }
            }

            return -1;
        }

        Point GetMousePos()
        {
            Point pt = MousePosition;
            pt = canvas.PointToClient(pt);
            return pt;
        }

        Color CreateRandomColor()
        {
            Random rd = new Random(Guid.NewGuid().GetHashCode());
            int R = rd.Next(255);

            rd = new Random(Guid.NewGuid().GetHashCode());
            int G = rd.Next(255);

            rd = new Random(Guid.NewGuid().GetHashCode());
            int B = rd.Next(255);

            B = (R + G > 400) ? R + G - 400 : B;//0 : 380 - R - G;
            B = (B > 255) ? 255 : B;

            return Color.FromArgb(R, G, B);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ptLists.Clear();
            ptList.Clear();
            state = 1;
            opMode = 0;
            tris = null;
            canvas.Refresh();
        }

        private void btnSplit_Click(object sender, EventArgs e)
        {
            CreateSplitTris();
            canvas.Refresh();
        }

        void CreateSplitTris()
        {
            List<Vector3d> vertList = new List<Vector3d>();
            List<Point> tmpPtList = ptLists[0];
            for (int i = 0; i < tmpPtList.Count; i++)
            {
                Vector3d vert = new Vector3d(tmpPtList[i].X, 0, tmpPtList[i].Y);
                vertList.Add(vert);
            }

            trisFillColor.Clear();

            Poly poly = null;
            double val = geoAlgor.TestClockWise2D(vertList.ToArray());
            if (val < 0)
            {
                Vector3d[] tmpVerts = new Vector3d[vertList.Count];
                int j = 0;
                for (int i = vertList.Count - 1; i >= 0; i--)
                    tmpVerts[j++] = vertList[i];

                List<Vector3d[]> polyVertexsList = new List<Vector3d[]>();
                polyVertexsList.Add(tmpVerts);

                List<Point> ptList;
                for (int i = 1; i < ptLists.Count; i++)
                {
                    ptList = ptLists[i];
                    j = 0;
                    tmpVerts = new Vector3d[ptList.Count];
                    for (int k = ptList.Count - 1; k >= 0; k--)
                    {
                        tmpVerts[j++] = new Vector3d(ptList[k].X, 0, ptList[k].Y);
                    }

                    polyVertexsList.Add(tmpVerts);
                }

                poly = geoAlgor.CreatePoly(polyVertexsList, Vector3d.down);
            }
            else
            {
                List<Vector3d[]> polyVertexsList = new List<Vector3d[]>();
                polyVertexsList.Add(vertList.ToArray());

                List<Point> ptList;
                for (int i = 1; i < ptLists.Count; i++)
                {
                    ptList = ptLists[i];
                    Vector3d[] tmpVerts = new Vector3d[ptList.Count];
                    for (int j = 0; j < ptList.Count; j++)
                    {
                        tmpVerts[j] = new Vector3d(ptList[j].X, 0, ptList[j].Y);
                    }

                    polyVertexsList.Add(tmpVerts);
                }

                poly = geoAlgor.CreatePoly(polyVertexsList, Vector3d.down);
            }


            poly = convSimplePoly.ConvertToSimplePoly2D(poly);
          
            tris = polySplitTris.Split(poly);

            for (int i = 0; i < tris.Count; i++)
            {
                Color color = CreateRandomColor();
                trisFillColor.Add(color);
            }
        }

        private void btnPreSet_Click(object sender, EventArgs e)
        {
            Point[] pts0 = new Point[]
            {
                new Point(232,59),
                new Point(536,59),
                new Point(472,367),
                new Point(172,320),
            };

            Point[] pts1 = new Point[]
            {
                new Point(250,103),
                new Point(264,135),
                new Point(354,103),
            };

            Point[] pts2 = new Point[]
           {
                new Point(430,105),
                new Point(502,113),
                new Point(435,150),
           };

            Point[] pts3 = new Point[]
          {
                new Point(236,202),
                new Point(256,265),
                new Point(329,221),
          };

            ptList.Clear();
            ptLists.Clear();

            List<Point> tmpPtList = new List<Point>();
            for (int i=0; i<pts0.Length; i++)
            {
                tmpPtList.Add(pts0[i]);
            }
            ptLists.Add(tmpPtList);

            tmpPtList = new List<Point>();
            for (int i = 0; i < pts1.Length; i++)
            {
                tmpPtList.Add(pts1[i]);
            }
            ptLists.Add(tmpPtList);

            tmpPtList = new List<Point>();
            for (int i = 0; i < pts2.Length; i++)
            {
                tmpPtList.Add(pts2[i]);
            }
            ptLists.Add(tmpPtList);

            for (int i = 0; i < pts3.Length; i++)
            {
                ptList.Add(pts3[i]);
            }
            ptLists.Add(ptList);

            canvas.Refresh();
        }
    }
}
