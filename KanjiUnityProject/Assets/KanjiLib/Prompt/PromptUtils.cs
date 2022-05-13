using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace KanjiLib.Prompts
{

public static class Utils
{
    public static readonly PromptDisplayType[] kanjiPrompts = new PromptDisplayType[]
    {
        PromptDisplayType.Kanji,
        PromptDisplayType.Hiragana,
        PromptDisplayType.Romaji,
        PromptDisplayType.Meaning,
    };

    public static readonly PromptDisplayType[] katakanaPrompts = new PromptDisplayType[]
    {
        PromptDisplayType.Katana,
        PromptDisplayType.Romaji,
    };

    public static readonly PromptDisplayType[] hiraganaPrompts = new PromptDisplayType[]
    {
        PromptDisplayType.Hiragana,
        PromptDisplayType.Romaji,
    };

    public static readonly PromptInputType[] kanjiInputs = new PromptInputType[]
    {
        PromptInputType.KeyHiragana,
        PromptInputType.KeyHiraganaWithRomaji,
        PromptInputType.Meaning,
        PromptInputType.WritingHiragana,
        PromptInputType.WritingKanji,
    };

    public static readonly PromptInputType[] katakanaInputs = new PromptInputType[]
    {
        PromptInputType.KeyKatakana,
        PromptInputType.KeyKatakanaWithRomaji,
        PromptInputType.WritingKatakana,
    };

    public static readonly PromptInputType[] hiraganaInputs = new PromptInputType[]
    {
    PromptInputType.KeyHiragana,
    PromptInputType.KeyHiraganaWithRomaji,
    PromptInputType.WritingHiragana,
    };

    public static PromptInputType GetRandomInput(this PromptInputType[] inputs)
    {
        int idx = Random.Range(0, inputs.Length - 1);
        return inputs[idx];
    }

    public static PromptDisplayType GetRandomPrompt(this PromptDisplayType[] prompts)
    {
        int idx = Random.Range(0, prompts.Length - 1);
        return prompts[idx];
    }

}


}