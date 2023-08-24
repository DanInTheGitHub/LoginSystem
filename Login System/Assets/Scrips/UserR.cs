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
    [SerializeField] TMP_Text ScoreTextElement;

    //Sections
    [SerializeField] GameObject Log;
    [SerializeField] GameObject Lis;
    [SerializeField] GameObject Inic;



    [SerializeField] TMP_Text NameUser;

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

        Log.SetActive(true);
        Inic.SetActive(false);
        Lis.SetActive(false);
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
    public void Logout()
    {
        PlayerPrefs.DeleteKey("token");
        PlayerPrefs.DeleteKey("username");

        NameUser.text = "";

        foreach (var scoreText in scoreTexts)
        {
            scoreText.text = "";
        }
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

    public void Button1()
    {
        Log.SetActive(false);
        Inic.SetActive(true);
    }
    public void Button2()
    {
        Lis.SetActive(false);
        Inic.SetActive(true);
    }
    public void Button3()
    {
        Inic.SetActive(false);
        Log.SetActive(true);
    }
    public void Button4()
    {
        Inic.SetActive(false);
        Lis.SetActive(true);
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
            ScoreTextElement.text = "Score: " + data.usuario.data.score.ToString();
            Debug.Log(request.downloadHandler.text);
        }
    }
    public IEnumerator SendRegister(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(URL + "usuarios", json);
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

                Debug.Log("Inicio sesion usuario  " + data.usuario.username);

                PlayerPrefs.SetString("token", data.token);
                PlayerPrefs.SetString("username", data.usuario.username);

                token = data.token;
                username = data.usuario.username;

                Debug.Log(data.token);

                NameUser.text = "Active user: " + username;

                Button1();
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
                Debug.Log("El usuario " + data.usuario.username + " esta activo");
                Debug.Log(data.usuario._id);

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
            Data data = JsonUtility.FromJson<Data>(request.downloadHandler.text);

            UserInfo[] lista = data.usuarios.OrderByDescending(u => u.data.score).ToArray();

            foreach (UserInfo us in lista)
            {
                Debug.Log(us.username + ": " + us.data.score);
            }
            for (int i = 0; i < lista.Length && i < scoreTexts.Count; i++)
            {
                UserInfo user = lista[i];
                scoreTexts[i].text = user.username + ": " + user.data.score;
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
    public string token;
    public UserInfo[] usuarios;
}

public class UserRegistryScore
{
    public string username;
    public DataUser data;
}

[System.Serializable]
public class UserInfo
{
    public string _id;
    public string username;
    public bool estado;
    public DataUser data;
}

[System.Serializable]
public class DataUser
{
    public int score;
}