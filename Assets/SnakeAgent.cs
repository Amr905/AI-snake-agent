using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeuralNetwork;
using UnityEngine.UI;
using System.Xml;

using System.IO;
using System.Xml.Serialization;
using System;

public class SnakeAgent : MonoBehaviour
{
    //Neural Network Variables
    public int numberOfDatasets;
    public int collectedData = 0;
    private bool trained;
    private const double MinimumError = 0.01;
    private const TrainingType TrType = TrainingType.MinimumError;
    private static NeuralNet net;
    private static List<DataSet> dataSets;
    //Agent Variables
    public bool automovement = true;
    public Sprite[] sprites;
    public bool secondChecker = true;
    public int EnergyPoints = 50;
    public int EnergyValue = 10;
    public int CollectedFood = 0;
    public Text text;
    public int MaxCollectedFood = 0;
    public GameObject tailPrefab;
    private GameObject objectToGoTo;
    Vector2 lastPosition;
    Transform position;
    public GameObject head;
    private Vector2 dir = new Vector2(.4f, 0), dir2;
    private List<Transform> tail = new List<Transform>();
    Vector2[] fourDir = { new Vector2(0, .4f), new Vector2(0, -.4f), new Vector2(.4f, 0), new Vector2(-.4f, 0) };
    public static GameObject ToGo;
    public Image[] links;
    public Sprite up, down, left, right;
    public Transform borderTop;
    public Transform borderBottom;
    public Transform borderLeft;
    public Transform borderRight;
    private int EnergyCounter;
    private int EnergyCounterLimit;
    List<GameObject> FoodList;
    List<GameObject> EnergyList;
    private float dirx = .4f, diry = .4f;//length and height of snke 
    public Boolean loadDataset = false;
    public SpriteRenderer Focus;
    private bool IsPaused = false;
    private GameObject pausePanel;
    private Vector2 tempVector2 = new Vector2(0, 0);
    void Start()
    {
        Focus.enabled = false;
        net = new NeuralNet(5, 25, 1);
        dataSets = new List<DataSet>();
        EnergyCounter = 0;
        EnergyCounterLimit = 10;
        FoodList = new List<GameObject>();
        EnergyList = new List<GameObject>();
        /*   int test = 10;
           //   SerializeObject(test, "test.ser");
           WriteToBinaryFile("test.ser", test);
           test = 20;
           test = ReadFromBinaryFile<int>("test.ser");
           //   dataSets = ReadFromBinaryFile<List<DataSet>>("dataset.ser");
           //  test =  DeSerializeObject<int>("test.ser");


          // Debug.Log("object read is :" + test);*/// testing writing and reading from file 
        InvokeRepeating("move", 0, .12f);
    }
    private void UiInput()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            // if(IsPaused)
            {
                Time.timeScale = 0;
                if (ToGo != null)
                    Focus.transform.position = ToGo.transform.position;
                Focus.enabled = true;

                // pausePanel.SetActive(true);
            }
        }
        else
        {
            Focus.enabled = false;
            Time.timeScale = 1;
            //  pausePanel.SetActive(false);
        }
        if (Input.GetKey(KeyCode.F))
        {
            if (IsPaused)
            {
                automovement = false;
                secondChecker = false;

            }
            else
            {
                automovement = true;
                secondChecker = true;
            }
            IsPaused ^= true;
        }
    }
    void DisplayLinks(double[] input)
    {
        for (int i = 0; i < 4; ++i)
        {
            links[i].enabled = input[i] > .5;
        }
        if (input[4] == 0)
            links[4].color = Color.cyan;
        else if (input[4] == .25)
            links[4].color = Color.green;
        else if (input[4] == .5)
            links[4].color = Color.red;
        else
            links[4].color = Color.yellow;
    }

    private void Update()
    {
        PerceiveEnvironment();
        DisplayUI_Text();
        DisplayLinks(Train());
        UiInput();
    }
    void PerceiveEnvironment()//The snake perceives the environment, as it detects food and energy
    {
        MaxCollectedFood = Mathf.Max(CollectedFood, MaxCollectedFood);
        Collider2D[] collideList = Physics2D.OverlapCircleAll(transform.position, 5f, 1 << LayerMask.NameToLayer("Food"));
        if (collideList.Length > 0)
        {
            foreach (Collider2D element in collideList)
            {
                if (element.tag == "food" && !FoodList.Contains(element.gameObject))
                {
                    FoodList.Add(element.gameObject);
                    FoodList.Sort(delegate (GameObject c1, GameObject c2) // sort according to distance between snake and the food
                    {
                        return Vector2.Distance(this.transform.position, c1.transform.position).CompareTo
                                    ((Vector2.Distance(this.transform.position, c2.transform.position)));
                    });
                    // FoodList.Sort();
                }
                else if (element.tag == "energy" && !EnergyList.Contains(element.gameObject))
                {
                    EnergyList.Add(element.gameObject);
                    EnergyList.Sort(delegate (GameObject c1, GameObject c2)// sort according to distance between snake and the energy
                    {
                        return Vector2.Distance(this.transform.position, c1.transform.position).CompareTo
                                    ((Vector2.Distance(this.transform.position, c2.transform.position)));
                    });
                }
                // Debug.Log(a.Length);
            }
            updateObjectToGo();
        }
    }
    /*
     void GotoDir(Vector2 direction )
     {
         if (direction == Vector2.up)
         {
             dir.x = 0; dir.y = diry;

             this.GetComponent<SpriteRenderer>().sprite = up;
         }
         else if (direction == Vector2.down)
         {
             dir.x = 0; dir.y = -diry;
             this.GetComponent<SpriteRenderer>().sprite = down;
         }
         else if (direction == Vector2.left)
         {
             dir.x = -dirx; dir.y = 0;
             this.GetComponent<SpriteRenderer>().sprite = left;
         }
         else if (direction == Vector2.right)
         {
             dir.x = dirx; dir.y = 0;
             this.GetComponent<SpriteRenderer>().sprite = right;
         }

     }*/
    void GotoDir(int val)
    {
        if (val == 0)
        {
            dir.x = 0; dir.y = diry;

            this.GetComponent<SpriteRenderer>().sprite = up;
        }
        else if (val == 1)
        {
            dir.x = 0; dir.y = -diry;
            this.GetComponent<SpriteRenderer>().sprite = down;
        }
        else if (val == 3)
        {
            dir.x = -dirx; dir.y = 0;
            this.GetComponent<SpriteRenderer>().sprite = left;
        }
        else if (val == 2)
        {
            dir.x = dirx; dir.y = 0;
            this.GetComponent<SpriteRenderer>().sprite = right;
        }

    }
    void DisplayUI_Text()
    {
        text.text = "Time Elapsed:\n\n" + (int)Time.time / 60 + "" + " mins " + (int)Time.time % 60
                + " secs\n\nEnergy:\n\n" + EnergyPoints
                + "\n\nFood Collected: " + CollectedFood
                + "\n\nMax. Food Collected: " + MaxCollectedFood
                + "\n\nHeading to: \n"
                + "\n\nCoordinates:\n " + (ToGo != null && ToGo.tag != "Untagged" ? "X:" + (int)ToGo.transform.position.x + "   Y: " + (int)ToGo.transform.position.y : "X:??   Y:??")
                + "\n\n Perceived Food: " + FoodList.Count
                + "\n\n Perceived Energy: " + EnergyList.Count;
        if (ToGo == null || ToGo.tag == "Untagged")
        {
            links[5].sprite = sprites[2];
        }
        else if (ToGo.tag == "food")
        {
            links[5].sprite = sprites[0];
        }
        else
            links[5].sprite = sprites[1];
    }
    public void updateObjectToGo()//Select what will the snake go to using Greedy algorithm
    {
        if (FoodList.Count == 0 && EnergyList.Count > 0) ToGo = EnergyList[0];
        else if (FoodList.Count > 0 && EnergyList.Count == 0) ToGo = FoodList[0];
        else if (FoodList.Count > 0 && EnergyList.Count > 0)
        {
            if (Vector2.Distance(this.transform.position, EnergyList[0].transform.position) < Vector2.Distance(this.transform.position, FoodList[0].transform.position))
                ToGo = EnergyList[0];
            else
            {
                float longDistnace = Vector2.Distance(transform.position, FoodList[0].transform.position) + Vector2.Distance(EnergyList[0].transform.position, FoodList[0].transform.position);
                if (EnergyPoints * 5 - longDistnace > 0)//1sec = 5 moves
                    ToGo = FoodList[0];
                else
                    ToGo = EnergyList[0];
            }
        }
        else
        {
            int x = (int)UnityEngine.Random.Range(borderLeft.position.x + 2, borderRight.position.x - 2);
            int y = (int)UnityEngine.Random.Range(borderBottom.position.y + 2, borderTop.position.y - 2);
            Vector2 randomVec = new Vector2(x, y);
            ToGo = new GameObject();//Go to a random place to scan the enviroment
            ToGo.transform.position = randomVec;
        }
    }
    private int DFS(Vector2 Node)
    {
        Debug.Log("DFSA");
        HashSet<Vector2> visited = new HashSet<Vector2>();
        Stack<KeyValuePair<Vector2, int>> s = new Stack<KeyValuePair<Vector2, int>>();
        s.Push(new KeyValuePair<Vector2, int>(Node, -1));
        int counter = 0;
        int maxCounter = 0;
        int pathCounter = 0;
        int lastPath = 0 ;
        int maxpath=0;
        Debug.Log("curr direc " +GetIndexOfFourDir(dir) +" before sort   "+dir+transform.position);
        {
            List<KeyValuePair<Vector2, int>> sortedWays = new List<KeyValuePair<Vector2, int>>();
            var temp = s.Pop();
            var current = temp.Key;

            int path = temp.Value;
            //Debug.Log(path); 

            int[] Ways = GetAllWays(current);

            for (int i = 0; i < 4; i++)
            {
                if (Ways[i] == 1 && !visited.Contains(current + fourDir[i]))
                {

                    sortedWays.Add(new KeyValuePair<Vector2, int>((current + fourDir[i]), i));
                    //   q.Enqueue(new KeyValuePair<Vector2, int>((current + fourDir[i]), i));
                    //TwoOptionsCounter.Add(i, 0);
                    //TwoOPtions.Add(i);
                  //  Debug.Log(i);

                    visited.Add(current + fourDir[i]);
                }

            }
           // Debug.Log("after sort ");
            foreach (var pair in sortedWays)
            {
                if (pair.Key != dir+current)
                {
                    s.Push(pair);
                    
             //       Debug.Log(pair.Value);
             //       Debug.Log(pair.Key);
                }
            }
            foreach (var pair in sortedWays)
            {
                if (pair.Key == dir+current)
                {
                    s.Push(pair);
                    maxpath = pair.Value;
                    lastPath = pair.Value;
             //       Debug.Log(pair.Value);
                    break;
                }
            }

        }
        while (s.Count > 0)
        {
            counter++;
            pathCounter++;
           // Debug.Log(s.Count + " elements");
            var temp = s.Pop();
            var current = temp.Key;
            int path = temp.Value;
           // if (visited.Contains(current))
            //    continue;
            int[] Ways = GetAllWays(current);
           // Debug.Log(Ways[0] + " ," + Ways[1] + " ," + Ways[2] + " ," + Ways[3]);
            //        Debug.Log("Distance: "+Vector2.Distance(FoodList[0].transform.position, current));
            //      if (Vector2.Distance(FoodList[0].transform.position, current) <= .2)
            //         return path;

            if (counter == 120)
                return path;
            if(path!= lastPath)
            {
                pathCounter = 0;
                lastPath = path;
            }
            if(pathCounter>maxCounter)
            {
                maxCounter = pathCounter;
                maxpath = path;
            }
            for (int i = 0; i < 4; i++)
            {
                if (Ways[i] == 1&&!visited.Contains(current + fourDir[i]))
                {
                    if (path != -1)
                    {
                        s.Push(new KeyValuePair<Vector2, int>((current + fourDir[i]), path));
                    }
                    else
                    {
                        s.Push(new KeyValuePair<Vector2, int>((current + fourDir[i]), i));
                        //Debug.Log(i);
                    }
                    visited.Add(current + fourDir[i]);
                }
            }
            
        }
        Debug.Log("Nope");
        return maxpath;
    }
    private int BFSFindNodeWithStartNode(Vector2 StartPoint)
    {
        Debug.Log("entered BFS :D ");
        var visited = new HashSet<Vector2>();
        // Mark this node as visited
        visited.Add(StartPoint);
        int TheOtherWay = 0;
        // Queue for BFS
        var q = new Queue<KeyValuePair<Vector2, int>>();
        // Add this node to the queue
        q.Enqueue(new KeyValuePair<Vector2, int>(StartPoint, -1));//enque state and current direction
        Dictionary<int, int> TwoOptionsCounter = new Dictionary<int, int>();
        List<int> TwoOPtions = new List<int>();
        int counter = 0, stopConter = 0;
        {
            List<KeyValuePair<Vector2, int>> sortedWays = new List<KeyValuePair<Vector2, int>>();
            var temp = q.Dequeue();
            var current = temp.Key;

            int path = temp.Value;
            //Debug.Log(path); 

            int[] Ways = GetAllWays(current);

            for (int i = 0; i < 4; i++)
            {
                if (Ways[i] == 1 && !visited.Contains(current + fourDir[i]))
                {

                    sortedWays.Add(new KeyValuePair<Vector2, int>((current + fourDir[i]), i));
                    //   q.Enqueue(new KeyValuePair<Vector2, int>((current + fourDir[i]), i));
                    //TwoOptionsCounter.Add(i, 0);
                    //TwoOPtions.Add(i);
                    Debug.Log(i);

                    visited.Add(current + fourDir[i]);
                }

            }
            foreach (var pair in sortedWays)
            {
                if (pair.Key == dir+current)
                {
                    q.Enqueue(pair);
                    TwoOptionsCounter.Add(pair.Value, 0);
                    TwoOPtions.Add(pair.Value);
                    break;
                }
            }
            foreach (var pair in sortedWays)
            {
                if (pair.Key != dir+current)
                { q.Enqueue(pair);
                    TwoOptionsCounter.Add(pair.Value, 0);
                    TwoOPtions.Add(pair.Value);
                }
            }

        }
        while (q.Count > 0)
        {
            counter++;
            // Debug.Log("entered queue ");
            var temp = q.Dequeue();
            var current = temp.Key;
            int path = temp.Value;
            //Debug.Log(path); 

            int[] Ways = GetAllWays(current);
            /*if (numberOfEmptyWays(Ways) >= 3)
            {
                Debug.Log(path+" Found it! "+Ways[0]+","+ Ways[1] + "," + Ways[2] + "," + Ways[3]);
                
                return path;
            }

        */

            for (int i = 0; i < 4; i++)
            {
                if (Ways[i] == 1 && !visited.Contains(current + fourDir[i]))
                {
                    if (path != -1)
                    {
                        q.Enqueue(new KeyValuePair<Vector2, int>((current + fourDir[i]), path));
                        TheOtherWay = path;
                        TwoOptionsCounter[path]++;
                    }
                    else
                    {
                        q.Enqueue(new KeyValuePair<Vector2, int>((current + fourDir[i]), i));
                        TwoOptionsCounter.Add(i, 0);
                        TwoOPtions.Add(i);
                        Debug.Log(i);
                    }
                    visited.Add(current + fourDir[i]);
                }

            }
            if (counter == 6)
            {
                counter = 0;
                if (TwoOptionsCounter[TwoOPtions[0]] == 0 && TwoOptionsCounter[TwoOPtions[1]] > 0)
                    return TwoOPtions[1];
                else if (TwoOptionsCounter[TwoOPtions[1]] == 0 && TwoOptionsCounter[TwoOPtions[0]] > 0)//return ther other way
                    return TwoOPtions[0];
                else
                {
                    stopConter++;
                    TwoOptionsCounter[TwoOPtions[0]] = 0;
                    TwoOptionsCounter[TwoOPtions[1]] = 0;
                }
            }
          if (counter ==10&& TwoOPtions.Count>3)
            {
                counter = 0;
                int least = TwoOptionsCounter[TwoOPtions[0]];
                int pos = 0;
                for (int i = 0; i < TwoOPtions.Count; i++)
                {
                    if (TwoOptionsCounter[TwoOPtions[i]] < least)
                        pos = i;
                }
                TwoOPtions.Remove(pos);
               
            }

            //    if (stopConter > 2 && TwoOPtions.Count == 3)
            //      return GetIndexOfFourDir(dir);

            /* if(TwoOptionsCounter[TwoOPtions[0]]>=3&& TwoOptionsCounter[TwoOPtions[1]] >= 3)
              {
                  TwoOptionsCounter[TwoOPtions[0]] = 0;
                  TwoOptionsCounter[TwoOPtions[1]] = 0;
              }
              if (TwoOptionsCounter[TwoOPtions[0]] > 3)
                  return TwoOPtions[0];
              else if (TwoOptionsCounter[TwoOPtions[1]] > 3)
                  return TwoOPtions[1];*/
        }
        Debug.Log("Could not find node!");
        return TheOtherWay;//choose the last expanded way which mean its the tallest path
    }
    private int GetIndexOfFourDir(Vector2 Direction)
    {
        for (int i = 0; i < 4; i++)
        {
            if (Direction == fourDir[i])
                return i;
        }
        Debug.Log("Error");
        return 0;
    }
    private int[] GetAllWays(Vector2 current)
    {
        GameObject[] Body;
        int[] input = new int[4];

        try
        {
            Body = GameObject.FindGameObjectsWithTag("Body");// get all the snake body
                                                             // int counterOfBadWays = 0;
            for (int i = 0; i < 4; i++)// { up, down, right, left };
            {
                Vector2 newdir = fourDir[i];


                bool safeWay = true;
                foreach (GameObject obj in Body)
                {
                    float distance = Vector2.Distance((Vector2)obj.transform.position, newdir + current);
                    if ((distance >= 0 && distance <= .2f))
                    {
                        // counterOfBadWays++;
                        safeWay = false;
                    }
                }
                if (safeWay)// if there is no obstacles 
                    input[i] = 1;
                else
                    input[i] = 0;

            }

        }

        catch
        {
            for (int i = 0; i < 4; i++)
                input[i] = 1;
        }
        //checking the wall
        Vector2 TheNewDir = current ;
        float[] distance1 = {
            Mathf.Abs(TheNewDir.y-borderTop.transform.position.y),
            Mathf.Abs(TheNewDir.y-borderBottom.transform.position.y),
            Mathf.Abs(TheNewDir.x- borderRight.transform.position.x),
            Mathf.Abs(TheNewDir.x-borderLeft.transform.position.x) };


        for (int j = 0; j < 4; j++)// checking if there are boarder near the snake 
        {
            if ((distance1[j] >= 0f && distance1[j] <= .2f))
            {
                input[j] = 0;
            }

        }
        return input;

    }
    private int numberOfEmptyWays(int[] Current)
    {
        int FreeWays = 0;
        foreach (int State in Current)
        {
            if (State == 1)
                FreeWays++;
        }
        return FreeWays;
    }
    Vector2 ChangeDirection(int index)
    {
        return fourDir[index];
    }
    public void AutoMovement()
    {
        float distance = Mathf.Abs(transform.position.x - ToGo.transform.position.x);
        //   Debug.Log(GetComponent<BoxCollider2D>().bounds.size);// to determine length of an object
        // Debug.Log(GetComponent<Renderer>().bounds.size);
        if (!(distance >= 0 && distance <= .2))//if the object is detected as the object's size is 0.4 
        {
            if (transform.position.x < ToGo.transform.position.x)
            {
                //dir = Vector2.right;
                if (dir.x != -.4f)// to prevent the snake from going to the diffrent direction 
                {
                    dir.x = .4f; dir.y = 0;
                    this.GetComponent<SpriteRenderer>().sprite = right;
                }
                else
                {
                    moveUpOrDown();// make the snake go one step left 
                    //  Debug.Log("shemal");
                }
            }
            else
            {
                if (dir.x != .4f)
                {
                    this.GetComponent<SpriteRenderer>().sprite = left;
                    dir.x = -.4f; dir.y = 0;
                    //  Debug.Log(":(");
                }
                else
                {
                    moveUpOrDown();
                    //    Debug.Log("ymeen");
                }

            }
        }

        else // If x coordinate is correct, we are going for y coordinate
        {
            Debug.Log(":D");
            if (transform.position.y < ToGo.transform.position.y)
            {
                if (dir.y != -.4f)
                {
                    dir.x = 0; dir.y = .4f;
                    this.GetComponent<SpriteRenderer>().sprite = up;
                }
                else
                    moveLeftOrRight();
            }
            else
            {
                if (dir.y != .4f)
                {
                    dir.x = 0; dir.y = -.4f;
                    this.GetComponent<SpriteRenderer>().sprite = down;
                }
                else moveLeftOrRight();
            }
        }
    }
    void NN_Command()//Neural Network commands to evaluate snake's action
    {
        double[] input = Train();
        double res = Compute(input);
        Debug.Log("Res is : " + res);
        if (res <= .5)//snake should look for another decision
        {
            for (int i = 0; i < 4; ++i)
            {
                if (input[i] != 0)
                {

                    Debug.Log("-------------------- lololololololi");
                    GotoDir(i);
                    /*  input[4] = i / 4f;
                      res = Compute(input);
                      Debug.Log("newwwwwwwww Res is : " + res);
                      if (res > .5f)
                      {
                          dir = ChangeDirection(i);
                          Debug.Log("using neural network to take new action -------------");
                          break;
                      }*/
                }
            }
        }
    }
    private void move()
    {
        // lastPosition = new Vector2(transform.position.x, transform.position.y) - dir;
        // Save current position of snake's head
        Vector2 currentPosition = transform.position;
        if (ToGo == null)
        {
            updateObjectToGo();
        }
        if (automovement)
            AutoMovement();
        // dir = objectToGoFor.transform.position - transform.position;
        EnergyPoints -= ((int)Time.time % 5 == 0 && Time.time - (int)Time.time == 0 ? 1 : 0);//every 1 sec snake will lose 1 energy point
        if (EnergyPoints == 0)
            GameOver();
        if (++EnergyCounter == EnergyCounterLimit)
        {
            EnergyCounter = 0;
            EnergyPoints--;
        }
        lastPosition = new Vector2(transform.position.x, transform.position.y) - dir;
        currentPosition = transform.position;
        var CurrentState = GetAllWays(currentPosition);
        Debug.Log(CurrentState.Length);
        if (numberOfEmptyWays(CurrentState) == 5)
        {
            //Debug.Log(" Found it! " + CurrentState[0] + "," + CurrentState[1] + "," + CurrentState[2] + "," + CurrentState[3]);
            int ValueFromBFS =BFSFindNodeWithStartNode(currentPosition);
            dir = fourDir[ValueFromBFS];
            Debug.Log("the new direction is " + ValueFromBFS);
        }
        if (numberOfEmptyWays(CurrentState) >= 2)
        {
            Debug.Log(" Found it! " + CurrentState[0] + "," + CurrentState[1] + "," + CurrentState[2] + "," + CurrentState[3]);
            int ValueFromDFS = DFS(currentPosition);
            dir = fourDir[ValueFromDFS];
            Debug.Log("the new of DFS direction is " + ValueFromDFS);
        }
        if (secondChecker) checkMove();
        //movement using keyboard
        ManualMovement(); //manual moving

        if (trained)
            NN_Command();
        transform.Translate(dir);// make snake move
        if (tail.Count > 0)
        {
            tail[tail.Count - 1].position = currentPosition;
            tail.Insert(0, tail[tail.Count - 1]);
            tail.RemoveAt(tail.Count - 1);
        }
    }

    double[] Train()//Create a dataset for the Network to train on
    {
        GameObject[] Body;
        double[] input = new double[5];
        double[] output = new double[1];
        output[0] = 1;
        try
        {
            Body = GameObject.FindGameObjectsWithTag("Body");// get all the snake body
            int counterOfBadWays = 0;
            for (int i = 0; i < 4; i++)// { up, down, right, left };
            {
                Vector2 newdir = fourDir[i];
                bool currentDirection = false;
                if (newdir == dir)
                {
                    input[4] = i / 4f;
                    currentDirection = true;
                }
                bool safeWay = true;
                foreach (GameObject obj in Body)
                {
                    float distance = Vector2.Distance((Vector2)obj.transform.position, newdir + (Vector2)transform.position);
                    if ((distance >= 0 && distance <= .2f))
                    {
                        counterOfBadWays++;
                        safeWay = false;
                    }
                }
                if (safeWay)// if there is no obstacles 
                    input[i] = 1;
                else
                    input[i] = 0;
                if (currentDirection && input[i] == 0)
                    output[0] = 0;
            }
            if (counterOfBadWays >= 4)
                output[0] = 0;
            // else
            // output[0] = 1;
        }

        catch
        {
            for (int i = 0; i < 4; i++)
                input[i] = 1;

            // output[0] = 1;
            for (int i = 0; i < 4; i++)
            {
                Vector2 newdir = fourDir[i];
                if (newdir == dir)
                    input[4] = i / 4f;
            }

        }
        //checking the wall
        Vector2 TheNewDir = (Vector2)transform.position + dir;
        float[] distance1 = {
            Mathf.Abs(TheNewDir.y-borderTop.transform.position.y+.1f),
            Mathf.Abs(TheNewDir.y- borderBottom.transform.position.y-0.1f),
            Mathf.Abs(TheNewDir.x- borderRight.transform.position.x+.1f),
            Mathf.Abs(TheNewDir.x-borderLeft.transform.position.x-.1f) };


        for (int j = 0; j < 4; j++)// checking if there are boarder near the snake 
        {
            if ((distance1[j] >= 0f && distance1[j] <= .2f))
            {
                input[j] = 0;
                if (fourDir[j] == dir)
                    output[0] = 0;
            }

        }
        if (!trained)
        {
            /* double[] output2 = { output[0]==0?1:0};//check if there is an input with diffrent output

             if (dataSets.Contains(new DataSet(input, output))&& dataSets.Contains(new DataSet(input, output2)))
                 Debug.Log("some duplicate exist " + input[0] + "," + input[1] + "," + input[2] + "," + input[3] + "," + input[4]);
             if (input[0] == 1 && input[1] == 1 && input[2] == 1 && input[3] == 0 && input[4] == 1 && output[0] == 0)
                 Debug.Log("nullllllllllllllllllll " + input[0] + "," + input[1] + "," + input[2] + "," + input[3] + "," + input[4] + "  output ," + output[0]);*/
            /*
                       // dataSets.Add(new DataSet(input, output));//insert data set
                       // ++collectedData;
                        if (output[0] == 0)
                        {
                            Debug.Log("outputed b zeroooooooooooooooooooooo");
                            //  else
                            Debug.Log("current NN input " + input[0] + "," + input[1] + "," + input[2] + "," + input[3] + "," + input[4]);

                        }*/
        }
        if (!trained && collectedData == numberOfDatasets)
        {
            // simulationProcess();
            WriteToBinaryFile("dataset.ser", dataSets);
            TrainNetwork();
            Debug.Log("Trained!");
            return null;
        }
        return input;
    }

    void permuteFourDir(int a, int b, int c, int d)
    {
        double[] input = new double[5];
        double[] output = new double[1];

        input[0] = a;
        input[1] = b;
        input[2] = c;
        input[3] = d;
        for (int i = 0; i < 4; i++)
        {
            output[0] = 1;
            input[4] = i / 4f;

            if (input[i] == 0)
                output[0] = 0;
            dataSets.Add(new DataSet(input, output));
        }
    }
    void TrainNetwork()
    {
        net.Train(dataSets, MinimumError);
        trained = true;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.tag == "food")//if the snake eats food
        {
            GameObject o = Instantiate(tailPrefab, new Vector2(100, 1000), Quaternion.identity);
            tail.Insert(0, o.transform);
            ToGo = null;
            FoodList.Remove(collision.gameObject);
            //Debug.Log("Food :D");
            CollectedFood++;
            Destroy(collision.gameObject);
        }
        else if (collision.tag == "energy") //if the snake eats energy
        {
            EnergyPoints += EnergyValue;
            ToGo = null;
            //Debug.Log("energy :D");
            EnergyList.Remove(collision.gameObject);
            Destroy(collision.gameObject);
        }
        else if (collision.tag == "Body" || collision.tag == "Border")
        {
            GameOver();
        }
    }
    public void checkMove()//Check for the next movment if it will hit an obstacle or not for the use before using the neural network outputs
    {
        Vector2 currentPosition = new Vector2(transform.position.x, transform.position.y);
        Vector2 plannedMove = currentPosition + dir;
        GameObject[] Body;
        bool goodMovement = true;
        bool nearWall = isNearWall(currentPosition + dir);
        //Debug.Log("Near wall = " + nearWall);
        if (nearWall)
            goodMovement = false;
        try
        {

            Body = GameObject.FindGameObjectsWithTag("Body");
        }
        catch
        {
            Debug.Log("No Tail");
            return;
        }
        foreach (GameObject obj in Body)
        {
            float distance = Vector2.Distance(obj.transform.position, plannedMove);
            if (distance >= 0 && distance <= .2f)
            {
                goodMovement = false;
                break;
            }
        }
        if (!goodMovement)// try the 3 other  possibiletes 
        {
            Debug.Log("not a good choice movement");
            bool NoWay = true;
            float[] dx = { 0, 0, .4f, -.4f };//up,down,right,left
            float[] dy = { .4f, -.4f, 0, 0 };
            Vector2[] fourDirtemp = { new Vector2(0, .4f), new Vector2(0, -.4f), new Vector2(.4f, 0), new Vector2(-.4f, 0) };
            //soring the array to make it take the best decsion 
            Sprite[] Head = { up, down, right, left };
            // sortFourDir(fourDirtemp, Head);

            for (int i = 0; i < 4; i++)
            {
                Vector2 newdir = fourDir[i];
                bool safeWay = true;

                if (newdir != dir)
                {
                    safeWay = !isNearWall(currentPosition + newdir);
                    //Debug.Log("safeway = " + safeWay);
                    foreach (GameObject obj in Body)
                    {

                        float distance = Vector2.Distance((Vector2)obj.transform.position, newdir + currentPosition);
                        if ((distance >= 0 && distance <= .2f))
                        {
                            safeWay = false;
                        }

                    }
                    if (safeWay)
                    {
                        Debug.Log("now we have good decsion :" + i);// +" ditstance is: " + distance + "body pos" + (Vector2)obj.transform.position + " new pos: " + (newdir + currentPosition));
                        this.GetComponent<SpriteRenderer>().sprite = Head[i];

                        dir = newdir;
                        NoWay = false;
                        //break;

                        return;
                    }
                }
            }
            if (NoWay == true) Debug.Log("Dead-No way-------------");
        }
    }
    bool isNearWall(Vector2 direction)//Check if the snake is near a wall
    {
        float[] distance1 = {
                        Mathf.Abs(direction.x-borderLeft.transform.position.x-.1f),
                     Mathf.Abs(direction.x- borderRight.transform.position.x+.1f),
                        Mathf.Abs(direction.y-borderTop.transform.position.y+.1f),
                    Mathf.Abs(direction.y- borderBottom.transform.position.y-0.1f) };
        for (int j = 0; j < 4; j++)
        {
            // Debug.Log("distance between  wall: " + distance1[j]);
            if ((distance1[j] >= 0 && distance1[j] <= .2f))
            {

                return true;
            }
        }
        return false;
    }
    void sortFourDir(Vector2[] fourDir, Sprite[] Head)//up,down,right,left // sort direction for the optimal direction
    {

        if (dir.y == 0 && transform.position.y >= ToGo.transform.position.y)//if the current direction is x then choos best y
        {
            /* Vector2 temp = fourDir[0];//
             fourDir[0] = fourDir[1];
             fourDir[1]= temp;
         */
            swap(ref Head[0], ref Head[1]);
            swap(ref fourDir[0], ref fourDir[1]);//  swap to make up is first direction
        }
        else if (dir.x == 0 && transform.position.x < ToGo.transform.position.x)
        {
            swap(ref fourDir[0], ref fourDir[2]);//right
            swap(ref Head[0], ref Head[2]);
        }
        else if (dir.x == 0 && transform.position.x >= ToGo.transform.position.x)
        {
            swap(ref fourDir[0], ref fourDir[3]);
            swap(ref Head[0], ref Head[3]);
        }

    }
    double Compute(double[] values) //Computes the output from the network after training
    {
        double[] results = net.Compute(values);
        return results[0];
    }
    public void swap<T>(ref T lhs, ref T rhs)
    {
        T temp = lhs;
        lhs = rhs;
        rhs = temp;
    }
    void GameOver()// The snake dies
    {

        foreach (Transform t in tail)
        {
            Destroy(t.gameObject);
        }
        tail.Clear();
        if (EnergyPoints == 0)
            Debug.Log("Out Of Energy");
        EnergyPoints = 30;
        Debug.Log("Deathpos:" + transform.position);
        MaxCollectedFood = Mathf.Max(CollectedFood, MaxCollectedFood);
        CollectedFood = 0;
        FoodList.Clear();
        EnergyList.Clear();
        head.transform.position = new Vector3(0, 0, this.transform.position.z);
    }
    public void moveUpOrDown()
    {
        if (transform.position.y < ToGo.transform.position.y)
        {
            dir.x = 0; dir.y = .4f;
            //dir=Vector2.up;
            this.GetComponent<SpriteRenderer>().sprite = up;
        }
        else
        {
            dir.x = 0; dir.y = -.4f;
            // dir = -Vector2.up;
            this.GetComponent<SpriteRenderer>().sprite = down;
        }

    }
    public void moveLeftOrRight()
    {
        if (transform.position.x < ToGo.transform.position.x)
        {
            if (dir.x != -.4f)
            {
                dir.x = .4f; dir.y = 0;
                this.GetComponent<SpriteRenderer>().sprite = right;
            }
        }
        else
        {
            if (dir.x != .4)
            {
                this.GetComponent<SpriteRenderer>().sprite = left;
                dir.x = -.4f; dir.y = 0;
            }
        }
    }

    void ManualMovement()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            if (dir.y != -.4f)
            {
                dir.x = 0; dir.y = .4f;

                this.GetComponent<SpriteRenderer>().sprite = up;
            }

        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            if (dir.y != .4f)
            {
                dir.x = 0; dir.y = -.4f;
                this.GetComponent<SpriteRenderer>().sprite = down;
            }
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            if (dir.x != -.4f)
            {
                dir.x = .4f; dir.y = 0;
                this.GetComponent<SpriteRenderer>().sprite = right;
            }
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (dir.x != .4f)
            {
                dir.x = -.4f; dir.y = 0;
                this.GetComponent<SpriteRenderer>().sprite = left;
            }
        }
    }

    public static void WriteToBinaryFile<T>(string filePath, T objectToWrite, bool append = false)
    {
        File.Delete(filePath);
        using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
        {
            var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            binaryFormatter.Serialize(stream, objectToWrite);
        }
    }
    public static T ReadFromBinaryFile<T>(string filePath)
    {
        using (Stream stream = File.Open(filePath, FileMode.Open))
        {
            var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            return (T)binaryFormatter.Deserialize(stream);
        }
    }
}
