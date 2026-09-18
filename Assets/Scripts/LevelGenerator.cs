using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Runtime Tile Template")]
    [SerializeField] private GameObject tileTemplate;

    [Header("Level Sprites")]
    [SerializeField] private Sprite outsideCornerSprite;
    [SerializeField] private Sprite outsideWallSprite;
    [SerializeField] private Sprite insideCornerSprite;
    [SerializeField] private Sprite insideWallSprite;
    [SerializeField] private Sprite pelletSprite;
    [SerializeField] private Sprite powerPelletSprite;
    [SerializeField] private Sprite tJunctionSprite;
    [SerializeField] private Sprite ghostExitSprite;

    [Header("Power Pellet Animation")]
    [SerializeField]
    private RuntimeAnimatorController powerPelletController;

    [Header("Layout")]
    [SerializeField] private float cellSize = 0.32f;

    private const int North = 1;
    private const int East = 2;
    private const int South = 4;
    private const int West = 8;

    private readonly int[,] levelMap =
    {
        {1,2,2,2,2,2,2,2,2,2,2,2,2,7},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,4},
        {2,6,4,0,0,4,5,4,0,0,0,4,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,3},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,5},
        {2,5,3,4,4,3,5,3,3,5,3,4,4,4},
        {2,5,3,4,4,3,5,4,4,5,3,4,4,3},
        {2,5,5,5,5,5,5,4,4,5,5,5,5,4},
        {1,2,2,2,2,1,5,4,3,4,4,3,0,4},
        {0,0,0,0,0,2,5,4,3,4,4,3,0,3},
        {0,0,0,0,0,2,5,4,4,0,0,0,0,0},
        {0,0,0,0,0,2,5,4,4,0,3,4,4,8},
        {2,2,2,2,2,1,5,3,3,0,4,0,0,0},
        {0,0,0,0,0,0,5,0,0,0,4,0,0,0},
    };

    private void Start()
    {
        GameObject manualLevel = GameObject.Find("Level01");

        if (manualLevel != null)
        {
            manualLevel.SetActive(false);
            Destroy(manualLevel);
        }

        int[,] fullMap = CreateFullMap();
        GenerateLevel(fullMap);
        FitCamera(fullMap);
    }

    private int[,] CreateFullMap()
    {
        int sourceRows = levelMap.GetLength(0);
        int sourceColumns = levelMap.GetLength(1);

        int fullRows = sourceRows * 2 - 1;
        int fullColumns = sourceColumns * 2;

        int[,] fullMap = new int[fullRows, fullColumns];

        for (int row = 0; row < fullRows; row++)
        {
            int sourceRow =
                row < sourceRows ? row : fullRows - 1 - row;

            for (int column = 0; column < fullColumns; column++)
            {
                int sourceColumn =
                    column < sourceColumns
                        ? column
                        : fullColumns - 1 - column;

                fullMap[row, column] =
                    levelMap[sourceRow, sourceColumn];
            }
        }

        return fullMap;
    }

    private void GenerateLevel(int[,] map)
    {
        if (tileTemplate == null)
        {
            Debug.LogError("LevelGenerator: Tile Template is missing.");
            return;
        }

        GameObject generatedLevel =
            new GameObject("GeneratedLevel01");

        int rows = map.GetLength(0);
        int columns = map.GetLength(1);

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                int tileType = map[row, column];

                if (tileType == 0)
                {
                    continue;
                }

                CreateTile(
                    map,
                    row,
                    column,
                    tileType,
                    generatedLevel.transform
                );
            }
        }
    }

    private void CreateTile(
        int[,] map,
        int row,
        int column,
        int tileType,
        Transform parent)
    {
        int rows = map.GetLength(0);
        int columns = map.GetLength(1);

        float x =
            (column - (columns - 1) * 0.5f) * cellSize;

        float y =
            ((rows - 1) * 0.5f - row) * cellSize;

        GameObject tile = Instantiate(
            tileTemplate,
            new Vector3(x, y, 0f),
            Quaternion.identity,
            parent
        );

        tile.name =
            $"Generated_R{row:00}_C{column:00}_Type{tileType}";

        SpriteRenderer spriteRenderer =
            tile.GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            spriteRenderer = tile.AddComponent<SpriteRenderer>();
        }

        spriteRenderer.sprite = GetSprite(tileType);
        spriteRenderer.color =
            tileType == 8
                ? new Color(1f, 0.25f, 0.75f, 1f)
                : Color.white;

        float rotation =
            GetRotation(map, row, column, tileType);

        tile.transform.rotation =
            Quaternion.Euler(0f, 0f, rotation);

        if (tileType == 6 &&
            powerPelletController != null)
        {
            Animator animator = tile.AddComponent<Animator>();
            animator.runtimeAnimatorController =
                powerPelletController;
        }
    }

    private Sprite GetSprite(int tileType)
    {
        switch (tileType)
        {
            case 1: return outsideCornerSprite;
            case 2: return outsideWallSprite;
            case 3: return insideCornerSprite;
            case 4: return insideWallSprite;
            case 5: return pelletSprite;
            case 6: return powerPelletSprite;
            case 7: return tJunctionSprite;
            case 8: return ghostExitSprite;
            default: return null;
        }
    }

    private float GetRotation(
        int[,] map,
        int row,
        int column,
        int tileType)
    {
        switch (tileType)
        {
            case 1:
                return GetOutsideCornerRotation(
                    GetNeighbourMask(
                        map,
                        row,
                        column,
                        IsOutsideWallPiece
                    )
                );

            case 2:
                return GetStraightRotation(
                    GetNeighbourMask(
                        map,
                        row,
                        column,
                        IsOutsideWallPiece
                    )
                );

            case 3:
                return GetInsideCornerRotation(
                    GetInsideCornerMask(map, row, column)
                );

            case 4:
                return GetInsideWallRotation(
                    map,
                    row,
                    column
                );

            case 7:
                return GetTJunctionRotation(
                    GetNeighbourMask(
                        map,
                        row,
                        column,
                        IsWallPiece
                    )
                );

            case 8:
                return GetStraightRotation(
                    GetNeighbourMask(
                        map,
                        row,
                        column,
                        IsWallPiece
                    )
                );

            default:
                return 0f;
        }
    }

    private int GetNeighbourMask(
        int[,] map,
        int row,
        int column,
        System.Predicate<int> acceptedType)
    {
        int mask = 0;

        if (IsAccepted(map, row - 1, column, acceptedType))
            mask |= North;

        if (IsAccepted(map, row, column + 1, acceptedType))
            mask |= East;

        if (IsAccepted(map, row + 1, column, acceptedType))
            mask |= South;

        if (IsAccepted(map, row, column - 1, acceptedType))
            mask |= West;

        return mask;
    }

    private bool IsAccepted(
        int[,] map,
        int row,
        int column,
        System.Predicate<int> acceptedType)
    {
        if (row < 0 ||
            row >= map.GetLength(0) ||
            column < 0 ||
            column >= map.GetLength(1))
        {
            return false;
        }

        return acceptedType(map[row, column]);
    }

    private bool IsWallPiece(int value)
    {
        return value == 1 ||
               value == 2 ||
               value == 3 ||
               value == 4 ||
               value == 7 ||
               value == 8;
    }

    private bool IsOutsideWallPiece(int value)
    {
        return value == 1 ||
               value == 2 ||
               value == 7;
    }

    private bool IsInsideWallPiece(int value)
    {
        return value == 3 ||
               value == 4 ||
               value == 7 ||
               value == 8;
    }

    private float GetStraightRotation(int mask)
    {
        bool hasHorizontal =
            (mask & (East | West)) != 0;

        bool hasVertical =
            (mask & (North | South)) != 0;

        if (hasVertical && !hasHorizontal)
        {
            return 90f;
        }

        return 0f;
    }

    private float GetInsideWallRotation(
        int[,] map,
        int row,
        int column)
    {
        int mask = GetNeighbourMask(
            map,
            row,
            column,
            IsInsideWallPiece
        );

        bool northAndSouth =
            (mask & North) != 0 &&
            (mask & South) != 0;

        bool eastAndWest =
            (mask & East) != 0 &&
            (mask & West) != 0;

        if (northAndSouth)
        {
            return 90f;
        }

        if (eastAndWest)
        {
            return 0f;
        }

        return GetStraightRotation(mask);
    }

    private int GetInsideCornerMask(
        int[,] map,
        int row,
        int column)
    {
        int mask = 0;

        CheckInsideCornerNeighbour(
            map, row - 1, column, North, ref mask);

        CheckInsideCornerNeighbour(
            map, row, column + 1, East, ref mask);

        CheckInsideCornerNeighbour(
            map, row + 1, column, South, ref mask);

        CheckInsideCornerNeighbour(
            map, row, column - 1, West, ref mask);

        return ChooseCornerPair(mask);
    }

    private void CheckInsideCornerNeighbour(
        int[,] map,
        int row,
        int column,
        int direction,
        ref int mask)
    {
        if (row < 0 ||
            row >= map.GetLength(0) ||
            column < 0 ||
            column >= map.GetLength(1))
        {
            return;
        }

        int neighbour = map[row, column];

        if (neighbour == 3 ||
            neighbour == 7 ||
            neighbour == 8)
        {
            mask |= direction;
            return;
        }

        if (neighbour != 4)
        {
            return;
        }

        float neighbourRotation =
            GetInsideWallRotation(map, row, column);

        bool horizontalDirection =
            direction == East || direction == West;

        bool verticalDirection =
            direction == North || direction == South;

        if ((horizontalDirection &&
             neighbourRotation == 0f) ||
            (verticalDirection &&
             neighbourRotation == 90f))
        {
            mask |= direction;
        }
    }

    private int ChooseCornerPair(int mask)
    {
        int eastSouth = East | South;
        int southWest = South | West;
        int northEast = North | East;
        int northWest = North | West;

        if ((mask & eastSouth) == eastSouth)
            return eastSouth;

        if ((mask & southWest) == southWest)
            return southWest;

        if ((mask & northEast) == northEast)
            return northEast;

        return northWest;
    }

    private float GetOutsideCornerRotation(int mask)
    {
        int pair = ChooseCornerPair(mask);

        if (pair == (East | South))
            return 0f;

        if (pair == (North | East))
            return 90f;

        if (pair == (North | West))
            return 180f;

        return 270f;
    }

    private float GetInsideCornerRotation(int mask)
    {
        if (mask == (South | West))
            return 0f;

        if (mask == (East | South))
            return 90f;

        if (mask == (North | East))
            return 180f;

        return 270f;
    }

    private float GetTJunctionRotation(int mask)
    {
        bool north = (mask & North) != 0;
        bool east = (mask & East) != 0;
        bool south = (mask & South) != 0;
        bool west = (mask & West) != 0;

        if (!north && east && south && west)
            return 0f;

        if (north && east && south && !west)
            return 90f;

        if (north && east && !south && west)
            return 180f;

        return 270f;
    }

    private void FitCamera(int[,] map)
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            return;
        }

        int rows = map.GetLength(0);
        int columns = map.GetLength(1);

        float requiredVerticalSize =
            rows * cellSize * 0.5f + cellSize;

        float requiredHorizontalSize =
            (columns * cellSize * 0.5f + cellSize) /
            Mathf.Max(mainCamera.aspect, 0.01f);

        mainCamera.orthographic = true;
        mainCamera.orthographicSize =
            Mathf.Max(
                requiredVerticalSize,
                requiredHorizontalSize
            );

        mainCamera.transform.position =
            new Vector3(0f, 0f, -10f);
    }
}