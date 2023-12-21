using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{ 
    [SerializeField] private GameObject cell;
    public float gridSizeX;
    public float gridSizeY;
    
    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        for (float x = -(gridSizeX / 2);  x < (gridSizeX) - (gridSizeX / 2); x++)
        {
            for (float y = -(gridSizeY / 2);  y < (gridSizeY) - (gridSizeY / 2); y++)
            {
                print(x + " , " + y);
                var pos = new Vector2(x, y);
                Instantiate(cell, pos, Quaternion.identity);
            }
        }
    }
}
