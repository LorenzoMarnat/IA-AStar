using System;
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
    public GameObject text;

    // Noeud du graphe
    public struct Tile
    {
        public int x { get; set; }
        public int y { get; set; }
        public int cost { get; set; }
        public int totalCost { get; set; }
        public int heuristique { get; set; }

        // Position du predecesseur
        public int predX { get; set; }
        public int predY { get; set; }
    }

    private Tile[,] tiles;

    private struct Loc
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    // Pile des déplacements de l'ennemi pour arriver jusqu'au joueur
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

    // Mode de pathfinding, aStar = true => algo AStar, sinon algo Dijkstra
    private bool aStar;


    // Start is called before the first frame update
    void Start()
    {
        CreateGrid();
        CreateEnnemy();
        aStar = true;
        InvokeRepeating("Pathfinding", 0, 0.1f);
        InvokeRepeating("EnnemyMove", 0, 0.1f);
    }

    // Choisi l'algo à executer selon le booléen aStar
    private void Pathfinding()
    {
        if (aStar)
            AStar();
        else
            Dijkstra();
    }

    // Crée la grille du niveau
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

                tiles[i, j] = new Tile() { x = i, y = j, cost = 1, heuristique = 0, totalCost = 0, predX = -1, predY = -1 };
            }
        }
    }

    // Crée l'ennemie
    private void CreateEnnemy()
    {
        ennemy = Instantiate(ennemySprite, transform);
        ennemy.transform.position = new Vector2(14.5f, 22);
    }


    private int Dijkstra()
    {
        // Récupère la position de l'ennemie comme noeud de départ de l'algo
        int x = Mathf.RoundToInt(ennemy.transform.position.x);
        int y = Mathf.RoundToInt(ennemy.transform.position.y);

        List<Tile> nodes = new List<Tile>();

        // Initialise les poids à une valeur arbitrairement grande
        int[,] poids = new int[grid.GetLength(0), grid.GetLength(1)];
        for (int i = 0; i < grid.GetLength(0); i++)
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                if (grid[i, j] != 1)
                {
                    nodes.Add(tiles[i, j]);
                    poids[i, j] = 9999;
                }
            }

        poids[x, y] = 0;

        while(nodes.Count > 0)
        {

            // Récupère le noeud de poids le plus faible et le supprime de la liste
            int minCost = 9999;
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

            // Récupère les 4 voisins autours du noeud (si non obstacle)
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

        // Retrace le chemin que l'ennemie doit effectuer en partant du joueur
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

        // Indique si un noeud à la position (i,j) est dans l'openList ou la closedList
        bool[,] inOpenlist = new bool[grid.GetLength(0), grid.GetLength(1)];
        bool[,] inClosedlist = new bool[grid.GetLength(0), grid.GetLength(1)];
        
        Tile depart = tiles[Mathf.RoundToInt(ennemy.transform.position.x), Mathf.RoundToInt(ennemy.transform.position.y)];
        Tile objectif = tiles[Mathf.RoundToInt(player.transform.position.x), Mathf.RoundToInt(player.transform.position.y)];
        int xO = objectif.x;
        int yO = objectif.y;
        int xD = depart.x;
        int yD = depart.y;

        // Place la position de départ dans l'openList
        inOpenlist[xD, yD] = true;
        tiles[xD, yD].totalCost = 0;
        openList.Add(tiles[xD, yD]);

        while (openList.Count > 0)
        {
            // Récupère le noeud de cout minimum
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

            // Enlève le noeud courrant de l'openList et le place dans closedList
            inOpenlist[x, y] = false;
            openList.Remove(tiles[x, y]);

            inClosedlist[x, y] = true;
            closedList.Add(tiles[x, y]);

            // Si le noeud courrant est l'objectif, fin de l'algo
            // Retrace le chemin depuis le joueur jusqu'à l'ennemie
            if (x == xO && y == yO)
            {
                int xP = Mathf.RoundToInt(player.transform.position.x);
                int yP = Mathf.RoundToInt(player.transform.position.y);
                ennemyMoves = new Stack<Loc>();
                do
                {
                    
                    int temp = xP;
                    xP = tiles[xP, yP].predX;
                    yP = tiles[temp, yP].predY;
                    if (xP == -1 || yP == -1)
                        Debug.Log("-1");
                    ennemyMoves.Push(new Loc() { x = xP, y = yP });
                }
                while (tiles[xP, yP].predX != Mathf.RoundToInt(ennemy.transform.position.x) || tiles[xP, yP].predY != Mathf.RoundToInt(ennemy.transform.position.y));
                return 0;
            }

            // Récupère les 4 voisins adjacents (si non obstacle)
            Stack<Tile> neighbors = new Stack<Tile>();
            if (grid[x + 1, y] == 0)
                neighbors.Push(tiles[x + 1, y]);
            if (grid[x - 1, y] == 0)
                neighbors.Push(tiles[x - 1, y]);

            if (grid[x, y + 1] == 0)
                neighbors.Push(tiles[x, y+1]);

            if (grid[x, y - 1] == 0)
                neighbors.Push(tiles[x  ,y-1]);

            
            while(neighbors.Count > 0)
            {
                Tile neighbor = neighbors.Pop();
                int xN = neighbor.x;
                int yN = neighbor.y;

                // Si le voisin est dans la closedList, passe au voisin suivant
                if (inClosedlist[xN, yN])
                    continue;
                else
                {
                    // Si le nouveau chemin vers le voisin à un cout plus faible que l'ancien
                    // Ou que le voisin n'est pas dans l'openList
                    if((Mathf.Abs(xN - x) + Mathf.Abs(yN - y)) + tiles[x,y].totalCost < tiles[xN, yN].totalCost || !inOpenlist[xN,yN])
                    {
                        // Distance entre le noeud courrant et le voisin
                        tiles[xN, yN].cost = (Mathf.Abs(xN - x) + Mathf.Abs(yN - y));
                        // Distance entre le noeud courrant et l'objectif
                        tiles[xN, yN].heuristique = (Mathf.Abs(xN - xO) + Mathf.Abs(yN - yO));
                        // Cout total
                        tiles[xN, yN].totalCost = tiles[xN, yN].cost + tiles[xN, yN].heuristique;

                        // Le predecesseur du noeud voisin devient le noeud courrant
                        tiles[xN, yN].predX = x;
                        tiles[xN, yN].predY = y;

                        // Ajoute le voisin à l'openList s'il n'y est pas
                        if(!inOpenlist[xN,yN])
                        {
                            inOpenlist[xN, yN] = true;
                            openList.Add(tiles[xN, yN]);
                        }
                    }
                }
            }
        }
        // Termine l'algo si aucun chemin n'est trouvé
        return -1;
    }

    // Récupère le prochain mouvement que l'ennemie doit effectuer
    private void EnnemyMove()
    {
        if (ennemyMoves.Count > 0)
            nextMove = ennemyMoves.Pop();
    }

    void Update()
    {
        // Dirige l'ennemie vers la case adjacente le rapprochant du joueur
        ennemy.transform.position = Vector2.MoveTowards(ennemy.transform.position, new Vector2(nextMove.x, nextMove.y), 4 * Time.deltaTime);

        // Change le mode de pathfinding quand le joueur appuie sur ESPACE
        if (Input.GetKeyDown(KeyCode.Space))
        {
            aStar = !aStar;
            if (aStar)
                text.GetComponent<UnityEngine.UI.Text>().text = "Press SPACE to switch pathfinding mode: A*";
            else
                text.GetComponent<UnityEngine.UI.Text>().text = "Press SPACE to switch pathfinding mode: Dijkstra";
        }
    }
}
