using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;
using UnityEngine.Tilemaps;
using Cinemachine;


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

    [Header("Set up Player Movement")]
    private Vector2 direction;
    public float moveSpeed;
    public float minDistance = 5;

    [Header("Victory Set up pour BlindPath")]
    public float currentScore;
    public float victoryScore;


    [Header("Set up Vibrations Behaviour")]
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
    }


    void Update()
    {
        Move();


        if(useVibration)
        {
            CheckVibration();

            if(currentScore == victoryScore)
            {
                Debug.Log("Winner");
            }
        }
        else
        {
            GamePad.SetVibration(PlayerIndex.One, 0, 0);

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


    private void Move()
    {
        direction.x = Input.GetAxis("Horizontal");
        direction.y = Input.GetAxis("Vertical");

        rb.velocity = direction.normalized * moveSpeed * Time.deltaTime;

        Debug.DrawRay(new Vector2(transform.position.x - offset, transform.position.y), Vector2.left, Color.blue, minDistance);
        Debug.DrawRay(new Vector2(transform.position.x + offset, transform.position.y), Vector2.right, Color.red, minDistance);
        Debug.DrawRay(new Vector2(transform.position.x, transform.position.y + offset), Vector2.up, Color.white, minDistance);

    }

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

    private void VibrationUpdate()
    {
        GamePad.SetVibration(PlayerIndex.One, (upMotors + leftMotor), (rightMotor + upMotors));
    }

    void DestroyTile(Vector3 pos)
    {
        Vector3Int tilePosition = tilemapVictory.WorldToCell(pos);

        if (tilemapVictory.HasTile(tilePosition))
        {
            currentScore += 1;
            tilemapVictory.SetTile(tilePosition, null);
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
            else
            {
                
            }
        }
        
        if(collision.gameObject.layer == 5)
        {
            wallTilempa.color = Color.white;
            whiteTilemap.color = Color.green;
            camPlayer.gameObject.SetActive(false);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 5)
        {
            wallTilempa.color = Color.black;
            tilemapVictory.color = Color.red;
            camPlayer.gameObject.SetActive(true);
        }
    }

}
