using System;

public class SimplexNoise
{
    private static readonly int[] Source =
    {
        151, 160, 137, 91, 90, 15, 131, 13,
        201, 95, 96, 53, 194, 233, 7, 225,
        140, 36, 103, 30, 69, 142, 8, 99,
        37, 240, 21, 10, 23, 190, 6, 148,
        247, 120, 234, 75, 0, 26, 197, 62,
        94, 252, 219, 203, 117, 35, 11, 32,
        57, 177, 33, 88, 237, 149, 56, 87,
        174, 20, 125, 136, 171, 168, 68, 175,
        74, 165, 71, 134, 139, 48, 27, 166,
        77, 146, 158, 231, 83, 111, 229, 122,
        60, 211, 133, 230, 220, 105, 92, 41,
        55, 46, 245, 40, 244, 102, 143, 54,
        65, 25, 63, 161, 1, 216, 80, 73,
        209, 76, 132, 187, 208, 89, 18, 169,
        200, 196, 135, 130, 116, 188, 159, 86,
        164, 100, 109, 198, 173, 186, 3, 64,
        52, 217, 226, 250, 124, 123, 5, 202,
        38, 147, 118, 126, 255, 82, 85, 212,
        207, 206, 59, 227, 47, 16, 58, 17,
        182, 189, 28, 42, 223, 183, 170, 213,
        119, 248, 152, 2, 44, 154, 163, 70,
        221, 153, 101, 155, 167, 43, 172, 9,
        129, 22, 39, 253, 19, 98, 108, 110,
        79, 113, 224, 232, 178, 185, 112, 104,
        218, 246, 97, 228, 251, 34, 242, 193,
        238, 210, 144, 12, 191, 179, 162, 241,
        81, 51, 145, 235, 249, 14, 239, 107,
        49, 192, 214, 31, 181, 199, 106, 157,
        184, 84, 204, 176, 115, 121, 50, 45,
        127, 4, 150, 254, 138, 236, 205, 93,
        222, 114, 67, 29, 24, 72, 243, 141,
        128, 195, 78, 66, 215, 61, 156, 180,
    };

    private int[] random;

    private const int RandomSize = 256;
    private const double Sqrt3 = 1.7320508075688772935;
    private const double Sqrt5 = 2.2360679774997896964;

    //> Skewing and un-skewing factors for 2D, 3D and 4D, some of them pre-multiplied.
    private const double F2 = 0.5 * (Sqrt3 - 1.0);
    private const double G2 = (3.0 - Sqrt3) / 6.0;
    private const double G22 = G2 * 2.0 - 1;
    private const double F3 = 1.0 / 3.0;
    private const double G3 = 1.0 / 6.0;
    private const double F4 = (Sqrt5 - 1.0) / 4.0;
    private const double G4 = (5.0 - Sqrt5) / 20.0;
    private const double G42 = G4 * 2.0;
    private const double G43 = G4 * 3.0;
    private const double G44 = G4 * 4.0 - 1.0;

    //> Gradient vectors for 3D (pointing to mid points of all edges of a unit cube
    private static readonly int[][] Grad3 =
    {
        new[] { 1, 1, 0 }, new[] { -1, 1, 0 },
        new[] { 1, -1, 0 }, new[] { -1, -1, 0 },
        new[] { 1, 0, 1 }, new[] { -1, 0, 1 },
        new[] { 1, 0, -1 }, new[] { -1, 0, -1 },
        new[] { 0, 1, 1 }, new[] { 0, -1, 1 },
        new[] { 0, 1, -1 }, new[] { 0, -1, -1 },
    };

    public SimplexNoise()
    {
        Randomize(0);
    }

    public SimplexNoise(int seed)
    {
        Randomize(seed);
    }

    /// <summary>
    /// Generates terrainValue, typically in range [-1, 1]
    /// </summary>
    public float Generate(UnityEngine.Vector3 point)
    {
        double x = point.x;
        double y = point.y;
        double z = point.z;
        double n0 = 0, n1 = 0, n2 = 0, n3 = 0;

        // Noise contributions from the four corners
        // Skew the input space to determine which simplex cell we're in
        double s = (x + y + z) * F3;

        // for 3D
        int i = FastFloor(x + s);
        int j = FastFloor(y + s);
        int k = FastFloor(z + s);

        double t = (i + j + k) * G3;

        // The x,y,z distances from the cell origin
        double x0 = x - (i - t);
        double y0 = y - (j - t);
        double z0 = z - (k - t);

        // For the 3D case, the simplex shape is a slightly irregular tetrahedron.
        // Determine which simplex we are in.
        // Offsets for second corner of simplex in (i,j,k)
        int i1, j1, k1;

        // coords
        int i2, j2, k2; // Offsets for third corner of simplex in (i,j,k) coords

        if (x0 >= y0)
        {
            if (y0 >= z0)
            {
                // X Y Z order
                i1 = 1;
                j1 = 0;
                k1 = 0;
                i2 = 1;
                j2 = 1;
                k2 = 0;
            }
            else if (x0 >= z0)
            {
                // X Z Y order
                i1 = 1;
                j1 = 0;
                k1 = 0;
                i2 = 1;
                j2 = 0;
                k2 = 1;
            }
            else
            {
                // Z X Y order
                i1 = 0;
                j1 = 0;
                k1 = 1;
                i2 = 1;
                j2 = 0;
                k2 = 1;
            }
        }
        else
        {
            // x0 < y0
            if (y0 < z0)
            {
                // Z Y X order
                i1 = 0;
                j1 = 0;
                k1 = 1;
                i2 = 0;
                j2 = 1;
                k2 = 1;
            }
            else if (x0 < z0)
            {
                // Y Z X order
                i1 = 0;
                j1 = 1;
                k1 = 0;
                i2 = 0;
                j2 = 1;
                k2 = 1;
            }
            else
            {
                // Y X Z order
                i1 = 0;
                j1 = 1;
                k1 = 0;
                i2 = 1;
                j2 = 1;
                k2 = 0;
            }
        }

        // A step of (1,0,0) in (i,j,k) means a step of (1-c,-c,-c) in (x,y,z),
        // a step of (0,1,0) in (i,j,k) means a step of (-c,1-c,-c) in (x,y,z),
        // and
        // a step of (0,0,1) in (i,j,k) means a step of (-c,-c,1-c) in (x,y,z),
        // where c = 1/6.

        // Offsets for second corner in (x,y,z) coords
        double x1 = x0 - i1 + G3;
        double y1 = y0 - j1 + G3;
        double z1 = z0 - k1 + G3;

        // Offsets for third corner in (x,y,z)
        double x2 = x0 - i2 + F3;
        double y2 = y0 - j2 + F3;
        double z2 = z0 - k2 + F3;

        // Offsets for last corner in (x,y,z)
        double x3 = x0 - 0.5;
        double y3 = y0 - 0.5;
        double z3 = z0 - 0.5;

        // Work out the hashed gradient indices of the four simplex corners
        int ii = i & 0xff;
        int jj = j & 0xff;
        int kk = k & 0xff;

        // Calculate the contribution from the four corners
        double t0 = 0.6 - x0 * x0 - y0 * y0 - z0 * z0;
        if (t0 > 0)
        {
            t0 *= t0;
            int gi0 = random[ii + random[jj + random[kk]]] % 12;
            n0 = t0 * t0 * Dot(Grad3[gi0], x0, y0, z0);
        }

        double t1 = 0.6 - x1 * x1 - y1 * y1 - z1 * z1;
        if (t1 > 0)
        {
            t1 *= t1;
            int gi1 = random[ii + i1 + random[jj + j1 + random[kk + k1]]] % 12;
            n1 = t1 * t1 * Dot(Grad3[gi1], x1, y1, z1);
        }

        double t2 = 0.6 - x2 * x2 - y2 * y2 - z2 * z2;
        if (t2 > 0)
        {
            t2 *= t2;
            int gi2 = random[ii + i2 + random[jj + j2 + random[kk + k2]]] % 12;
            n2 = t2 * t2 * Dot(Grad3[gi2], x2, y2, z2);
        }

        double t3 = 0.6 - x3 * x3 - y3 * y3 - z3 * z3;
        if (t3 > 0)
        {
            t3 *= t3;
            int gi3 = random[ii + 1 + random[jj + 1 + random[kk + 1]]] % 12;
            n3 = t3 * t3 * Dot(Grad3[gi3], x3, y3, z3);
        }

        // Add contributions from each corner to get the final noise terrainValue.
        // The result is scaled to stay just inside [-1,1]
        return (float)(n0 + n1 + n2 + n3) * 32;
    }

    private void Randomize(int seed)
    {
        random = new int[RandomSize * 2];

        if (seed != 0)
        {
            // Shuffle the array using the given seed
            // Unpack the seed into 4 bytes then perform a bitwise XOR operation
            // with each byte
            var F = new byte[4];
            UnpackLittleUint32(seed, ref F);

            for (int i = 0; i < Source.Length; i++)
            {
                random[i] = Source[i] ^ F[0];
                random[i] ^= F[1];
                random[i] ^= F[2];
                random[i] ^= F[3];

                random[i + RandomSize] = random[i];
            }
        }
        else
        {
            for (int i = 0; i < RandomSize; i++) random[i + RandomSize] = random[i] = Source[i];
        }
    }

    private static double Dot(int[] g, double x, double y, double z, double t) => g[0] * x + g[1] * y + g[2] * z + g[3] * t;
    private static double Dot(int[] g, double x, double y, double z) => g[0] * x + g[1] * y + g[2] * z;
    private static double Dot(int[] g, double x, double y) => g[0] * x + g[1] * y;

    private static int FastFloor(double x) => x >= 0 ? (int)x : (int)x - 1;

    private static byte[] UnpackLittleUint32(int value, ref byte[] buffer)
    {
        if (buffer.Length < 4) Array.Resize(ref buffer, 4);

        buffer[0] = (byte)(value & 0x00ff);
        buffer[1] = (byte)((value & 0xff00) >> 8);
        buffer[2] = (byte)((value & 0x00ff0000) >> 16);
        buffer[3] = (byte)((value & 0xff000000) >> 24);

        return buffer;
    }
}