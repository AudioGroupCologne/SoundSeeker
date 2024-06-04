using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleTarget : Target
{
    private Vector2 position;
 


    public SingleTarget(Vector2 position)
    {
        Position = position; //Set "center position" 
        Vertices = new List<Vector2>();
        Vertices.Add(position); //..as well as single vertix
        this.Type = 1;
    }

    public SingleTarget()
    {
        Position = Vector2.zero;
        Debug.LogWarning("Single target initialized without position. Position set to (0,0)");
        Vertices = new List<Vector2>();
        Type = 1;
    }
}
