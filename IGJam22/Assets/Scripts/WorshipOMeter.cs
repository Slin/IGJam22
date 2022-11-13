using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WorshipOMeter : MonoBehaviour
{
    private TMP_Text textmeshPro;
    private TikiSettlers settlers;

    // Start is called before the first frame update
    void Start()
    {
        textmeshPro = GetComponent<TMP_Text>();
        settlers = FindObjectOfType<TikiSettlers>();
    }

    // Update is called once per frame
    void Update()
    {
         textmeshPro.SetText("{0:0}", settlers.worshipOMeter);
    }
}
