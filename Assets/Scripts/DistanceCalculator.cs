using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//TODO: Make so only static function calls from this class have to be made (no more instances)
public class DistanceCalculator
{
    private GameController gridHandler = GameObject.Find("GameController").GetComponent<GameController>(); //TODO only for debugging, remove later

    //Calculate which polygon side is closest to player position and returns distance to this side
    public float MinDistanceToTarget(Target target, Vector2 playerPosition)
    {
        float minDist = float.MaxValue;

        if (target.Type == 0)
        {
            Vector2 cp = Vector2.zero;
            for (int i = 0; i < target.Vertices.Count; i++)
            {
                float temp;
                Vector2 cpTemp;
                if (i != target.Vertices.Count - 1)
                {
                    cpTemp = MinDistancePoint(target.Vertices[i + 1], target.Vertices[i], playerPosition);
                    temp = Vector2.Distance(cpTemp, playerPosition);
                }
                else
                {
                    cpTemp = MinDistancePoint(target.Vertices[i], target.Vertices[0], playerPosition);
                    temp = Vector2.Distance(cpTemp, playerPosition);
                }
                if (temp < minDist)
                {
                    minDist = temp;
                    cp = cpTemp;
                }
            }
            gridHandler.RecDistData(playerPosition, cp); //for debugging
        }
        else if (target.Type == 1)
        {
            minDist = Vector2.Distance(playerPosition, target.Vertices[0]); //return euclidian distance between player and target if in single target mode
        }

        return minDist;
    }

    // Returns point on line between V and W with closest distance to P
    private Vector2 MinDistancePoint(Vector2 v, Vector2 w, Vector2 p)
    {
        Vector2 vw = w - v;
        Vector2 vp = p - v;

        float proj = Vector2.Dot(vp, vw);
        float lenVWSq = (float)Math.Pow(Math.Sqrt(Math.Pow(vw.x, 2) + Math.Pow(vw.y, 2)), 2);
        float d = proj / lenVWSq;
        Vector2 closestPoint;

        //if d <= 0 V is closest to P; if d >= 1 then W is closest to P; if between then the closest point is on the line between V and W
        if (d <= 0)
        {
            closestPoint = v;
        }
        else if (d >= 1)
        {
            closestPoint = w;
        }
        else
        {
            closestPoint = v + vw * d;
        }
        return closestPoint;
    }
}
