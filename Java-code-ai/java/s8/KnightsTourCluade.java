package java.s8;

import java.util.*;

/**
 * Knight's Tour Problem
 * ---------------------
 * Given an N×N chess board with a knight placed on the first block of an empty board,
 * the knight must visit each square exactly once, moving according to chess rules.
 * This algorithm finds and prints the order in which squares are visited.
 *
 * Example Output (8×8 board):
 *    0  59  38  33  30  17   8  63
 *   37  34  31  60   9  62  29  16
 *   58   1  36  39  32  27  18   7
 *   35  48  41  26  61  10  15  28
 *   42  57   2  49  40  23   6  19
 *   47  50  45  54  25  20  11  14
 *   56  43  52   3  22  13  24   5
 *   51  46  55  44  53   4  21  12
 */
public class KnightsTourCluade {

    // Constants
    private static final int BOARD_SIZE = 12;
    private static final int BORDER_WIDTH = 2;
    private static final int PLAYABLE_SIZE = BOARD_SIZE - 2 * BORDER_WIDTH;
    
    // Knight's possible moves in chess (as coordinate changes)
    private static final int[][] KNIGHT_MOVES = {
        {1, -2}, {2, -1}, {2, 1}, {1, 2}, 
        {-1, 2}, {-2, 1}, {-2, -1}, {-1, -2}
    };
    
    // The chess board: -1 for border, 0 for unvisited, positive numbers for move sequence
    private static int[][] chessBoard;
    
    // Total number of squares that need to be visited
    private static int totalSquares;

    public static void main(String[] args) {
        initializeBoard();
        placeKnightRandomly();
        
        if (findTour()) {
            printSolution();
        } else {
            System.out.println("No solution found");
        }
    }

    /**
     * Initializes the chess board with borders
     */
    private static void initializeBoard() {
        chessBoard = new int[BOARD_SIZE][BOARD_SIZE];
        totalSquares = PLAYABLE_SIZE * PLAYABLE_SIZE;
        
        // Mark border cells as -1 (invalid)
        for (int row = 0; row < BOARD_SIZE; row++) {
            for (int col = 0; col < BOARD_SIZE; col++) {
                if (isOutsidePlayableArea(row, col)) {
                    chessBoard[row][col] = -1;
                }
            }
        }
    }
    
    /**
     * Checks if a position is outside the playable area
     */
    private static boolean isOutsidePlayableArea(int row, int col) {
        return row < BORDER_WIDTH || row >= BOARD_SIZE - BORDER_WIDTH || 
               col < BORDER_WIDTH || col >= BOARD_SIZE - BORDER_WIDTH;
    }

    /**
     * Places the knight randomly on the board
     */
    private static void placeKnightRandomly() {
        int startRow = BORDER_WIDTH + (int) (Math.random() * PLAYABLE_SIZE);
        int startCol = BORDER_WIDTH + (int) (Math.random() * PLAYABLE_SIZE);
        
        // Mark first position with 1
        chessBoard[startRow][startCol] = 1;
    }

    /**
     * Main algorithm to find a knight's tour using Warnsdorff's heuristic
     * @return true if a solution is found, false otherwise
     */
    private static boolean findTour() {
        // Find the starting position (marked with 1)
        int startRow = -1, startCol = -1;
        for (int r = 0; r < BOARD_SIZE; r++) {
            for (int c = 0; c < BOARD_SIZE; c++) {
                if (chessBoard[r][c] == 1) {
                    startRow = r;
                    startCol = c;
                    break;
                }
            }
        }
        
        // Start the tour from move 2 (since position 1 is already placed)
        return solve(startRow, startCol, 2);
    }

    /**
     * Recursive backtracking algorithm to solve the Knight's Tour
     * 
     * @param row Current row position
     * @param col Current column position
     * @param moveNumber Current move number
     * @return true if tour is completed, false if no solution from this position
     */
    private static boolean solve(int row, int col, int moveNumber) {
        // Base case: all squares have been visited
        if (moveNumber > totalSquares) {
            return true;
        }

        // Get all possible next positions
        List<MoveOption> nextMoves = getNextMoveOptions(row, col);

        // If no valid moves and we haven't completed the tour, this path fails
        if (nextMoves.isEmpty() && moveNumber != totalSquares) {
            return false;
        }

        // Sort moves by Warnsdorff's heuristic: fewer onward moves first
        Collections.sort(nextMoves);

        // Try each possible move
        for (MoveOption move : nextMoves) {
            int nextRow = move.row;
            int nextCol = move.col;
            
            // Place the knight
            chessBoard[nextRow][nextCol] = moveNumber;
            
            // Skip this move if it creates an "orphan" square
            if (!hasOrphanedSquare(moveNumber, nextRow, nextCol) && 
                solve(nextRow, nextCol, moveNumber + 1)) {
                return true;
            }
            
            // Backtrack if this move doesn't lead to a solution
            chessBoard[nextRow][nextCol] = 0;
        }

        return false;
    }

    /**
     * Represents a possible knight move with its accessibility score
     * (number of onward moves from that position)
     */
    private static class MoveOption implements Comparable<MoveOption> {
        int row;
        int col;
        int accessibilityScore;
        
        MoveOption(int row, int col, int accessibilityScore) {
            this.row = row;
            this.col = col;
            this.accessibilityScore = accessibilityScore;
        }
        
        @Override
        public int compareTo(MoveOption other) {
            // Sort by accessibility (fewer exit moves first, as per Warnsdorff's heuristic)
            return this.accessibilityScore - other.accessibilityScore;
        }
    }

    /**
     * Gets all valid next moves from the current position with their accessibility scores
     */
    private static List<MoveOption> getNextMoveOptions(int row, int col) {
        List<MoveOption> moveOptions = new ArrayList<>();

        for (int[] move : KNIGHT_MOVES) {
            int newRow = row + move[1];  // y-coordinate
            int newCol = col + move[0];  // x-coordinate
            
            if (isValidUnvisitedSquare(newRow, newCol)) {
                int accessibilityScore = countAccessibleSquares(newRow, newCol);
                moveOptions.add(new MoveOption(newRow, newCol, accessibilityScore));
            }
        }
        
        return moveOptions;
    }
    
    /**
     * Checks if a square is valid and unvisited
     */
    private static boolean isValidUnvisitedSquare(int row, int col) {
        return row >= 0 && row < BOARD_SIZE && 
               col >= 0 && col < BOARD_SIZE && 
               chessBoard[row][col] == 0;
    }

    /**
     * Counts how many unvisited squares are accessible from a given position
     */
    private static int countAccessibleSquares(int row, int col) {
        int count = 0;
        for (int[] move : KNIGHT_MOVES) {
            int newRow = row + move[1];
            int newCol = col + move[0];
            
            if (isValidUnvisitedSquare(newRow, newCol)) {
                count++;
            }
        }
        return count;
    }

    /**
     * Checks if moving to a particular square would create an orphaned square
     * (a square with no valid moves from it)
     */
    private static boolean hasOrphanedSquare(int moveNumber, int row, int col) {
        // No need to check for orphans on the last two moves
        if (moveNumber >= totalSquares - 1) {
            return false;
        }
        
        // Check if any of the next possible positions would become orphaned
        List<MoveOption> nextMoves = getNextMoveOptions(row, col);
        
        for (MoveOption move : nextMoves) {
            // If a next move has no further moves, it would become orphaned
            if (countAccessibleSquares(move.row, move.col) == 0) {
                return true;
            }
        }
        
        return false;
    }

    /**
     * Prints the solution board
     */
    private static void printSolution() {
        for (int row = 0; row < BOARD_SIZE; row++) {
            for (int col = 0; col < BOARD_SIZE; col++) {
                // Skip border cells
                if (chessBoard[row][col] == -1) {
                    continue;
                }
                System.out.printf("%2d ", chessBoard[row][col]);
            }
            System.out.println();
        }
    }
}