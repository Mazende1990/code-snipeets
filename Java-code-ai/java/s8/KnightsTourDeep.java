package java.s8;
import java.util.*;

/**
 * Solves the Knight's Tour problem using backtracking.
 * The Knight's Tour is a sequence of moves of a knight on a chessboard
 * such that the knight visits every square exactly once.
 */
public class KnightsTourDeep {

    // Constants for board configuration
    private static final int BOARD_PADDING = 2;
    private static final int BASE_SIZE = 12; // 8x8 board with 2-cell padding on each side
    private static final int[][] KNIGHT_MOVES = {
        {1, -2}, {2, -1}, {2, 1}, {1, 2}, 
        {-1, 2}, {-2, 1}, {-2, -1}, {-1, -2}
    };
    
    private static int[][] chessboard;
    private static int totalSquares;

    public static void main(String[] args) {
        initializeChessboard();
        
        // Start from a random position on the actual board (ignoring padding)
        int startRow = BOARD_PADDING + (int) (Math.random() * (BASE_SIZE - 2 * BOARD_PADDING));
        int startCol = BOARD_PADDING + (int) (Math.random() * (BASE_SIZE - 2 * BOARD_PADDING));
        
        chessboard[startRow][startCol] = 1; // Mark first position

        if (solveTour(startRow, startCol, 2)) {
            printChessboard();
        } else {
            System.out.println("No solution exists");
        }
    }

    /**
     * Initializes the chessboard with padding marked as inaccessible (-1)
     */
    private static void initializeChessboard() {
        chessboard = new int[BASE_SIZE][BASE_SIZE];
        totalSquares = (BASE_SIZE - 2 * BOARD_PADDING) * (BASE_SIZE - 2 * BOARD_PADDING);

        // Set padding areas as inaccessible
        for (int row = 0; row < BASE_SIZE; row++) {
            for (int col = 0; col < BASE_SIZE; col++) {
                if (row < BOARD_PADDING || row >= BASE_SIZE - BOARD_PADDING || 
                    col < BOARD_PADDING || col >= BASE_SIZE - BOARD_PADDING) {
                    chessboard[row][col] = -1;
                }
            }
        }
    }

    /**
     * Recursive backtracking solver for the Knight's Tour
     */
    private static boolean solveTour(int currentRow, int currentCol, int moveCount) {
        if (moveCount > totalSquares) {
            return true;
        }

        List<int[]> validMoves = getValidMoves(currentRow, currentCol);
        
        // Check if we're stuck before completing the tour
        if (validMoves.isEmpty() && moveCount != totalSquares) {
            return false;
        }

        // Sort moves based on Warnsdorff's heuristic (accessibility count)
        validMoves.sort(Comparator.comparingInt(move -> move[2]));

        for (int[] move : validMoves) {
            int nextRow = move[0];
            int nextCol = move[1];
            
            chessboard[nextRow][nextCol] = moveCount;
            
            if (!hasOrphanedSquares(moveCount, nextRow, nextCol) && 
                solveTour(nextRow, nextCol, moveCount + 1)) {
                return true;
            }
            
            // Backtrack
            chessboard[nextRow][nextCol] = 0;
        }

        return false;
    }

    /**
     * Returns a list of valid moves from the current position, 
     * each annotated with its accessibility count
     */
    private static List<int[]> getValidMoves(int row, int col) {
        List<int[]> moves = new ArrayList<>();

        for (int[] move : KNIGHT_MOVES) {
            int newRow = row + move[1];
            int newCol = col + move[0];
            
            if (chessboard[newRow][newCol] == 0) {
                int accessibilityCount = countAccessibleMoves(newRow, newCol);
                moves.add(new int[]{newRow, newCol, accessibilityCount});
            }
        }
        return moves;
    }

    /**
     * Counts how many moves are possible from a given position
     */
    private static int countAccessibleMoves(int row, int col) {
        int count = 0;
        for (int[] move : KNIGHT_MOVES) {
            if (chessboard[row + move[1]][col + move[0]] == 0) {
                count++;
            }
        }
        return count;
    }

    /**
     * Checks if making this move would orphan any squares 
     * (make them inaccessible for the remaining tour)
     */
    private static boolean hasOrphanedSquares(int currentMove, int row, int col) {
        if (currentMove < totalSquares - 1) {
            for (int[] move : getValidMoves(row, col)) {
                if (countAccessibleMoves(move[0], move[1]) == 0) {
                    return true;
                }
            }
        }
        return false;
    }

    /**
     * Prints the chessboard, excluding the padding areas
     */
    private static void printChessboard() {
        for (int row = BOARD_PADDING; row < BASE_SIZE - BOARD_PADDING; row++) {
            for (int col = BOARD_PADDING; col < BASE_SIZE - BOARD_PADDING; col++) {
                System.out.printf("%2d ", chessboard[row][col]);
            }
            System.out.println();
        }
    }
}