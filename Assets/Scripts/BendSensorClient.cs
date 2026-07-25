using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class BendSensorClient : MonoBehaviour
{
    public static BendSensorClient Instance { get; private set; }

    [SerializeField] string serverUrl = "http://192.168.1.100:8001/data";
    [SerializeField] float pollInterval = 0.1f;

    public int Left { get; private set; }
    public int Right { get; private set; }
    public bool Connected { get; private set; }

    [Serializable]
    class SensorData
    {
        public int left;
        public int right;
        public bool connected;
        public float updatedAt;
    }

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        Debug.Log($"[ARDEBUG] BendSensorClient started. url={serverUrl}");
        StartCoroutine(PollLoop());
    }

    IEnumerator PollLoop()
    {
        var wait = new WaitForSeconds(pollInterval);
        while (true)
        {
            using (var req = UnityWebRequest.Get(serverUrl))
            {
                req.timeout = 1;
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    var data = JsonUtility.FromJson<SensorData>(req.downloadHandler.text);
                    if (data != null)
                    {
                        Left = data.left;
                        Right = data.right;
                        Connected = data.connected;
                        Debug.Log($"[ARDEBUG] Sensor OK left={Left} right={Right} connected={Connected}");
                    }
                }
                else
                {
                    Connected = false;
                    Debug.Log($"[ARDEBUG] Sensor request failed: result={req.result} error={req.error} code={req.responseCode}");
                }
            }
            yield return wait;
        }
    }
}
