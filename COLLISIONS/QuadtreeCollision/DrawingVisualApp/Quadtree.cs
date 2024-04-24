using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace DrawingVisualApp
{
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

        public bool Contains(Point p)
        {
            return  p.X <= x + w &&
                    p.X >= x &&
                    p.Y <= y + h &&
                    p.Y >= y;
        }

        public bool Intersects(Rectangle range)
        {
            return !(
              range.x > x + w ||
              range.x + range.w < x ||
              range.y > y + h ||
              range.y + range.h < y
            );
        }

        public Rectangle Copy() => new Rectangle(x, y, w, h);
    }
    
    class QuadTree
    {
        Rectangle boundary;
        int capacity;
        bool divided;
        QuadTree northeast, northwest, southeast, southwest;
        List<Point> points;

        public QuadTree(Rectangle boundary, int n)
        {
            this.boundary = boundary.Copy();
            capacity = n;
            points = new List<Point>();
            divided = false;
        }

        void Subdivide()
        {
            var x = boundary.x;
            var y = boundary.y;
            var w = boundary.w;
            var h = boundary.h;
            var ne = new Rectangle(x + w / 2, y, w / 2, h / 2);
            northeast = new QuadTree(ne, capacity);
            var nw = new Rectangle(x, y, w / 2, h / 2);
            northwest = new QuadTree(nw, capacity);
            var se = new Rectangle(x + w / 2, y + h / 2, w / 2, h / 2);
            southeast = new QuadTree(se, capacity);
            var sw = new Rectangle(x, y + h / 2, w / 2, h / 2);
            southwest = new QuadTree(sw, capacity);
            divided = true;
        }

        public bool Insert(Point point)
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
            else
            {
                if (!divided)
                    Subdivide();

                if (northeast.Insert(point))
                {
                    return true;
                }
                else if (northwest.Insert(point))
                {
                    return true;
                }
                else if (southeast.Insert(point))
                {
                    return true;
                }
                else if (southwest.Insert(point))
                {
                    return true;
                }
                else return false;
            } 
        }


        // Query переделано по примеру с википедии
        //https://ru.wikipedia.org/wiki/%D0%94%D0%B5%D1%80%D0%B5%D0%B2%D0%BE_%D0%BA%D0%B2%D0%B0%D0%B4%D1%80%D0%B0%D0%BD%D1%82%D0%BE%D0%B2
        
        public List<Point> Query(Rectangle range)
        {
            List<Point> found = new List<Point>();

            if (!boundary.Intersects(range))
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

            dc.DrawRectangle(null, new Pen(Brushes.White, 0.08), rect);

            foreach (var p in points)
            {
                dc.DrawEllipse(Brushes.Green, null, p, 1, 1);
            }

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
