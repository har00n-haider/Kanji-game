using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.VFX;
using TMPro;
using System.Collections.Generic;
using Manabu.Core;
using RythmGame;


/// <summary>
/// Question and answers group for simple MCQ style kana test for reading
/// </summary>
public class ReadTargetSpawnData
{
    public bool kanaToRomaji;
    public Beat questionBeat;
    public Beat answerBeat;
    public Vector3 position;
    public Character questionChar;
    public List<Character> answers;
    public bool spawned;
}


public class ReadTarget : MonoBehaviour
{
    // refs
    [SerializeField]
    private ReadContainerTarget readContainerTargetPrefab;

    // state stuff
    public Character character;
    public bool selected = false;
    public ReadTargetSpawnData targetData;
    private ReadTargetConfig config;

    public ReadContainerTarget question;
    public List<ReadContainerTarget> answers;

    // hard coded positions
    Vector3 up = new Vector3(0, 1, 0) * distanceFromQuestion;
    Vector3 left = new Vector3(-1, -1, 0).normalized * distanceFromQuestion;
    Vector3 right = new Vector3(1, -1, 0).normalized * distanceFromQuestion;
    readonly static float distanceFromQuestion = 4.2f;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void Init(ReadTargetSpawnData readTargetData, ReadTargetConfig readTargetConfig)
    {
        targetData = readTargetData;

        // create the question target
        question = Instantiate(readContainerTargetPrefab,
            readTargetData.position,
            Quaternion.identity,
            transform);
        question.Init(readTargetData.questionBeat,
            this,
            readTargetData.questionChar,
            ReadContainerTarget.Type.Question,
            readTargetConfig,
            readTargetData,
            HandleSelected
        );
        config = readTargetConfig;
    }

    private void HandleSelected()
    {
        // create the answers
        // TODO: make sure to use the same limit here as the number of answers
        for (int i = 0; i < 3; i++)
        {
            Vector3 position = new Vector3();
            if (i == 0) position = transform.position + up;
            if (i == 1) position = transform.position + left;
            if (i == 2) position = transform.position + right;
            // create the answer targets
            ReadContainerTarget answerTarget = Instantiate(readContainerTargetPrefab,
                position,
                Quaternion.identity,
                transform);
            answerTarget.Init(targetData.answerBeat,
                this,
                targetData.answers[i],
                ReadContainerTarget.Type.Answer,
                config,
                targetData,
                null
            );
            answers.Add(answerTarget);
        }
    }

}
