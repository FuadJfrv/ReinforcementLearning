using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Cell : MonoBehaviour
{
    public enum Type
    {
        Move,
        Exit,
        Ice
    }
    public Type type;

    public GameObject finishIcon;
    public GameObject spikeIcon;
    public GameObject iceIcon;
    public GameObject randomIcon;
    
    
    private float startRightReward = 0f;
    private float startLeftReward = 0f;
    private float startUpReward = 0f;
    private float startDownReward = 0f;
    
    private Dictionary<string, float> actionValues = new()
    {
    };

    
    public TextMeshProUGUI valueText;
    
    [Range(-1,1)] public float instantReward;
    private float _startInstantReward;
    
    [HideInInspector] public bool isGrounded;

    private GridManager _gridManager;
    private SpriteRenderer _sprite;

    public bool randomExit;
    [Range(0,1)]public float probOfSuccess;

    private bool showText;
    private Color _defaultColor;
    public void Init()
    {
        _gridManager = FindObjectOfType<GridManager>();
        _sprite = GetComponent<SpriteRenderer>();
        _defaultColor = _sprite.color;


        _startInstantReward = instantReward;
        
        AddAvailableActions();
        //SetIcons();
    }

    public void IconMode()
    {
        showText = false;
        _sprite.color = _defaultColor;
        if (type == Type.Exit && !randomExit && instantReward > 0)
        {
            finishIcon.SetActive(true);
            spikeIcon.SetActive(false);
            iceIcon.SetActive(false);
            randomIcon.SetActive(false);
        }
        else if (type == Type.Exit && randomExit && instantReward > 0)
        {
            finishIcon.SetActive(false);
            spikeIcon.SetActive(false);
            iceIcon.SetActive(false);
            randomIcon.SetActive(true);
        }
        else if (type == Type.Exit && instantReward < 0)
        {
            finishIcon.SetActive(false);
            spikeIcon.SetActive(true);
            iceIcon.SetActive(false);
            randomIcon.SetActive(false);
        }
        else if (type == Type.Ice)
        {
            finishIcon.SetActive(false);
            spikeIcon.SetActive(false);
            iceIcon.SetActive(true);
            randomIcon.SetActive(false);
        }
    }

    public void TextMode()
    {
        showText = true;
        finishIcon.SetActive(false);
        spikeIcon.SetActive(false);
        iceIcon.SetActive(false);
        randomIcon.SetActive(false);
    }

    private void AddAvailableActions()
    {
        if (_gridManager.GetNextCell(transform.position, "right") != null)
        {
            actionValues.Add("right", startRightReward);
        }

        if (_gridManager.GetNextCell(transform.position, "left") != null)
        {
            actionValues.Add("left", startLeftReward);
        }

        if (_gridManager.GetNextCell(transform.position, "up") != null)
        {
            // if the cell below is not an exit cell
            actionValues.Add("up", startUpReward);
        }

        if (_gridManager.GetNextCell(transform.position, "down") != null)
        {
            actionValues.Add("down", startDownReward);
        }
        else
        {
            isGrounded = true;
        }
    }

    private void Update()
    {
        string output = "";
        if (showText)
        {
            if (type == Type.Exit)
            {
                output = instantReward.ToString("F2");
            }
            else
            {
                foreach (var pair in actionValues)
                {
                    string coloredValue = FormatValueWithLerpColor(pair.Value);
                    output += $"{pair.Key}: {coloredValue}\n";
                }
            }

            _sprite.color = GetLerpColorCell(instantReward);

        }
        valueText.text = output;
    }

    private string FormatValueWithLerpColor(float value)
    {
        Color valueColor = GetLerpColorText(value);
        string hexColor = ColorUtility.ToHtmlStringRGB(valueColor);
        return $"<color=#{hexColor}>{value.ToString("F2")}</color>";
    }
    private Color GetLerpColorText(float value)
    {
        if (value < 0) return Color.Lerp(Color.yellow, Color.red, Mathf.Abs(value));
        if (value > 0) return Color.Lerp(Color.yellow, Color.green, value);
        return Color.black;
    }
    private Color GetLerpColorCell(float value)
    {
        if (value < 0) return Color.Lerp(Color.black, Color.red * 0.9f, Mathf.Abs(value));
        if (value > 0) return Color.Lerp(Color.black, Color.green * 0.9f, value);
        return Color.black;
    }
    
    public void CantGoUp()
    {
        actionValues.Remove("up");
    }
    public void CantGoRight()
    {
        actionValues.Remove("right");
    }
    public void CantGoLeft()
    {
        actionValues.Remove("left");
    }
    
    public string ReturnBestAction()
    {
        List<string> contenders = new List<string>();
        float maxValue = actionValues.Values.Max();

        foreach (var pair in actionValues)
        {
            if (Math.Abs(pair.Value - maxValue) < 0.01) contenders.Add(pair.Key); 
        }

        var bestAction = contenders[Random.Range(0, contenders.Count)];

        return bestAction;
    }

    public string ReturnRandomAction(string except = "")
    {
        List<string> contenders = new List<string>();
        foreach (var pair in actionValues)
        {
            contenders.Add(pair.Key); 
        }

        var randAction = contenders[Random.Range(0, contenders.Count)];
        if (randAction == except) return ReturnRandomAction(except);
        return randAction;
    }
    
    public float ReturnBestValue() => actionValues.Values.Max();

    public void SetActionValue(string action, float newValue)
    {
        if (actionValues.ContainsKey(action))
        {
            actionValues[action] = newValue;
        }
    }
    
    public float GetActionValue(string action)
    {
        if (actionValues.ContainsKey(action))
        {
            return actionValues[action];
        }
        
        //print("action does not exist");
        return -100000f;
    }

    public virtual void OnPlayerEnter()
    {
        if (randomExit)
        {
            var r = Random.Range(0f, 1f);
            if (r > probOfSuccess) instantReward = -1f;
        }
    }

    public void OnPlayerMove()
    {
    }

    public void ResetReward()
    {
        instantReward = _startInstantReward;
    }

}
