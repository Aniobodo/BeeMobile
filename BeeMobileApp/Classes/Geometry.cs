namespace BeeMobileApp.Classes
{
    public readonly struct Point2D
    {
        public readonly float X, Y;
        public Point2D(float x, float y) { X = x; Y = y; }
        public Point2D(Point2D p) { X = p.X; Y = p.Y; }
        public static Point2D operator +(Point2D p1, Point2D p2) => new Point2D(p1.X + p2.X, p1.Y + p2.Y);
        public static Point2D operator -(Point2D p1, Point2D p2) => new Point2D(p1.X - p2.X, p1.Y - p2.Y);
        public static Point2D operator *(float faktor, Point2D p) => new Point2D(p.X * faktor, p.Y * faktor);
        public static bool operator ==(Point2D p1, Point2D p2) => p1.X == p2.X && p1.Y == p2.Y;
        public static bool operator !=(Point2D p1, Point2D p2) => p1.X != p2.X || p1.Y != p2.Y;
        public override bool Equals(object obj) => obj is Point2D p && Equals(p);
        public override int GetHashCode() => HashCode.Combine(X, Y);
    }
    public readonly struct Line2D
    {
        public readonly Point2D P1, P2;
        public Line2D(Point2D p1, Point2D p2) { P1 = p1; P2 = p2; }
        public static Line2D operator +(Line2D linie, Point2D p) => new Line2D(linie.P1 + p, linie.P2 + p);
        public static Line2D operator -(Line2D linie, Point2D p) => new Line2D(linie.P1 - p, linie.P2 - p);
        public static bool operator ==(Line2D l1, Line2D l2) => l1.P1 == l2.P1 && l1.P2 == l2.P2;
        public static bool operator !=(Line2D l1, Line2D l2) => l1.P1 != l2.P1 || l1.P2 != l2.P2;
        public override bool Equals(object obj) => obj is Line2D l && Equals(l);
        public override int GetHashCode() => HashCode.Combine(P1, P2);
    }
    public readonly struct Bar2D
    {
        public readonly Line2D B; public readonly int Diameter;
        public Bar2D(Line2D l1, int dia) { B = l1; Diameter = dia; }
        public static bool operator ==(Bar2D b1, Bar2D b2) => b1.B == b2.B && b1.Diameter == b2.Diameter;
        public static bool operator !=(Bar2D b1, Bar2D b2) => b1.B != b2.B || b1.Diameter != b2.Diameter;
        public override bool Equals(object obj) => obj is Bar2D b && Equals(b);
        public override int GetHashCode() => HashCode.Combine(B, Diameter);

    }
    public readonly struct Vector2D
    {
        public readonly float X, Y;
        public Vector2D(Point2D start, Point2D end, bool unitvec = false)
        {
            X = unitvec ? (end.X - start.X) / start.Length(end) : end.X - start.X;
            Y = unitvec ? (end.Y - start.Y) / start.Length(end) : end.Y - start.Y;
        }
        public Vector2D(float x, float y, float scale = 1) { X = x * scale; Y = y * scale; }
        public static Vector2D operator +(Vector2D v1, Vector2D v2) => new Vector2D(v1.X + v2.X, v1.Y + v2.Y);
        public static Vector2D operator -(Vector2D v1, Vector2D v2) => new Vector2D(v1.X - v2.X, v1.Y - v2.Y);
        public static float operator *(Vector2D v1, Vector2D v2) => v1.X * v2.X + v1.Y * v2.Y; //dot product
        public static Vector2D operator *(float factor, in Vector2D v) => new Vector2D(v.X * factor, v.Y * factor);
        public static bool operator ==(in Vector2D v1, in Vector2D v2) => v1.X == v2.X && v1.Y == v2.Y;
        public static bool operator !=(in Vector2D v1, in Vector2D v2) => v1.X != v2.X || v1.Y != v2.Y;
        public override bool Equals(object obj) => obj is Vector2D v && Equals(v);
        public override int GetHashCode() => HashCode.Combine(X, Y);
    }
    public readonly struct BeeCarpet
    {
        public readonly string Lfdnr;
        public readonly string Text;        //Text found under the rollout arrow with mass length and wigth
        public readonly Point2D P1;         //One of the corner point of the carpet 
        public readonly Point2D P2;         //One of the corner point of the carpet
        public readonly Point2D P3;         //One of the corner point of the carpet
        public readonly Point2D P4;         //One of the corner point of the carpet
        public readonly Line2D Rollout;     //Line of the rollout arrow P1 = rollout head P2 = rollout tail
        public readonly Vector2D RollVec;   //Opposite to rollout direction
        public readonly Point2D[] Arrow;    //Arrow head of rollout points
        public BeeCarpet(string name, string text, Point2D p1, Point2D p2, Point2D p3, Point2D p4, Line2D rollout, Vector2D rollvec, Point2D[] arrow)
        {
            Lfdnr = name; Text = text; P1 = p1; P2 = p2; P3 = p3; P4 = p4; Rollout = rollout; RollVec = rollvec; Arrow = arrow;
        }
    }
}