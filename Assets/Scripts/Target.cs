using System.Collections.Generic;
using UnityEngine;

public class Target
{
    private short type;

    //Type 0 = polygon, type 1 = single target
    public short Type
    {
        get { return type; }
        set { type = value; }
    }

    private Vector2 position; //Center position of polygon or single target position
    public Vector2 Position
    {
        get { return position; }
        set
        {
            position = value;
            if (Type == 1)
            {
                vertices[0] = value;

            }
        }
    }

    private List<Vector2> vertices;
    public List<Vector2> Vertices
    {
        get { return vertices; }
        set { vertices = value; }
    }


}