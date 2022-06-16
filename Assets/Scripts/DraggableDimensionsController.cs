using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class DraggableDimensionsController : MonoBehaviour
{
    public Transform contents;
    public RectTransform rectContents;
    public GameObject draggableDimensionPrefab;
    List<GameObject> allDraggables = new List<GameObject>();
    GameObject[] draggablesInOriginalOrder;
    List<int>[] dimensionOrdering = new List<int>[3];
    private Dictionary<GameObject, int> OriginalValue = new Dictionary<GameObject, int>();
    int count = 3;
    int unit = 27;
    public ChessControllerND mainController;
    int frameNumber = 0;


    public bool curvedPath = true;//changes whether tiles move in diagonal lines or curves when the dimension display order changes
    List<int>[] lastOrder;
    DateTime changedTime = new DateTime(0);
    bool changingDimensionPos = false;

    // Start is called before the first frame update
    void Start()
    {
        GameObject temp = GameObject.Find("Main Controller");
        if (temp != null)
            mainController = temp.GetComponent<ChessControllerND>();
        if(mainController!=null)
        {
            count = mainController.boardDimensions.Length;
        }
        GameObject lastMade;
        for(int i =0;i<count;i++)
        {
            lastMade= Instantiate(draggableDimensionPrefab, contents);
            lastMade.GetComponent<RectTransform>().localScale = new Vector3((float)unit/115, (float)unit /115, (float)unit /115);
            lastMade.GetComponent<DraggableDimensions>().SetText(i);
            allDraggables.Add(lastMade);
            OriginalValue.Add(lastMade, i);
            lastMade.transform.localPosition = FirstFrameCoordToLocalPos(i/2, i % 2);

        }
        for (int i = 0; i < dimensionOrdering.Length; i++)
            dimensionOrdering[i] = new List<int>();
        draggablesInOriginalOrder = allDraggables.ToArray();
        rectContents.sizeDelta = new Vector2(unit * Mathf.Max(0, count-2), unit*3);
        ReadDimensionDisplayOrder();
    }
    private void Update()
    {
        if(frameNumber==1)
        {
            ReadDimensionDisplayOrder();//I recal this not working in start, but it seems that's no longer an issue
        }
        frameNumber++;
        UpdateTilePositions();
    }

    public void ReadDimensionDisplayOrder()
    {
        Debug.Log("start order: " + mainController.ArrayToString(dimensionOrdering));
        bool firstCycle = false;
        if (lastOrder == null)
            firstCycle = true;
        if(!firstCycle)
            lastOrder = (List<int>[])dimensionOrdering.Clone();
        for (int i = 0; i < dimensionOrdering.Length; i++)
            dimensionOrdering[i] = new List<int>();
        //Debug.Log("Before: " + allDraggables[0].GetComponent<RectTransform>().anchoredPosition.x + ", " + allDraggables[1].GetComponent<RectTransform>().anchoredPosition.x + ", " + allDraggables[2].GetComponent<RectTransform>().anchoredPosition.x);
        allDraggables = allDraggables.OrderBy(thing => thing.GetComponent<RectTransform>().anchoredPosition.x).ToList();
        //Debug.Log("After:  " + allDraggables[0].GetComponent<RectTransform>().anchoredPosition.x + ", " + allDraggables[1].GetComponent<RectTransform>().anchoredPosition.x + ", " + allDraggables[2].GetComponent<RectTransform>().anchoredPosition.x);

        for (int i = 0; i < allDraggables.Count; i++)
        {
            if (allDraggables[i].transform.localPosition.y < CoordToLocalPos(0, 1.5f).y)
                dimensionOrdering[2].Add(OriginalValue[allDraggables[i]]);
            else if (allDraggables[i].transform.localPosition.y < CoordToLocalPos(0, 0.5f).y)
                dimensionOrdering[1].Add(OriginalValue[allDraggables[i]]);
            else
                dimensionOrdering[0].Add(OriginalValue[allDraggables[i]]);
        }
        //Debug.Log(ArrayToText(dimensionOrdering));
        if(firstCycle)
            lastOrder = (List<int>[])dimensionOrdering.Clone();
        DisplayDraggables();
        changedTime = DateTime.Now;
        changingDimensionPos = true;
        //Debug.Log("End: " + mainController.ArrayToString(lastOrder) + "\n\n" + mainController.ArrayToString(dimensionOrdering));
    }


    Vector3 FirstFrameCoordToLocalPos(float x, float y)
    {//I have no idea why it has to work differently on frame 0
        y = 2 - y;
        return new Vector3(unit * (+x - (float)count/2), unit * (y - 1f), 0);
    }
    Vector3 CoordToLocalPos(float x, float y)
    {
        y = 2 - y;
        return new Vector3(unit * (+x + 0.5f), unit * (y - 2.5f), 0);
    }
    public void DisplayDraggables()
    {
        for(int i=0;i<dimensionOrdering.Length;i++)
        {
            for(int j=0;j< dimensionOrdering[i].Count;j++)
            {
                draggablesInOriginalOrder[dimensionOrdering[i][j]].transform.localPosition = CoordToLocalPos(j,i);
            }
        }
        //Debug.Log(ArrayToText(dimensionOrdering));
    }
    public void UpdateTilePositions()
    {
        if (!changingDimensionPos)
            return;
        double ratio = (DateTime.Now - changedTime).TotalSeconds / 2;//max time of 2 seconds
        Array myTiles = mainController.getAllTiles();

        if (ratio >= 1)
        {
            changingDimensionPos = false;
            foreach (TileAI tile in myTiles)
            {
                tile.transform.position = PosToCoord(tile.pos, dimensionOrdering,mainController.boardDimensions);
            }
            return;
        }
        List<Vector3> positions = new List<Vector3>();
        //Debug.Log(ChessControllerND.ArrayToString(lastOrder)  + "\n\n" + ChessControllerND.ArrayToString(dimensionOrdering));
        foreach (TileAI tile in myTiles)//initiates a non-linear interpolation between the coordinates
        {
            var coords = tile.pos;
            positions.Clear();
            positions.Add(PosToCoord(coords, lastOrder, mainController.boardDimensions));
            positions.Add(PosToCoord(coords, dimensionOrdering, mainController.boardDimensions));
            positions.Insert(1, new Vector3(Math.Max(positions[0].x, positions[1].x), Math.Max(positions[0].y, positions[1].y), Math.Max(positions[0].z, positions[1].z)));
            if (!curvedPath)
            {
                //on one reading, equal to halfway between the values reached by averaging the x+1 with x, and x-1 with x, aka 25%x-1, 50%x, 25% x+1;
                int last = Math.Max((int)(ratio * positions.Count - 1), 0);
                int current = (int)(ratio * positions.Count);
                int next = Math.Min((int)(ratio * positions.Count + 1), positions.Count - 1);
                float k = (float)ratio * positions.Count % 1;
                Debug.Log(ratio + ", " + last + ", " + current + ", " + next + ", " + k);
                //tile.transform.position = ((positions[last] + positions[current]) * (1 - k) + k * (positions[current] + positions[next])) / 2;
                tile.transform.position = (positions[last] * (1 - k) + positions[current] + k * positions[next]) / 2;
            }
            else//rotates the squares about the origin through a curve of 90 degrees
            {
                Vector3 diffA = positions[0] - positions[1];
                Vector3 diffB = positions[2] - positions[1];
                tile.transform.position = positions[0] - (float)Math.Sin(Math.PI * ratio / 2) * diffA + (float)(1 - Math.Cos(Math.PI * ratio / 2)) * diffB;
            }
        }

    }
    public static Vector3 PosToCoord(int[] pos, List<int>[] ordering, int[] boardDimensions)
    {
        int[] myCoords = { 0, 0, 0 };
        int[] multiplier = { 1, 1, 6 };
        for (int displayDimension = 0; displayDimension < 3; displayDimension++)
        {
            for (int i = 0; i < ordering[displayDimension].Count; i++)
            {
                myCoords[displayDimension] += pos[ordering[displayDimension][i]] * multiplier[displayDimension];
                multiplier[displayDimension] = multiplier[displayDimension] * boardDimensions[ordering[displayDimension][i]] + 1;
            }
        }
        return new Vector3(myCoords[0], myCoords[1], myCoords[2]);
    }
    public static string ArrayToText(List<int>[] a)
    {
        string myOut = "";
        for (int i = 0; i < a.Length; i++)
        {
            for (int j = 0; j < a[i].Count; j++)
            {
                myOut += a[i][j] + ", ";
            }
            myOut += "\n";
        }
        return (myOut);

    }
}
/*class DraggableComparer : IComparer
{
    public int Compare (GameObject x, GameObject y)
    {
        float temp = x.transform.position.x - y.transform.position.x;
        if (temp > 0) return 1;
        if (temp < 0) return -1;
        return 0;
    }
}*/
