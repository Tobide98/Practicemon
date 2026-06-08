using UnityEngine;

public class UIParticleEffect : MonoBehaviour
{
    [SerializeField] private RectTransform uiTarget;
    [SerializeField] private ParticleSystem particlePrefab;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private float worldDepth = 10f;
    [SerializeField] private bool playOnEnable;
    [SerializeField] private bool followTarget = true;

    private ParticleSystem particleInstance;

    private void Awake()
    {
        if (uiTarget == null)
        {
            uiTarget = transform as RectTransform;
        }

        if (uiCamera == null)
        {
            uiCamera = Camera.main;
        }

        CreateParticleInstance();
    }

    private void OnEnable()
    {
        if (playOnEnable)
        {
            Play();
        }
    }

    private void LateUpdate()
    {
        if (followTarget && particleInstance != null)
        {
            particleInstance.transform.position = GetTargetWorldPosition();
        }
    }

    public void Play()
    {
        CreateParticleInstance();

        if (particleInstance == null)
        {
            return;
        }

        particleInstance.transform.position = GetTargetWorldPosition();
        particleInstance.Play(true);
    }

    public void Stop()
    {
        if (particleInstance != null)
        {
            particleInstance.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    private void CreateParticleInstance()
    {
        if (particleInstance != null || particlePrefab == null)
        {
            return;
        }

        particleInstance = Instantiate(particlePrefab, GetTargetWorldPosition(), Quaternion.identity);
    }

    private Vector3 GetTargetWorldPosition()
    {
        if (uiTarget == null)
        {
            return transform.position;
        }

        Camera cameraToUse = uiCamera != null ? uiCamera : Camera.main;
        Vector3 screenPosition = RectTransformUtility.WorldToScreenPoint(cameraToUse, uiTarget.position);

        if (cameraToUse == null)
        {
            screenPosition.z = worldDepth;
            return screenPosition;
        }

        screenPosition.z = worldDepth;
        return cameraToUse.ScreenToWorldPoint(screenPosition);
    }
}
