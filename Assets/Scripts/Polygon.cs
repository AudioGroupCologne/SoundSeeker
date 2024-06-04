using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Polygon : Target
{
    private List<Vector2> vectors;

    public List<Vector2> Vectors
    {
        get { return vectors; }
        set { vectors = value; }
    }

    private float sideLength;


    public Polygon(List<Vector2> vertices)
    {
        this.Vertices = vertices;
        this.vectors = new List<Vector2>();
        Type = 0;
        Position = GetCenterPosition();
        FillVectorList();
        CalculateSideLength();
    }

    private void CalculateSideLength()
    {
        foreach (var v in vectors)
        {
            sideLength += v.magnitude;
        }
    }

    public float GetSideLength()
    {
        return sideLength;
    }

    //Calculate vectors between vertices by building 
    private void FillVectorList()
    {
        for (int i = 0; i < Vertices.Count; ++i)
        {
            if (i != Vertices.Count - 1)
            {
                vectors.Add(Vertices[i + 1] - Vertices[i]);
            }
            else
            {
                vectors.Add(Vertices[0] - Vertices[i]);
            }

        }
    }

    //Returns the polygon's centroid for angle calculation
    public Vector2 GetCenterPosition()
    {
        float A = 0f;
        for (int i = 0; i < Vertices.Count - 1; ++i)
        {
            A += Vertices[i].x * Vertices[i + 1].y - Vertices[i + 1].x * Vertices[i].y;
        }
        A *= 0.5f;

        float xs = 0f;
        float ys = 0f;
        for (int j = 0; j < Vertices.Count - 1; ++j)
        {
            xs += (Vertices[j].x * Vertices[j + 1].x) * (Vertices[j].x * Vertices[j + 1].y - Vertices[j + 1].x * Vertices[j + 1].y);
            ys += (Vertices[j].y * Vertices[j + 1].y) * (Vertices[j].x * Vertices[j + 1].y - Vertices[j + 1].x * Vertices[j + 1].y);
        }
        xs /= (6 * A);
        ys /= (6 * A);
        return new Vector2(xs, ys);
    }




}
