package java.s8;

import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.List;

public class KnightsTourGemi {

    private static final int BOARD_SIZE = 12;
    private static final int[][] KNIGHT_MOVES = {
        {1, -2}, {2, -1}, {2, 1}, {1, 2}, {-1, 2}, {-2, 1}, {-2, -1}, {-1, -2}
    };
    private static int[][] board;
    private static int totalSquares;

    public static void main(String[] args) {
        initializeBoard();
        int startRow = 2 + (int) (Math.random() * (BOARD_SIZE - 4));
        int startCol = 2 + (int) (Math.random() * (BOARD_SIZE - 4));
        board[startRow][startCol] = 1;

        if (solveKnightTour(startRow, startCol, 2)) {
            printSolution();
        } else {
            System.out.println("No solution found.");
        }
    }

    private static void initializeBoard() {
        board = new int[BOARD_SIZE][BOARD_SIZE];
        totalSquares = (BOARD_SIZE - 4) * (BOARD_SIZE - 4);

        for (int row = 0; row < BOARD_SIZE; row++) {
            for (int col = 0; col < BOARD_SIZE; col++) {
                if (row < 2 || row > BOARD_SIZE - 3 || col < 2 || col > BOARD_SIZE - 3) {
                    board[row][col] = -1; // Mark as invalid
                }
            }
        }
    }

    private static boolean solveKnightTour(int row, int col, int moveCount) {
        if (moveCount > totalSquares) {
            return true;
        }

        List<int[]> possibleMoves = getPossibleMoves(row, col);
        if (possibleMoves.isEmpty() && moveCount != totalSquares) {
            return false;
        }

        Collections.sort(possibleMoves, Comparator.comparingInt(move -> move[2]));

        for (int[] nextMove : possibleMoves) {
            int nextRow = nextMove[0];
            int nextCol = nextMove[1];
            board[nextRow][nextCol] = moveCount;

            if (!isOrphan(moveCount, nextRow, nextCol) && solveKnightTour(nextRow, nextCol, moveCount + 1)) {
                return true;
            }

            board[nextRow][nextCol] = 0; // Backtrack
        }

        return false;
    }

    private static List<int[]> getPossibleMoves(int row, int col) {
        List<int[]> moves = new ArrayList<>();
        for (int[] move : KNIGHT_MOVES) {
            int nextRow = row + move[1];
            int nextCol = col + move[0];
            if (board[nextRow][nextCol] == 0) {
                int neighborCount = countNeighbors(nextRow, nextCol);
                moves.add(new int[]{nextRow, nextCol, neighborCount});
            }
        }
        return moves;
    }

    private static int countNeighbors(int row, int col) {
        int count = 0;
        for (int[] move : KNIGHT_MOVES) {
            if (board[row + move[1]][col + move[0]] == 0) {
                count++;
            }
        }
        return count;
    }

    private static boolean isOrphan(int moveCount, int row, int col) {
        if (moveCount < totalSquares - 1) {
            List<int[]> possibleMoves = getPossibleMoves(row, col);
            for (int[] nextMove : possibleMoves) {
                if (countNeighbors(nextMove[0], nextMove[1]) == 0) {
                    return true;
                }
            }
        }
        return false;
    }

    private static void printSolution() {
        for (int[] row : board) {
            for (int cell : row) {
                if (cell != -1) {
                    System.out.printf("%2d ", cell);
                }
            }
            System.out.println();
        }
    }
}