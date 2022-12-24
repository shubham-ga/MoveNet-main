using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterMove : MonoBehaviour
{
    [SerializeField] CharacterController controller;
    [SerializeField] Vector3 playerVelocity;
    [SerializeField] Transform playerTransform;
    private bool groundedPlayer, isFirst = false, isDoorOpen;
    public float playerSpeed = 4.0f, waterSpeed = 0.4f, speedMultiPlayer = 10f;
    private float jumpHeight = 1.0f;
    private float gravityValue = -9.81f;
    [SerializeField] Text touchTxt;
    [SerializeField] internal Animator playerAnimator, doorAnimator;
    [SerializeField] internal bool isStart, isUp, isGravity = true, isWater, isMountainLevel, isMountainClimb;
    [SerializeField] internal GameObject Player;
    [SerializeField] internal Vector3 ladderEndPos;
    [SerializeField] float t;
    private void Start()
    {
        GameManager.gameManager.characterMove = this;
    }
    void Update()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        // blazepos input
        if (playerSpeed > 0)
        {
            Vector3 movement = Vector3.zero;
            movement += playerTransform.forward * playerSpeed * speedMultiPlayer * Time.deltaTime;
            controller.Move(movement);
        }

        if (isMountainLevel == false)
        {
            if (isStart)
            {
                /*Vector3 movement = Vector3.zero;
                float v = Input.GetAxis("Vertical");
                float h = Input.GetAxis("Horizontal");
                movement += transform.forward * v * playerSpeed * Time.deltaTime;
                movement += transform.right * h * playerSpeed * Time.deltaTime;
                movement += Physics.gravity;
                controller.Move(movement);
                transform.Rotate(new Vector3(0, h * Time.deltaTime * 360, 0));*/

                if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
                {
                    Vector3 movement = Vector3.zero;
                    float v = Input.GetAxis("Vertical");
                    movement += playerTransform.forward * v * playerSpeed * Time.deltaTime;
                    controller.Move(movement);
                    playerAnimator.SetBool("isRun", true);
                }
                if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.W))
                {
                    playerAnimator.SetBool("isRun", false);
                }

                if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                {
                    Vector3 movement = Vector3.zero;
                    float h = Input.GetAxis("Horizontal");
                    movement += playerTransform.right * h * playerSpeed * Time.deltaTime;
                    controller.Move(movement);
                    playerTransform.Rotate(new Vector3(0, h * Time.deltaTime * 360, 0));
                    //playerAnimator.SetBool("isRun", true);
                }
            }
        }

        #region //only for mountain climbing
        if (isMountainLevel)
        {
            if (isMountainClimb)
            {
                //if ((controller.collisionFlags & CollisionFlags.Below) == 0)
                //{
                //    Debug.Log("53");
                //}
                playerAnimator.SetBool("isRun", false);
                if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
                {
                    Vector3 move = new Vector3(0, Input.GetAxis("Vertical"), 0);
                    controller.Move((move * Time.deltaTime) / 2);
                    playerAnimator.SetBool("isMountainUp", true);
                }
                if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.W))
                {
                    playerAnimator.SetBool("isMountainUp", false);
                }

                if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                {
                    Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, 0);
                    controller.Move((move * Time.deltaTime) / 2);
                    playerAnimator.SetBool("isLeftHang", true);
                }
                if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.A))
                {
                    playerAnimator.SetBool("isLeftHang", false);
                }

                if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                {
                    Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, 0);
                    controller.Move((move * Time.deltaTime) / 2);
                    playerAnimator.SetBool("isRightHang", true);
                }
                if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.D))
                {
                    playerAnimator.SetBool("isRightHang", false);
                }
            }
            else
            {
                float h = Input.GetAxis("Horizontal");
                float v = Input.GetAxis("Vertical");
                Vector3 movement = Vector3.zero;
                movement += transform.forward * v * playerSpeed * Time.deltaTime;
                movement += transform.right * h * playerSpeed * Time.deltaTime;

                controller.Move(movement);
                transform.Rotate(new Vector3(0, h * Time.deltaTime * 360, 0));

                if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
                {
                    playerAnimator.SetBool("isRun", true);
                }
                if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.W))
                {
                    playerAnimator.SetBool("isRun", false);
                }
            }
        }
        #endregion

        if (isUp)
        {
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                playerAnimator.SetBool("isClimb", true);
                Vector3 move = new Vector3(0, Input.GetAxis("Vertical"), 0);
                controller.Move((move * Time.deltaTime) / 2);
            }
            if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.W))
            {
                playerAnimator.SetBool("isClimb", false);
            }
        }

        if (isGravity)
        {
            playerVelocity.y += gravityValue * Time.deltaTime;
            controller.Move(playerVelocity * Time.deltaTime);
        }

        if (isWater)
        {
            Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            controller.Move(move * Time.deltaTime * waterSpeed);

            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                playerAnimator.SetBool("isSwimming", true);
                playerAnimator.speed = 1.4f;
            }
            if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.W))
            {
                playerAnimator.speed = 0.2f;
            }
        }

        //if (move != Vector3.zero)
        //{
        //    gameObject.transform.forward = move;
        //}

        // Changes the height position of the player..
        if (Input.GetButtonDown("Jump") && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }

        if (isDoorOpen)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                StartCoroutine(ForOpenDoor());
            }
        }
    }

    // for first time touch to game start
    public void OnStart()
    {
        if (isFirst == false)
        {
            touchTxt.text = "";
            isStart = true;
            //playerAnimator.SetBool("isRun", true);
            isFirst = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("ladder"))
        {
            playerAnimator.SetBool("isRun", false);
            isStart = false;
            isUp = true;
            isGravity = false;
        }
        if (other.gameObject.CompareTag("EndLadder"))
        {
            isUp = false;
            playerAnimator.SetBool("isClimbEnd", true);
        }
        if (other.gameObject.CompareTag("door"))
        {
            doorAnimator = other.gameObject.GetComponentInParent<Animator>();
            playerAnimator.SetBool("isRun", false);
            isStart = false;
            isDoorOpen = true;
            other.GetComponent<Collider>().enabled = false;
        }
        if (other.gameObject.CompareTag("Water"))
        {
            //Debug.Log("water");
            //isGravity = false;
        }
        if (other.gameObject.CompareTag("mountain"))
        {
            //Debug.Log("181");
            playerAnimator.SetBool("isRun", false);
            isMountainClimb = true;
            isGravity = false;
        }
        if (other.gameObject.CompareTag("MountainEnd"))
        {
            playerAnimator.SetBool("isMountainEnd", true);
            //Debug.Log("219");
        }
        if (other.gameObject.CompareTag("CaveWater"))
        {
            t = Camera.main.transform.localPosition.z;
            //Debug.Log("243");
            //StartCoroutine(CameraZoomOut(t));
            Camera.main.transform.localPosition += new Vector3(0, 0, -1.7f);
            other.GetComponent<BoxCollider>().enabled = false;
            //iTween.MoveTo(Camera.main.gameObject, Camera.main.transform.position + new Vector3(0, 0, -1.5f), 2f);
            // Camera.main.transform.localPosition = Vector3.MoveTowards(Camera.main.transform.localPosition, Camera.main.transform.localPosition - new Vector3(0, 0, 1.7f), t/Camera.main.transform.localPosition.z);
        }
        //if (other.gameObject.CompareTag("Terrain"))
        //{
        //    isGravity = true;
        //}
    }

    // for open door and kick animation
    IEnumerator ForOpenDoor()
    {
        playerAnimator.SetBool("isKick", true);
        yield return new WaitForSeconds(0.6f);
        doorAnimator.SetBool("isDoor", true);
        yield return new WaitForSeconds(1.3f);
        //Debug.Log("after kick");
        playerAnimator.SetBool("isKick", false);
        //playerAnimator.SetBool("isRun", true);
        isStart = true;
        isDoorOpen = false;
    }

    /*private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("mountain"))
        {
            playerAnimator.SetBool("isRun", false);
            // Debug.Log("m");
            isMountainClimb = true;
            isGravity = false;
        }
    }*/

    // for camera zomm out
    IEnumerator CameraZoomOut(float t)
    {
        while (Camera.main.transform.localPosition.z > -3f)
        {
            t -= 0.02f;
            Camera.main.transform.localPosition += new Vector3(0, 0, t);
            yield return new WaitForSeconds(0.4f);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("CaveWaterExit"))
        {
            //Debug.Log("243");
            //StartCoroutine(CameraZoomOut());
            Camera.main.transform.localPosition += new Vector3(0, 0, 1.7f);
            //iTween.MoveTo(Camera.main.gameObject, Camera.main.transform.position + new Vector3(0, 0, 1.5f), 2f);
        }
    }
}