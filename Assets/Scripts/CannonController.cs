using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public struct ImpactResult
{
    public float timeOfFlight;
    public Vector3 impactPoint;
    public float relativeSpeed;
    public float collisionImpulse;
    public int piecesDown;
}

public class CannonController : MonoBehaviour
{
    [Header("References")]
    public GameObject projectilePrefab;
    public Transform spawnPoint;
    private float currentAngle = 45f;

    [Header("UI")]
    public Slider angleSlider;
    public TMP_InputField angleInput;

    public Slider forceSlider;
    public TMP_InputField forceInput;

    public Slider massSlider;
    public TMP_Text massLabel;

    public Button fireButton;
    public TMP_Text reportText;

    [Header("Visual")]
    public Transform cannonVisual;

    [Header("Options")]
    public bool useAddForce = true;
    public ForceMode forceMode = ForceMode.Impulse;
    private GameObject currentProjectile;

    void Start()
    {
        if (angleSlider) angleSlider.onValueChanged.AddListener(OnAngleSliderChanged);
        if (forceSlider) forceSlider.onValueChanged.AddListener(OnForceSliderChanged);
        if (massSlider) massSlider.onValueChanged.AddListener(OnMassSliderChanged);
        if (fireButton) fireButton.onClick.AddListener(Fire);

        SyncAngleUI(angleSlider ? angleSlider.value : 45f);
        SyncForceUI(forceSlider ? forceSlider.value : 10f);
        SyncMassUI(massSlider ? massSlider.value : 1f);

        if (reportText) reportText.text = "Ready to fire.";
    }
    void Update()
    {
        UpdateCannonVisual(currentAngle);
    }

    void OnDestroy()
    {
        if (angleSlider) angleSlider.onValueChanged.RemoveListener(OnAngleSliderChanged);
        if (forceSlider) forceSlider.onValueChanged.RemoveListener(OnForceSliderChanged);
        if (massSlider) massSlider.onValueChanged.RemoveListener(OnMassSliderChanged);
        if (fireButton) fireButton.onClick.RemoveListener(Fire);
    }

    #region UI callbacks
    void OnAngleSliderChanged(float val)
    {
        SyncAngleUI(val);
    }

    void OnForceSliderChanged(float val)
    {
        SyncForceUI(val);
    }

    void OnMassSliderChanged(float val)
    {
        SyncMassUI(val);
    }

    void SyncAngleUI(float val)
    {
        if (angleInput) angleInput.text = val.ToString("F1");
        currentAngle = val;
    }

    void SyncForceUI(float val)
    {
        if (forceInput) forceInput.text = val.ToString("F1");
    }

    void SyncMassUI(float val)
    {
        if (massLabel) massLabel.text = val.ToString("F2") + " kg";
    }
    #endregion

    public void Fire()
    {
        if (!projectilePrefab || !spawnPoint)
        {
            Debug.LogWarning("Projectile prefab or spawn point not assigned.");
            return;
        }

        if (currentProjectile) Destroy(currentProjectile);

        currentProjectile = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);
        var rb = currentProjectile.GetComponent<Rigidbody>();
        var tracker = currentProjectile.GetComponent<ProjectileTracker>();
        if (!rb)
        {
            Debug.LogError("Projectile prefab must have a Rigidbody component.");
            return;
        }
        if (!tracker)
        {
            tracker = currentProjectile.AddComponent<ProjectileTracker>();
        }

        float angle = angleSlider ? angleSlider.value : (angleInput ? ParseFloat(angleInput.text, 45f) : 45f);
        float force = forceSlider ? forceSlider.value : (forceInput ? ParseFloat(forceInput.text, 10f) : 10f);
        float mass = massSlider ? massSlider.value : 1f;

        rb.mass = Mathf.Max(0.0001f, mass);
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        tracker.OnImpact += HandleImpact;
        tracker.LaunchTime = Time.time;

        Vector3 dir = Quaternion.Euler(0f, 0f, -angle) * spawnPoint.up;
        Vector3 launchVector = dir.normalized * force;

        if (useAddForce)
        {
            rb.AddForce(launchVector, forceMode);
        }
        else
        {
            rb.linearVelocity = launchVector;
        }

        if (reportText) reportText.text = "Disparado: �ngulo=" + angle.ToString("F1") + "�, fuerza=" + force.ToString("F1") + ", masa=" + mass.ToString("F2") + "kg";
    }

    float ParseFloat(string s, float fallback)
    {
        float r;
        if (float.TryParse(s, out r)) return r;
        return fallback;
    }
    void HandleImpact(ImpactResult result)
    {
        int baseScore = Mathf.Clamp(result.piecesDown * 100 + Mathf.RoundToInt(result.collisionImpulse * 10f), 0, 9999);

        string report = "--- Informe de tiro ---\n" +
                        $"Tiempo de vuelo: {result.timeOfFlight:F2} s\n" +
                        $"Punto de impacto: {result.impactPoint.ToString("F2")}\n" +
                        $"Velocidad relativa: {result.relativeSpeed:F2} m/s\n" +
                        $"Impulso de colisión: {result.collisionImpulse:F2} Nós\n" +
                        $"Piezas derribadas: {result.piecesDown}\n" +
                        $"Puntuación: {baseScore}\n" +
                        "----------------------";

        if (reportText) reportText.text = report;

        Debug.Log(report);
    }
    void OnDrawGizmos()
    {
        if (!spawnPoint) return;

        float angle = angleSlider ? angleSlider.value : 45f;

        Vector3 dir = Quaternion.Euler(0f, 0f, -angle) * spawnPoint.up;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + dir * 3f);
        Gizmos.DrawSphere(spawnPoint.position + dir * 3f, 0.1f);
    }

    void UpdateCannonVisual(float angle)
    {
        if (!cannonVisual) return;
        cannonVisual.localRotation = Quaternion.Euler(0f, 0f, -angle);
    }
}

[RequireComponent(typeof(Rigidbody))]
public class ProjectileTracker : MonoBehaviour
{
    public float LaunchTime { get; set; }

    public Action<ImpactResult> OnImpact;

    [Header("Heuristic settings")]
    public float checkRadius = 2f;
    public float fallenHeightThreshold = -1f;

    void OnCollisionEnter(Collision collision)
    {
        float impactTime = Time.time - LaunchTime;
        Vector3 impactPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;
        float relSpeed = collision.relativeVelocity.magnitude;
        float impulse = collision.impulse.magnitude;

        int pieces = CountFallenPiecesNearby();

        ImpactResult res = new ImpactResult
        {
            timeOfFlight = impactTime,
            impactPoint = impactPoint,
            relativeSpeed = relSpeed,
            collisionImpulse = impulse,
            piecesDown = pieces
        };

        OnImpact?.Invoke(res);
    }

    int CountFallenPiecesNearby()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, checkRadius);
        int count = 0;
        foreach (var c in cols)
        {
            Rigidbody rb = c.attachedRigidbody;
            if (rb == null) continue;

            if (rb.linearVelocity.y < -0.5f || rb.transform.position.y < fallenHeightThreshold)
            {
                if (rb.gameObject.CompareTag("StructurePiece") || rb.gameObject.layer == LayerMask.NameToLayer("Structure"))
                    count++;
            }
        }
        return count;
    }
}
