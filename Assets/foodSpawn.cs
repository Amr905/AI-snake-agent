using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class foodSpawn : MonoBehaviour
{
    public float rate;
    public GameObject foodPrefab;
    public Transform borderTop;
    public Transform borderBottom;
    public Transform borderLeft;
    public Transform borderRight;
    public Text text;
    void Start()
    {
        StartCoroutine(repeatInvoke());
       
        //InvokeRepeating("spawn", 1, rate);
    }
    IEnumerator repeatInvoke()
    {
        while (true)
        {
            yield return new WaitForSeconds(rate);
            spawn();
        }
    }
    void Update()
    {
        if (Input.GetKey(KeyCode.P))
        {
            if (rate - 0.1f >= 0.3f)
                rate -= 0.1f;
            else rate = 0.3f;
        }
        if (Input.GetKey(KeyCode.O))
        {
            if (rate + 0.1f <= 10)
                rate += 0.1f;
            else rate = 10;
        }
        text.text = "Food Spawn Rate: " + rate + " ";
    }
    void spawn()
    {

        float x = Random.Range(borderLeft.position.x+1, borderRight.position.x-1);
        float y = Random.Range(borderBottom.position.y+1, borderTop.position.y-1);
      
       Instantiate(foodPrefab, new Vector2(x, y), Quaternion.identity);
       // SnakeAgent.ToGo = SnakeAgent.objectsInPlay[0];
    }

}
