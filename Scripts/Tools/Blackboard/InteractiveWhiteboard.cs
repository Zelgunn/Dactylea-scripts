using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InteractiveWhiteboard : MonoBehaviour
{
    #region Structures
    public struct IntVector2
    {
        public int x;
        public int y;

        public static IntVector2 operator +(IntVector2 point1, IntVector2 point2)
        {
            IntVector2 res = new IntVector2();
            res.x = point1.x + point2.x;
            res.y = point1.y + point2.y;
            return res;
        }

        public static IntVector2 operator -(IntVector2 point1, IntVector2 point2)
        {
            IntVector2 res = new IntVector2();
            res.x = point1.x - point2.x;
            res.y = point1.y - point2.y;
            return res;
        }

        public static bool operator ==(IntVector2 point1, IntVector2 point2)
        {
            return (point1.x == point2.x) && (point1.y == point2.y);
        }

        public static bool operator !=(IntVector2 point1, IntVector2 point2)
        {
            return (point1.x != point2.x) || (point1.y != point2.y);
        }

        public override bool Equals(object obj)
        {
            return (((IntVector2)obj).x == x) && (((IntVector2)obj).y == y);
        }
        public override int GetHashCode()
        {
            return x + y;
        }

        public static IntVector2 Lerp(IntVector2 point1, IntVector2 point2, float factor)
        {
            IntVector2 res = new IntVector2();
            res.x = (int)(point1.x * (1 - factor) + point2.x * factor);
            res.y = (int)(point1.y * (1 - factor) + point2.y * factor);
            return res;
        }

        public static IntVector2 Lerp(IntVector2 point1, IntVector2 point2, int mul, int div)
        {
            IntVector2 res = new IntVector2();
            res.x = point1.x - point1.x * mul / div + point2.x * mul / div;
            res.y = point1.y - point1.y * mul / div + point2.y * mul / div;
            return res;
        }

        public static int MaxDistanceInOneDimension(IntVector2 point1, IntVector2 point2)
        {
            int dx = Mathf.Abs(point1.x - point2.x);
            int dy = Mathf.Abs(point1.y - point2.y);

            return Mathf.Max(dx, dy);
        }
    }

    public struct Circle
    {
        public IntVector2 center;
        public int radius;
    }

    public struct Polygon
    {
        public List<IntVector2> points;

        public void AddPoint(int x, int y)
        {
            IntVector2 point = new IntVector2();
            point.x = x;
            point.y = y;

            AddPoint(point);
        }

        public void AddPoint(IntVector2 point)
        {
            if (points == null) points = new List<IntVector2>();
            points.Add(point);
        }

        public void Move(int x, int y)
        {
            IntVector2 delta = new IntVector2();
            delta.x = x;
            delta.y = y;

            Move(delta);
        }

        public void Move(IntVector2 delta)
        {
            for (int i = 0; i < points.Count; i++)
            {
                points[i] += delta;
            }
        }

        public int pointCount
        {
            get { return points.Count; }
        }
    }

    public struct PixelMask
    {
        public bool[] mask;
        public int width;
        public int height
        {
            get
            {
                if (width == 0) return 0;
                return mask.Length / width;
            }
        }
        public bool At(int x, int y)
        {
            return mask[x + y * width];
        }
    }
    #endregion

    #region Champs

    private RectTransform m_rectTransform;

    private List<PenEdge> m_currentPenEdges = new List<PenEdge>();
    private Dictionary<PenEdge, IntVector2> m_penLastPositions = new Dictionary<PenEdge, IntVector2>();

    private List<Transform> m_registredPolygonPenPoints = new List<Transform>();
    private List<Transform> m_registredPolygonPenPointsBuffer;
    private List<PolygonPen> m_registredPolygonPens = new List<PolygonPen>();
    private Dictionary<PolygonPen, Vector3> m_registredPolygonPensLastPosition = new Dictionary<PolygonPen, Vector3>();

    [SerializeField] private int m_columnCount;
    [SerializeField] private int m_rowCount;
    [SerializeField] private int m_width;
    [SerializeField] private int m_height;

    private int m_singleTextureWidth;
    private int m_singleTextureHeight;

    private Texture2D[][] m_textures;
    private RawImage[][] m_images;
    private Color32[][][] m_colors;

    private bool[][] m_texturesTags;

    #endregion

    #region Unity's overloaded functions

    private void Awake()
    {
        m_rectTransform = GetComponent<RectTransform>();

        InitSubTextures();
    }

    private void InitSubTextures()
    {
        float colPace = 1f / m_columnCount;
        float rowPace = 1f / m_rowCount;

        m_singleTextureWidth = m_width / m_columnCount;
        m_singleTextureHeight = m_height / m_rowCount;

        m_images = new RawImage[m_columnCount][];
        m_textures = new Texture2D[m_columnCount][];
        m_colors = new Color32[m_columnCount][][];
        m_texturesTags = new bool[m_columnCount][];

        for (int column = 0; column < m_columnCount; column++)
        {
            m_images[column] = new RawImage[m_rowCount];
            m_textures[column] = new Texture2D[m_rowCount];
            m_colors[column] = new Color32[m_rowCount][];
            m_texturesTags[column] = new bool[m_rowCount];

            for (int row = 0; row < m_rowCount; row++)
            {
                GameObject imageGameObject = new GameObject();
                imageGameObject.name = "Whiteboard Texture (col " + column + ", row " + row + ")";

                RawImage image = imageGameObject.AddComponent<RawImage>();

                image.rectTransform.SetParent(transform);
                image.rectTransform.localPosition = Vector3.zero;

                image.rectTransform.anchorMin = new Vector2(column * colPace, row * rowPace);
                image.rectTransform.anchorMax = new Vector2((column + 1) * colPace, (row + 1) * rowPace);
                image.rectTransform.pivot = image.rectTransform.anchorMin + new Vector2(colPace / 2, rowPace / 2);
                image.rectTransform.offsetMin = Vector2.zero;
                image.rectTransform.offsetMax = Vector2.zero;


                Texture2D texture = new Texture2D(m_singleTextureWidth, m_singleTextureHeight);
                texture.name = "Whiteboard Texture2D (col " + column + ", row " + row + ")";
                Color32[] colors = new Color32[m_singleTextureHeight * m_singleTextureWidth];

                for (int i = 0; i < m_singleTextureHeight; i++)
                {
                    for (int j = 0; j < m_singleTextureWidth; j++)
                    {
                        colors[i * m_singleTextureWidth + j] = new Color32(255, 255, 255, 0);
                    }
                }

                m_colors[column][row] = colors.Clone() as Color32[];
                texture.SetPixels32(m_colors[column][row]);
                texture.Apply();

                m_textures[column][row] = texture;
                image.texture = texture;
                m_images[column][row] = image;
                m_texturesTags[column][row] = false;
            }
        }

    }

    private void Update()
    {
        bool updated = UpdatePolygonPen() | UpdateRoundPen();

        if(updated)
        {
            UpdateTaggedTextures();
        }
    }

    private void UpdateTaggedTextures()
    {
        int updatedCount = 0;

        for (int column = 0; column < m_columnCount; column++)
        {
            for (int row = 0; row < m_rowCount; row++)
            {
                bool tagged = m_texturesTags[column][row];
                if (!tagged) continue;

                updatedCount++;

                m_textures[column][row].SetPixels32(m_colors[column][row]);
                m_textures[column][row].Apply();

                m_texturesTags[column][row] = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PolygonPen polygonPen = other.GetComponentInParent<PolygonPen>();
        if (polygonPen)
        {
            m_registredPolygonPenPoints.Add(other.transform);
            if (!m_registredPolygonPens.Contains(polygonPen))
            {
                m_registredPolygonPens.Add(polygonPen);
                m_registredPolygonPensLastPosition.Add(polygonPen, polygonPen.transform.position);
            }
        }
        else
        {
            PenEdge penEdge = other.GetComponent<PenEdge>();
            if (penEdge)
            {
                m_currentPenEdges.Add(penEdge);
                m_penLastPositions.Add(penEdge, WorldPositionToBoardPosition(penEdge.transform.position));
            }
        }

    }

    private void OnTriggerExit(Collider other)
    {
        PolygonPen polygonPen = other.GetComponentInParent<PolygonPen>();
        if (polygonPen)
        {
            m_registredPolygonPenPoints.Remove(other.transform);
            if (!ContainsAnyPointOf(polygonPen))
            {
                m_registredPolygonPens.Remove(polygonPen);
                m_registredPolygonPensLastPosition.Remove(polygonPen);
            }
        }
        else
        {

            PenEdge penEdge = other.GetComponent<PenEdge>();
            if (penEdge)
            {
                m_currentPenEdges.Remove(penEdge);
                m_penLastPositions.Remove(penEdge);
            }
        }

    }

    #endregion

    #region Pen Updates
    private bool UpdatePolygonPen()
    {
        if (m_registredPolygonPenPoints.Count == 0) return false;
        bool updated = false;

        m_registredPolygonPenPointsBuffer = new List<Transform>(m_registredPolygonPenPoints);
        IntVector2 tmpPoint = new IntVector2();

        foreach (PolygonPen polygonPen in m_registredPolygonPens)
        {
            if (polygonPen.transform.position == m_registredPolygonPensLastPosition[polygonPen]) continue;

            if (ContainsAllPointsOf(polygonPen, true))
            {
                Transform[] points = polygonPen.points;
                Polygon polygon = new Polygon();

                foreach (Transform point in points)
                {
                    tmpPoint = WorldPositionToBoardPosition(point.position);

                    polygon.AddPoint(tmpPoint);
                }

                DrawPolygon(polygon, polygonPen.color);

                updated = true;
            }

            m_registredPolygonPensLastPosition[polygonPen] = polygonPen.transform.position;
        }

        return updated;
    }

    private bool UpdateRoundPen()
    {
        if (m_currentPenEdges.Count == 0) return false;
        bool updated = false;

        for (int i = 0; i < m_currentPenEdges.Count; i++)
        {
            PenEdge penEdge = m_currentPenEdges[i];
            IntVector2 penPosition = WorldPositionToBoardPosition(penEdge.transform.position);

            if (!penEdge.hasCork)
            {
                Circle from = new Circle();
                from.center = m_penLastPositions[penEdge];
                from.radius = penEdge.width;

                Circle to = new Circle();
                to.center = penPosition;
                to.radius = from.radius;

                DrawCircleLine(from, to, penEdge.color, 3);

                updated = true;
            }

            m_penLastPositions[penEdge] = penPosition;
        }

        return updated;
    }
    #endregion

    private bool ContainsAnyPointOf(PolygonPen polygonPen)
    {
        Transform[] points = polygonPen.points;
        foreach (Transform point in points)
        {
            if (m_registredPolygonPenPoints.Contains(point))
            {
                return true;
            }
        }

        return false;
    }

    private bool ContainsAllPointsOf(PolygonPen polygonPen, bool useBuffer)
    {
        Transform[] points = polygonPen.points;

        if (useBuffer)
        {
            foreach (Transform point in points)
            {
                if (!m_registredPolygonPenPointsBuffer.Contains(point))
                {
                    return false;
                }
                else
                {
                    m_registredPolygonPenPointsBuffer.Remove(point);
                }
            }
        }
        else
        {
            foreach (Transform point in points)
            {
                if (!m_registredPolygonPenPoints.Contains(point))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private IntVector2 WorldPositionToBoardPosition(Vector3 worldPosition)
    {
        Vector3 delta = transform.position - worldPosition;
        delta = Vector3.Project(delta, transform.forward);

        Vector3 worldPositionProjected = worldPosition + delta - transform.position;

        IntVector2 res = new IntVector2();
        res.x = (int)((0.5f + worldPositionProjected.x / m_rectTransform.rect.width) * m_width);
        res.y = (int)((0.5f + worldPositionProjected.y / m_rectTransform.rect.height) * m_height);

        return res;
    }

    #region Drawing

    private void DrawPolygon(Polygon polygon, Color32 color)
    {
        PixelMask pixelMask = PixelMaskFromPolygon(polygon);

        IntVector2 topLeft, bottomRight;
        CornersOf(polygon, out topLeft, out bottomRight);
        IntVector2 center = topLeft;
        center.x -= pixelMask.width / 2;
        center.y -= pixelMask.height / 2;

        for (int x = 0; x < pixelMask.width; x++)
        {
            for (int y = 0; y < pixelMask.height; y++)
            {
                if (pixelMask.At(x, y))
                {
                    IntVector2 tmp = new IntVector2();
                    tmp.x = x;
                    tmp.y = y;
                    DrawPoint(center + tmp, color);
                }
            }
        }
    }

    private void DrawCircleLine(Circle from, Circle to, Color32 color, int quality)
    {
        int dx = to.center.x - from.center.x;
        int dy = to.center.y - from.center.y;

        float lineAngle = Mathf.Atan2(dx, dy);

        IntVector2 fromPoint1, fromPoint2, toPoint1, toPoint2;
        fromPoint1 = PointOnCircleAtAngle(from, lineAngle);
        fromPoint2 = PointOnCircleAtAngle(from, lineAngle + Mathf.PI);
        toPoint1 = PointOnCircleAtAngle(to, lineAngle);
        toPoint2 = PointOnCircleAtAngle(to, lineAngle + Mathf.PI);

        //DrawLine(fromPoint1, toPoint1, color);
        //DrawLine(fromPoint2, toPoint2, color);

        int lineCount = Mathf.Max(IntVector2.MaxDistanceInOneDimension(fromPoint1, fromPoint2), IntVector2.MaxDistanceInOneDimension(toPoint1, toPoint2)) * quality;
        IntVector2 tmpFrom = new IntVector2(), tmpTo = new IntVector2();

        for (int i = 0; i < lineCount; i++)
        {
            tmpFrom = IntVector2.Lerp(fromPoint1, fromPoint2, i, lineCount);
            tmpTo = IntVector2.Lerp(toPoint1, toPoint2, i, lineCount);

            DrawLine(tmpFrom, tmpTo, color);
        }

        DrawCircle(to, color);
    }

    private void DrawCircle(Circle circle, Color32 color)
    {
        IntVector2 tmp = new IntVector2();
        for (int x = -circle.radius; x < circle.radius; x++)
        {
            tmp.x = x;
            int height = (int)Mathf.Sqrt(circle.radius * circle.radius - x * x);

            for (int y = -height; y < height; y++)
            {
                tmp.y = y;
                DrawPoint(circle.center + tmp, color);
            }
        }
    }

    private void DrawLine(IntVector2 from, IntVector2 to, Color32 color)
    {
        int dx = to.x - from.x;
        int dy = to.y - from.y;

        int dxLenght = Mathf.Abs(dx);
        int dyLenght = Mathf.Abs(dy);

        bool dxIsLonger = dxLenght >= dyLenght;

        int lineLenght = Mathf.Max(dxLenght, dyLenght);

        IntVector2 point = new IntVector2();
        int tmp;

        for (int i = 0; i < lineLenght; i++)
        {
            if (dxIsLonger)
            {
                tmp = from.y + i * dy / lineLenght;

                if (tmp != point.y)
                {
                    point.y = tmp;
                    DrawPoint(point, color);
                }

                point.x = from.x + i * dx / lineLenght;
            }
            else
            {
                tmp = from.x + i * dx / lineLenght;

                if (tmp != point.x)
                {
                    point.x = tmp;
                    DrawPoint(point, color);
                }

                point.y = from.y + i * dy / lineLenght;
            }

            DrawPoint(point, color);
        }
    }

    private void DrawPoint(IntVector2 point, Color32 color)
    {
        DrawPoint(point.x, point.y, color);
    }

    private void DrawPoint(int x, int y, Color32 color)
    {
        if (CheckPoint(x, y))
        {
            int column = x / m_singleTextureWidth;
            int row = y / m_singleTextureHeight;

            Color32[] colors = m_colors[column][row];

            colors[PointToTextureIndex(x, y)] = color;

            m_texturesTags[column][row] = true;
        }
    }

    private bool CheckPoint(int x, int y)
    {
        return (x >= 0) && (y >= 0) && (x < m_width) && (y < m_height);
    }

    private int PointToTextureIndex(int x, int y)
    {
        x %= m_singleTextureWidth;
        y %= m_singleTextureHeight;

        return (x + y * m_singleTextureWidth);
    }

    private Texture2D TextureAtPoint(int x, int y)
    {
        int column = x / m_singleTextureWidth;
        int row = y / m_singleTextureHeight;

        return m_textures[column][row];
    }

    private Circle CircleFromRaw(int x, int y, int radius)
    {
        Circle circle = new Circle();

        circle.center = new IntVector2();
        circle.center.x = x;
        circle.center.y = y;

        circle.radius = radius;

        return circle;
    }

    private IntVector2 PointOnCircleAtAngle(Circle circle, float angle)
    {
        IntVector2 res = new IntVector2();

        res.x = circle.center.x + (int)(circle.radius * Mathf.Cos(angle));
        res.y = circle.center.y - (int)(circle.radius * Mathf.Sin(angle));

        return res;
    }

    private PixelMask PixelMaskFromPolygon(Polygon polygon)
    {
        IntVector2 topLeft, bottomRight;
        CornersOf(polygon, out topLeft, out bottomRight);

        int maskWidth = (bottomRight.x - topLeft.x) + 1;
        int maskHeight = (bottomRight.y - topLeft.y) + 1;

        PixelMask pixelMask = new PixelMask();
        pixelMask.mask = new bool[maskWidth * maskHeight];
        pixelMask.width = maskWidth;

        IntVector2 pointToTest = new IntVector2();
        for (int x = 0; x < maskWidth; x++)
        {
            for (int y = 0; y < maskHeight; y++)
            {
                pointToTest.x = x;
                pointToTest.y = y;

                int intersectCount = 0;
                for (int i = 0; i < polygon.pointCount; i++)
                {
                    if (IntFasterLineSegmentIntersection(topLeft + pointToTest, topLeft, polygon.points[i], polygon.points[(i + 1) % polygon.pointCount]))
                    {
                        intersectCount++;
                    }
                }

                if ((intersectCount % 2) == 1)
                {
                    pixelMask.mask[x + y * maskWidth] = true;
                }
                else
                {
                    pixelMask.mask[x + y * maskWidth] = false;
                }
            }
        }

        return pixelMask;
    }

    private void CornersOf(Polygon polygon, out IntVector2 topLeft, out IntVector2 bottomRight)
    {
        topLeft = new IntVector2();
        topLeft.x = polygon.points[0].x;
        topLeft.y = polygon.points[0].y;

        bottomRight = new IntVector2();
        bottomRight.x = polygon.points[0].x;
        bottomRight.y = polygon.points[0].y;

        foreach (IntVector2 point in polygon.points)
        {
            if (topLeft.x > point.x) topLeft.x = point.x;
            if (topLeft.y > point.y) topLeft.y = point.y;
            if (bottomRight.x < point.x) bottomRight.x = point.x;
            if (bottomRight.y < point.y) bottomRight.y = point.y;
        }
    }

    private bool IntFasterLineSegmentIntersection(IntVector2 p1, IntVector2 p2, IntVector2 p3, IntVector2 p4)
    {
        IntVector2 a = p2 - p1;
        IntVector2 b = p3 - p4;
        IntVector2 c = p1 - p3;

        int alphaNumerator = b.y * c.x - b.x * c.y;
        int alphaDenominator = a.y * b.x - a.x * b.y;
        int betaNumerator = a.x * c.y - a.y * c.x;
        int betaDenominator = a.y * b.x - a.x * b.y;

        bool doIntersect = true;

        if ((alphaDenominator == 0) || (betaDenominator == 0))
        {
            doIntersect = false;
        }
        else
        {
            if (alphaDenominator > 0)
            {
                if ((alphaNumerator < 0) || (alphaNumerator > alphaDenominator))
                {
                    doIntersect = false;
                }
            }
            else if ((alphaNumerator > 0) || (alphaNumerator < alphaDenominator))
            {
                doIntersect = false;
            }

            if (doIntersect && (betaDenominator > 0))
            {
                if (betaNumerator < 0 || betaNumerator > betaDenominator)
                {
                    doIntersect = false;
                }
            }
            else if ((betaNumerator > 0) || (betaNumerator < betaDenominator))
            {
                doIntersect = false;
            }
        }

        return doIntersect;
    }

    #endregion

    public int width
    {
        get { return m_width; }
    }

    public int height
    {
        get { return m_height; }
    }
}
