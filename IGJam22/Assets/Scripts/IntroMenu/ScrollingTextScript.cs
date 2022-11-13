using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScrollingTextScript : MonoBehaviour
{
    public GameObject SourceOfText;
    public GameObject DestinationOfText;
    public float speed = 1;

    private TextMeshProUGUI refDestinationOfText;
    private TextMeshProUGUI refSourceOfText;
    private int counter = 0;
    private float time = 0;
    private int page = 1;
    // Start is called before the first frame update
    void Start()
    {
        refDestinationOfText = DestinationOfText.GetComponent<TextMeshProUGUI>();
        refSourceOfText = SourceOfText.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {

        time += Time.deltaTime * speed;
        if (counter / page > 220)
        {
            refDestinationOfText.text = string.Empty;
            page++;
        }
        if (time > 1)
        {
            
            if (counter < refSourceOfText.text.Length)
            {
                refDestinationOfText.text += refSourceOfText.text[counter];
                counter++;
            }
            time = 0;
        }
        

    }
}
