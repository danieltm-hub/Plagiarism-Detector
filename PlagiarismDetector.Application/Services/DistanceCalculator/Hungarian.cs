namespace PlagiarismDetector.Application.Services.DistanceCalculator;

public readonly struct HungarianResult(int totalCost, int[] rowToCol, int[] colToRow)
{
    public int TotalCost { get; } = totalCost;
    public int[] RowToCol { get; } = rowToCol;
    public int[] ColToRow { get; } = colToRow;
}

public static class Hungarian
{
    public static HungarianResult Solve(int[,] cost)
    {
        int n = cost.GetLength(0);

        if (n != cost.GetLength(1))
            throw new ArgumentException("Cost matrix must be square.");

        int[] u = new int[n + 1];
        int[] v = new int[n + 1];
        int[] p = new int[n + 1];
        int[] way = new int[n + 1];

        for (int i = 1; i <= n; i++)
        {
            p[0] = i;

            int j0 = 0;
            int[] minv = new int[n + 1];
            bool[] used = new bool[n + 1];

            for (int j = 1; j <= n; j++)
            {
                minv[j] = int.MaxValue;
                used[j] = false;
            }

            do
            {
                used[j0] = true;

                int delta = int.MaxValue;
                int i0 = p[j0];
                int j1 = 0;

                for (int j = 1; j <= n; j++)
                {
                    if (!used[j])
                    {
                        int cur = cost[i0 - 1, j - 1] - u[i0] - v[j];
                        if (minv[j] > cur)
                        {
                            minv[j] = cur;
                            way[j] = j0;
                        }
                        if (minv[j] < delta)
                        {
                            delta = minv[j];
                            j1 = j;
                        }
                    }
                }

                for (int j = 0; j <= n; j++)
                {
                    if (used[j])
                    {
                        u[p[j]] += delta;
                        v[j] -= delta;
                    }
                    else
                    {
                        minv[j] -= delta;
                    }
                }

                j0 = j1;
            } while (p[j0] != 0);

            do
            {
                int j1 = way[j0];
                p[j0] = p[j1];
                j0 = j1;
            } while (j0 != 0);
        }

        int[] rowToCol = new int[n];
        int[] colToRow = new int[n];

        for (int j = 1; j <= n; j++)
        {
            int i = p[j] - 1;
            rowToCol[i] = j - 1;
            colToRow[j - 1] = i;
        }

        int totalCost = 0;

        for (int i = 0; i < n; i++)
        {
            totalCost += cost[i, rowToCol[i]];
        }

        // Console.WriteLine("Algoritmo Hungaro");
        // for (int j = 1; j <= n; j++)
        // {
        //     Console.WriteLine($"{p[j] - 1}, {j - 1}");
        // }
        // Console.WriteLine($"------ {totalCost} ------");

        return new HungarianResult(totalCost, rowToCol, colToRow);
    }
}