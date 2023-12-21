using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public List<Cell> cells;
    private bool iconMode = true;
    private void Awake()
    {
        cells = new List<Cell>(GetComponentsInChildren<Cell>());

        foreach (var cell in cells)
        {
            cell.Init();
            if (iconMode) cell.IconMode();
        }
        
        foreach (var cell in cells) //remove ability to jump from cells that lead to a spike
        {
            if (cell.type == Cell.Type.Exit && cell.instantReward < -0.25f)
            {
                var spikeRange = new Vector2(cell.transform.localPosition.x, cell.transform.localPosition.y);
                spikeRange += Vector2.up;
                var upCell = GetCell(spikeRange);
                
                while (upCell != null)
                {
                    var rightCell = GetCell(new Vector2(spikeRange.x + 1, spikeRange.y));
                    var leftCell = GetCell(new Vector2(spikeRange.x - 1, spikeRange.y));
                    if (rightCell != null && rightCell.isGrounded && leftCell != null && leftCell.isGrounded)
                    {
                        upCell.CantGoRight();
                        upCell.CantGoLeft();
                    }
                    
                    upCell.CantGoUp();
                    
                    spikeRange += Vector2.up;
                    upCell = GetCell(spikeRange);
                }
            }
        }
        
        foreach (var cell in cells) //remove ability to jump from cells that lead to a spike
        {
            var cellPos = new Vector2(cell.transform.localPosition.x, cell.transform.localPosition.y);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (iconMode)
            {
                iconMode = false;
                foreach (var cell in cells) cell.TextMode();
            }
            else
            {
                iconMode = true;
                foreach (var cell in cells) cell.IconMode();
            }
        }
    }

    public Cell GetCell(Vector2 cellPos)
    {
        foreach (var c in cells)
        {
            Vector2 cPos = new Vector2(c.transform.localPosition.x, c.transform.localPosition.y);
            if (cPos == cellPos )
            {
                return c;
            }
        }
        return null;
    }
    public Cell GetNextCell(Vector2 currentCellPos, string action)
    {
        Vector2 positionToLookFor = new Vector2(0,0);

        if (action == "right") positionToLookFor = new Vector2(currentCellPos.x + 1, currentCellPos.y); 
        else if (action == "left") positionToLookFor = new Vector2(currentCellPos.x - 1, currentCellPos.y); 
        else if (action == "up") positionToLookFor = new Vector2(currentCellPos.x, currentCellPos.y + 1); 
        else if (action == "down") positionToLookFor = new Vector2(currentCellPos.x, currentCellPos.y - 1); 
            
        foreach (var cell in cells)
        {
            Vector2 cellPos = new Vector2(cell.transform.position.x, cell.transform.position.y);
            if (cellPos == positionToLookFor)
            {
                return cell;
            }
        }
        //print("No cell found according to the action");
        return null;
    }
    
    public List<Cell> GetNeighbours(Cell cell)
    {
        var position = cell.transform.localPosition;
        
        var cellUp = new Vector2(position.x, position.y + 1);
        var cellDown = new Vector2(position.x, position.y - 1);
        var cellRight = new Vector2(position.x + 1, position.y);
        var cellLeft = new Vector2(position.x - 1, position.y);
        
        List<Cell> neighbours = new List<Cell>();

        foreach (var c in cells)
        {
            Vector2 cPos = new Vector2(c.transform.localPosition.x, c.transform.localPosition.y);
            if (cPos == cellUp ||  cPos == cellDown || cPos == cellRight || cPos == cellLeft)
            {
                neighbours.Add(c);
            }
        }

        return neighbours;
    }

    public void OnPlayerRestart()
    {
        foreach (var cell in cells)
        {
            cell.ResetReward();
        }
    }

    public void OnPlayerMove()
    {
        foreach (var cell in cells)
        {
            cell.OnPlayerMove();
            if (cell.type == Cell.Type.Exit)
            {
                //cell.instantReward *= discountFactor;
            }
        }
    }
}
