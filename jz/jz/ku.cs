using Grasshopper2.Components;
using Grasshopper2.Extensions;
using Grasshopper2.Types.Numeric;
using Grasshopper2.UI;
using GrasshopperIO;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace jianjuchanshuhua
{
    class Zhuzi
    {
        public Point3d center;
        public double chang;
        public double kuan;
        public double gao;
        public Vector3d vector;

        public Zhuzi(Point3d center, double chang, double kuan, double gao)
        {
            this.center = center;
            this.chang = chang;
            this.kuan = kuan;
            this.gao = gao;
            this.vector = new Vector3d(center.X+1,center.Y,center.Z);
        }
        public Zhuzi(Point3d p0,Point3d p1, double chang, double kuan, double gao)
        {
            this.center = p0;
            this.chang = chang;
            this.kuan = kuan;
            this.gao = gao;
            this.vector = p0 - p1;
        }


        public Box ViewZhuzi()
        {
            Plane plane = new Plane(new Point3d(center.X, center.Y, center.Z), new Vector3d(0, 0, 1));

            Box box = new Box(plane, new Interval(-chang / 2, chang / 2), new Interval(-kuan / 2, kuan / 2), new Interval(0, gao));
            Point3d pnew = new Point3d(center.X + 1, center.Y, center.Z);
            Transform tran = Transform.Rotation(center-pnew ,vector, center);
            double radians = Vector3d.VectorAngle(center - pnew, center - pnew);
            Transform tran2 = Transform.Rotation(radians, center);
           
                box.Transform(tran);

          
            Transform mirr = Transform.Mirror(plane);
            Point3d pxuan = box.Center;
            if (pxuan.Z != center.Z + gao * 0.5)
            {
                box.Transform(mirr);
            }
            return box;
        }
       

    }
    class Wall
    {
        public Point3d p0;
        public Point3d p1;
        public double hight;
        public double d;
        public List<int> angle0;
        public List<int> angle1;
        public Window win;
        public bool havewin;
        public Wall(Point3d p0,Point3d p1,double hight,double d)
        {
            bool b = false;
            this.p0 = p0;
            this.p1 = p1;
            this.hight = hight;
            this.d = d;
            this.win = new Window();
            this.havewin = b;
        }
        public Wall()
        { }
        public void addwindow(Window winn)
        {
           win = winn;
           havewin = true;
        }
       public Box WallView()
        {
            Box box = new Box();
           
           Point3d center = new Point3d((p0.X + p1.X) * 0.5, (p0.X + p1.X) * 0.5, (p0.Z + p1.Z) * 0.5);
            Point3d pnew = new Point3d(p0.X, p0.Y, p0.Z + 1);
            Plane plane = new Plane(p0,pnew-p0);
            double l = new Line(p0, p1).Length;
            Point3d pchankao = new Point3d(p0.X+l, p0.Y, p0.Z);

            box = new Box(plane,new Interval(0, l),new Interval( -d * 0.5, d * 0.5), new Interval(0, hight));
            Transform tran = Transform.Rotation(p0 - pchankao, p0 - p1,  p0);
            double radians = Vector3d.VectorAngle(p0-pchankao, p0-p1);
            Transform tran2 = Transform.Rotation(radians, p0);
            if(radians==Math.PI)
            {
             box.Transform(tran2);

            }
            else
            {
             box.Transform (tran);
            }

                return box;
        }
        

    }
    class Anglewall
    {
        public Wall wall0;
        public Wall wall1;
        public List<Wall> awalls;
        public Point3d center;
        public List<Point3d> other;
        public Brep qjiao;
        public Brep view;
        public Anglewall(Wall wall0, Wall wall1)
        {
            Point3d p0 = wall0.p0;
            Point3d p1 = wall0.p1;
            Point3d p2 = wall1.p0;
            Point3d p3 = wall1.p1;
            if(p0==p2)
            {
                this.wall0 = new Wall(p0, p1, wall0.hight, wall0.d);
                this.wall1 = new Wall(p2, p3, wall1.hight, wall1.d);
                this.center = p0;
            }
            if(p0==p3)
            {
                this.wall0 = new Wall(p0, p1, wall0.hight, wall0.d);
                this.wall1 = new Wall(p3, p2, wall1.hight, wall1.d);
                this.center = p0;

            }
            if (p1==p2)
            {
                this.wall0 = new Wall(p1, p0, wall0.hight, wall0.d);
                this.wall1 = new Wall(p2, p3, wall1.hight, wall1.d);
                this.center = p1;

            }
            if (p1 == p3)
            {
                this.wall0 = new Wall(p1, p0, wall0.hight, wall0.d);
                this.wall1 = new Wall(p3, p2, wall1.hight, wall1.d);
                this.center = p1;

            }
            

            this.view = qiangjiao1(this.wall0,this.wall1);
        }
        public Anglewall(List<Wall>awalls)
        {
            List<Point3d> pts = new List<Point3d>();
            for (int i = 0; i < awalls.Count; i++)
            {
                pts.Add(awalls[i].p0);
                pts.Add(awalls[i].p1);
            }
            var center = thesameone(pts);
            List<Point3d> other = new List<Point3d>();
            for (int i = 0; i < awalls.Count; i++)
            {
                if (awalls[i].p0==center)
                {
                    other.Add(awalls[i].p1);
                }
                if(awalls[i].p0 != center)
                {
                    other.Add(awalls[i].p0);

                }
            }
            List<double> ds = new List<double>();
            double min = 0;
            Wall wall00 = new Wall();
            Wall wall11 = new Wall();

            for (int i = 0; i < awalls.Count; i++)
            {
                for (int j = 0; j < awalls.Count; j++)
                {
                    if (i != j)
                    {
                        var v1 =  other[i]-center;
                        var v2 = other[j] - center;

                        double angle = Vector3d.VectorAngle(v1, v2);
                        if(angle>min)
                        {
                            min = angle;
                            wall00 = awalls[i];
                            wall11 = awalls[j];
                           
                        }
                    }
                }



            }
            Point3d p0 = wall00.p0;
            Point3d p1 = wall00.p1;
            Point3d p2 = wall11.p0;
            Point3d p3 = wall11.p1;
            if (p0 == p2)
            {
                wall00 = new Wall(p0, p1, wall00.hight, wall00.d);
                wall11 = new Wall(p2, p3, wall11.hight, wall11.d);
               
            }
            if (p0 == p3)
            {
                wall00 = new Wall(p0, p1, wall00.hight, wall00.d);
                wall11 = new Wall(p3, p2, wall11.hight, wall11.d);
                

            }
            if (p1 == p2)
            {
                wall00 = new Wall(p1, p0, wall00.hight, wall00.d);
                wall11 = new Wall(p2, p3, wall11.hight, wall11.d);
                this.center = p1;

            }
            if (p1 == p3)
            {
                wall00 = new Wall(p1, p0, wall00.hight, wall00.d);
                wall11 = new Wall(p3, p2, wall11.hight, wall11.d);
                

            }

            var v = qiangjiao1(wall00, wall11);

            


            this.view = v;
            //
            this.center = center;
            this.other = other;
            this.wall0 = wall00;
            this.wall1 = wall11;
        }
        public bool pdangle(Wall wall0, Wall wall1)
        {
            bool b = false;
            if(wall0.p0==wall1.p0)
            {
                b = true;
            }
            if (wall0.p0 == wall1.p1)
            {
                b = true;
            }
            return b;
        }
        public Point3d thesameone(List<Point3d>pts)
        {
            Point3d p = new Point3d();
            for (int i = 0; i < pts.Count; i++)
            {
                for (int j = 0; j < pts.Count; j++)
                {
                    if(i!=j)
                    {
                        if (pts[i] == pts[j])
                        {
                            p = pts[j];
                        }
                    }
                }
            }
            return p;
        }



        //
        public Brep qiangjiao1(Wall w0,Wall w1)
        {
            var bs = new Brep();
            if(w0 ==null|| w1 ==null)
            {
                return bs;
            }
            List<object> view = new List<object> ();
            
            var p0 = w0.p0;
            var p1 = w0.p1;
            var p2 = w1.p1;
            var d0 = w0.d;
            var d1 = w1.d;
            var h = w0.hight;
           // List<Curve> A = new List<object>();
            //
            Line l0 = new Line(p0, p1);
            Line l1 = new Line(p0, p2);

            var clod0 = offictopolyline(l0, d0);
            var clod1 = offictopolyline(l1, d1);
            //
            var pts0 = curvepoint(clod0);
            var pts1 = curvepoint(clod1);
            var b0 = pdzbz(pts0, clod1, out var plei0);
            var b1 = pdzbz(pts1, clod0, out var plei1);
            var pm0 = new Point3d();
            var pm1 = new Point3d();
            var pjiao = new Point3d();
            List<Point3d> ptsbhe = new List<Point3d>();
            Curve jiao = null;
            Brep a = new Brep();
            Brep[] c = new Brep[1];
           
            if (b0 && b1)
            {
                Transform tran = Transform.Rotation(Math.PI, p0);
                plei0.Transform(tran);
                plei1.Transform(tran);
                Transform tran0 = Transform.Translation(p0 - p1);
                Transform tran1 = Transform.Translation(p0 - p2);
                pm0 = plei0;
                pm1 = plei1;
                pm0.Transform(tran0);
                pm1.Transform(tran1);
                pjiao = lineline(new Line(plei0, pm0), new Line(plei1, pm1));
                ptsbhe.Add(pjiao);
                ptsbhe.Add(plei0);
                ptsbhe.Add(p0);
                ptsbhe.Add(plei1);
                jiao = NurbsCurve.Create(true, 1, ptsbhe);
                //
                var s = NurbsSurface.CreateFromCorners(pjiao, plei0, p0, plei1).ToBrep();
                 c = Brep.CreatePlanarBreps(jiao, 0.001);
               
                var j = c[0];
                 a = Surface.CreateExtrusion(jiao,new Vector3d(0,0,h) ).ToBrep();
                Transform reanm = Transform.Translation(new Vector3d(0, 0, h));
                Transform reanm1 = Transform.Translation(new Vector3d(0, 0, 0));

                var B2 = j;
               // bs.Add(c[0]);
                B2.Transform(reanm);

                // bs.Add(a);
                // bs.Add(B2);
                
                Brep obj2 = a.CapPlanarHoles(0.001);
                bs = obj2;
                // qjiao = a;
            }
            view.Add(p0);
            view.Add(l0);
            view.Add(l1);

            view.Add(clod0);
            view.Add(clod1);
            view.Add(jiao);
            // Brep[] join = Brep.JoinBreps(bs, 0.001);
            

            return bs;
           
        }



        //
        public Line offic(Line C, double D)
        {
            Vector3d v0 = C.From - C.To;
            var v1 = v0;
            Transform tran1 = Transform.Rotation(Math.PI / 2 * D / Math.Abs(D),C.From);
            v1.Transform(tran1);
            // Compute the offset (might not be a single continuous curve)
            //   Curve[] offsets = C.ToNurbsCurve().Offset(Plane.WorldXY, D, 0.0001, (CurveOffsetCornerStyle)1);
            Transform tran = Transform.Translation(v1 / v1.Length * Math.Abs(D)*0.5);
            var l = C;
            l.Transform(tran);
            return l;

        }
        public Curve offictopolyline(Line c, double d)
        {
            var l1 = offic(c, d);
            var l2 = offic(c, -d);
            List<Point3d> pts = new List<Point3d>();
            pts.Add(l1.From);
            pts.Add(l1.To);
            pts.Add(l2.To);
            pts.Add(l2.From);

            var polyline = NurbsCurve.Create(true, 1, pts);
            return polyline.ToNurbsCurve();
        }
        public bool pdzbz(List<Point3d> pts, Curve C, out Point3d pout)
        {
            pout = new Point3d();
            bool b = false;
            int m = 0;
            for (int i = 0; i < pts.Count; i++)
            {
                if (pz(pts[i], C))
                {
                    m++;
                    pout = pts[i];
                }
            }
            if (m == 1)
            {
                b = true;

            }
            return b;
        }
        public bool pz(Point3d P, Curve C)
        {
            Plane pln = new Plane();
            bool xy = C.TryGetPlane(out pln);
            if (!xy) pln = Plane.WorldXY;
            //Plane.FitPlaneToPoints(pts, out pln);

            // Test for containment, need to specify plane
            // Will not match GH output unless defined plane of curve is correct
            PointContainment inside = C.Contains(P, pln, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

            // Change output value to match GH output
            int intValue = (int)inside;
            switch (intValue)
            {
                case 2:
                    intValue = 0;
                    break;
                case 1:
                    intValue = 2;
                    break;
                case 3:
                    intValue = 1;
                    break;
            }
            bool b = false;
            if (intValue == 2)
            {
                b = true;
            }
            return b;
        }
        public List<Point3d> curvepoint(Curve C)
        {
            List<Point3d> Points = new List<Point3d>();

            for (int i = 0, n = C.ToNurbsCurve().Points.Count; i < n; i++)
            {
                Point3d temp = new Point3d(C.ToNurbsCurve().Points[i].X, C.ToNurbsCurve().Points[i].Y, C.ToNurbsCurve().Points[i].Z);

                //Let's add this temp to our point list.
                Points.Add(temp);
            }
            Points.RemoveAt(0);
            return Points;
        }
        public Point3d lineline(Line A, Line B)
        {
            bool success = Rhino.Geometry.Intersect.Intersection.LineLine
           (A, B, out var paramA, out var paramB);


            var pointA = A.PointAt(paramA);
            return pointA;

        }
    }
    class Walls
    {
        public List<Wall> walls;
        public List<Anglewall> angles;
        public List<Point3d> anglepoints;
        public List<Point3d> cypoint;
        public Walls(List<Wall> walls)
        {
            List<Point3d> pds = new List<Point3d>();
            this.walls = walls;
            this.anglepoints = delsameone(allp(walls));
            this.angles = getangle(walls, delsameone(allp(walls)));
            for(int i=0;i<walls.Count;i++)
            {
                pds.Add(walls[i].p0);
                pds.Add(walls[i].p1);
            }
            this.cypoint = pds;
        }




        //
        public List<Point3d> allp(List<Wall> ws)
        {
            List<Point3d> pts = new List<Point3d>();
            for (int i = 0; i < ws.Count ; i++)
            {
                pts.Add(ws[i].p0);
                pts.Add(ws[i].p1);

            }
            return pts;
        }
        public List<Point3d> delsameone(List<Point3d>pts)
        {
            List<Point3d> dsame = new List<Point3d>();
            List<Point3d> view = new List<Point3d>();
            view = pts;

            for (int i = 0; i < pts.Count; i++)
            {
                int m = 0;
                if (dsameone(dsame, pts[i]) == false)
                {
                    for (int j = 0; j < pts.Count; j++)
                    {
                     
                        if (i != j && pts[i] == pts[j])
                        {
                            m++;
                        }
                        
                    }
                    if (m != 0)
                    {
                        dsame.Add(pts[i]);
                    }
                }
            }
            return dsame;

        }
        public bool dsameone(List<Point3d>pts,Point3d p)
        {
            int m = 0;
            for (int i = 0; i < pts.Count; i++)
            {
                if (pts[i]==p)
                {
                    m++;
                }
            }
            bool b = false;
            if(m!=0)
            {
                b = true;
            }
            return b;
        }
        public List<Anglewall>getangle(List<Wall>ws,List<Point3d>pts)
        {
            List<Anglewall> angs= new List<Anglewall>();
            for (int i = 0; i < pts.Count; i++)
            {
                List<Wall> wb = new List<Wall>();
                for (int j = 0; j < ws.Count; j++)
                {
                    if (ws[j].p0 == pts[i])
                    {
                        wb.Add(ws[j]);
                    }
                    if (ws[j].p1 == pts[i])
                    {
                        wb.Add(ws[j]);
                    }
                }
                if (wb.Count > 1)
                {
                    Anglewall aone = new Anglewall(wb);
                    angs.Add(aone);
                }
                
            }
            return angs;
        }
        public List<object> viewangles()
        {
            List<object> view = new List<object>();
            for (int i = 0; i < angles.Count; i++)
            {
                if (angles[i].view != null)
                {
                    Brep bso = angles[i].view;
                    
                        if (bso != null)
                        {
                            view.Add(bso);
                        }
                    
                }
            }
            return view;
        }

    }
   
    class   Room
    {
        public string name;
        public List<Line> wl;
        public Point3d center;
        public Walls ws;
        public Room(string name, List<Line> ls, Point3d center)
        {
            this.name = name;
            this.wl = ls;
            this.center = center;
        }
        public Room(string name, Walls ws, Point3d center)
        {
            this.name = name;
            List<Line> ls = new List<Line>();
            for (int i = 0; i < ws.walls.Count; i++)
            {
                var l= new Line(ws.walls[i].p0, ws.walls[i].p1);
                ls.Add(l);
            }
            this.wl = ls;
            this.center = center;
            this.ws = ws;
        }

        public Point3d centerwitnpts(List<Point3d>ps)
        {
            double x = 0;
            double y = 0;
            double z = 0;
            for (int i = 0; i < ps.Count; i++)
            {
                x = x + ps[i].X;
                y = y + ps[i].Y;
                z = z + ps[i].Z;
            }
            var p = new Point3d(x / ps.Count, y / ps.Count, z / ps.Count);
            return p;

        }
    }
    class  Window
    {
        public Wall wall;
        public double P0;
        public double P1;
        public double dmgaodu;
        public double chgaodu;
        public Window(Wall wall, double p0, double p1, double dmgaodu, double chgaodu)
        {
            this.wall = wall;
            this.P0 = p0;
            this.P1 = p1;
            this.dmgaodu = dmgaodu;
            this.chgaodu = chgaodu;
        }
        public Window()
        {

        }

        public Box WindowView()
        {
            Box b = new Box();
            var ps = wall.p0;
            var pd = wall.p1;
            Line l22 = new Line(ps, pd);
            var p0 = l22.PointAt(P0);
            var p1 = l22.PointAt(P1);
            Box box = new Box();
            double d = wall.d;
            double hight = chgaodu;
            Point3d center = new Point3d((p0.X + p1.X) * 0.5, (p0.X + p1.X) * 0.5, (p0.Z + p1.Z) * 0.5+ dmgaodu);
            Point3d pnew = new Point3d(p0.X, p0.Y, p0.Z + dmgaodu+ 1);
            Point3d p01 = new Point3d(p0.X, p0.Y, p0.Z + dmgaodu);

            Plane plane = new Plane(p01, pnew - p0);
            double l = new Line(p0, p1).Length;
            Point3d pchankao = new Point3d(p0.X + l, p0.Y, p0.Z);

            box = new Box(plane, new Interval(0, l), new Interval(-d * 0.5, d * 0.5), new Interval(0, hight));
            Transform tran = Transform.Rotation(p0 - pchankao, p0 - p1, p0);
            double radians = Vector3d.VectorAngle(p0 - pchankao, p0 - p1);
            Transform tran2 = Transform.Rotation(radians, p0);
            if (radians == Math.PI)
            {
                box.Transform(tran2);

            }
            else
            {
                box.Transform(tran);
            }
            b = box;
            return b;
        }

    }





}
