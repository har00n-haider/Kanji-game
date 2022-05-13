using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KanjiLib.Core;


namespace KanjiLib.Prompts
{

public class PromptLabel : MonoBehaviour
{
    // refs
    public PromptHolder promptHolder;

    public KanjiManager kanjiManager;

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }

    // unity event method
    public void UpdateSelectedPromptHolderFromLabel()
    {
        kanjiManager.UpdateSelectedPromptHolderFromLabel(promptHolder);
    }
}

}