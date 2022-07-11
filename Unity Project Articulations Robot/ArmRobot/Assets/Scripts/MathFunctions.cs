using System;
using UnityEngine;

public enum RotateAxis
{
    OX,
    OY,
    OZ
}

public static class MathFunctions
{
    /// <param name="fullRealHandLength">длина реальной роборуки в вытянутом состоянии</param>
    /// <param name="fullUnityHandLength">длина роборуки из симуляции в Unity в вытянутом состоянии</param>
    public static Vector3 NormalizeCoordinates(float fullRealHandLength, float fullUnityHandLength, Vector3 oldCoord) =>
        new Vector3(oldCoord.x * fullUnityHandLength / fullRealHandLength,
            oldCoord.y * fullUnityHandLength / fullRealHandLength,
            oldCoord.z * fullUnityHandLength / fullRealHandLength);
    
    public static Vector3 GetPosition(RotateAxis rotateAxis0, float rotateAngle0, RotateAxis rotateAxis1, float rotateAngle1, RotateAxis rotateAxis2, float rotateAngle2,
        RotateAxis offsetAxis, float offsetDist, Vector3 endposition)
    {
        var newPosition = endposition;
        
        var radians = rotateAngle1 * Mathf.PI / 180;
        
        radians = rotateAngle2 * Mathf.PI / 180;
        newPosition = RotateAndOffset(offsetAxis, offsetDist, rotateAxis2, radians, endposition);
        
        radians = rotateAngle1 * Mathf.PI / 180;
        newPosition = RotateAroundAxis(newPosition, rotateAxis1, radians);
        
        radians = rotateAngle0 * Mathf.PI / 180;
        newPosition = RotateAroundAxis(newPosition, rotateAxis0, radians);

        return newPosition;
    }

    public static Vector3 RotateAroundAxis(Vector3 oldCoord, RotateAxis axis, float alpha)
    {
        // n, p - vertical, m, q - horizontal
        int n = 3, m = 3, p = 3, q = 1;

        float[,] rotateMatrix = GetRotateMatrix(axis, alpha);

        float[,] oldCoordinates = {{oldCoord.x}, {oldCoord.y}, {oldCoord.z}};
        var result = MatrixMultiplication(rotateMatrix, oldCoordinates, m, n, p, q);

        var resVector = new Vector3(result[0, 0], result[1, 0], result[2, 0]);
        return resVector;
    }

    public static Vector3 Offset(Vector3 oldCoord, RotateAxis offsetAxis, float offset)
    {
        // n, p - vertical, m, q - horizontal
        // n,m - first matrix, p,q - second
        int n = 4, m = 4, p = 4, q = 1;
        float[,] offsetMatrix = GetOffsetMatrix(offsetAxis, offset);
        
        float[,] oldCoordinates = {{oldCoord.x}, {oldCoord.y}, {oldCoord.z}, {1}};
        
        var result = MatrixMultiplication(offsetMatrix, oldCoordinates, m, n, p, q);
        var resVector = new Vector3(result[0, 0], result[1, 0], result[2, 0]);
        return resVector;
    }
    
    public static Vector3 RotateAndOffset(RotateAxis offsetAxis1, float offset1, RotateAxis rotateAxis, float alpha, Vector3 oldCoord)
    {
        // n, p - vertical, m, q - horizontal
        // n,m - first matrix, p,q - second
        int n = 4, m = 4, p = 4, q = 4;

        float[,] offsetMatrix = GetOffsetMatrix(offsetAxis1, -offset1);
        var transposedOffsetMatrix = Transpose(offsetMatrix);
        
        float[,] rotateMatrix = GetRotateMatrix(rotateAxis, alpha, 4);
        var transposedRotateMatrix = Transpose(rotateMatrix);
        
        float[,] oldCoordinates = {{oldCoord.x}, {oldCoord.y}, {oldCoord.z}, {1}};
        
        // offset to (0,0,0) coordinates system
        var newCoords = MatrixMultiplication(offsetMatrix, oldCoordinates, m, n, p, 1);
        
        // rotate
        var result = MatrixMultiplication(rotateMatrix, newCoords, m, n, p, 1);
        
        // offset back
        offsetMatrix = GetOffsetMatrix(offsetAxis1, offset1);
        result = MatrixMultiplication(offsetMatrix, result, m, n, p, 1);

        var resVector = new Vector3(result[0, 0], result[1, 0], result[2, 0]);
        return resVector;
    }
    
    public static float[,] MatrixMultiplication(float[,] a, float[,] b, int m, int n, int p, int q)
    {
        float[,] c = new float[m, q];

        if (n != p)
            return c;

        for (var i = 0; i < m; i++)
        {
            for (var j = 0; j < q; j++)
            {
                c[i, j] = 0;
                for (var k = 0; k < n; k++)
                    c[i, j] += a[i, k] * b[k, j];
            }
        }

        return c;
    }

    private static float[,] GetRotateMatrix(RotateAxis rotateAxis, float alpha, int countOfElements = 3)
    {
        float[,] rotateMatrix = { };
        switch (rotateAxis)
        {
            case RotateAxis.OX:
                if (countOfElements == 3)
                {
                    rotateMatrix = new[,]
                        {
                            {1, 0, 0}, 
                            {0, Mathf.Cos(alpha), -Mathf.Sin(alpha)}, 
                            {0, Mathf.Sin(alpha), Mathf.Cos(alpha)}
                        };
                }
                else if (countOfElements == 4)
                {
                    rotateMatrix = new[,]
                        {
                            {1, 0, 0, 0}, 
                            {0, Mathf.Cos(alpha), -Mathf.Sin(alpha), 0}, 
                            {0, Mathf.Sin(alpha), Mathf.Cos(alpha), 0},
                            {0, 0, 0, 1}
                        };
                }
                break;
            case RotateAxis.OY:
                if (countOfElements == 3)
                {
                    rotateMatrix = new[,]
                    {
                        {Mathf.Cos(alpha), 0, Mathf.Sin(alpha)}, 
                        {0, 1, 0}, 
                        {-Mathf.Sin(alpha), 0, Mathf.Cos(alpha)}
                    };
                }
                else if (countOfElements == 4)
                {
                    rotateMatrix = new[,]
                    {
                        {Mathf.Cos(alpha), 0, Mathf.Sin(alpha), 0}, 
                        {0, 1, 0, 0}, 
                        {-Mathf.Sin(alpha), 0, Mathf.Cos(alpha), 0},
                        {0, 0, 0, 1}
                    };
                }
                break;
            case RotateAxis.OZ:
                if (countOfElements == 3)
                {
                    rotateMatrix = new[,]
                    {
                        {Mathf.Cos(alpha), -Mathf.Sin(alpha), 0}, 
                        {Mathf.Sin(alpha), Mathf.Cos(alpha), 0}, 
                        {0, 0, 1}
                    };
                }
                else if (countOfElements == 4)
                {
                    rotateMatrix = new[,]
                    {
                        {Mathf.Cos(alpha), -Mathf.Sin(alpha), 0, 0}, 
                        {Mathf.Sin(alpha), Mathf.Cos(alpha), 0, 0}, 
                        {0, 0, 1, 0},
                        {0, 0, 0, 1}
                    };
                }
                break;
        }

        return rotateMatrix;
    }
    
    private static float[,] GetOffsetMatrix(RotateAxis offsetAxis, float offset)
    {
        float[,] offsetMatrix = { };
        
        switch (offsetAxis)
        {
            case RotateAxis.OX:
                offsetMatrix = new[,]
                {
                    {1, 0, 0, offset}, 
                    {0, 1, 0, 0}, 
                    {0, 0, 1, 0},
                    {0, 0, 0, 1}
                };
                break;
            case RotateAxis.OY:
                offsetMatrix = new[,]
                {
                    {1, 0, 0, 0}, 
                    {0, 1, 0, offset}, 
                    {0, 0, 1, 0},
                    {0, 0, 0, 1}
                };
                break;
            case RotateAxis.OZ:
                offsetMatrix = new[,]
                {
                    {1, 0, 0, 0}, 
                    {0, 1, 0, 0}, 
                    {0, 0, 1, offset},
                    {0, 0, 0, 1}
                };
                break;
        }

        return offsetMatrix;
    }
    
    public static float[,] Transpose(float[,] matrix)
    {
        int w = matrix.GetLength(0);
        int h = matrix.GetLength(1);

        float[,] result = new float[h, w];

        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                result[j, i] = matrix[i, j];
            }
        }

        return result;
    }
}