using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using UnityEngine;
public class Grid : MonoBehaviour
{
    public float size;

    public GameObject floorSprite;
    public GameObject wallSprite;

    public GameObject ennemySprite;

    private GameObject ennemy;

    public struct Tile
    {
        public GameObject tile { get; set; }
        public int cost { get; set; }
        public int heuristique { get; set; }
        public bool inClosedList { get; set; }
        public bool inOpenList { get; set; }
        public int predX;
        public int predY;
    }
    private Tile[,] tiles;

    private int[,] grid = new int[,]
    {
        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
     };
    // Start is called before the first frame update
    void Start()
    {
        CreateGrid();
        CreateEnnemy();

        //Debug.Log(Mathf.RoundToInt(10.4f));
        //AStar();
        Dijkstra();
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

                tiles[i, j] = new Tile() { tile = tile, cost = 1, heuristique = 0, inClosedList = false, inOpenList = false };
            }
        }
    }

    private void CreateEnnemy()
    {
        ennemy = Instantiate(ennemySprite, transform);
        ennemy.transform.position = new Vector2(17, 25);
    }

    private int Dijkstra()
    {
        int x = Mathf.RoundToInt(ennemy.transform.position.x);
        int y = Mathf.RoundToInt(ennemy.transform.position.y);
        List<Tile> nodes = new List<Tile>();
        Tile fin = new Tile();
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
        nodes.Remove(tiles[x, y]);
        poids[x, y] = 0;

        while(nodes.Count > 0)
        {
            int minCost = 99;
            Tile tile = new Tile();
            foreach(Tile t in nodes)
            {
                if(t.cost < minCost)
                {
                    minCost = t.cost;
                    tile = t;
                }
            }
            x = Mathf.RoundToInt(tile.tile.transform.position.x);
            y = Mathf.RoundToInt(tile.tile.transform.position.y);
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
                int xV = Mathf.RoundToInt(t.tile.transform.position.x);
                int yV = Mathf.RoundToInt(t.tile.transform.position.y);
                if (poids[xV, yV] > poids[x, y] + 1)
                {
                    poids[xV, yV] = poids[x, y] + 1;
                    t.predX = x;
                    t.predY = y;
                }
                
            }
            fin = tile;
        }
        Debug.Log
        
        return 1;
    }
    private int AStar()
    {
        List<Tile> closedList = new List<Tile>();
        Stack<Tile> openList = new Stack<Tile>();

        Tile depart = tiles[17, 25];
        Tile objectif = tiles[17, 24];
        int xO = Mathf.RoundToInt(objectif.tile.transform.position.x);
        int yO = Mathf.RoundToInt(objectif.tile.transform.position.y);
        openList.Push(depart);
        depart.inOpenList = true;
        Debug.Log("Start");
        while (openList.Count > 0)
        {
            Tile current = openList.Pop();
            current.inOpenList = false;

            int x = Mathf.RoundToInt(current.tile.transform.position.x);
            int y = Mathf.RoundToInt(current.tile.transform.position.y);
            Debug.Log("Position "+x+" "+y);
            Stack<Tile> neighbors = new Stack<Tile>();
            if (x == objectif.tile.transform.position.x && y == objectif.tile.transform.position.y)
            {
                Debug.Log("Succes");
                return 0;
            }
            
            if (grid[x + 1, y] == 0)
                neighbors.Push(tiles[x + 1, y]);
            if (grid[x - 1, y] == 0)
                neighbors.Push(tiles[x - 1, y]);
            if (grid[x, y + 1] == 0)
                neighbors.Push(tiles[x, y+1]);
            if (grid[x, y-1] == 0)
                neighbors.Push(tiles[x, y-1]);
            
            while(neighbors.Count > 0)
            {
                Debug.Log("Voisins " + neighbors.Count);
                Tile neighbor = neighbors.Pop();
                if (!(neighbor.inClosedList || (neighbor.inOpenList && neighbor.cost < current.cost)))
                {
                    Debug.Log("Entrer voisin");
                    neighbor.cost = current.cost +1;

                    int xN = Mathf.RoundToInt(neighbor.tile.transform.position.x);
                    int yN = Mathf.RoundToInt(neighbor.tile.transform.position.y);

                    neighbor.heuristique = neighbor.cost + (Mathf.Abs(xN - xO) + Mathf.Abs(yN - yO));
                    Debug.Log("Score " + neighbor.heuristique);
                    openList.Push(neighbor);
                    neighbor.inOpenList = true;
                }
            }
            openList.OrderBy(t => t.heuristique);
            closedList.Add(current);
            current.inClosedList = true;
            
        }
        Debug.Log("Echec");
        return 1;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
