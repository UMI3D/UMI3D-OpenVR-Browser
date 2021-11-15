//using BrowserDesktop.Controller;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    //variables

    [SerializeField] private float moveSpeed;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;

    private Vector3 moveDirection;
    private Vector3 velocity;

    [SerializeField] private bool isGrounded;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float gravity;

    [SerializeField] private float jumpHeight;

    //references
    Vector2 Movement = Vector2.zero;

    private Animator anim;

    //private IKControl IKControl;

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        //IKControl = this.GetComponent<IKControl>();
    }

    private void Update()
    {
        //if (Input.GetKey(InputLayoutManager.GetInputCode(InputLayoutManager.Input.Forward))) { Movement.x += 1; }
        //if (Input.GetKey(InputLayoutManager.GetInputCode(InputLayoutManager.Input.Backward))) { Movement.x -= 1; }
        //if (Input.GetKey(InputLayoutManager.GetInputCode(InputLayoutManager.Input.Right))) { Movement.y += 1; }
        //if (Input.GetKey(InputLayoutManager.GetInputCode(InputLayoutManager.Input.Left))) { Movement.y -= 1; }

        //Move();

        //Movement = Vector2.zero;

        //IKControl.feetIkActive = Input.GetKey(InputLayoutManager.GetInputCode(InputLayoutManager.Input.Squat));

        //Idle();
    }

    //private void Move()
    //{
    //    if(Movement.x != 0 && !Input.GetKey(KeyCode.LeftShift))
    //    {
    //        //walk
    //        Walk();
    //    }
    //    else if (Movement.x != 0 && Input.GetKey(KeyCode.LeftShift))
    //    {
    //        //run
    //        Run();
    //    }
    //    else if(Movement.x == 0)
    //    {
    //        //idle
    //        Idle();
    //    }

    //    moveDirection *= moveSpeed;
    //}

    private void Idle()
    {
        anim.SetFloat("Speed", 0f, 0.1f, Time.deltaTime);
    }

    private void Walk()
    {
        moveSpeed = walkSpeed;
        anim.SetFloat("Speed", 0.5f, 0.1f, Time.deltaTime);
    }

    private void Run()
    {
        moveSpeed = runSpeed;
        anim.SetFloat("Speed", 1f, 0.1f, Time.deltaTime);
    }
}
