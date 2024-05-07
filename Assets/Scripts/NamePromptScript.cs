using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NamePromptScript : MonoBehaviour
{
    [SerializeField] private string prompt;
    // Stalizrt is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public string getPrompt()
    {
        return prompt;
    }
}
