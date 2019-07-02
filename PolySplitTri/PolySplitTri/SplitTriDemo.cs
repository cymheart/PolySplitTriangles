﻿using Geometry_Algorithm;
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
        GeometryAlgorithm geoAlgor = new GeometryAlgorithm();
        Graphics g;
        Pen pen = new Pen(Color.Red);
        int state = 0;
        int inPointIdx = -1;
        int opMode = 0;

        List<Point> ptList = new List<Point>();
        Point movePoint = new Point(0,0);
        List<Vector3d[]> tris = null;
        List<Color> trisFillColor = new List<Color>();

        public SplitTriDemo()
        {
            InitializeComponent();
            polySplitTris = new PolySplitTriangles(geoAlgor);

            this.DoubleBuffered = true;

        }

        private void canvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (state == 1)
            {
                Point mousePt = GetMousePos();
                inPointIdx = InRegionIdx(mousePt);

                if (inPointIdx != -1)
                    opMode = 1;
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                Point pt = GetMousePos();
                ptList.Add(pt);
                canvas.Refresh();
            }
            else
            {
                state = 1;
            }

        }

        private void canvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (opMode == 1)
                opMode = 2;
            else
                opMode = 0;
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (opMode == 0)
            {
                movePoint = GetMousePos();
                canvas.Refresh();
            }
            else if(opMode == 1)
            {
                ptList[inPointIdx] = GetMousePos();

                if(tris != null)
                    CreateSplitTris();

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

            Point pt;
            for(int i=1; i<ptList.Count; i++)
            {
                pt = ptList[i];
                g.DrawLine(Pens.DarkBlue, pt, ptList[i - 1]);
            }


            //
            switch(state)
            {
                case 0:
                    {
                        if (ptList.Count > 0)
                        {
                            Point lastpt = ptList[ptList.Count - 1];
                            g.DrawLine(Pens.DarkBlue, lastpt, movePoint);
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
            for (int i = 0; i < ptList.Count; i++)
            {
                pt = ptList[i];
                int w = 10, h = 10;
                int x = pt.X - w / 2;
                int y = pt.Y - h / 2;
                Rectangle rect = new Rectangle(x, y, w, h);
                g.FillEllipse(Brushes.Red, rect);
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
            ptList.Clear();
            state = 0;
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

            for (int i = 0; i < ptList.Count; i++)
            {
                Vector3d vert = new Vector3d(ptList[i].X, 0, ptList[i].Y);
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

                poly = geoAlgor.CreatePoly(tmpVerts, Vector3d.down);

            }
            else
            {
                poly = geoAlgor.CreatePoly(vertList.ToArray(), Vector3d.down);
            }

            tris = polySplitTris.Split(poly);

            for (int i = 0; i < tris.Count; i++)
            {
                Color color = CreateRandomColor();
                trisFillColor.Add(color);
            }
        }
    }
}
