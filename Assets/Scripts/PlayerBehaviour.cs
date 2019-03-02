using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;
using UnityEngine.Tilemaps;
using Cinemachine;
using TMPro;
using System;
using UnityEngine.SceneManagement;



public class PlayerBehaviour : MonoBehaviour
{
    [Header("Récupération des components")]
    public Rigidbody2D rb;
    public Tilemap tilemapVictory;
    public Tilemap wallTilempa;
    public Tilemap whiteTilemap;
    public TilemapRenderer whiteTilemapRenderer;
    public Camera cam;
    public CinemachineVirtualCamera camPlayer;
    public TextMeshProUGUI timerText;

    [Header("Player Movement Set Up")]
    private Vector2 direction;
    public float moveSpeed;
    public float minDistance = 5;

    [Header("Parameters Set Up - BlindPath")]
    public float currentScore;
    public float victoryScore;
    public float currentTimer;
    public float resetTimerAt;
    private bool danger;
    public int sceneIndex;


    [Header("Vibrations Set Up")]
    private RaycastHit2D leftRay;
    private RaycastHit2D rightRay;
    private RaycastHit2D upRay;
    private int raycastLayer;
    public float offset;
    [SerializeField]
    private float leftMotor;
    [SerializeField]
    private float rightMotor;
    [SerializeField]
    private float upMotors;
    public bool useVibration = true;
    public AnimationCurve vibrationCurve;






    private void OnApplicationQuit()
    {
        GamePad.SetVibration(PlayerIndex.One, 0,0);
    }


    private void Awake()
    {
        raycastLayer = LayerMask.GetMask("Raycast");
        currentTimer = resetTimerAt;
    }


    void Update()
    {
        Move();

        if (Input.GetKeyUp(KeyCode.R))
        {
            GameOver();
        }
        
        if(useVibration)
        {
            CheckVibration();
        }
        else
        {
            GamePad.SetVibration(PlayerIndex.One, 0, 0);

            if (currentScore == victoryScore)
            {
                Debug.Log("Winner");
            }

            if(danger)
            {
                currentTimer -= Time.deltaTime;
            }

            int timerInt = (int)currentTimer;
            timerText.text = timerInt.ToString();

            if(currentTimer < 0)
            {
                GameOver();
            }

            Bounds PCBounds = this.gameObject.GetComponent<Collider2D>().bounds;

            Vector3 colliderFront = new Vector3(transform.position.x, PCBounds.max.y, PCBounds.max.z);
            Vector3 colliderBack = new Vector3(transform.position.x, PCBounds.min.y, PCBounds.max.z);
            Vector3 colliderLeftSide = new Vector3(PCBounds.min.x, transform.position.y, PCBounds.max.z);
            Vector3 colliderRightSide = new Vector3(PCBounds.max.x, transform.position.y, PCBounds.max.z);

            DestroyTile(colliderFront);
            DestroyTile(colliderBack);
            DestroyTile(colliderLeftSide);
            DestroyTile(colliderRightSide);
        }
    }

    private void GameOver()
    {
        Debug.Log("Loser");
        SceneManager.LoadScene(sceneIndex);
    }

    /// <summary>
    /// Déplacement du joueur
    /// </summary>
    private void Move()
    {
        direction.x = Input.GetAxis("Horizontal");
        direction.y = Input.GetAxis("Vertical");

        rb.velocity = direction.normalized * moveSpeed * Time.deltaTime;

        //Debug de Raycast
        Debug.DrawRay(new Vector2(transform.position.x - offset, transform.position.y), Vector2.left, Color.blue, minDistance);
        Debug.DrawRay(new Vector2(transform.position.x + offset, transform.position.y), Vector2.right, Color.red, minDistance);
        Debug.DrawRay(new Vector2(transform.position.x, transform.position.y + offset), Vector2.up, Color.white, minDistance);

    }


    /// <summary>
    /// Set up les vibrations gauche et droite en fonction des différents Raycasts
    /// </summary>
    public void CheckVibration()
    {
        leftRay = Physics2D.Raycast(new Vector2(transform.position.x - offset, transform.position.y), Vector2.left, minDistance, raycastLayer);
        rightRay = Physics2D.Raycast(new Vector2(transform.position.x + offset, transform.position.y), Vector2.right, minDistance, raycastLayer);
        upRay = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + offset), Vector2.up, minDistance, raycastLayer);



        if (leftRay.collider)
        {
            float percent = leftRay.distance / minDistance;
            leftMotor = vibrationCurve.Evaluate(percent);
            Debug.Log("gauche" + leftRay.distance);
        }

        if (rightRay.collider)
        {
            float percent = rightRay.distance / minDistance;
            rightMotor = vibrationCurve.Evaluate(percent);
            Debug.Log("droite" + rightRay.distance);
        }

        if (upRay.collider)
        {
            float percent = upRay.distance / minDistance;
            upMotors = vibrationCurve.Evaluate(percent);
            Debug.Log("haut" + upRay.distance);
        }



        if(upRay.collider == null)
        {
            upMotors = vibrationCurve.Evaluate(1);
        }

        if (rightRay.collider == null)
        {
            rightMotor = vibrationCurve.Evaluate(1);
        }

        if (leftRay.collider == null)
        {
            leftMotor = vibrationCurve.Evaluate(1);
        }


        VibrationUpdate();

    }

    /// <summary>
    /// Update les Vibrations en fonction du Set Up posé dans le CheckVibration()
    /// </summary>
    private void VibrationUpdate()
    {
        GamePad.SetVibration(PlayerIndex.One, (upMotors + leftMotor), (rightMotor + upMotors));
    }

    /// <summary>
    /// Destruction des Tiles
    /// </summary>
    /// <param name="pos"></param>
    void DestroyTile(Vector3 pos)
    {
        Vector3Int tilePosition = tilemapVictory.WorldToCell(pos);

        if (tilemapVictory.HasTile(tilePosition))
        {
            currentScore += 1;
            tilemapVictory.SetTile(tilePosition, null);
            currentTimer = resetTimerAt;
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == 0)
        {
            if(useVibration)
            {
                tilemapVictory.color = Color.green;
                cam.backgroundColor = Color.white;
                whiteTilemapRenderer.sortingOrder = -1;
            }
        }
        
        if(collision.gameObject.layer == 5)
        {
            wallTilempa.color = Color.white;
            whiteTilemap.color = Color.green;
            camPlayer.gameObject.SetActive(false);
            danger = false;
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 5)
        {
            wallTilempa.color = Color.black;
            whiteTilemap.color = Color.red;
            camPlayer.gameObject.SetActive(true);
            danger = true;
        }
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 5)
        {
            currentTimer = resetTimerAt;
        }
    }

}
