using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace DrawingVisualApp
{
    class myPoint
    {
        public double x, y;
        public Particle userData;

        public myPoint(double x, double y, Particle userData)
        {
            this.x = x;
            this.y = y;
            this.userData = userData;
        }
    }
    
    class Rectangle
    {
        public double x, y, w, h;

        public Rectangle(double x, double y, double w, double h)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
        }

        public bool Contains(myPoint p)
        {
            return  p.x <= x + w &&
                    p.x >= x &&
                    p.y <= y + h &&
                    p.y >= y;
        }

        public bool Intersects(Rectangle range)
        {
            return !(
              //range.x - range.w > x + w ||
              //range.x + range.w < x - w ||
              //range.y - range.h > y + h ||
              //range.y + range.h < y - h
              range.x > x + w ||
              range.x + range.w < x ||
              range.y > y + h ||
              range.y + range.h < y

              //return (range.y < y || range.y1 > b.y || range.x1 < b.x || range.x > b.x1);
            );
        }

        public Rectangle Copy()
        {
            return new Rectangle(x, y, w, h);
        }
    }

    class Circle
    {
        public double x, y, rSquared;
        int r;

        public Circle(double x, double y, int r)
        {
            this.x = x;
            this.y = y;
            this.r = r;
            rSquared = r * r;
        }

        public bool Contains(myPoint point)
        {
            // check if the point is in the circle by checking if the euclidean distance of
            // the point and the center of the circle if smaller or equal to the radius of
            // the circle
            var d = Math.Pow(point.x - x, 2) + Math.Pow(point.y - y, 2);
            return d <= rSquared;
        }

        public bool Intersects(Rectangle range)
        {
            var xDist = Math.Abs(range.x - x);
            var yDist = Math.Abs(range.y - y);

            // radius of the circle
            var r = this.r;

            var w = range.w;
            var h = range.h;

            var edges = Math.Pow(xDist - w, 2) + Math.Pow(yDist - h, 2);

            // no intersection
            if (xDist > r + w || yDist > r + h) return false;

            // intersection within the circle
            if (xDist <= w || yDist <= h) return true;

            // intersection on the edge of the circle
            return edges <= rSquared;
        }
    }
    
    class QuadTree
    {
        Rectangle boundary;
        int capacity;
        bool divided;
        QuadTree northeast, northwest, southeast, southwest;
        List<myPoint> points;

        public QuadTree(Rectangle boundary, int n)
        {
            this.boundary = boundary.Copy();
            capacity = n;
            points = new List<myPoint>();
            divided = false;
        }

        void Subdivide()
        {
            var x = boundary.x;
            var y = boundary.y;
            var w = boundary.w / 2;
            var h = boundary.h / 2;
            var ne = new Rectangle(x + w, y, w, h);
            northeast = new QuadTree(ne, capacity);
            var nw = new Rectangle(x, y, w, h);
            northwest = new QuadTree(nw, capacity);
            var se = new Rectangle(x + w, y + h, w, h);
            southeast = new QuadTree(se, capacity);
            var sw = new Rectangle(x, y + h, w, h);
            southwest = new QuadTree(sw, capacity);
            divided = true;
        }

        public bool Insert(myPoint point)
        {
            if (!boundary.Contains(point))
            {
                return false;
            }


            if (points.Count < capacity)
            {
                points.Add(point);
                return true;
            }
            
            if (!divided)
                Subdivide();

            if (
                northeast.Insert(point) ||
                northwest.Insert(point) ||
                southeast.Insert(point) ||
                southwest.Insert(point) 
                )
            {
                return true;
            }

            return false;
        }


        // Query переделано по примеру с википедии
        //https://ru.wikipedia.org/wiki/%D0%94%D0%B5%D1%80%D0%B5%D0%B2%D0%BE_%D0%BA%D0%B2%D0%B0%D0%B4%D1%80%D0%B0%D0%BD%D1%82%D0%BE%D0%B2
        
        public List<myPoint> Query(Circle range)
        {
            List<myPoint> found = new List<myPoint>();

            if (!range.Intersects(boundary))
            {
                return found;
            }
            else
            {
                foreach (var p in points)
                {
                    if (range.Contains(p))
                    {
                        found.Add(p);
                    }
                }
                if (divided)
                {
                    found.AddRange(northwest.Query(range));
                    found.AddRange(northeast.Query(range));
                    found.AddRange(southwest.Query(range));
                    found.AddRange(southeast.Query(range));
                }
            }
            return found;
        }

        public void Show(DrawingContext dc)
        {
            Rect rect = new Rect(new Point(boundary.x, boundary.y), 
                        new Point(boundary.x + boundary.w, boundary.y + boundary.h));

            dc.DrawRectangle(null, new Pen(Brushes.White, 0.3), rect);

            if (divided)
            {
                northeast.Show(dc);
                northwest.Show(dc);
                southeast.Show(dc);
                southwest.Show(dc);
            }
        }
    }
}
