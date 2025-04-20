using System.Collections.Generic;
using UnityEngine;
using Kutie.Extensions;
using Kutie;

public class LevelGenerator5 : LevelGenerator
{
    [System.Serializable]
    class RoomSizeEntry
    {
        public Vector2Int Size;
        public int Weight;
    }


    [System.Serializable]
    public class RoomDoor
    {
        public Vector2Int Position;
        public DirectionMask Direction;

        public override string ToString()
        {
            return $"RoomDoor(Position: {Position}, Direction: {Direction})";
        }
    }

    [System.Serializable]
    class FixedRoomEntry
    {
        public Alignment2D Alignment;
        public Vector2Int Size;
        public Vector2Int Offset;
        public GameObject Prefab;
        public int PrefabRotation = 0;
        public Color GizmosColorOverride = Color.clear;
        public List<RoomDoor> Doors = new();
        public bool IsSpawnPoint = false;
    }

    [System.Serializable]
    class RequiredPathEntry
    {
        public Alignment2D StartAlignment;
        public Vector2Int StartOffset;
        public Alignment2D EndAlignment;
        public Vector2Int EndOffset;
    }
    
    [System.Serializable]
    class BlockedRoomEntry
    {
        public Alignment2D Alignment;
        public Vector2Int Offset;
        public Vector2Int Size;
    }

    
    [SerializeField] public int Floor;
    [SerializeField] public Elevators Elevators;
    [SerializeField] LevelGrid5 grid;
    [SerializeField] int officeWidth = 5;
    [SerializeField] int parkLength = 12;
    [SerializeField] int maxAttempts = 10;

    [SerializeField] List<FixedRoomEntry> fixedRooms = new();
    [SerializeField] List<GameObject> roomPrefabs = new();
    [SerializeField] List<RoomSizeEntry> roomSizes = new();
    [SerializeField] List<BlockedRoomEntry> blockedRooms = new();

    /// <summary>
    /// For every path in requiredPaths, there must be a path
    /// between the start and end room
    /// </summary>
    [SerializeField] List<RequiredPathEntry> requiredPaths = new();

    [SerializeField, HideInInspector] List<RectInt> roomRects = new();
    // if we have 4 rooms:
    // 0 1
    // 2 3
    // a door between 0 and 1 is horizontalDoors[0] = horizontalDoors[1-1]
    // a door between 1 and 3 is verticalDoors[]
    [SerializeField, HideInInspector] List<bool> horizontalDoors = new();
    [SerializeField, HideInInspector] List<bool> verticalDoors = new();
    [SerializeField, HideInInspector] List<List<Vector2Int>> paths = new();
    [SerializeField, HideInInspector] List<int> fixedRoomRectIndices = new();
    [SerializeField, HideInInspector] Room5 spawnRoom = null;
    [SerializeField, HideInInspector] List<Room5> rooms = new();

    private Dictionary<int, Room5> placedRooms = new();

    Queue<Vector2Int> spawnRoomQueue = new();

    Vector2Int maxRoomSize;

    class RegenerateException : System.Exception { }

    public override Vector3 GetPlayerSpawnPoint()
    {
        if(spawnRoom == null){
            Debug.LogError("No spawn room found");
            return Vector3.zero;
        }
        return spawnRoom.transform.position + spawnRoom.PlayerSpawnPoint;
    }

    void Reset()
    {
        grid = GetComponent<LevelGrid5>();
        grid.Size = Vector2Int.one * (2 * officeWidth + parkLength);
    }

    public override void Clear()
    {
        grid.Clear();
        roomRects.Clear();
        paths.Clear();
        fixedRoomRectIndices.Clear();
        this.ClearChildren();
        placedRooms.Clear();
        rooms.Clear();
    }

    public override void Generate(int seed)
    {
        Random.InitState(seed);
        Debug.Log($"Generating level with seed {seed}");

        grid.Size = grid.Size = Vector2Int.one * (2 * officeWidth + parkLength);
        int attempts = 0;
        while (attempts < maxAttempts)
        {
            try
            {
                Clear();
                CalculateRoomSizes();
                InitializeGrid();
                InitializeSpawning();
                AddFixedRoomsToGrid();
                PartitionGrid();
                FindPaths();
                InstantiateFixedRooms();
                InstantiateRooms();
                SpawnDoors();

                return;
            }
            catch (RegenerateException)
            {
                ++attempts;
            }
        }
    }

    void CalculateRoomSizes()
    {
        maxRoomSize = Vector2Int.zero;
        foreach (var sizeEntry in roomSizes)
        {
            maxRoomSize.x = System.Math.Max(maxRoomSize.x, sizeEntry.Size.x);
            maxRoomSize.y = System.Math.Max(maxRoomSize.y, sizeEntry.Size.y);
        }
    }

    void InitializeGrid()
    {
        for (int x = officeWidth; x < officeWidth + parkLength; ++x)
        {
            for (int y = officeWidth; y < officeWidth + parkLength; ++y)
            {
                grid.SetCellType(new Vector2Int(x, y), CellType5.Blocked);
            }
        }
        
        foreach(var blockedRoom in blockedRooms){
            var roomSize = blockedRoom.Size;
            var roomPosition = grid.AlignmentOffsetRectToCell(
                blockedRoom.Alignment,
                blockedRoom.Offset,
                roomSize
            );
            RectInt roomRect = new(
                roomPosition.x, roomPosition.y,
                roomSize.x, roomSize.y
            );
            PlaceRoomRect(roomRect, Color.red, CellType5.Blocked);
        }
    }

    void InitializeSpawning()
    {
        spawnRoomQueue = new();
    }

    void AddFixedRoomsToGrid()
    {
        foreach (var entry in fixedRooms)
        {
            // calculate room position
            Vector2Int roomSize = entry.Size;
            Vector2Int roomPosition = grid.AlignmentOffsetRectToCell(
                entry.Alignment,
                entry.Offset,
                roomSize
            );

            // place room
            RectInt roomRect = new(
                roomPosition.x, roomPosition.y,
                roomSize.x, roomSize.y
            );
            PlaceRoomRect(roomRect, entry.GizmosColorOverride);
            fixedRoomRectIndices.Add(roomRects.Count - 1);

            // set the doors
            foreach (var doorEntry in entry.Doors)
            {
                Vector2Int doorPosition = doorEntry.Position;
                grid.GetCell(roomPosition + doorPosition).DoorDirections = doorEntry.Direction;
            }
        }
    }

    void PlaceRoomRect(RectInt chosenRoomRect, CellType5 cellType = CellType5.Occupied)
    {
        PlaceRoomRect(chosenRoomRect, Color.clear, cellType);
    }

    void PlaceRoomRect(RectInt chosenRoomRect, Color colorOverride, CellType5 cellType = CellType5.Occupied)
    {
        for (int x = chosenRoomRect.xMin; x < chosenRoomRect.xMax; ++x)
        {
            for (int y = chosenRoomRect.yMin; y < chosenRoomRect.yMax; ++y)
            {
                var cell = grid.GetCell(
                    x, y
                );
                cell.Type = cellType;
                cell.RoomIndex = roomRects.Count;
                cell.ColorOverride = colorOverride;
            }
        }

        roomRects.Add(chosenRoomRect);

        // add all cells surrounding the room rect to the queue
        for (int x = chosenRoomRect.xMin - 1; x < chosenRoomRect.xMax + 1; ++x)
        {
            spawnRoomQueue.Enqueue(new Vector2Int(x, chosenRoomRect.yMin - 1));
            spawnRoomQueue.Enqueue(new Vector2Int(x, chosenRoomRect.yMax));
        }
        for (int y = chosenRoomRect.yMin; y < chosenRoomRect.yMax; ++y)
        {
            spawnRoomQueue.Enqueue(new Vector2Int(chosenRoomRect.xMin - 1, y));
            spawnRoomQueue.Enqueue(new Vector2Int(chosenRoomRect.xMax, y));
        }
    }

    void PartitionGrid()
    {
        if (spawnRoomQueue.Count == 0)
        {
            spawnRoomQueue.Enqueue(new Vector2Int(0, 0));
        }
        while (spawnRoomQueue.Count > 0)
        {
            var cell = spawnRoomQueue.Dequeue();
            if (!grid.CellInBounds(cell) || grid.GetCellType(cell) != CellType5.Empty)
            {
                continue;
            }

            bool CanFit(RectInt rect)
            {
                for (int x = rect.xMin; x < rect.xMax; ++x)
                {
                    for (int y = rect.yMin; y < rect.yMax; ++y)
                    {
                        if (
                            !grid.CellInBounds(x, y)
                            || grid.GetCellType(x, y) != CellType5.Empty
                        )
                        {
                            return false;
                        }
                    }
                }
                return true;
            }

            List<(RectInt, int)> viableRoomPositions = new();
            foreach (var roomSizeEntry in roomSizes)
            {
                var roomSize = roomSizeEntry.Size;
                // one room rect for each rotation
                // and for each pivot point
                RectInt[] roomRects = {
                    new(cell.x, cell.y, roomSize.x, roomSize.y),
                    new(cell.x, cell.y, roomSize.y, roomSize.x),

                    new(cell.x - roomSize.x + 1, cell.y, roomSize.x, roomSize.y),
                    new(cell.x - roomSize.y + 1, cell.y, roomSize.y, roomSize.x),

                    new(cell.x, cell.y - roomSize.x + 1, roomSize.x, roomSize.y),
                    new(cell.x, cell.y - roomSize.y + 1, roomSize.y, roomSize.x),

                    new(cell.x - roomSize.x + 1, cell.y - roomSize.y + 1, roomSize.x, roomSize.y),
                    new(cell.x - roomSize.y + 1, cell.y - roomSize.x + 1, roomSize.y, roomSize.x),
                };

                foreach (var roomRect in roomRects)
                {
                    if (CanFit(roomRect))
                    {
                        viableRoomPositions.Add((roomRect, roomSizeEntry.Weight));
                    }
                }
            }

            if (viableRoomPositions.Count == 0)
            {
                throw new RegenerateException();
            }

            // weight by room size to make larger rooms more likely
            int totalWeight = 0;
            foreach (var (roomRect, weight) in viableRoomPositions)
            {
                totalWeight += weight;
            }

            int choice = Random.Range(0, totalWeight);
            RectInt chosenRoomRect = new();
            foreach (var (roomRect, weight) in viableRoomPositions)
            {
                choice -= weight;
                if (choice <= 0)
                {
                    chosenRoomRect = roomRect;
                    break;
                }
            }

            PlaceRoomRect(chosenRoomRect);
        }
    }



    void FindPaths()
    {
        List<(Vector2Int, Vector2Int)> startEnds = new();
        foreach (var requiredPath in requiredPaths)
        {
            Vector2Int start = grid.AlignmentOffsetToCell(
                requiredPath.StartAlignment,
                requiredPath.StartOffset
            );
            Vector2Int end = grid.AlignmentOffsetToCell(
                requiredPath.EndAlignment,
                requiredPath.EndOffset
            );
            startEnds.Add((start, end));
        }

        HashSet<Vector2Int> reachableCells = new();
        foreach (var (start, end) in startEnds)
        {
            // do A* to find path and place doors
            // mark cells as reachable
            // so that we can break if a path is already connected
            var path = Kutie.Algorithm.AStar(
                start,
                heuristic: cell => Vector2Int.Distance(cell, end),
                isTarget: cell => cell == end
                                  || reachableCells.Contains(cell),
                isWalkable: cell => (
                    grid.CellInBounds(cell)
                    && grid.GetCellType(cell) != CellType5.Blocked
                )
            ) ?? throw new RegenerateException();

            foreach (var cell in path)
            {
                reachableCells.Add(cell);
            }
            paths.Add(path);
        }
    }

    string GetRoomName(int index){
        var rect = roomRects[index];
        return $"Room {index} {rect.x}x{rect.y}";
    }

    Room5 InstantiateRoom(
        int roomIndex,
        GameObject roomPrefab,
        int rotation,
        List<RoomDoor> doors = null
    ){
        var roomGO = KPrefabUtility.InstantiatePrefab(
            roomPrefab,
            parent: transform
        );
        var room = roomGO.GetComponent<Room5>();
        var roomRect = roomRects[roomIndex];
        room.gameObject.name = GetRoomName(roomIndex);
        var rot = Quaternion.Euler(0, -90 * rotation, 0) *
                  room.transform.localRotation;
        // maps rotation to origin
        //		0 rot -> (0,0)
        //		1 rot -> (1,0)
        //		2 rot -> (1,1)
        //		3 rot -> (0,1)
        var roomOrigin = ((
            new Vector3(
                1,
                0,
                1
            )
            - rot * new Vector3(
                1,
                0,
                1
            )
        ) / 2).Hammard(
            new Vector3(roomRect.size.x, 0, roomRect.size.y)
        );
        room.transform.localPosition = (
            new Vector3(
                roomRect.x,
                0,
                roomRect.y
            )
            + roomOrigin
        ) * grid.CellLength;
        room.transform.localRotation = rot;
        room.transform.SetAsLastSibling();

        room.LevelGenerator = this;
        room.Grid = grid;
        room.Position = new Vector2Int(
            roomRects[roomIndex].x,
            roomRects[roomIndex].y
        );
        room.Rotation = rotation;
        room.OpenDoors = doors ?? new List<RoomDoor>();

        placedRooms[roomIndex] = room;
		
        rooms.Add(room);

        return room;
    }

    void InstantiateFixedRooms(){
        for(int i = 0; i < fixedRooms.Count; ++i){
            var fixedRoom = fixedRooms[i];
            var roomIndex = fixedRoomRectIndices[i];
            var room = InstantiateRoom(
                roomIndex,
                fixedRoom.Prefab,
                fixedRoom.PrefabRotation
            );
            if(fixedRoom.IsSpawnPoint){
                spawnRoom = room;
            }
        }
    }

    List<(Room5, int, List<RoomDoor>)> FindRoomsWithDoors(
        int curRoomIndex,
        List<RoomDoor> doors
    )
    {
        var size = roomRects[curRoomIndex].size;

        // Debug.Log($"Looking for rooms of size {size} with doors:");
        foreach(var door in doors){
            // Debug.Log($"Door: {door.Position} {door.Direction}");
        }

        List<(Room5, int, List<RoomDoor>)> rooms = new();
        foreach (var roomPrefab in roomPrefabs)
        {
            var room = roomPrefab.GetComponent<Room5>();
            if (room == null)
            {
                Debug.LogError("Room prefab does not have a Room component");
                continue;
            }
            // 4 rotations
            for (int rotation = 0; rotation < 4; ++rotation)
            {
                if (room.Size.Rotate90(rotation).Abs() != size)
                {
                    continue;
                }

                // maps rotation to origin
                // to prevent negative positions
                //		0 rot -> (0,0)
                //		1 rot -> (1,0)
                //		2 rot -> (1,1)
                //		3 rot -> (0,1)
                var maxCoord = room.Size - Vector2Int.one;
                var originOffset = maxCoord.Rotate90(rotation);
                // min each component with 0
                originOffset = -Vector2Int.Min(originOffset, Vector2Int.zero);

                // imagine this room:
                // a b
                // c d
                // if it is rotated 90, then it becomes
                // b d
                // a c
                // thus, 	c (0,0) -> (1,0) = (0,0) + (1,0)
                // 			d (1,0) -> (1,1) = (0,1) + (1,0)
                //			b (1,1) -> (0,1) = (-1,1) + (1,0)
                //			a (0,1) -> (0,0) = (-1,0) + (1,0)
                // all of these are equal to the rotated position
                // plus the origin offset

                bool hasDoors = true;
                List<RoomDoor> pickedDoors = new();
                foreach (var door in doors)
                {
                    bool found = false;
                    var doorPosition = door.Position;
                    var doorMask = door.Direction;
                    // Debug.Log($"{curRoomIndex} Looking for door {doorPosition} {doorMask} ({size})");
                    foreach (var roomDoor in room.Doors)
                    {
                        var roomDoorPosition = roomDoor.Position.Rotate90(rotation) + originOffset;
                        var roomDoorDirection = roomDoor.Direction.Rotate90(rotation);
                        // Debug.Log($"{curRoomIndex} ({rotation}, {room.Size}, {room.gameObject.name}) {originOffset}+{roomDoor.Position}={roomDoorPosition} ({roomDoor.Direction}->{roomDoorDirection})");
                        if (
                            doorPosition == roomDoorPosition
                            && roomDoorDirection.HasFlag(doorMask)
                        )
                        {
                            // Debug.Log("Found");
                            pickedDoors.Add(new RoomDoor {
                                Position = roomDoor.Position,
                                Direction = door.Direction.Rotate90(-rotation)
                            });
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        // Debug.Log("Not found");
                        hasDoors = false;
                        break;
                    }
                }

                if (hasDoors)
                {
                    rooms.Add((room, rotation, pickedDoors));
                }
            }
        }
        return rooms;
    }

    void InstantiateRooms()
    {
        RoomDoor entranceDoor = null;
        RoomDoor exitDoor;
        int curRoomIndex = -1;

        // place room along each room in the path
        foreach (var path in paths)
        {
            for (int i = 0; i < path.Count - 1; ++i)
            {
                var start = path[i];
                var end = path[i + 1];
                int startRoomIndex = grid.GetCell(start).RoomIndex;
                int endRoomIndex = grid.GetCell(end).RoomIndex;
                if (curRoomIndex < 0)
                {
                    curRoomIndex = startRoomIndex;
                }
                if (startRoomIndex != endRoomIndex)
                {
                    // we are entering a new room
                    exitDoor = new()
                    {
                        Position = start,
                        Direction = (end - start).ToDirection()
                    };
                    // Debug.Log($"doors: {entranceDoor?.Position} {exitDoor.Position}");

                    // place room if not already placed
                    if (!placedRooms.TryGetValue(curRoomIndex, out var placedRoom))
                    {
                        Vector2Int origin = roomRects[curRoomIndex].position;
                        List<RoomDoor> doorsRelative = new();
                        if (entranceDoor != null)
                        {
                            doorsRelative.Add(new RoomDoor
                            {
                                Position = entranceDoor.Position - origin,
                                Direction = entranceDoor.Direction
                            });
                        }
                        if(exitDoor != null){
                            doorsRelative.Add(new RoomDoor
                            {
                                Position = exitDoor.Position - origin,
                                Direction = exitDoor.Direction
                            });
                        }
                        // find room with entrance and exit
                        var rooms = FindRoomsWithDoors(
                            curRoomIndex,
                            doorsRelative
                        );
                        if (rooms.Count == 0)
                        {
                            throw new RegenerateException();
                        }

                        // spawn the room
                        var (roomPrefab, rotation, doors) = rooms[Random.Range(0, rooms.Count)];
                        InstantiateRoom(
                            curRoomIndex,
                            roomPrefab.gameObject,
                            rotation,
                            doors
                        );
                    }
                    // otherwise add to the set of open doors in the room
                    else
                    {
                        // find coordinate relative to room
                        var pos = exitDoor.Position - placedRoom.Position;
                        placedRoom.OpenDoors.Add(new RoomDoor
                        {
                            Position = pos,
                            Direction = exitDoor.Direction
                        });
                    }

                    entranceDoor = exitDoor;
                    entranceDoor.Direction = entranceDoor.Direction.Rotate90(2);
                    entranceDoor.Position = end;
                    exitDoor = null;
                    curRoomIndex = endRoomIndex;
                }
            }
        }
    }

    void SpawnDoors()
    {
        foreach(var room in rooms){
            room.SpawnDoors();
        }
    }

    void OnDrawGizmos()
    {
        foreach (var roomRect in roomRects)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(
                transform.position + new Vector3(
                    roomRect.x + (float)roomRect.width / 2,
                    0,
                    roomRect.y + (float)roomRect.height / 2
                ) * grid.CellLength,
                new Vector3(roomRect.width, 0, roomRect.height) * grid.CellLength
            );
        }

        for (int x = 0; x < grid.Size.x; ++x)
        {
            for (int y = 0; y < grid.Size.y; ++y)
            {
                var cell = grid.GetCell(new Vector2Int(x, y));
                if (cell.DoorDirections != DirectionMask.None)
                {
                    Gizmos.color = Color.blue;
                    var center = transform.position + new Vector3(x + 0.5f, 0, y + 0.5f) * grid.CellLength;
                    (DirectionMask, Vector3)[] doorDirections = {
                        (DirectionMask.Up, Vector3.forward),
                        (DirectionMask.Down, Vector3.back),
                        (DirectionMask.Left, Vector3.left),
                        (DirectionMask.Right, Vector3.right),
                    };

                    foreach (var (direction, directionVector) in doorDirections)
                    {
                        if ((cell.DoorDirections & direction) != 0)
                        {
                            Gizmos.DrawRay(
                                center,
                                directionVector * grid.CellLength
                            );
                        }
                    }
                }
            }
        }
        foreach (var requiredPath in requiredPaths)
        {
            Gizmos.color = Color.red;
            var startCenter = grid.AlignmentOffsetToCell(
                requiredPath.StartAlignment,
                requiredPath.StartOffset
            ) + Vector2.one / 2;
            var endCenter = grid.AlignmentOffsetToCell(
                requiredPath.EndAlignment,
                requiredPath.EndOffset
            ) + Vector2.one / 2;
            Gizmos.DrawLine(
                transform.position + new Vector3(startCenter.x, 0, startCenter.y) * grid.CellLength,
                transform.position + new Vector3(endCenter.x, 0, endCenter.y) * grid.CellLength
            );
        }

        // draw paths
        foreach (var path in paths)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < path.Count - 1; ++i)
            {
                var start = path[i];
                var end = path[i + 1];
                Gizmos.DrawLine(
                    transform.position + new Vector3(start.x + 0.5f, 0, start.y + 0.5f) * grid.CellLength,
                    transform.position + new Vector3(end.x + 0.5f, 0, end.y + 0.5f) * grid.CellLength
                );
            }
        }
    }
}