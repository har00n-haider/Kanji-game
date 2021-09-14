using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MeaningButton : MonoBehaviour
{
    [HideInInspector]
    public MeaningInput meaningInput;
    private TextMeshProUGUI textMesh;
    public string text { get { return textMesh.text; } set { textMesh.text = value; } }
    private Button button;

    public void Init()
    {
        button = GetComponent<Button>();
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        button.onClick.AddListener(() => meaningInput.UpdateWordMeaning(textMesh.text));
    }

    private void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }

}
