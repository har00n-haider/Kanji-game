using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.VFX;
using TMPro;
using System.Collections.Generic;
using Manabu.Core;
using System;

public class ReadTarget : MonoBehaviour, ITarget
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

    public Action<ITarget> OnBeatResult { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public Beat Beat => targetData.questionBeat;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void Init(ReadTargetSpawnData readTargetData, ReadTargetConfig readTargetConfig)
    {
        targetData = readTargetData;

        // Set the answers up on the read target
        // TODO: is the responsibility of this class to set up the answer data?
        int correctAnswer = UnityEngine.Random.Range(0, 3);
        for (int i = 0; i < 3; i++)
        {
            Character p;
            if (i == correctAnswer)
            {
                p = Utils.Clone(readTargetData.character);
            }
            else
            {
                p = GameManager.Instance.Database.GetRandomCharacter(readTargetData.character, readTargetData.character.type);
            }
            p.DisplayType = !readTargetData.kanaToRomaji ? DisplayType.Hiragana : DisplayType.Romaji;
            readTargetData.answers.Add(p);
        }

        // create the question target
        question = Instantiate(readContainerTargetPrefab,
            readTargetData.normalisedPosition,
            Quaternion.identity,
            transform);
        question.Init(readTargetData.questionBeat,
            this,
            readTargetData.character,
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

    public void HandleBeatResult(BeatResult hitResult)
    {
        //throw new NotImplementedException();
    }
}
