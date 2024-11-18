using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject); // Keep GameManager alive across scenes
    }

    void Start()
    {
       
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Level 1");
    }

    public void LoadHowToPlay()
    {
        SceneManager.LoadScene("HowToPlay");
    }

   
}
