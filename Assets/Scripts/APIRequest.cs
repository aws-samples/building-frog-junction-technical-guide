// Copyright 2020 Amazon.com, Inc. or its affiliates. All Rights Reserved.
using FrogJunction;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

// This is a helper request class to communicate with the games API. It signs the API and deals
// with JSON encoding if needed.
public class APIRequest : MonoBehaviour
{
    public delegate void OnError(long code, string error);
    public delegate void OnSuccess(long code, string result);

    public static APIRequest Instance
    {
        get
        {
            return _instance;
        }
    }


    public void PostRequest<T>(string resource, T requestBody, OnSuccess onSuccess, OnError onError)
    {
        var jsonBody = JsonUtility.ToJson(requestBody);
        StartCoroutine(ProcessPostReuqest(resource, jsonBody, onSuccess, onError));
    }

    public void GetRequest(string resource, OnSuccess onSuccess, OnError onError)
    {
        StartCoroutine(ProcessGetRequest(resource, onSuccess, onError));
    }

    private IEnumerator ProcessPostReuqest(string resource, string body, OnSuccess onSuccess, OnError onError)
    {
        string uri = AWSStaticConfiguration.API_ENDPOINT + "/" + resource;
        var request = new UnityWebRequest(uri, UnityWebRequest.kHttpVerbPOST);

        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(body);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", PlayerIdentificationSystem.Instance.AuthenticationTokens.IdToken);

        Debug.Log($"POST {uri}");
        Debug.Log($"Body: {body}");
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError($"API Post Request error code: {request.responseCode} Message: {request.error} Text: {request.downloadHandler.text}");
            onError(request.responseCode, request.error + " : " + request.downloadHandler.text);
        }
        else
        {
            Debug.Log($"API Post Request success code {request.responseCode}");
            onSuccess(request.responseCode, request.downloadHandler.text);
        }

    }

    private IEnumerator ProcessGetRequest(string resource, OnSuccess onSuccess, OnError onError)
    {
        string uri = AWSStaticConfiguration.API_ENDPOINT + "/" + resource;
        var request = UnityWebRequest.Get(uri);
        Debug.Log($"GET {uri}");
        request.SetRequestHeader("Authorization", PlayerIdentificationSystem.Instance.AuthenticationTokens.IdToken);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError($"API Post Request error code: {request.responseCode} Message: {request.error} Text: {request.downloadHandler.text}");
            onError(request.responseCode, request.error + " : " + request.downloadHandler.text);

        }
        else
        {
            Debug.Log($"API Get Request success code {request.responseCode}");
            onSuccess(request.responseCode, request.downloadHandler.text);
        }

    }

    private static APIRequest _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if(_instance == null)
        {
            _instance = new GameObject("WebRequest").AddComponent<APIRequest>();
            DontDestroyOnLoad(_instance.gameObject);
        }
    }
    
}
