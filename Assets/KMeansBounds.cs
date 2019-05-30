using UnityEngine;

public class KMeansBounds : MonoBehaviour {

    public bool showClustered = true;
    public int sampleSize = 50;
    public int spaceSize = 5;
    public int clustersCount = 3;
    public int iterations = 100;
    public float boxMaxSize = 1f;

    private KMeansResults result;
    private Bounds[] data;

    private void Start() {

        data = new Bounds[sampleSize];

        for (int i = 0; i < data.Length; i++) {
            float x = Random.Range(-1f, 1f);
            float y = Random.Range(-1f, 1f);
            float z = Random.Range(-1f, 1f);
            float sx = Random.Range(0.1f, boxMaxSize);
            float sy = Random.Range(0.1f, boxMaxSize);
            float sz = Random.Range(0.1f, boxMaxSize);
            data[i] = new Bounds(new Vector3(x, y, z) * spaceSize, new Vector3(sx, sy, sz));
        }

        result = KMeans.Cluster(data, clustersCount, iterations, 0);
    }

    void OnDrawGizmos() {

        if (result == null)
            return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < result.clusters.Length; i++) {
            if (showClustered) {
                Color color = Color.HSVToRGB(1f * i / result.clusters.Length, 1f, 1f);
                Gizmos.color = color;
            }
            for (int j = 0; j < result.clusters[i].Length; j++) {
                Gizmos.DrawCube(data[result.clusters[i][j]].center, data[result.clusters[i][j]].size);
            }
        }
    }
}