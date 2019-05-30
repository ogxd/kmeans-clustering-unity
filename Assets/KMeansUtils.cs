using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class KMeansUtils
{

    /// <summary>
    /// Defines a property or field as an attribute to use for the k-means clustering
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class KMeansValueAttribute : Attribute { }

    /// <summary>
    /// Delegate that can be passed in to the <see cref="KMeans.Cluster{T}"/> function that allows the caller to provide their own distance calculation function 
    /// for a point to a centroid.
    /// </summary>
    /// <param name="point">the point being calculated</param>
    /// <param name="centroid">the centroid that is being calculated against</param>
    /// <returns>the distance value between the point and the centroid</returns>
    public delegate double KMeansCalculateDistanceDelegate(double[] point, double[] centroid);

    public delegate double CoordinateGetterDelegate<T>(T item, int index);

    public class CoordinateGetter<T> {

        public CoordinateGetterDelegate<T>[] delegates;

        public CoordinateGetter(params CoordinateGetterDelegate<T>[] delegates) {
            this.delegates = delegates;
        }
    }

    /// <summary>
    /// Provides a simple implementation of the k-Means algorithm. This solution is quite simple and does not support any parallel execution as of yet.
    /// </summary>
    public static class KMeans {

        private static double[][] ConvertEntities<T>(T[] items) {

            string typeStrVector2 = typeof(Vector2).ToString();
            string typeStrVector3 = typeof(Vector3).ToString();
            string typeStrBounds = typeof(Bounds).ToString();

            switch (typeof(T).ToString()) {
                case :
            }

            if (typeof(T) == typeof(Vector3)) {

            }

            double[][] result = new double[items.Length][];

            for (int i = 0; i < items.Length; i++) {
                Vector3 v = (Vector3)(object)items[i];
                result[i] = new double[] { v.x, v.y, v.z };
            }

            return result;
        }

        /// <summary>
        /// Clusters the given item set into the desired number of clusters. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">the list of data items that should be processed, this can be an array of primitive values such as <see cref="System.Double[]"/> 
        /// or a class struct that exposes properties using the <see cref="KMeansValueAttribute"/></param>
        /// <param name="clusterCount">the desired number of clusters</param>
        /// <param name="maxIterations">the maximum number of iterations to perform</param>
        /// <param name="calculateDistanceFunction">optional, custom distance function, if omitted then the euclidean distance will be used as default</param>
        /// <param name="randomSeed">optional, a seed for the random generator that initially arranges the clustering of the nodes (specify the same value to ensure that the start ordering will be the same)</param>
        /// <param name="initialCentroidIndices">optional, the initial centroid configuration (as indicies into the <see cref="items"/> array). When this is used the <see cref="randomSeed"/> has no effect.
        /// Experiment with this as the initial arrangements of the centroids has a huge impact on the final cluster arrangement.</param>
        /// <returns>a result containing the items arranged into clusters as well as the centroids converged on and the total distance value for the cluster nodes.</returns>
        public static KMeansResults<T> Cluster<T>(T[] items, int clusterCount, int maxIterations, CoordinateGetter<T> a, KMeansCalculateDistanceDelegate calculateDistanceFunction = null, int randomSeed = 0, int[] initialCentroidIndices = null) {

            double[][] data = ConvertEntities(items);

            // Use the built in Euclidean distance calculation if no custom one is specified
            if (calculateDistanceFunction == null)
                calculateDistanceFunction = CalculateDistance;

            bool hasChanges = true;
            int iteration = 0;
            double totalDistance = 0;
            int numData = data.Length;
            int numAttributes = data[0].Length;

            // Create a random initial clustering assignment
            int[] clustering = InitializeClustering(numData, clusterCount, randomSeed);

            // Create cluster means and centroids
            double[][] means = CreateMatrix(clusterCount, numAttributes);
            int[] centroidIdx = new int[clusterCount];
            int[] clusterItemCount = new int[clusterCount];

            // If we specify initial centroid indices then let's assign clustering based on those immediately
            if (initialCentroidIndices != null && initialCentroidIndices.Length == clusterCount) {
                centroidIdx = initialCentroidIndices;
                AssignClustering(data, clustering, centroidIdx, clusterCount, calculateDistanceFunction);
                //                Debug.WriteLine("Pre-Seeded Centroids resulted in initial clustering: " + string.Join(",", clustering.Select(x => x.ToString()).ToArray()));
            }

            // Perform the clustering
            while (hasChanges && iteration < maxIterations) {
                clusterItemCount = new int[clusterCount];
                totalDistance = CalculateClusteringInformation(data, clustering, ref means, ref centroidIdx, clusterCount, ref clusterItemCount, calculateDistanceFunction);

                //                Debug.WriteLine("------------- Iter: " + iteration);
                //                Debug.WriteLine("Clustering: " + string.Join(",", clustering.Select(x => x.ToString()).ToArray()));
                //                Debug.WriteLine("Means: " + string.Join(",", means.Select(x => "[" + string.Join(",", x.Select(y => y.ToString("#0.0")).ToArray()) + "]").ToArray()));
                //                Debug.WriteLine("Centroids: " + string.Join(",", centroidIdx.Select(x => x.ToString()).ToArray()));
                //                Debug.WriteLine("Cluster Counts: " + string.Join(",", clusterItemCount.Select(x => x.ToString()).ToArray()));

                hasChanges = AssignClustering(data, clustering, centroidIdx, clusterCount, calculateDistanceFunction);
                ++iteration;
            }

            // Create the final clusters
            T[][] clusters = new T[clusterCount][];
            for (int k = 0; k < clusters.Length; k++)
                clusters[k] = new T[clusterItemCount[k]];

            int[] clustersCurIdx = new int[clusterCount];
            for (int i = 0; i < clustering.Length; i++) {
                clusters[clustering[i]][clustersCurIdx[clustering[i]]] = items[i];
                ++clustersCurIdx[clustering[i]];
            }

            // Return the results
            return new KMeansResults<T>(clusters, means, centroidIdx, totalDistance);
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

        private static double CalculateClusteringInformation(double[][] data, int[] clustering, ref double[][] means, ref int[] centroidIdx, int clusterCount, ref int[] clusterItemCount, KMeansCalculateDistanceDelegate calculateDistanceFunction) {
            
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
                //var distance = CalculateDistance(data[i], means[clusterIdx]);
                var distance = calculateDistanceFunction(data[i], means[clusterIdx]);
                totalDistance += distance;
                if (distance < minDistances[clusterIdx]) {
                    minDistances[clusterIdx] = distance;
                    centroidIdx[clusterIdx] = i;
                }
            }
            //double totalCentroidDistance = minDistances.Sum();

            return totalDistance;
        }

        /// <summary>
        /// Calculates the distance for each point in <see cref="data"/> from each of the centroid in <see cref="centroidIdx"/> and 
        /// assigns the data item to the cluster with the minimum distance.
        /// </summary>
        /// <returns>true if any clustering arrangement has changed, false if clustering did not change.</returns>
        private static bool AssignClustering(double[][] data, int[] clustering, int[] centroidIdx, int clusterCount, KMeansCalculateDistanceDelegate calculateDistanceFunction) {
            bool changed = false;

            for (int i = 0; i < data.Length; i++) {
                double minDistance = double.MaxValue;
                int minClusterIndex = -1;

                for (int k = 0; k < clusterCount; k++) {
                    double distance = calculateDistanceFunction(data[i], data[centroidIdx[k]]);
                    if (distance < minDistance) {
                        minDistance = distance;
                        minClusterIndex = k;
                    }
                    // todo: track outliers here as well and maintain an average and std calculation for the distances!
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
            //return Math.Sqrt(point.Select((t, i) => Math.Pow(centroid[i] - t, 2)).Sum()); // LINQ is slower than doing the for-loop!
        }
    }

    /// <summary>
    /// Represents a single result from the <see cref="KMeans"/> algorithm. 
    /// Contains the original items arranged into the clusters converged on as well as the centroids chosen and the total distance of the converged solution.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class KMeansResults<T> {
        /// <summary>
        /// The original items arranged into the clusters converged on
        /// </summary>
        public T[][] Clusters { get; private set; }

        /// <summary>
        /// The final mean values used for the clusters. Mostly for debugging purposes.
        /// </summary>
        public double[][] Means { get; private set; }

        /// <summary>
        /// The list of centroids used in the final solution. These are indicies into the original data.
        /// </summary>
        public int[] Centroids { get; private set; }

        /// <summary>
        /// The total distance between all the nodes and their centroids in the final solution. 
        /// This can be used as a reference point on how "good" the solution is when the algorithm is run repeatedly with different starting configuration.
        /// Lower is "usually" better.
        /// </summary>
        public double TotalDistance { get; private set; }

        public KMeansResults(T[][] clusters, double[][] means, int[] centroids, double totalDistance) {
            Clusters = clusters;
            Means = means;
            Centroids = centroids;
            TotalDistance = totalDistance;
        }
    }
}
