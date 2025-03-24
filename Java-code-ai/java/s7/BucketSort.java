package java.s7;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Random;

/**
 * Bucket Sort implementation.
 * Source: https://en.wikipedia.org/wiki/Bucket_sort
 */
public class BucketSort {

    public static void main(String[] args) {
        int[] array = new int[10];

        // Generate 10 random numbers between -50 and 49
        Random random = new Random();
        for (int i = 0; i < array.length; ++i) {
            array[i] = random.nextInt(100) - 50;
        }

        bucketSort(array);

        // Verify the array is sorted
        for (int i = 0; i < array.length - 1; ++i) {
            assert array[i] <= array[i + 1];
        }
    }

    /**
     * Sorts the input array using the Bucket Sort algorithm.
     *
     * @param array the array to sort
     */
    private static void bucketSort(int[] array) {
        int min = findMin(array);
        int max = findMax(array);
        int numberOfBuckets = max - min + 1;

        // Initialize empty buckets
        List<List<Integer>> buckets = new ArrayList<>(numberOfBuckets);
        for (int i = 0; i < numberOfBuckets; ++i) {
            buckets.add(new ArrayList<>());
        }

        // Distribute elements into corresponding buckets
        for (int value : array) {
            int bucketIndex = getBucketIndex(value, min, numberOfBuckets);
            buckets.get(bucketIndex).add(value);
        }

        // Sort each bucket individually
        for (List<Integer> bucket : buckets) {
            Collections.sort(bucket);
        }

        // Concatenate all buckets into the original array
        int index = 0;
        for (List<Integer> bucket : buckets) {
            for (int value : bucket) {
                array[index++] = value;
            }
        }
    }

    /**
     * Calculates the index of the bucket for a given element.
     *
     * @param value the value to place in a bucket
     * @param min the minimum value in the array
     * @param numberOfBuckets the total number of buckets
     * @return the index of the appropriate bucket
     */
    private static int getBucketIndex(int value, int min, int numberOfBuckets) {
        return (value - min) / numberOfBuckets;
    }

    /**
     * Finds the maximum value in the array.
     *
     * @param array the input array
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
     * @param array the input array
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
}
