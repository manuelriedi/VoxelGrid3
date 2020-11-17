
public class VoxelData
{
    //L-Tetromino                   Row 1        Row 2
    int[,] data = new int[,] { { 1, 1, 1 }, { 1, 0, 0 } };  //Liegend zu Z- 
    //int[,] data = new int[,] { { 1, 1, 1 }, { 0, 0, 1 } };  //Liegend zu Z+
    //int[,] data = new int[,] { { 1, 0, 0 }, { 1, 0, 0 } };  //Liegend zu X+
    //int[,,] data = new int[,,] {{ { 1, 0, 0 }, { 1, 0, 0 }, { 1, 1, 1} }};

    public int Width
    {
        get { return data.GetLength(0); }
    }

    public int Depth
    {
        get { return data.GetLength(1); }
    }

    public int GetCell(int x, int z)
    {
        return data[x, z];
    }

    //public int GetNeighbor(int x, int z, Direction dir)
    //{
    //    //Offset of current coordinate
    //    DataCoordinate offsetToCheck = offsets[(int)dir];
    //    DataCoordinate neighborCoord = new DataCoordinate(x + offsetToCheck.x, 0 + offsetToCheck.y, z + offsetToCheck.z);

    //    //Wenn neighborrCoord ausserhalb der grenze liegt gibt 0 zurück, sonst den Nachbar
    //    if (neighborCoord.x < 0 || neighborCoord.x > Width || neighborCoord.y != 0 || neighborCoord.z < 0 || neighborCoord.z >= Depth)
    //    {
    //        return 0;
    //    }
    //    else
    //    {
    //        return GetCell(neighborCoord.x, neighborCoord.z);
    //        //return 1;
    //    }      
    //}

    //struct DataCoordinate
    //{
    //    public int x;
    //    public int y;
    //    public int z;

    //    public DataCoordinate(int x, int y, int z)
    //    {
    //        this.x = x;
    //        this.y = y;
    //        this.z = z;
    //    }
    //}

    //Liste neuer Datencoordinaten erstellen
    //DataCoordinate[] offsets =
    //{
    //    new DataCoordinate( 0,0,1),     //North
    //    new DataCoordinate( 1,0,0),     //East
    //    new DataCoordinate( 0,0,-1),    //South
    //    new DataCoordinate(-1,0,0),     //West
    //    new DataCoordinate( 0,1,0),
    //    new DataCoordinate( 0,-1,0)


    //};
}

public enum Direction 
{
    North,  //the 0 Index is alway north
    East,
    South,  //the 2 Index is alway north
    West,
    Up,
    Down
}
