using UnityEngine;

public class WallController : MonoBehaviour
{
    public void Start()
    {
        var leftWall = GameObject.Find("LeftWall");
        var rightWall = GameObject.Find("RightWall");
    }

    public void Update()
    {
    }


    public void ChangeWallTigger(string name, bool value)
    {
        var go = GameObject.Find(name);
        var collider = go.GetComponent<BoxCollider2D>();
        collider.isTrigger = value;
    }

}
