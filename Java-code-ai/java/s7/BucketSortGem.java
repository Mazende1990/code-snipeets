package java.s7;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Random;

/**
 * Wikipedia: https://en.wikipedia.org/wiki/Bucket_sort
 */
public class BucketSortGem{

    public static void main(String[] args) {
        int[] array = generateRandomArray(10, -50, 49);
        bucketSort(array);
        assertArrayIsSorted(array);
    }

    /**
     * BucketSort algorithm implementation.
     *
     * @param array the array to be sorted
     */
    private static void bucketSort(int[] array) {
        int minValue = findMin(array);
        int maxValue = findMax(array);
        int bucketCount = maxValue - minValue + 1;

        List<List<Integer>> buckets = initializeBuckets(bucketCount);
        distributeElementsIntoBuckets(array, buckets, minValue, bucketCount);
        sortBuckets(buckets);
        concatenateBucketsToArray(array, buckets);
    }

    /**
     * Generates an array of random integers within a specified range.
     *
     * @param size  the size of the array
     * @param min   the minimum value of the random numbers
     * @param max   the maximum value of the random numbers
     * @return the generated array
     */
    private static int[] generateRandomArray(int size, int min, int max) {
        int[] array = new int[size];
        Random random = new Random();
        for (int i = 0; i < size; i++) {
            array[i] = random.nextInt(max - min + 1) + min;
        }
        return array;
    }

    /**
     * Initializes the buckets for the bucket sort.
     *
     * @param bucketCount the number of buckets
     * @return the list of buckets
     */
    private static List<List<Integer>> initializeBuckets(int bucketCount) {
        List<List<Integer>> buckets = new ArrayList<>(bucketCount);
        for (int i = 0; i < bucketCount; i++) {
            buckets.add(new ArrayList<>());
        }
        return buckets;
    }

    /**
     * Distributes the elements of the array into the buckets.
     *
     * @param array       the array to be sorted
     * @param buckets     the list of buckets
     * @param minValue    the minimum value of the array
     * @param bucketCount the number of buckets
     */
    private static void distributeElementsIntoBuckets(int[] array, List<List<Integer>> buckets, int minValue, int bucketCount) {
        for (int value : array) {
            int bucketIndex = calculateBucketIndex(value, minValue, bucketCount);
            buckets.get(bucketIndex).add(value);
        }
    }

    /**
     * Calculates the index of the bucket for a given element.
     *
     * @param element     the element to be placed in a bucket
     * @param minValue    the minimum value of the array
     * @param bucketCount the number of buckets
     * @return the index of the bucket
     */
    private static int calculateBucketIndex(int element, int minValue, int bucketCount) {
        return (element - minValue) / bucketCount;
    }

    /**
     * Sorts the individual buckets.
     *
     * @param buckets the list of buckets
     */
    private static void sortBuckets(List<List<Integer>> buckets) {
        for (List<Integer> bucket : buckets) {
            Collections.sort(bucket);
        }
    }

    /**
     * Concatenates the sorted buckets back into the original array.
     *
     * @param array   the original array
     * @param buckets the list of sorted buckets
     */
    private static void concatenateBucketsToArray(int[] array, List<List<Integer>> buckets) {
        int index = 0;
        for (List<Integer> bucket : buckets) {
            for (int value : bucket) {
                array[index++] = value;
            }
        }
    }

    /**
     * Finds the maximum value in the array.
     *
     * @param array the array
     * @return the maximum value
     */
    private static int findMax(int[] array) {
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
     * @param array the array
     * @return the minimum value
     */
    private static int findMin(int[] array) {
        int min = array[0];
        for (int value : array) {
            if (value < min) {
                min = value;
            }
        }
        return min;
    }

    /**
     * Asserts that the array is sorted in ascending order.
     *
     * @param array the array to check
     */
    private static void assertArrayIsSorted(int[] array) {
        for (int i = 0, limit = array.length - 1; i < limit; i++) {
            assert array[i] <= array[i + 1];
        }
    }
}