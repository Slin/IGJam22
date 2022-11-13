using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButtonAction : MonoBehaviour
{
    public GameObject ExplainScreen;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            StartGame();
        }
    }
    public void StartGame()
    {
        SceneManager.LoadScene("Island");
    }

    public void StartExplain()
    {
        ExplainScreen.SetActive(true);
        gameObject.SetActive(false);
    }
}
