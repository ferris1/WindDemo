using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerMovementInputController : MonoBehaviour
{
    private NavMeshAgent _agent;
    public Vector2 _move;
    public Vector2 _look;
    public float aimValue;
    public float fireValue;

    public Vector3 nextPosition;
    public Quaternion nextRotation;

    public float rotationPower = 3f;
    public float rotationLerp = 0.5f;
    public float lastTime = 0;
    public float speed = 1f;
    public Camera camera;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    public void OnMove(InputValue value)
    {
        
        _move = value.Get<Vector2>();
        GameMgr.inst.SendMovePakcet();
    }

    public void OnLook(InputValue value)
    {
        _look = value.Get<Vector2>();
        GameMgr.inst.SendMovePakcet();
    }

    public void OnAim(InputValue value)
    {
        //aimValue = value.Get<float>();
    }
    
    public void OnFire(InputValue value)
    {
        fireValue = value.Get<float>();
    }

    public GameObject followTransform;

    private void Update()
    {


        //Rotate the Follow Target transform based on the input
        followTransform.transform.rotation *= Quaternion.AngleAxis(_look.x * rotationPower, Vector3.up);
        followTransform.transform.rotation *= Quaternion.AngleAxis(_look.y * rotationPower, Vector3.right);

        var angles = followTransform.transform.localEulerAngles;
        angles.z = 0;

        var angle = followTransform.transform.localEulerAngles.x;

        //Clamp the Up/Down rotation
        if (angle > 180 && angle < 340)
        {
            angles.x = 340;
        }
        else if(angle < 180 && angle > 40)
        {
            angles.x = 40;
        }

        followTransform.transform.localEulerAngles = angles;
        if (Time.realtimeSinceStartup - lastTime > 1)
        {
            lastTime = Time.realtimeSinceStartup;
            GameMgr.inst.SendPlayerTransform();
        }
        if (_move.x == 0 && _move.y == 0) 
        {   
            nextPosition = transform.position;

            if (aimValue == 1)
            {
                transform.rotation = Quaternion.Euler(0, followTransform.transform.rotation.eulerAngles.y, 0);
                followTransform.transform.localEulerAngles = new Vector3(angles.x, 0, 0);
            }
            return; 
        }
        float moveSpeed = speed / 100f;
        Vector3 position = (transform.forward * _move.y * moveSpeed) + (transform.right * _move.x * moveSpeed);
        nextPosition = transform.position + position;       

        transform.rotation = Quaternion.Euler(0, followTransform.transform.rotation.eulerAngles.y, 0);
        followTransform.transform.localEulerAngles = new Vector3(angles.x, 0, 0);
        
    }

   
}
