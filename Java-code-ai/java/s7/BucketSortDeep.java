package java.s7;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Random;

/**
 * Implementation of Bucket Sort algorithm.
 * Wikipedia: https://en.wikipedia.org/wiki/Bucket_sort
 */
public class BucketSortDeep {

    public static void main(String[] args) {
        int[] array = generateRandomArray(10, -50, 50);
        sort(array);
        verifySorted(array);
    }

    /**
     * Generates an array of random integers within a specified range.
     *
     * @param size the size of the array to generate
     * @param min the minimum value (inclusive)
     * @param max the maximum value (exclusive)
     * @return the generated array
     */
    private static int[] generateRandomArray(int size, int min, int max) {
        int[] array = new int[size];
        Random random = new Random();
        for (int i = 0; i < size; i++) {
            array[i] = random.nextInt(max - min) + min;
        }
        return array;
    }

    /**
     * Verifies that the array is sorted in ascending order.
     *
     * @param array the array to verify
     * @throws AssertionError if the array is not sorted
     */
    private static void verifySorted(int[] array) {
        for (int i = 0; i < array.length - 1; i++) {
            assert array[i] <= array[i + 1];
        }
    }

    /**
     * Sorts the given array using Bucket Sort algorithm.
     *
     * @param array the array to be sorted
     */
    public static void sort(int[] array) {
        if (array.length == 0) {
            return;
        }

        int maxValue = findMaxValue(array);
        int minValue = findMinValue(array);
        int bucketCount = maxValue - minValue + 1;

        List<List<Integer>> buckets = initializeBuckets(bucketCount);
        distributeElementsToBuckets(array, buckets, minValue, bucketCount);
        sortIndividualBuckets(buckets);
        mergeBucketsIntoArray(buckets, array);
    }

    /**
     * Initializes the buckets with empty lists.
     *
     * @param bucketCount the number of buckets to initialize
     * @return the list of initialized buckets
     */
    private static List<List<Integer>> initializeBuckets(int bucketCount) {
        List<List<Integer>> buckets = new ArrayList<>(bucketCount);
        for (int i = 0; i < bucketCount; i++) {
            buckets.add(new ArrayList<>());
        }
        return buckets;
    }

    /**
     * Distributes array elements into the appropriate buckets.
     *
     * @param array the source array
     * @param buckets the buckets to distribute to
     * @param minValue the minimum value in the array
     * @param bucketCount the number of buckets
     */
    private static void distributeElementsToBuckets(int[] array, 
            List<List<Integer>> buckets, int minValue, int bucketCount) {
        for (int value : array) {
            int bucketIndex = calculateBucketIndex(value, minValue, bucketCount);
            buckets.get(bucketIndex).add(value);
        }
    }

    /**
     * Sorts each individual bucket.
     *
     * @param buckets the buckets to sort
     */
    private static void sortIndividualBuckets(List<List<Integer>> buckets) {
        for (List<Integer> bucket : buckets) {
            Collections.sort(bucket);
        }
    }

    /**
     * Merges the sorted buckets back into the original array.
     *
     * @param buckets the sorted buckets
     * @param array the target array
     */
    private static void mergeBucketsIntoArray(List<List<Integer>> buckets, int[] array) {
        int index = 0;
        for (List<Integer> bucket : buckets) {
            for (int value : bucket) {
                array[index++] = value;
            }
        }
    }

    /**
     * Calculates the appropriate bucket index for a given value.
     *
     * @param value the value to be placed in a bucket
     * @param minValue the minimum value in the array
     * @param bucketCount the number of buckets
     * @return the index of the bucket for this value
     */
    private static int calculateBucketIndex(int value, int minValue, int bucketCount) {
        return (value - minValue) / bucketCount;
    }

    /**
     * Finds the maximum value in the array.
     *
     * @param array the array to search
     * @return the maximum value
     */
    private static int findMaxValue(int[] array) {
        int max = array[0];
        for (int value : array) {
            if (value > max) {
                max = value;
            }
        }
        return max;
    }

    /**
     * Finds the minimum value in the array.
     *
     * @param array the array to search
     * @return the minimum value
     */
    private static int findMinValue(int[] array) {
        int min = array[0];
        for (int value : array) {
            if (value < min) {
                min = value;
            }
        }
        return min;
    }
}