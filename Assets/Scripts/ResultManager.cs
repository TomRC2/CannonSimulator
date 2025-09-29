using System;
using UnityEngine;
using Proyecto26;

[Serializable]
public class ShotResult
{
    public float angle;
    public float force;
    public float mass;
    public bool hit;
    public float distance;
    public int affected;

    public ShotResult() { }

    public ShotResult(float angle, float force, float mass, bool hit, float distance, int affected)
    {
        this.angle = angle;
        this.force = force;
        this.mass = mass;
        this.hit = hit;
        this.distance = distance;
        this.affected = affected;
    }
}

public class ResultManager : MonoBehaviour
{
    public static ResultManager Instance { get; private set; }

    [Tooltip("Ej: https://mi-proyecto-default-rtdb.firebaseio.com/shots")]
    public string baseURL = "https://cannonsimulator-default-rtdb.firebaseio.com/shots";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SaveShot(ShotResult shot)
    {
        if (string.IsNullOrEmpty(baseURL))
        {
            Debug.LogError("ResultManager: baseURL no configurada.");
            return;
        }

        RestClient.Post(baseURL + ".json", shot).Then(response =>
        {
            Debug.Log("Resultado guardado: " + response.Text);
        }).Catch(err =>
        {
            Debug.LogError("Error guardando en Firebase: " + err.Message);
        });
    }

    public void GetShots(Action<string> callback)
    {
        RestClient.Get(baseURL + ".json").Then(response =>
        {
            try
            {
                Debug.Log("Datos recibidos: " + response.Text);
                callback?.Invoke(response.Text);
            }
            catch (Exception ex)
            {
                Debug.LogError("Error procesando respuesta: " + ex);
                callback?.Invoke(null);
            }
        }).Catch(err =>
        {
            Debug.LogError("Error leyendo Firebase: " + (err != null ? err.Message : "err es null"));
            callback?.Invoke(null);
        });

    }
}
