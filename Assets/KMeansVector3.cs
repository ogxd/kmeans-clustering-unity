using UnityEngine;

public class KMeansVector3 : MonoBehaviour {

    public bool showClustered = true;
    public int sampleSize = 50;
    public int spaceSize = 5;
    public int clustersCount = 3;
    public int iterations = 100;

    private KMeansResults result;
    private Vector3[] data;

    private void Start() {

        data = new Vector3[sampleSize];

        for (int i = 0; i < data.Length; i++) {
            float x = Random.Range(-1f, 1f);
            float y = Random.Range(-1f, 1f);
            float z = Random.Range(-1f, 1f);
            data[i] = new Vector3(x, y, z) * spaceSize;
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
                //color.a = 0.5f;
                Gizmos.color = color;
            }
            for (int j = 0; j < result.clusters[i].Length; j++) {
                //Gizmos.DrawSphere(data[result.clusters[i][j]], 0.1f);
                GizmosUtils.DrawText(GUI.skin, i.ToString(), data[result.clusters[i][j]], Gizmos.color, 10);
            }
        }
    }
}