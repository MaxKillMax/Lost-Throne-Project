using System.Collections;
using UnityEngine;
using Cinemachine;

public class Zoom : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    [SerializeField] private float[] zoomValues;
    [SerializeField] private float[] zoomTimes;

    private ZoomType _currentZoom = ZoomType.Standard;

    public ZoomType GetZoomState() => _currentZoom;

    public void SetZoom(ZoomType type)
    {
        _currentZoom = type;

        StopAllCoroutines();
        StartCoroutine(WaitForZoom(virtualCamera.m_Lens.OrthographicSize, zoomValues[(int)type], zoomTimes[(int)type]));
    }

    private IEnumerator WaitForZoom(float startZoom, float zoomValue, float time)
    {
        int tick = 0;
        int maxTick = 50;
        while (tick < maxTick)
        {
            tick++;
            virtualCamera.m_Lens.OrthographicSize = startZoom + ((zoomValue - startZoom) * ((float)tick / (float)maxTick));
            yield return new WaitForSeconds(time / maxTick);
        }
    }
}

public enum ZoomType
{
    Standard,
    InBuilding,
    InFight
}
