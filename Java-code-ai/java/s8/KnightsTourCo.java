package java.s8;

import java.util.*;

/*
 * Problem Statement: -
 * Given a N*N board with the Knight placed on the first block of an empty board. Moving according to the rules of
 * chess knight must visit each square exactly once. Print the order of each cell in which they are visited.
 * 
 * Example: -
 * 
 * Input : N = 8
 * 
 * Output:
 * 0  59  38  33  30  17   8  63
 * 37  34  31  60   9  62  29  16
 * 58   1  36  39  32  27  18   7
 * 35  48  41  26  61  10  15  28
 * 42  57   2  49  40  23   6  19
 * 47  50  45  54  25  20  11  14
 * 56  43  52   3  22  13  24   5
 * 51  46  55  44  53   4  21  12
 */

public class KnightsTourCo {

    private static final int BASE = 12;
    private static final int[][] MOVES = {
        {1, -2}, {2, -1}, {2, 1}, {1, 2}, 
        {-1, 2}, {-2, 1}, {-2, -1}, {-1, -2}
    };
    private static int[][] grid;
    private static int totalSquares;

    public static void main(String[] args) {
        initializeGrid();
        int startRow = getRandomStartPosition();
        int startCol = getRandomStartPosition();
        grid[startRow][startCol] = 1;

        if (solve(startRow, startCol, 2)) {
            printResult();
        } else {
            System.out.println("No result");
        }
    }

    private static void initializeGrid() {
        grid = new int[BASE][BASE];
        totalSquares = (BASE - 4) * (BASE - 4);

        for (int r = 0; r < BASE; r++) {
            for (int c = 0; c < BASE; c++) {
                if (r < 2 || r > BASE - 3 || c < 2 || c > BASE - 3) {
                    grid[r][c] = -1;
                }
            }
        }
    }

    private static int getRandomStartPosition() {
        return 2 + (int) (Math.random() * (BASE - 4));
    }

    private static boolean solve(int row, int col, int count) {
        if (count > totalSquares) {
            return true;
        }

        List<int[]> neighbors = getNeighbors(row, col);
        if (neighbors.isEmpty() && count != totalSquares) {
            return false;
        }

        neighbors.sort(Comparator.comparingInt(a -> a[2]));

        for (int[] neighbor : neighbors) {
            int nextRow = neighbor[0];
            int nextCol = neighbor[1];
            grid[nextRow][nextCol] = count;

            if (!isOrphanDetected(count, nextRow, nextCol) && solve(nextRow, nextCol, count + 1)) {
                return true;
            }

            grid[nextRow][nextCol] = 0;
        }

        return false;
    }

    private static List<int[]> getNeighbors(int row, int col) {
        List<int[]> neighbors = new ArrayList<>();

        for (int[] move : MOVES) {
            int newRow = row + move[1];
            int newCol = col + move[0];
            if (grid[newRow][newCol] == 0) {
                int numNeighbors = countNeighbors(newRow, newCol);
                neighbors.add(new int[]{newRow, newCol, numNeighbors});
            }
        }

        return neighbors;
    }

    private static int countNeighbors(int row, int col) {
        int count = 0;
        for (int[] move : MOVES) {
            if (grid[row + move[1]][col + move[0]] == 0) {
                count++;
            }
        }
        return count;
    }

    private static boolean isOrphanDetected(int count, int row, int col) {
        if (count < totalSquares - 1) {
            List<int[]> neighbors = getNeighbors(row, col);
            for (int[] neighbor : neighbors) {
                if (countNeighbors(neighbor[0], neighbor[1]) == 0) {
                    return true;
                }
            }
        }
        return false;
    }

    private static void printResult() {
        for (int[] row : grid) {
            for (int cell : row) {
                if (cell != -1) {
                    System.out.printf("%2d ", cell);
                }
            }
            System.out.println();
        }
    }
}