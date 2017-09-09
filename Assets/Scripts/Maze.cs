using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Random = UnityEngine.Random;

// TODO: Test the maze by watching its display after every computation
// Might need to change the amount of time delayed
public class Maze : MonoBehaviour
{
    private static readonly System.Random Rand = new System.Random();

    private int _tileNumX = 60;
    private int _tileNumY = 60;
    private int _tileSizeX = 1;
    private int _tileSizeY = 1;
    private Rect mazeRect;

    private enum TileType
    { Wall, Floor, Lava }

    private GameObject[,] _indexToGameObject;
    private TileType[,] _indexToType;
    private List<Vector2> _wallIndicesToCheck;
    private Dictionary<Vector2, HashSet<Vector2>> _indexToFloorSet;
    private Dictionary<TileType, Sprite> _tileTypeToSprite;

    // Use this for initialization
    private void Start()
    {
        _tileTypeToSprite = new Dictionary<TileType, Sprite>();
        _wallIndicesToCheck = new List<Vector2>();
        _indexToFloorSet = new Dictionary<Vector2, HashSet<Vector2>>();

        mazeRect = new Rect(0, 0, _tileNumX, _tileNumY);

        InitializeSpriteDict();
        InitializeTiles();

        CreateMaze();
        //SmoothMaze();

        CreateTileGameObjects();
        SetAllTileTextures();

        StartCoroutine(StartFloodingIterative(new Vector2(0, 0), .05f));
    }

    // Update is called once per frame
    private void Update()
    {
        SetAllTileTextures();
    }

    private Vector2 getRandomWallIndex()
    {
        return _wallIndicesToCheck[Rand.Next(0, _wallIndicesToCheck.Count - 1)];
    }

    // TODO: Refactor this function
    // TODO: Take away the yield and display
    private void CreateMaze()
    {
        while (_wallIndicesToCheck.Count != 0)
        {
            Vector2 startingIndex = getRandomWallIndex();

            Vector2 direction = new Vector2();
            int minFloorLenght = 4;

            int randDirection = (int)Random.Range(0f, 3.99f);

            switch (randDirection)
            {
                case 0:
                    direction = new Vector2(0, 1);
                    break;

                case 1:
                    direction = new Vector2(1, 0);
                    break;

                case 2:
                    direction = new Vector2(0, -1);
                    break;

                case 3:
                    direction = new Vector2(-1, 0);
                    break;
            }

            for (int i = 0; i < minFloorLenght; i++)
            {
                Vector2 index = startingIndex + direction * i;
                if (mazeRect.Contains(index))
                {
                    if (_indexToType[(int)index.x, (int)index.y] == TileType.Floor) break;

                    CreateMazeTile(index);
                }
            }
        }
    }

    private void CreateMazeTile(Vector2 wallIndex)
    {
        if (mazeRect.Contains(wallIndex) == false) return;

        _wallIndicesToCheck.Remove(wallIndex);

        List<HashSet<Vector2>> neighboringSets = ReturnNeighboringSets(wallIndex);
        bool allDistinctSets = allSetsAreDistinct(ref neighboringSets);

        int neighborCount = neighboringSets.Count;
        if (neighborCount == 0)
        {
            // Remove from wall list, create as floor, and change dictionary
            // but don't need to deal with neighboring sets
            _indexToType[(int)wallIndex.x, (int)wallIndex.y] = TileType.Floor;

            HashSet<Vector2> floorSet = _indexToFloorSet[wallIndex];
            floorSet.Add(wallIndex);
            _indexToFloorSet[wallIndex] = floorSet;
        }
        else if (1 <= neighborCount && neighborCount <= 2 && allDistinctSets == true)
        {
            // Remove from wall list, create as floor, and change dictionary
            _indexToType[(int)wallIndex.x, (int)wallIndex.y] = TileType.Floor;

            HashSet<Vector2> unionOfAllFloorSets = new HashSet<Vector2>();
            foreach (HashSet<Vector2> set in neighboringSets)
            {
                unionOfAllFloorSets.UnionWith(set);
            }

            unionOfAllFloorSets.Add(wallIndex);
            foreach (Vector2 index in unionOfAllFloorSets)
            {
                _indexToFloorSet[index] = unionOfAllFloorSets;
            }
        }
    }

    private bool allSetsAreDistinct(ref List<HashSet<Vector2>> neighboringSets)
    {
        bool allDistinctSets = true;
        for (int i = 0; i < neighboringSets.Count; i++)
        {
            for (int j = 0; j < neighboringSets.Count; j++)
            {
                HashSet<Vector2> firstSet = neighboringSets[i];
                HashSet<Vector2> secondSet = neighboringSets[j];

                if (i != j && firstSet == secondSet)
                {
                    allDistinctSets = false;
                }
            }
        }
        return allDistinctSets;
    }

    // Starting at top, add in clockwise direction
    // Only adds a set to the list if the set is non-empty
    // TODO: refactor this to reduce the amount of code
    private List<HashSet<Vector2>> ReturnNeighboringSets(Vector2 index)
    {
        List<HashSet<Vector2>> neighboringSets = new List<HashSet<Vector2>>();

        AddSetToList(index + new Vector2(0, 1), ref neighboringSets);
        AddSetToList(index + new Vector2(1, 0), ref neighboringSets);
        AddSetToList(index + new Vector2(0, -1), ref neighboringSets);
        AddSetToList(index + new Vector2(-1, 0), ref neighboringSets);

        return neighboringSets;
    }

    private void AddSetToList(Vector2 index, ref List<HashSet<Vector2>> neighboringSets)
    {
        if (mazeRect.Contains(index) == false) return;

        HashSet<Vector2> floorSet = _indexToFloorSet[index];
        if (floorSet.Count != 0)
            neighboringSets.Add(floorSet);
    }

    private void InitializeTiles()
    {
        _indexToType = new TileType[_tileNumX, _tileNumY];
        _indexToGameObject = new GameObject[_tileNumX, _tileNumY];

        for (var x = 0; x < _tileNumX; x++)
        {
            for (var y = 0; y < _tileNumY; y++)
            {
                _indexToType[x, y] = TileType.Wall;
                _wallIndicesToCheck.Add(new Vector2(x, y));

                HashSet<Vector2> emptySet = new HashSet<Vector2>();
                _indexToFloorSet.Add(new Vector2(x, y), emptySet);
            }
        }
    }

    private void InitializeSpriteDict()
    {
        Sprite[] tileSprites = Resources.LoadAll<Sprite>("Sprites");
        foreach (Sprite sprite in tileSprites)
        {
            if (sprite.name == "Floor")
                _tileTypeToSprite[TileType.Floor] = sprite;
            else if (sprite.name == "Wall")
                _tileTypeToSprite[TileType.Wall] = sprite;
            else if (sprite.name == "Lava")
                _tileTypeToSprite[TileType.Lava] = sprite;
        }
    }

    private void SmoothMaze()
    {
        for (int x = 0; x < _tileNumX; x++)
        {
            for (int y = 0; y < _tileNumY; y++)
            {
                Vector2 index = tileIndexToWorldCoord(new Vector2(x, y));
                if (SurroundedByOther(index))
                {
                    TileType myType = _indexToType[(int)index.x, (int)index.y];

                    if (myType == TileType.Floor)
                        _indexToType[(int)index.x, (int)index.y] = TileType.Wall;
                    else if (myType == TileType.Wall)
                        _indexToType[(int)index.x, (int)index.y] = TileType.Floor;
                }
            }
        }
    }

    private bool SurroundedByOther(Vector2 index)
    {
        HashSet<Vector2> allNeighbors = new HashSet<Vector2>();

        if (index.x > 0)
            allNeighbors.Add(index + new Vector2(-1, 0));
        if (index.x < _tileNumX - 1)
            allNeighbors.Add(index + new Vector2(1, 0));
        if (index.y > 0)
            allNeighbors.Add(index + new Vector2(0, -1));
        if (index.y < _tileNumY - 1)
            allNeighbors.Add(index + new Vector2(0, 1));

        TileType myType = _indexToType[(int)index.x, (int)index.y];

        foreach (Vector2 neighborIndex in allNeighbors)
        {
            TileType otherType = _indexToType[(int)neighborIndex.x, (int)neighborIndex.y];
            if (myType == otherType)
            {
                return false;
            }
        }

        return true;
    }

    private void CreateTileGameObjects()
    {
        for (int x = 0; x < _tileNumX; x++)
        {
            for (int y = 0; y < _tileNumY; y++)
            {
                GameObject tile = new GameObject();
                tile.name = "Tile_" + x + "_" + y;
                tile.transform.SetParent(this.transform);

                Vector2 tileWorldCoord = tileIndexToWorldCoord(new Vector2(x, y));
                tile.transform.position = tileWorldCoord;
                tile.AddComponent<SpriteRenderer>().sprite = _tileTypeToSprite[TileType.Wall];
                _indexToGameObject[x, y] = tile;
            }
        }
    }

    private IEnumerator StartFloodingRecursive(Vector2 index, int level)
    {
        // Apply flooding to this tile
        if (mazeRect.Contains(index) &&
            _indexToType[(int)index.x, (int)index.y] == TileType.Floor)
        {
            _indexToType[(int)index.x, (int)index.y] = TileType.Lava;
        }
        else
        {
            yield return new WaitForSeconds(0);
        }

        yield return new WaitForSeconds(1f);

        // Recurse on surroudning tiles
        HashSet<Vector2> neighboringTiles = new HashSet<Vector2>();

        if (index.x > 0)
            neighboringTiles.Add(index + new Vector2(-1, 0));
        if (index.x < _tileNumX - 1)
            neighboringTiles.Add(index + new Vector2(1, 0));
        if (index.y > 0)
            neighboringTiles.Add(index + new Vector2(0, -1));
        if (index.y < _tileNumY - 1)
            neighboringTiles.Add(index + new Vector2(0, 1));

        foreach (Vector2 neighboringTile in neighboringTiles)
        {
            StartCoroutine(StartFloodingRecursive(neighboringTile, level + 1));
        }

        yield return new WaitForSeconds(0);
    }

    private IEnumerator StartFloodingIterative(Vector2 startingIndex, float waitTime)
    {
        HashSet<Vector2> closedList = new HashSet<Vector2>();
        Queue<Vector2> openList = new Queue<Vector2>();

        openList.Enqueue(startingIndex);

        while (openList.Count != 0)
        {
            Vector2 currIndex = openList.Dequeue();
            _indexToType[(int)currIndex.x, (int)currIndex.y] = TileType.Lava;

            Vector2[] possibleIndices = {new Vector2(0, 1) + currIndex, new Vector2(1, 0) + currIndex,
                new Vector2(0, -1) + currIndex, new Vector2(-1, 0) + currIndex};

            foreach (Vector2 possibleIndex in possibleIndices)
            {
                if (mazeRect.Contains(possibleIndex))
                {
                    if (_indexToType[(int)possibleIndex.x, (int)possibleIndex.y] == TileType.Floor)
                    {
                        if (closedList.Contains(possibleIndex) == false)
                        {
                            closedList.Add(possibleIndex);
                            openList.Enqueue(possibleIndex);
                        }
                    }
                }
            }

            yield return new WaitForSeconds(waitTime);
        }
    }

    private void SetAllTileTextures()
    {
        // Test the display by destroying all gameobjects
        for (int x = 0; x < _tileNumX; x++)
        {
            for (int y = 0; y < _tileNumY; y++)
            {
                _indexToGameObject[x, y].GetComponent<SpriteRenderer>().sprite =
                    _tileTypeToSprite[_indexToType[x, y]];
            }
        }
    }

    private Vector2 tileIndexToWorldCoord(Vector2 index)
    {
        return index;
    }
}