using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintBrushPrompts : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private List<string> promptsList;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void addPrompt(string prompt)
    {
        if (promptsList.Count < 3)
        {
            promptsList.Add(prompt);
        }
    }

    public void wipePromptList()
    {
        promptsList = new List<string>();
    }

    public List<string> passAndWipePromptList()
    {
        List<string> copy = new List<string>(promptsList);
        promptsList = new List<string>();
        copy.Sort();
        return copy;
    }
}
