using System;

namespace GridLock.LevelLoader
{
    public static class RoadGridCsvParser
    {
        public static int[,] Parse(string csvText)
        {
            string[] lines = csvText.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0)
            {
                throw new ArgumentException("CSV text is empty");
            }

            int rowCount = lines.Length;
            string[] firstRow = lines[0].Split(',');
            int colCount = firstRow.Length;

            var grid = new int[rowCount, colCount];

            for (int row = 0; row < rowCount; row++)
            {
                string[] cells = lines[row].Split(',');
                if (cells.Length != colCount)
                {
                    throw new ArgumentException($"Row {row} has {cells.Length} columns, expected {colCount}");
                }

                for (int col = 0; col < colCount; col++)
                {
                    string trimmed = cells[col].Trim();
                    if (!int.TryParse(trimmed, out int value))
                    {
                        throw new ArgumentException($"Invalid value '{trimmed}' at row {row}, col {col}");
                    }

                    if (value < 0 || value > 4) // TODO refactor this to a defined expected range
                    {
                        throw new ArgumentException($"Value {value} out of range (0-4) at row {row}, col {col}");
                    }

                    grid[row, col] = value;
                }
            }

            return grid;
        }
    }
}
