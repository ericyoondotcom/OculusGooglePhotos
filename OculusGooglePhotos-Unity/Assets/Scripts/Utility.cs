using System;
public class Utility
{
    public enum PhotoTypes
    {
        Unspecified = 0,
        RectangularMono,
        RectangularStereo,
        SphericalMono,
        SphericalStereo
    }

    public static string GenerateUUID()
    {
        Guid guid = Guid.NewGuid();
        return guid.ToString();
    }
}
