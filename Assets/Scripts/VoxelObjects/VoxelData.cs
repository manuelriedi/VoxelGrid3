
public class VoxelData
{
    //L-Tetromino

    //Foot to -x                           0,0          0,1          0,2             1,0          1,1          1,2             2,0          2,1          2,2
  //int[,,] data = new int[3, 3, 3] { {{ 0, 0, 0 }, { 0, 1, 1 }, { 0, 0, 0 }},  {{ 0, 0, 0 }, { 0, 1, 0 }, { 0, 0, 0 }},  {{ 0, 0, 0 }, { 0, 1, 0 }, { 0, 0, 0 }} };    // +z
  //int[,,] data = new int[3, 3, 3] { {{ 0, 0, 0 }, { 1, 1, 0 }, { 0, 0, 0 }},  {{ 0, 0, 0 }, { 0, 1, 0 }, { 0, 0, 0 }},  {{ 0, 0, 0 }, { 0, 1, 0 }, { 0, 0, 0 }} };    // -z
    int[,,] data = new int[3, 3, 3] { {{ 0, 1, 0 }, { 0, 1, 0 }, { 0, 0, 0 }},  {{ 0, 0, 0 }, { 0, 1, 0 }, { 0, 0, 0 }},  {{ 0, 0, 0 }, { 0, 1, 0 }, { 0, 0, 0 }} };    // -y
  //int[,,] data = new int[3, 3, 3] { {{ 0, 0, 0 }, { 0, 1, 0 }, { 0, 1, 0 }},  {{ 0, 0, 0 }, { 0, 1, 0 }, { 0, 0, 0 }},  {{ 0, 0, 0 }, { 0, 1, 0 }, { 0, 0, 0 }} };    // +y


    public int Width
    {
        get { return data.GetLength(0); }
    }

    public int Depth
    {
        get { return data.GetLength(1); }
    }

    public int Height
    {
        get { return data.GetLength(2); }
    }

    public int GetCell(int x, int y, int z)
    {
        return data[x, y, z];
    }
}

public enum Direction 
{
    North,
    East,
    South, 
    West,
    Up,
    Down
}
