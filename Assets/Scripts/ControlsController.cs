using UnityEngine;

public class ControlsController : MonoBehaviour
{
    public float touchBorderX;

    public bool tapLeft = false;
    public bool holdLeft = false;
    public bool tapRight = false;
    public bool holdRight = false;


    // Use this for initialization
    void Start()
    {

    }

    // -1 - left, 0 - non selected, 1 - right
    private int getControlSide()
    {
        int controllingSide = 0;

        float axis = Input.GetAxis("Horizontal");
        if (axis < 0)
        {
            controllingSide = -1;
        }
        else if (axis > 0)
        {
            controllingSide = 1;
        }


        // for touch devices
        if (Input.touchSupported)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.position.x < touchBorderX)
            {
                controllingSide = -1;
            }
            else
            {
                controllingSide = 1;
            }
        }

        if (Input.mousePosition.x < touchBorderX && axis == 0.0f)
        {
            controllingSide = -1;
        }
        else if (Input.mousePosition.x > touchBorderX && axis == 0.0f)
        {
            controllingSide = 1;
        }

        return controllingSide;
    }

    // Update is called once per frame
    void Update()
    {
        bool down = Input.GetButtonDown("Horizontal") || Input.GetButtonDown("Fire1");
        bool held = Input.GetButton("Horizontal") || Input.GetButton("Fire1");
        bool up = Input.GetButtonUp("Horizontal") || Input.GetButtonUp("Fire1");

        if (down)
        {
            if (getControlSide() < 0)
            {
                tapLeft = true;
            }
            else
            {
                tapRight = true;
            }
        }
        else if (held)
        {
            if (getControlSide() < 0)
            {
                holdLeft = true;
            }
            else
            {
                holdRight = true;
            }
        }
        else if (up)
        {
            tapLeft = false;
            holdLeft = false;
            tapRight = false;
            holdRight = false;
        }
    }
}