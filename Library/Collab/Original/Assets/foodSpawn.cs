using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class foodSpawn : MonoBehaviour
{
    public float rate;
    public GameObject foodPrefab;
    public Transform borderTop;
    public Transform borderBottom;
    public Transform borderLeft;
    public Transform borderRight;
    void Start()
    {
        InvokeRepeating("spawn", 1, rate);
    }
    void spawn()
    {
        int x = (int)Random.Range(borderLeft.position.x+1, borderRight.position.x-1);
        int y = (int)Random.Range(borderBottom.position.y+1, borderTop.position.y-1);
        GameObject temp =Instantiate(foodPrefab, new Vector2(x, y), Quaternion.identity);
            // SnakeAgent.ToGo = SnakeAgent.objectsInPlay[0];
    }

}
