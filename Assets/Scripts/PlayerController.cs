using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public ControlsController controls;

    public float moveSpeed = 1200.0f;

    public int playerPosition = -1;

    private Rigidbody2D rigidbody;
    private Animator animator;
    private WallController wallController;

    public bool didJumpLeft;
    public bool didJumpRight;

    public bool haveToSwitchToRight = false;
    public bool haveToSwitchToLeft = false;

    public int divingInWall = 0; // 0 - none, 1 - left, 2 - right, 
    public int comingFromWall = 0; // 0 - none, 1 - left, 2 - right

    private float middleWallX;

    public void Start()
    {
        this.rigidbody = this.GetComponent<Rigidbody2D>();
        this.animator = this.GetComponent<Animator>();
        this.wallController = this.GetComponent<WallController>();

        var middleWallTransform = GameObject.Find("MiddleWall").GetComponent<Transform>();
        this.middleWallX = middleWallTransform.position.x;
    }

    public void Update()
    {
        if (this.controls.tapLeft && this.playerPosition == 1)
        {
            haveToSwitchToLeft = true;
        }
        else if (this.controls.tapRight && this.playerPosition == -1)
        {
            haveToSwitchToRight = true;
        }

        if (this.controls.holdLeft)
        {
            this.didJumpLeft = true;
        }
        else if (this.controls.holdRight)
        {
            this.didJumpRight = true;
        }
        else
        {
            this.didJumpLeft = false;
            this.didJumpRight = false;
        }
    }

    public void FixedUpdate()
    {
        var pos = this.transform.position;

        pos.y = 2.2f;
        this.transform.position = pos;


        if (this.haveToSwitchToRight)
        {
            this.wallController.ChangeWallTigger("LeftWall", true);
            this.rigidbody.AddForce(new Vector2(-moveSpeed, 0));
        }
        else if (this.haveToSwitchToLeft)
        {
            this.wallController.ChangeWallTigger("RightWall", true);
            this.rigidbody.AddForce(new Vector2(moveSpeed, 0));
        }

        if (!haveToSwitchToLeft && !haveToSwitchToRight)
        {
            if (this.didJumpLeft && this.playerPosition == -1)
            {
                this.rigidbody.AddForce(new Vector2(this.moveSpeed, 0));
                this.animator.SetBool("LeftJump", true);
            }
            else if (this.didJumpRight && this.playerPosition == 1)
            {
                this.rigidbody.AddForce(new Vector2(-this.moveSpeed, 0));
            }
            else
            {
                if (this.playerPosition == -1)
                {
                    this.rigidbody.AddForce(new Vector2(-this.moveSpeed, 0));
                }
                else if (this.playerPosition == 1)
                {
                    this.rigidbody.AddForce(new Vector2(this.moveSpeed, 0));
                }
            }
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (comingFromWall == 0)
        {
            if (other.gameObject.name == "LeftWall")
            {
                this.divingInWall = 1;
                Debug.Log("diving in left");
            }

            if (other.gameObject.name == "RightWall")
            {
                this.divingInWall = 2;
                Debug.Log("diving in right");
            }
        }
        if (divingInWall == 0)
        {
            if (other.gameObject.name == "LeftWall")
            {
                this.comingFromWall = 1;
                Debug.Log("coming in right-middle");
            }

            if (other.gameObject.name == "RighttWall")
            {
                this.comingFromWall = 2;
                Debug.Log("coming in right");
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Wall") && this.divingInWall != 0)
        {
            this.divingInWall = 0;

            if (this.playerPosition == -1)
            {
                Debug.Log("changed pplayer pos -1");
                // move the player to the right
                this.playerPosition = 1;
                this.comingFromWall = 2;

                this.wallController.ChangeWallTigger("RightWall", true);
                this.wallController.ChangeWallTigger("LeftWall", false);
                this.wallController.ChangeWallTigger("MiddleWall", false);

                this.rigidbody.Sleep();
                var transform = this.rigidbody.transform;
                //TODO:  remove hardcoded numbers
                this.rigidbody.transform.position = new Vector2(7f, transform.position.y);
                //this.rigidbody.AddForce(new Vector2(-this.moveSpeed, 0));
            }
            else if (this.playerPosition == 1)
            {
                Debug.Log("changed pplayer pos 1");
                // move the player to the left
                this.playerPosition = -1;
                this.comingFromWall = 1;

                this.wallController.ChangeWallTigger("RightWall", false);
                this.wallController.ChangeWallTigger("LeftWall", true);
                this.wallController.ChangeWallTigger("MiddleWall", false);

                this.rigidbody.Sleep();
                var transform = this.rigidbody.transform;
                //TODO:  remove hardcoded numbers
                this.rigidbody.transform.position = new Vector2(-1f, transform.position.y);
                this.rigidbody.AddForce(new Vector2(this.moveSpeed, 0));
            }
        }

        else if (other.CompareTag("Wall") && this.comingFromWall != 0)
        {
            if (this.comingFromWall == 2)
            {
                this.haveToSwitchToRight = false;
                this.wallController.ChangeWallTigger("RightWall", false);
            }
            else if (this.comingFromWall == 1)
            {
                this.haveToSwitchToLeft = false;
                this.wallController.ChangeWallTigger("LeftWall", false);
            }

            this.comingFromWall = 0;
        }
    }
}
