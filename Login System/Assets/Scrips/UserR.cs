using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class UserR : MonoBehaviour
{
    [SerializeField] TMP_InputField UsernameInputField;
    [SerializeField] TMP_InputField PasswordInputField;
    [SerializeField] TMP_InputField ScoreInputField;

    [SerializeField] private TMP_Text[] Text;
    [SerializeField] List<TMP_Text> scoreTexts;

    [SerializeField] string URL = "https://sid-restapi.onrender.com/api/";

    private string token;
    private string username;

    void Start()
    {
        token = PlayerPrefs.GetString("token");

        if (string.IsNullOrEmpty(token))
        {
            Debug.Log("No token");
        }
        else
        {
            username = PlayerPrefs.GetString("username");
            StartCoroutine(GetPerfil(username));
        }
    }

    public void Registre()
    {
        Data data = new Data();

        data.username = UsernameInputField.text;
        data.password = PasswordInputField.text;

        string json = JsonUtility.ToJson(data);

        StartCoroutine(SendRegister(json));
    }
    public void Login()
    {
        Data data = new Data();

        data.username = UsernameInputField.text;
        data.password = PasswordInputField.text;

        string json = JsonUtility.ToJson(data);

        StartCoroutine(SendLogin(json));
    }
    public void UpdateScoreList()
    {
        StartCoroutine(GetUsers());
    }
    public void SetNewScore()
    {
        UserRegistryScore data = new UserRegistryScore();
        data.username = username;

        data.data = new DataUser();
        data.data.score = int.Parse(ScoreInputField.text);

        string json = JsonUtility.ToJson(data);

        StartCoroutine(SendScore(json));
    }

    public IEnumerator SendScore(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(URL + "usuarios", json);
        request.SetRequestHeader("Content-Type", "application/json");
        request.method = "PATCH";
        request.SetRequestHeader("x-token", token);
        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            Debug.Log("Error");
        }
        else
        {
            Data data = JsonUtility.FromJson<Data>(request.downloadHandler.text);
            Debug.Log(data.usuario.usernames + ": " + data.usuario.data.score);
        }
    }
    public IEnumerator SendRegister(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(URL + "usuario", json);
        request.SetRequestHeader("Content-Type", "application/json");
        request.method = "POST";
        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            Debug.Log("Netword Error: " + request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                Data data = JsonUtility.FromJson<Data>(request.downloadHandler.text);

                Debug.Log("Se registro el usuario con id " + data.usuario._id);
            }
            else
            {
                Debug.Log(request.error);
            }
        }
    }
    public IEnumerator SendLogin(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(URL + "auth/login", json);
        request.SetRequestHeader("Content-Type", "application/json");
        request.method = "POST";
        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            Debug.Log("Network Error: " + request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);

            if (request.responseCode == 200)
            {
                Data data = JsonUtility.FromJson<Data>(request.downloadHandler.text);

                Debug.Log("Inicio sesion usuario  " + data.usuario.usernames);

                PlayerPrefs.SetString("token", data.tokens);
                PlayerPrefs.SetString("username", data.usuario.usernames);

                token = data.tokens;
                username = data.usuario.usernames;

                Debug.Log(data.tokens);


            }
            else
            {
                Debug.Log(request.error);
            }
        }
    }
    public IEnumerator GetPerfil(string username)
    {
        UnityWebRequest request = UnityWebRequest.Get(URL + "usuarios/" + username);
        request.SetRequestHeader("x-token", token);
        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            Debug.Log("Network Error: " + request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                Data data = JsonUtility.FromJson<Data>(request.downloadHandler.text);
                Debug.Log("El usuario " + data.usuario.usernames + " esta activo");
                Debug.Log(data.usuario.data.score);
            }
            else
            {
                Debug.Log(request.error);
            }
        }
    }
    public IEnumerator GetUsers()
    {
        UnityWebRequest request = UnityWebRequest.Get(URL + "usuarios");
        request.SetRequestHeader("x-token", token);
        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            Debug.Log("Error");
        }
        else
        {
            //Debug.Log(request.downloadHandler.text);
            Data data = JsonUtility.FromJson<Data>(request.downloadHandler.text);

            data.usuarios.OrderByDescending(u => u.data.score);

            foreach (UserInfo us in data.usuarios)
            {
                Debug.Log(us.usernames + ": " + us.data.score);
            }
            for (int i = 0; i < data.usuarios.Length && i < scoreTexts.Count; i++)
            {
                UserInfo user = data.usuarios[i];
                scoreTexts[i].text = user.usernames + ": " + user.data.score;
            }

        }
    }
}

[System.Serializable]
public class Data
{
    public string username;
    public string password;
    public UserInfo usuario;
    public string tokens;
    public UserInfo[] usuarios;
}

public class UserRegistryScore
{
    public string username;
    public DataUser data;
}

public class UserInfo
{
    public string _id;
    public string usernames;
    public bool estado;
    public DataUser data;
}

[System.Serializable]
public class DataUser
{
    public int score;
}