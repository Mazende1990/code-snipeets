package java.s7;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Random;

/**
 * Implementation of the Bucket Sort algorithm.
 * 
 * Bucket Sort works by distributing elements into a number of buckets,
 * sorting each bucket individually, and then concatenating the results.
 * 
 * Time Complexity: O(n + k) where n is the number of elements and k is the number of buckets
 * Space Complexity: O(n + k)
 * 
 * @see <a href="https://en.wikipedia.org/wiki/Bucket_sort">Wikipedia: Bucket Sort</a>
 */
public class BucketSortClaude {

    public static void main(String[] args) {
        // Create an array to hold the test data
        int[] numbers = generateRandomArray(10, -50, 49);
        
        // Sort the array
        bucketSort(numbers);
        
        // Verify the array is sorted correctly
        validateSortedArray(numbers);
    }

    /**
     * Generates an array of random integers within a specified range.
     * 
     * @param size The size of the array to generate
     * @param minValue The minimum value (inclusive)
     * @param maxValue The maximum value (inclusive)
     * @return An array of random integers
     */
    private static int[] generateRandomArray(int size, int minValue, int maxValue) {
        int[] array = new int[size];
        Random random = new Random();
        int range = maxValue - minValue + 1;
        
        for (int i = 0; i < size; i++) {
            array[i] = random.nextInt(range) + minValue;
        }
        
        return array;
    }
    
    /**
     * Validates that an array is sorted in ascending order.
     * 
     * @param array The array to validate
     * @throws AssertionError if the array is not sorted
     */
    private static void validateSortedArray(int[] array) {
        for (int i = 0; i < array.length - 1; i++) {
            assert array[i] <= array[i + 1] : "Array is not sorted at index " + i;
        }
        System.out.println("Array is successfully sorted");
    }

    /**
     * Sorts an array using the Bucket Sort algorithm.
     * 
     * @param array The array to be sorted
     */
    public static void bucketSort(int[] array) {
        if (array == null || array.length <= 1) {
            return; // Array is already sorted or empty
        }
        
        // Find the range of values
        int minValue = findMinValue(array);
        int maxValue = findMaxValue(array);
        int range = maxValue - minValue + 1;
        
        // Create buckets
        List<List<Integer>> buckets = createBuckets(range);
        
        // Distribute elements into buckets
        distributeToBuckets(array, buckets, minValue, range);
        
        // Sort each bucket and gather results
        mergeBuckets(array, buckets);
    }

    /**
     * Creates a list of empty buckets.
     * 
     * @param numberOfBuckets The number of buckets to create
     * @return A list containing empty buckets
     */
    private static List<List<Integer>> createBuckets(int numberOfBuckets) {
        List<List<Integer>> buckets = new ArrayList<>(numberOfBuckets);
        
        for (int i = 0; i < numberOfBuckets; i++) {
            buckets.add(new ArrayList<>());
        }
        
        return buckets;
    }
    
    /**
     * Distributes array elements into appropriate buckets.
     * 
     * @param array The array containing elements to distribute
     * @param buckets The list of buckets
     * @param minValue The minimum value in the array
     * @param range The range of values in the array
     */
    private static void distributeToBuckets(int[] array, List<List<Integer>> buckets, int minValue, int range) {
        for (int value : array) {
            int bucketIndex = calculateBucketIndex(value, minValue, range);
            buckets.get(bucketIndex).add(value);
        }
    }
    
    /**
     * Calculates which bucket an element should go into.
     * 
     * @param value The value to place in a bucket
     * @param minValue The minimum value in the array
     * @param range The range of values in the array
     * @return The index of the bucket
     */
    private static int calculateBucketIndex(int value, int minValue, int range) {
        return (value - minValue) / range;
    }
    
    /**
     * Sorts each bucket and merges the results back into the original array.
     * 
     * @param array The original array to populate with sorted values
     * @param buckets The list of buckets containing the values
     */
    private static void mergeBuckets(int[] array, List<List<Integer>> buckets) {
        int arrayIndex = 0;
        
        for (List<Integer> bucket : buckets) {
            Collections.sort(bucket);
            
            for (int value : bucket) {
                array[arrayIndex++] = value;
            }
        }
    }

    /**
     * Finds the maximum value in an array.
     * 
     * @param array The array to search
     * @return The maximum value in the array
     */
    public static int findMaxValue(int[] array) {
        if (array == null || array.length == 0) {
            throw new IllegalArgumentException("Array cannot be null or empty");
        }
        
        int max = array[0];
        for (int value : array) {
            if (value > max) {
                max = value;
            }
        }
        return max;
    }

    /**
     * Finds the minimum value in an array.
     * 
     * @param array The array to search
     * @return The minimum value in the array
     */
    public static int findMinValue(int[] array) {
        if (array == null || array.length == 0) {
            throw new IllegalArgumentException("Array cannot be null or empty");
        }
        
        int min = array[0];
        for (int value : array) {
            if (value < min) {
                min = value;
            }
        }
        return min;
    }
}