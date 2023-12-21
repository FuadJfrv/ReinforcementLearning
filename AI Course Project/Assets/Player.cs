using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour
{
    private GridManager _gridManager;
    
    public Cell startCell;
    public bool preCalculate;
    private Cell _currentCell;
    private Cell _nextCell;

    [Range(0,1)] public float alpha = 0.9f;
    [Range(0,1)] public float epsilonGreedy = 0.0f;
    [Range(0,1)] public float penalty = 0.0f;
    [Range(0,1)] public float probNotToSlip = 0.8f;
    

    private bool _shouldFallDown = false;
    private bool _grounded;
    private int _jumpCount = 2;

    private int _iterations;
    private bool started;
    private float timer;

    private string intendedAction;

    //ANIMATION
    public float moveSpeed = 0.1f;
    private bool _groundedJump; 
    private SpriteRenderer _sprite;
    private Animator _animator;
    private Ease _ease = Ease.Linear;

    private void Awake()
    {
        _sprite = GetComponentInChildren<SpriteRenderer>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        _gridManager = FindObjectOfType<GridManager>();
        GroundPlayer();


        if (preCalculate)
        {
            for (int i = 0; i < 100000; i++)
            {
                Learn1();
            }
            epsilonGreedy = 0;
        }

        StartCoroutine(Learn());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            Time.timeScale *= 2f;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            Time.timeScale *= 0.5f;
        }
    }

    
    private void Learn1()
    {
        _currentCell = startCell;
        transform.position = _currentCell.transform.position;
        _currentCell.OnPlayerEnter();
        
        
        while (true)
        {
            var rand = Random.Range(0f, 1f);
            string action;
            if (rand > epsilonGreedy)
                action = _currentCell.ReturnBestAction();
            else
                action = _currentCell.ReturnRandomAction();

            //jumping and falling logic
            if (action == "up" && _grounded)
            {
                _jumpCount--;
                _grounded = false;
            }
            else if (!_grounded && !_shouldFallDown)
            {
                if (action == "up" )
                {
                    if (_jumpCount == 1) _jumpCount = 0;
                    else
                    {
                        action = "down";
                        _shouldFallDown = true;
                    }
                }
                else
                { 
                    if (_jumpCount == 1) _jumpCount = 0;
                    else _shouldFallDown = true;
                }
            }
            else if (_shouldFallDown)
            {
                action = "down";
            }


            intendedAction = action; 
            if (_currentCell.type == Cell.Type.Ice)
            {
                var r = Random.Range(0f, 1f);
                if (r > 0.8f) action = _currentCell.ReturnRandomAction(intendedAction);
            }

            
            _nextCell = _gridManager.GetNextCell(_currentCell.transform.position, action);

            _nextCell.OnPlayerEnter();
            var sample = _nextCell.instantReward + penalty*(_nextCell.ReturnBestValue()); //instant reward + what i get from acting optimally
            var newValue = (1f - alpha) * _currentCell.GetActionValue(intendedAction) + alpha * sample; //old + new
            
            
            _currentCell.SetActionValue(intendedAction, newValue);

            _currentCell = _nextCell;
            transform.position = _currentCell.transform.position;
            
            _gridManager.OnPlayerMove();
            if (_currentCell.isGrounded)
            {
                GroundPlayer();
            }
            else
            {
                if (_grounded) //means walked off the edge
                {
                    _grounded = false;
                    _shouldFallDown = true;
                }
            }
            
            
            if (_currentCell.type == Cell.Type.Exit)
            {
                _gridManager.OnPlayerRestart();
                GroundPlayer();
                break;
            }
        }

    }
    
    private IEnumerator Learn()
    {
        _currentCell = startCell;
        transform.position = _currentCell.transform.position;
        _currentCell.OnPlayerEnter();
        
        yield return new WaitForSeconds(moveSpeed);
        
        while (true)
        {
            var rand = Random.Range(0f, 1f);
            string action;
            if (rand > epsilonGreedy)
                action = _currentCell.ReturnBestAction();
            else
                action = _currentCell.ReturnRandomAction();

            //jumping and falling logic
            if (action == "up" && _grounded)
            {
                _jumpCount--;
                _grounded = false;
                _groundedJump = true;
            }
            else if (!_grounded && !_shouldFallDown)
            {
                if (action == "up" )
                {
                    if (_jumpCount == 1) _jumpCount = 0;
                    else
                    {
                        action = "down";
                        _shouldFallDown = true;
                    }
                }
                else
                { 
                    if (_jumpCount == 1) _jumpCount = 0;
                    else _shouldFallDown = true;
                }
            }
            else if (_shouldFallDown)
            {
                action = "down";
            }


            intendedAction = action; //right
            if (_currentCell.type == Cell.Type.Ice)
            {
                var r = Random.Range(0f, 1f);
                if (r > probNotToSlip) action = _currentCell.ReturnRandomAction(intendedAction);
            }

            //change sprite
            if (action == "left")
            {
                _sprite.flipX = true;
            }
            else if (action == "right")
            {
                _sprite.flipX = false;
            }
            
            
            _nextCell = _gridManager.GetNextCell(_currentCell.transform.position, action);

            _nextCell.OnPlayerEnter();
            var sample = _nextCell.instantReward + penalty*(_nextCell.ReturnBestValue()); 
            var newValue = (1f - alpha) * _currentCell.GetActionValue(intendedAction) + alpha * sample;
            
            
            _currentCell.SetActionValue(intendedAction, newValue);

            _currentCell = _nextCell;



            if (_currentCell.isGrounded)
            {
                GroundPlayer();
            }
            else
            {
                if (_grounded) //means walked off the edge
                {
                    _grounded = false;
                    _shouldFallDown = true;
                }
            }

            float tempMoveSpeed = moveSpeed;

            if (!_grounded) //if in air move twice as fast
            {
                _animator.speed = 2;
                tempMoveSpeed = moveSpeed / 2;
                _ease = Ease.InExpo;
            }
            else
            {
                _animator.speed = 1;
                _ease = Ease.Linear;
            }

            if (_groundedJump)  //wait before jumping
            {
                _groundedJump = false;
                yield return new WaitForSeconds(tempMoveSpeed); 
            }
            
            _animator.SetBool("isWalking", true);
            transform.DOMove(_currentCell.transform.position, tempMoveSpeed)
                .OnComplete(() => transform.position = _currentCell.transform.position).SetEase(Ease.Linear);
            yield return new WaitForSeconds(tempMoveSpeed);
            _animator.SetBool("isWalking", false);

            if (_currentCell.type == Cell.Type.Exit)
            {
                _gridManager.OnPlayerRestart();
                GroundPlayer();
                StartCoroutine(Learn());
                break;
            }
        }

    }

    
    
    private void GroundPlayer()
    {
        _shouldFallDown = false;
        _grounded = true;
        _jumpCount = 2;
    }
}
