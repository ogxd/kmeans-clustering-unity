using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// Provides a simple implementation of the k-Means algorithm. This solution is quite simple and does not support any parallel execution as of yet.
/// </summary>
public static class KMeans {

    public static KMeansResults Cluster(Bounds[] items, int clusterCount, int maxIterations, int seed) {
        double[][] data = new double[items.Length][];
        for (int i = 0; i < items.Length; i++) {
            Vector3 v = items[i].center;
            data[i] = new double[] { v.x, v.y, v.z };
        }
        return Cluster(data, clusterCount, maxIterations, seed);
    }

    public static KMeansResults Cluster(Vector2[] items, int clusterCount, int maxIterations, int seed) {
        double[][] data = new double[items.Length][];
        for (int i = 0; i < items.Length; i++) {
            Vector2 v = items[i];
            data[i] = new double[] { v.x, v.y };
        }
        return Cluster(data, clusterCount, maxIterations, seed);
    }

    public static KMeansResults Cluster(Vector3[] items, int clusterCount, int maxIterations, int seed) {
        double[][] data = new double[items.Length][];
        for (int i = 0; i < items.Length; i++) {
            Vector3 v = items[i];
            data[i] = new double[] { v.x, v.y, v.z };
        }
        return Cluster(data, clusterCount, maxIterations, seed);
    }

    public static KMeansResults Cluster(double[][] data, int clusterCount, int maxIterations, int seed) {

        bool hasChanges = true;
        int iteration = 0;
        double totalDistance = 0;
        int numData = data.Length;
        int numAttributes = data[0].Length;

        // Create a random initial clustering assignment
        int[] clustering = InitializeClustering(numData, clusterCount, seed);

        // Create cluster means and centroids
        double[][] means = CreateMatrix(clusterCount, numAttributes);
        int[] centroidIdx = new int[clusterCount];
        int[] clusterItemCount = new int[clusterCount];

        // Perform the clustering
        while (hasChanges && iteration < maxIterations) {
            clusterItemCount = new int[clusterCount];
            totalDistance = CalculateClusteringInformation(data, clustering, ref means, ref centroidIdx, clusterCount, ref clusterItemCount);
            hasChanges = AssignClustering(data, clustering, centroidIdx, clusterCount);
            ++iteration;
        }

        // Create the final clusters
        int[][] clusters = new int[clusterCount][];
        for (int k = 0; k < clusters.Length; k++)
            clusters[k] = new int[clusterItemCount[k]];

        int[] clustersCurIdx = new int[clusterCount];
        for (int i = 0; i < clustering.Length; i++) {
            clusters[clustering[i]][clustersCurIdx[clustering[i]]] = i;
            ++clustersCurIdx[clustering[i]];
        }

        // Return the results
        return new KMeansResults(clusters, means, centroidIdx, totalDistance);
    }

    private static int[] InitializeClustering(int numData, int clusterCount, int seed) {

        var rnd = new System.Random(seed);
        var clustering = new int[numData];

        for (int i = 0; i < numData; ++i)
            clustering[i] = rnd.Next(0, clusterCount);

        return clustering;
    }

    private static double[][] CreateMatrix(int rows, int columns) {
        var matrix = new double[rows][];

        for (int i = 0; i < matrix.Length; i++)
            matrix[i] = new double[columns];

        return matrix;
    }

    private static double CalculateClusteringInformation(double[][] data, int[] clustering, ref double[][] means, ref int[] centroidIdx, int clusterCount, ref int[] clusterItemCount) {
            
        // Reset the means to zero for all clusters
        foreach (var mean in means)
            for (int i = 0; i < mean.Length; i++)
                mean[i] = 0;

        // Calculate the means for each cluster
        // Do this in two phases, first sum them all up and then divide by the count in each cluster
        for (int i = 0; i < data.Length; i++) {
            // Sum up the means
            var row = data[i];
            var clusterIdx = clustering[i]; // What cluster is data i assigned to
            ++clusterItemCount[clusterIdx]; // Increment the count of the cluster that row i is assigned to
            for (int j = 0; j < row.Length; j++)
                means[clusterIdx][j] += row[j];
        }

        // Now divide to get the average
        for (int k = 0; k < means.Length; k++) {
            for (int a = 0; a < means[k].Length; a++) {
                int itemCount = clusterItemCount[k];
                means[k][a] /= itemCount > 0 ? itemCount : 1;
            }
        }

        double totalDistance = 0;
        // Calc the centroids
        double[] minDistances = new double[clusterCount].Select(x => double.MaxValue).ToArray();
        for (int i = 0; i < data.Length; i++) {
            var clusterIdx = clustering[i]; // What cluster is data i assigned to
            var distance = CalculateDistance(data[i], means[clusterIdx]);
            totalDistance += distance;
            if (distance < minDistances[clusterIdx]) {
                minDistances[clusterIdx] = distance;
                centroidIdx[clusterIdx] = i;
            }
        }

        return totalDistance;
    }

    /// <summary>
    /// Calculates the distance for each point in <see cref="data"/> from each of the centroid in <see cref="centroidIdx"/> and 
    /// assigns the data item to the cluster with the minimum distance.
    /// </summary>
    /// <returns>true if any clustering arrangement has changed, false if clustering did not change.</returns>
    private static bool AssignClustering(double[][] data, int[] clustering, int[] centroidIdx, int clusterCount) {
        bool changed = false;

        for (int i = 0; i < data.Length; i++) {
            double minDistance = double.MaxValue;
            int minClusterIndex = -1;

            for (int k = 0; k < clusterCount; k++) {
                double distance = CalculateDistance(data[i], data[centroidIdx[k]]);
                if (distance < minDistance) {
                    minDistance = distance;
                    minClusterIndex = k;
                }
            }

            // Re-arrange the clustering for datapoint if needed
            if (minClusterIndex != -1 && clustering[i] != minClusterIndex) {
                changed = true;
                clustering[i] = minClusterIndex;
            }
        }

        return changed;
    }

    /// <summary>
    ///  Calculates the eculidean distance from the <see cref="point"/> to the <see cref="centroid"/>
    /// </summary>
    private static double CalculateDistance(double[] point, double[] centroid) {
        // For each attribute calculate the squared difference between the centroid and the point
        double sum = 0;
        for (int i = 0; i < point.Length; i++)
            sum += Math.Pow(centroid[i] - point[i], 2);

        return Math.Sqrt(sum);
    }
}


/// <summary>
/// Represents a single result from the <see cref="KMeans"/> algorithm. 
/// Contains the original items arranged into the clusters converged on as well as the centroids chosen and the total distance of the converged solution.
/// </summary>
/// <typeparam name="T"></typeparam>
public class KMeansResults {

    /// <summary>
    /// The original items arranged into the clusters converged on
    /// </summary>
    public readonly int[][] clusters;

    /// <summary>
    /// The final mean values used for the clusters. Mostly for debugging purposes.
    /// </summary>
    public readonly double[][] means;

    /// <summary>
    /// The list of centroids used in the final solution. These are indicies into the original data.
    /// </summary>
    public readonly int[] centroids;

    /// <summary>
    /// The total distance between all the nodes and their centroids in the final solution. 
    /// This can be used as a reference point on how "good" the solution is when the algorithm is run repeatedly with different starting configuration.
    /// Lower is "usually" better.
    /// </summary>
    public readonly double totalDistance;

    public KMeansResults(int[][] clusters, double[][] means, int[] centroids, double totalDistance) {
        this.clusters = clusters;
        this.means = means;
        this.centroids = centroids;
        this.totalDistance = totalDistance;
    }
}
