using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;


public class PlayerBehaviour : MonoBehaviour
{

    public Rigidbody2D rb;
    private Vector2 direction;

    public float moveSpeed;
    public float minDistance = 5;

    

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
        //offset = GetComponent<BoxCollider2D>().size.x;
        
    }


    void Update()
    {
        Move();


        if(useVibration)
        {
            CheckVibration();
        }
        else
        {
            GamePad.SetVibration(PlayerIndex.One, 0, 0);
        }
    }


    private void Move()
    {
        direction.x = Input.GetAxis("Horizontal");
        direction.y = Input.GetAxis("Vertical");

        rb.velocity = direction.normalized * moveSpeed * Time.deltaTime;

        Debug.DrawRay(new Vector2(transform.position.x - offset, transform.position.y), Vector2.left, Color.blue, minDistance);
        Debug.DrawRay(new Vector2(transform.position.x + offset, transform.position.y), Vector2.right, Color.white, minDistance);
        Debug.DrawRay(new Vector2(transform.position.x, transform.position.y + offset), Vector2.up, Color.red, minDistance);

    }

    public void CheckVibration()
    {
        leftRay = Physics2D.Raycast(new Vector2(transform.position.x - offset, transform.position.y), Vector2.left, minDistance, raycastLayer);
        rightRay = Physics2D.Raycast(new Vector2(transform.position.x + offset, transform.position.y), Vector2.right, minDistance, raycastLayer);
        upRay = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + offset), Vector2.up, minDistance, raycastLayer);

        Debug.Log("gauche" + leftRay.distance);
        Debug.Log("droite" + rightRay.distance);
        Debug.Log("haut" + upRay.distance);


        if (leftRay.collider)
        {
            float percent = leftRay.distance / minDistance;
            leftMotor = vibrationCurve.Evaluate(percent);
        }
        


        if (rightRay.collider)
        {
            float percent = rightRay.distance / minDistance;
            rightMotor = vibrationCurve.Evaluate(percent);
        }
        


        if (upRay.collider)
        {
            float percent = upRay.distance / minDistance;
            upMotors = vibrationCurve.Evaluate(percent);
        }
        
        

        VibrationUpdate();

    }

    private void VibrationUpdate()
    {
        GamePad.SetVibration(PlayerIndex.One, (upMotors + leftMotor), (rightMotor + upMotors));
    }



}
