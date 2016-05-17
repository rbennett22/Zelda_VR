using Immersio.Utility;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

public class QuickDraw : Singleton<QuickDraw>
{
    // USAGE:
    //
    // string _lineTag = QuickDraw.Instance.AddLine(p0, p1);
    // QuickDraw.Instance.ChangeLine(_lineTag, p0, p1);


    const int DefaultLineWidth = 3;
    static Color DefaultLineColor = Color.green;

    static int NextID;


    VectorLine _line;
    Dictionary<string, VectorLine> _lines;


    override protected void Awake()
    {
        base.Awake();

        _lines = new Dictionary<string, VectorLine>();
    }


    public string AddLine(Vector3 p0, Vector3 p1)
    {
        string tag = "Line " + NextID++;

        Vector3[] points = new Vector3[] { p0, p1 };
        VectorLine line = new VectorLine(tag, points, DefaultLineColor, null, DefaultLineWidth);
        line.Draw3DAuto();
        _lines.Add(tag, line);

        return tag;
    }

    public string AddVector(Vector3 origin, Vector3 displacement)
    {
        return AddLine(origin, origin + displacement);
    }

    public string AddRay(Ray ray)
    {
        return AddLine(ray.origin, ray.origin + ray.direction * 9999);
    }


    public void ChangeLine(string tag, Vector3 p0, Vector3 p1)
    {
        VectorLine line;
        if (_lines.TryGetValue(tag, out line))
        {
            ChangeLine(line, p0, p1);
        }
    }

    public void RemoveLine(string tag)
    {
        VectorLine line;
        if (_lines.TryGetValue(tag, out line))
        {
            line.StopDrawing3DAuto();
            ClearLine(line);
            _lines.Remove(tag);
        }
    }


    void ChangeLine(VectorLine line, Vector3 p0, Vector3 p1)
    {
        line.points3[0] = p0;
        line.points3[1] = p1;
    }

    void ClearLine(VectorLine line)
    {
        line.ZeroPoints();
        line.Draw3D();
    }
}