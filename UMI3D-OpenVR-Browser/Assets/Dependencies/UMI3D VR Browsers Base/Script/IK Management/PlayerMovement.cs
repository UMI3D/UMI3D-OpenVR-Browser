/*
Copyright 2019 - 2022 Inetum

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using UnityEngine;


namespace umi3dVRBrowsersBase.ikManagement
{
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
        private Vector2 Movement = Vector2.zero;

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
}