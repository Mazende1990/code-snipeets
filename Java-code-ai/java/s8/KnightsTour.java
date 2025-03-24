package java.s8;

import java.util.*;

public class KnightsTour {

    private static final int BOARD_SIZE = 8;
    private static final int PADDING = 2;
    private static final int EXTENDED_SIZE = BOARD_SIZE + 2 * PADDING;
    private static final int[][] KNIGHT_MOVES = {
        {1, -2}, {2, -1}, {2, 1}, {1, 2},
        {-1, 2}, {-2, 1}, {-2, -1}, {-1, -2}
    };

    private static int[][] board;
    private static int totalMoves;

    public static void main(String[] args) {
        initializeBoard();
        totalMoves = BOARD_SIZE * BOARD_SIZE;

        int startRow = PADDING + (int) (Math.random() * BOARD_SIZE);
        int startCol = PADDING + (int) (Math.random() * BOARD_SIZE);

        board[startRow][startCol] = 1;

        if (solveTour(startRow, startCol, 2)) {
            printBoard();
        } else {
            System.out.println("No solution found.");
        }
    }

    private static void initializeBoard() {
        board = new int[EXTENDED_SIZE][EXTENDED_SIZE];

        for (int r = 0; r < EXTENDED_SIZE; r++) {
            for (int c = 0; c < EXTENDED_SIZE; c++) {
                if (r < PADDING || r >= EXTENDED_SIZE - PADDING || c < PADDING || c >= EXTENDED_SIZE - PADDING) {
                    board[r][c] = -1; // Mark borders as invalid
                }
            }
        }
    }

    private static boolean solveTour(int row, int col, int moveCount) {
        if (moveCount > totalMoves) return true;

        List<int[]> neighbors = getValidMoves(row, col);
        if (neighbors.isEmpty()) return false;

        neighbors.sort(Comparator.comparingInt(a -> a[2]));

        for (int[] move : neighbors) {
            int nextRow = move[0], nextCol = move[1];
            board[nextRow][nextCol] = moveCount;

            if (!hasOrphan(moveCount, nextRow, nextCol) && solveTour(nextRow, nextCol, moveCount + 1)) {
                return true;
            }

            board[nextRow][nextCol] = 0; // Backtrack
        }
        return false;
    }

    private static List<int[]> getValidMoves(int row, int col) {
        List<int[]> moves = new ArrayList<>();

        for (int[] move : KNIGHT_MOVES) {
            int newRow = row + move[1];
            int newCol = col + move[0];
            if (board[newRow][newCol] == 0) {
                int onwardMoves = countOnwardMoves(newRow, newCol);
                moves.add(new int[]{newRow, newCol, onwardMoves});
            }
        }

        return moves;
    }

    private static int countOnwardMoves(int row, int col) {
        int count = 0;
        for (int[] move : KNIGHT_MOVES) {
            if (board[row + move[1]][col + move[0]] == 0) {
                count++;
            }
        }
        return count;
    }

    private static boolean hasOrphan(int moveCount, int row, int col) {
        if (moveCount >= totalMoves - 1) return false;

        for (int[] move : getValidMoves(row, col)) {
            if (countOnwardMoves(move[0], move[1]) == 0) {
                return true;
            }
        }
        return false;
    }

    private static void printBoard() {
        for (int r = PADDING; r < EXTENDED_SIZE - PADDING; r++) {
            for (int c = PADDING; c < EXTENDED_SIZE - PADDING; c++) {
                System.out.printf("%2d ", board[r][c]);
            }
            System.out.println();
        }
    }
}
