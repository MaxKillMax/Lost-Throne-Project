using System.Collections;
using Cinemachine;
using UnityEngine;

public class Zoom : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _virtualCamera;

    [SerializeField] private float[] _zoomValues;
    [SerializeField] private float[] _zoomTimes;

    private ZoomType _currentZoom = ZoomType.Standard;

    public ZoomType GetZoomState() => _currentZoom;

    public void SetZoom(ZoomType type)
    {
        _currentZoom = type;

        StopAllCoroutines();
        StartCoroutine(WaitForZoom(_virtualCamera.m_Lens.OrthographicSize, _zoomValues[(int)type], _zoomTimes[(int)type]));
    }

    private IEnumerator WaitForZoom(float startZoom, float zoomValue, float time)
    {
        int tick = 0;
        int maxTick = 50;
        while (tick < maxTick)
        {
            tick++;
            _virtualCamera.m_Lens.OrthographicSize = startZoom + ((zoomValue - startZoom) * (tick / (float)maxTick));
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
