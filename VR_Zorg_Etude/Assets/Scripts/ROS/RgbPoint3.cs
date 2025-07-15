// Took from https://github.com/siemens/ros-sharp/blob/master/Libraries/RosBridgeClient/PointCloud.cs

using System;
using System.Collections.Generic;
using RosMessageTypes.Sensor;

/// <summary>
/// Class that contain the position and the color of a point 
/// </summary>
public class RgbPoint3
{
    public readonly float x;
    public readonly float y;
    public readonly float z;
    public readonly int[] rgb;

    /// <summary>
    /// Constructor that set the value of the position and the color of the point
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="fields"></param>
    public RgbPoint3(byte[] bytes, PointFieldMsg[] fields)
    {
        foreach (var field in fields)
        {
            var slice = new byte[field.count * 4];
            Array.Copy(bytes, field.offset, slice, 0, field.count * 4);

            switch (field.name)
            {
                case "x":
                    x = GetValue(slice);
                    break;
                case "y":
                    y = GetValue(slice);
                    break;
                case "z":
                    z = GetValue(slice);
                    break;
                case "rgb":
                    rgb = GetRGB(slice);
                    break;
            }
        }
    }
    
    /// <summary>
    /// Get the information of the point in a string
    /// </summary>
    /// <returns>The values of the point in a string</returns>
    public override string ToString()
    {
        return "xyz=(" + x + ", " + y + ", " + z + ")"
               + " rgb=(" + rgb[0] + ", " + rgb[1] + ", " + rgb[2] + ")";
    }
    
    /// <summary>
    /// Decode a byte to return it's value
    /// </summary>
    /// <param name="bytes">Contain the information</param>
    /// <returns>The value of the bytes</returns>
    private static float GetValue(byte[] bytes)
    {
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        return BitConverter.ToSingle(bytes, 0);
    }

    /// <summary>
    /// Decode the byte to return it's rgb values
    /// </summary>
    /// <param name="bytes">Contain the information</param>
    /// <returns>The value of the bytes</returns>
    private static int[] GetRGB(IReadOnlyList<byte> bytes)
    {
        var rgb = new int[3];
        rgb[0] = Convert.ToInt16(bytes[0]);
        rgb[1] = Convert.ToInt16(bytes[1]);
        rgb[2] = Convert.ToInt16(bytes[2]);

        return rgb;
    }
}
