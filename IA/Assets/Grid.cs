using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Grid : MonoBehaviour
{
    public float size;

    public GameObject floorSprite;
    public GameObject wallSprite;

    public GameObject ennemySprite;

    private GameObject ennemy;
    public GameObject player;

    public struct Tile
    {
        public int x { get; set; }
        public int y { get; set; }
        public int cost { get; set; }
        public int totalCost { get; set; }
        public int heuristique { get; set; }
        public bool inClosedList { get; set; }
        public bool inOpenList { get; set; }
        public int predX { get; set; }
        public int predY { get; set; }
    }
    private Tile[,] tiles;

    private struct Loc
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    private Stack<Loc> ennemyMoves;

    private Loc nextMove;

    private int[,] grid = new int[,]
    {
        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
        {1,0,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,1},
        {1,0,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,1},
        {1,0,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,1},
        {1,0,0,0,0,0,1,1,0,0,0,0,0,1,1,0,0,0,0,0,1,0,0,0,1},
        {1,0,0,0,0,0,1,1,0,0,0,0,0,1,1,0,0,0,0,0,1,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,1,1,1,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,1,1,1,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,1,1,1,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,1,1,1,1,0,0,0,0,0,1,0,0,0,0,0,1,1,1,1,1,1,1},
        {1,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
     };
    // Start is called before the first frame update
    void Start()
    {
        CreateGrid();
        CreateEnnemy();

        AStar();
        //InvokeRepeating("Dijkstra", 0, 0.10f);
        //InvokeRepeating("EnnemyMove", 0, 0.10f);
    }

    private void CreateGrid()
    {
        tiles = new Tile[grid.GetLength(0), grid.GetLength(1)];

        for(int i =0;i<grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                GameObject tile;

                if (grid[i,j] == 0)
                    tile = Instantiate(floorSprite, transform);
                else
                    tile = Instantiate(wallSprite, transform);

                tile.transform.position = new Vector2(i* size, j * size);

                tiles[i, j] = new Tile() { x = i, y = j, cost = 1, heuristique = 0, totalCost = 0, inClosedList = false, inOpenList = false, predX = -1, predY = -1 };
            }
        }
    }

    private void CreateEnnemy()
    {
        ennemy = Instantiate(ennemySprite, transform);
        ennemy.transform.position = new Vector2(14.5f, 22);
    }

    private int Dijkstra()
    {
        int x = Mathf.RoundToInt(ennemy.transform.position.x);
        int y = Mathf.RoundToInt(ennemy.transform.position.y);
        List<Tile> nodes = new List<Tile>();

        int[,] poids = new int[grid.GetLength(0), grid.GetLength(1)];
        for (int i = 0; i < grid.GetLength(0); i++)
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                if (grid[i, j] != 1)
                {
                    nodes.Add(tiles[i, j]);
                    poids[i, j] = 99;
                }
            }
        //nodes.Remove(tiles[x, y]);
        poids[x, y] = 0;

        while(nodes.Count > 0)
        {

            int minCost = 99;
            Tile tile = new Tile();
            foreach(Tile t in nodes)
            {
                if(poids[t.x,t.y] < minCost)
                {
                    minCost = poids[t.x, t.y];
                    tile = t;
                }
            }
            x = Mathf.RoundToInt(tile.x);
            y = Mathf.RoundToInt(tile.y);
            nodes.Remove(tile);

            Stack<Tile> neighbors = new Stack<Tile>();
            if (grid[x + 1, y] == 0)
                neighbors.Push(tiles[x + 1, y]);
            if (grid[x - 1, y] == 0)
                neighbors.Push(tiles[x - 1, y]);
            if (grid[x, y + 1] == 0)
                neighbors.Push(tiles[x, y + 1]);
            if (grid[x, y - 1] == 0)
                neighbors.Push(tiles[x, y - 1]);

            while (neighbors.Count > 0)
            {
                
                Tile t = neighbors.Pop();
                int xV = Mathf.RoundToInt(t.x);
                int yV = Mathf.RoundToInt(t.y);
                if (poids[xV, yV] > poids[x, y] + tiles[xV,yV].cost)
                {

                    poids[xV, yV] = poids[x, y] + 1;
                    tiles[xV, yV].predX = x;
                    tiles[xV, yV].predY = y;

                }
                
            }
            
        }
        int xP = Mathf.RoundToInt(player.transform.position.x);
        int yP = Mathf.RoundToInt(player.transform.position.y);
        ennemyMoves = new Stack<Loc>();
        do
        {
            int temp = xP;
            xP = tiles[xP, yP].predX;
            yP = tiles[temp, yP].predY;
            ennemyMoves.Push(new Loc() { x = xP, y = yP });
        }
        while (tiles[xP, yP].predX != Mathf.RoundToInt(ennemy.transform.position.x) || tiles[xP, yP].predY != Mathf.RoundToInt(ennemy.transform.position.y));



        return 1;
    }
    private int AStar()
    {
        List<Tile> closedList = new List<Tile>();
        List<Tile> openList = new List<Tile>();

        Tile depart = tiles[Mathf.RoundToInt(ennemy.transform.position.x), Mathf.RoundToInt(ennemy.transform.position.y)];
        Tile objectif = tiles[Mathf.RoundToInt(player.transform.position.x), Mathf.RoundToInt(player.transform.position.y)];
        int xO = objectif.x;
        int yO = objectif.y;
        int xD = depart.x;
        int yD = depart.y;

        depart.inOpenList = true;
        openList.Add(tiles[xD, yD]);

        Debug.Log("Start");
        while (openList.Count > 0)
        {
            int minCost = 99999;

            int x = 0;
            int y = 0;
            foreach (Tile t in openList)
            {
                if (t.totalCost < minCost)
                {
                    minCost = t.totalCost;
                    x = t.x;
                    y = t.y;
                }
            }
            Debug.Log("Courrant  " + x + "   " + y);
            Tile current = tiles[x, y];

            Debug.Log(openList.Count);
            current.inOpenList = false;
            openList.Remove(tiles[x, y]);

            current.inClosedList = true;
            closedList.Add(current);
            
            if (x == objectif.x && y == objectif.y)
            {
                Debug.Log("Succes");
                return 0;
            }

            Stack<Tile> neighbors = new Stack<Tile>();
            if (grid[x + 1, y] == 0)
            {
                neighbors.Push(tiles[x + 1, y]);
                //tiles[x + 1, y].predX = x;
               // tiles[x + 1, y].predY = y;
            }
            if (grid[x - 1, y] == 0)
            {
                neighbors.Push(tiles[x - 1, y]);
               // tiles[x - 1, y].predX = x;
               // tiles[x - 1, y].predY = y;
            }
            if (grid[x, y + 1] == 0)
            {
                neighbors.Push(tiles[x, y+1]);
                //tiles[x , y+1].predX = x;
                //tiles[x, y+1].predY = y;
            }
            if (grid[x, y - 1] == 0)
            {
                neighbors.Push(tiles[x  ,y-1]);
                //tiles[x, y-1].predX = x;
                //tiles[x, y-1].predY = y;
            }
            
            while(neighbors.Count > 0)
            {
                //Debug.Log("Voisins " + neighbors.Count);
                
                Tile neighbor = neighbors.Pop();
                int xN = neighbor.x;
                int yN = neighbor.y;
                
                if (neighbor.inClosedList)
                    continue;
                else
                {
                    if((Mathf.Abs(xN - x) + Mathf.Abs(yN - y)) < neighbor.totalCost || !neighbor.inOpenList)
                    {
                        neighbor.cost = (Mathf.Abs(xN - x) + Mathf.Abs(yN - y));
                        neighbor.heuristique = (Mathf.Abs(xN - xO) + Mathf.Abs(yN - yO));
                        neighbor.totalCost = neighbor.cost + neighbor.heuristique;
                        if(!neighbor.inOpenList)
                        {
                            neighbor.inOpenList = true;
                            openList.Add(neighbor);
                        }
                    }
                }
            }

           
            
        }
        Debug.Log("Echec");
        return 1;
    }

    private void EnnemyMove()
    {
        if (ennemyMoves.Count > 0)
            nextMove = ennemyMoves.Pop();
    }
    // Update is called once per frame
    void Update()
    {
        //ennemy.transform.position = Vector2.MoveTowards(ennemy.transform.position, new Vector2(nextMove.x, nextMove.y), 4 * Time.deltaTime);
        //Debug.Log(nextMove.x + "  " + nextMove.y);
    }
}
