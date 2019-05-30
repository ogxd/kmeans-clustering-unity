using UnityEngine;

public class KMeansTest : MonoBehaviour {

    public bool showClustered = true;
    public int sampleSize = 50;
    public int spaceSize = 5;
    public int clustersCount = 3;
    public int iterations = 100;

    private KMeansUtils.KMeansResults<Vector3> clusters;

    private void Start() {

        var points = new Vector3[sampleSize];

        for (int i = 0; i < points.Length; i++) {
            float x = Random.Range(-1f, 1f);
            float y = Random.Range(-1f, 1f);
            float z = Random.Range(-1f, 1f);
            points[i] = new Vector3(x, y, z) * spaceSize;
        }

        clusters = KMeansUtils.KMeans.Cluster(points, clustersCount, iterations);
    }

    void OnDrawGizmos() {

        if (clusters == null)
            return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < clusters.Clusters.Length; i++) {
            if (showClustered) {
                Color color = Color.HSVToRGB(1f * i / clusters.Clusters.Length, 1f, 1f);
                //color.a = 0.5f;
                Gizmos.color = color;
            }
            for (int j = 0; j < clusters.Clusters[i].Length; j++) {
                Gizmos.DrawSphere(clusters.Clusters[i][j], 0.1f);
            }
        }
    }
}