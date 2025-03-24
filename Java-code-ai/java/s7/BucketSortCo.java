package java.s7;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Random;

/**
 * Wikipedia: https://en.wikipedia.org/wiki/Bucket_sort
 */
public class BucketSortCo {

    public static void main(String[] args) {
        int[] arr = new int[10];

        // Generate 10 random numbers from -50 to 49
        Random random = new Random();
        for (int i = 0; i < arr.length; ++i) {
            arr[i] = random.nextInt(100) - 50;
        }

        bucketSort(arr);

        // Check if array is sorted
        for (int i = 0; i < arr.length - 1; ++i) {
            assert arr[i] <= arr[i + 1];
        }
    }

    /**
     * BucketSort algorithm implementation
     *
     * @param arr the array containing elements
     */
    private static void bucketSort(int[] arr) {
        int max = max(arr);
        int min = min(arr);
        int numberOfBuckets = max - min + 1;

        List<List<Integer>> buckets = new ArrayList<>(numberOfBuckets);

        // Initialize buckets
        for (int i = 0; i < numberOfBuckets; ++i) {
            buckets.add(new ArrayList<>());
        }

        // Distribute elements into buckets
        for (int value : arr) {
            int hash = hash(value, min, numberOfBuckets);
            buckets.get(hash).add(value);
        }

        // Sort individual buckets
        for (List<Integer> bucket : buckets) {
            Collections.sort(bucket);
        }

        // Concatenate buckets into original array
        int index = 0;
        for (List<Integer> bucket : buckets) {
            for (int value : bucket) {
                arr[index++] = value;
            }
        }
    }

    /**
     * Get the index of the bucket for the element
     *
     * @param elem the element of the array to be sorted
     * @param min the minimum value of the array
     * @param numberOfBuckets the number of buckets
     * @return the index of the bucket
     */
    private static int hash(int elem, int min, int numberOfBuckets) {
        return (elem - min) / numberOfBuckets;
    }

    /**
     * Calculate the maximum value of the array
     *
     * @param arr the array containing elements
     * @return the maximum value of the given array
     */
    private static int max(int[] arr) {
        int max = arr[0];
        for (int value : arr) {
            if (value > max) {
                max = value;
            }
        }
        return max;
    }

    /**
     * Calculate the minimum value of the array
     *
     * @param arr the array containing elements
     * @return the minimum value of the given array
     */
    private static int min(int[] arr) {
        int min = arr[0];
        for (int value : arr) {
            if (value < min) {
                min = value;
            }
        }
        return min;
    }
}