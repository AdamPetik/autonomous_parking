using UnityEngine;

public class TimeScale : MonoBehaviour
{
    [SerializeField]
    private int timeScale = 1;
    void Start()
    {
        Time.timeScale = timeScale;
    }

    private void OnDestroy() {
        Time.timeScale = 1;
    }
}
