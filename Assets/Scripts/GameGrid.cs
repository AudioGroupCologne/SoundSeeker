using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGrid
{
    private int width;
    private int height;
    private float cellSize; // 
    private Vector3 origin; // Vector from origin of local coordinate system (gird) to origin of global coordinate system (Unity) -> translation
    private Vector2 origin2D;
    private Target target;

    private Vector2 PlayerLastPosition;
    private Vector2 PlayerLastGridPosition;
    private Vector2 targetPosition;
    private float PlayerPathTravelledDistance;
    private float[,] gridArray;
    public float[,] GridArray
    {
        get { return gridArray; }
        set { gridArray = value; }
    }


    public Target Target
    {
        get { return target; }
        set { target = value; }
    }




    public GameGrid(int width, int height, float cellsize, Vector3 origin, Vector2 playerStart, Target target)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellsize;
        this.origin = origin;
        this.origin2D = new Vector2(origin.x, origin.z);
        this.PlayerPathTravelledDistance = 0;
        this.PlayerLastPosition = playerStart;
        this.Target = target;
        gridArray = new float[width, height];


    }



    public Vector3 GridToWorldPosition(int x, int y)
    {
        Vector3 worldPosition = new Vector3(x, 0, y) - origin;
        worldPosition *= cellSize;
        return worldPosition + new Vector3(cellSize / 2, 0, cellSize / 2);
    }

    private Vector2 WorldToGridPosition(Vector2 pos)
    {
        int x = Mathf.FloorToInt((pos.x / cellSize + origin2D.x));
        int y = Mathf.FloorToInt((pos.y / cellSize + origin2D.y));

        return new Vector2(x, y);
    }


    //Mark a cell of the grid as visited by the player by logging the current distance to the polygon
    public void SetGridValue(Vector2 pos, float value)
    {
        Vector2 adjpos = WorldToGridPosition(pos);
        if (gridArray[(int)adjpos.x, (int)adjpos.y] == 0)
        {
            gridArray[(int)adjpos.x, (int)adjpos.y] = value;
        }
    }

    public void UpdateDistanceTravelled(Vector2 pos)
    {
        PlayerPathTravelledDistance += Vector2.Distance(PlayerLastPosition, pos);
        PlayerLastPosition = pos;
        //Debug.Log("Travelled:" + PlayerPathTravelledDistance);
    }

    public float GetDistanceTravelled()
    {
        return this.PlayerPathTravelledDistance;
    }





}
