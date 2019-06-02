using UnityEngine;

public class KMeansVector2 : MonoBehaviour {

    public bool showClustered = true;

    public int sampleSize = 50;
    public int spaceSize = 5;

    [Range(1, 50)]
    public int clustersCount = 3;

    [Range(10, 50)]
    public int iterations = 100;

    private KMeansResults result;
    private Vector2[] data;

    private void Start() {
        getData();
    }

    public void getData() {
        data = new Vector2[sampleSize];

        for (int i = 0; i < data.Length; i++) {
            float x = Random.Range(-1f, 1f);
            float y = Random.Range(-1f, 1f);
            data[i] = new Vector2(x, y) * spaceSize;
        }

        compute();
    }

    public void compute() {
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
                GizmosUtils.DrawText(GUI.skin, i.ToString(), new Vector3(data[result.clusters[i][j]].x, 0, data[result.clusters[i][j]].y), Gizmos.color, 10);
            }
        }
    }
}