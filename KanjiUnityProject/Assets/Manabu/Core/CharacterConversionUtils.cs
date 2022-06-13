// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// Do test the code! You usually need to change a few small bits.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Manabu.Core
{

    public class CubicBezier
    {
        public Vector2 p1 = new Vector2();
        public Vector2 p2 = new Vector2();
        public Vector2 p3 = new Vector2();
        public Vector2 p4 = new Vector2();
        public float estimatedLength;
    }

    public static class SVGUtils
    {

        // t must be a value from 0 - 1
        private static Vector2 GetPntOnCubicBezier(float t, CubicBezier cB)
        {
            var ti = 1 - t;
            Vector2 term1 = cB.p1 * (ti * ti * ti);
            Vector2 term2 = cB.p2 * (3 * ti * ti * t);
            Vector2 term3 = cB.p3 * (3 * ti * t * t);
            Vector2 term4 = cB.p4 * (t * t * t);
            Vector2 r = term1 + term2 + term3 + term4;
            return r;
        }

        public static float GetLengthOfCubicBezier(CubicBezier cB, float res = 0.01f)
        {
            float length = 0;
            Vector2 curPnt = cB.p1;
            for (float i = res; i <= 1; i += res)
            {
                Vector2 newPnt = GetPntOnCubicBezier(i, cB);
                length += (newPnt - curPnt).magnitude;
                curPnt = newPnt;
            }
            return length;
        }

        private struct PathInfo
        {
            public float[] lengths;
            public float totLength;
        };

        public static Vector2 GetPointOnVectorStroke(Stroke stroke, float t)
        {
            Vector2 result = Vector2.zero;
            // find relevant path, adapt t for it, then find point
            float tStrokeDistance = t * stroke.unscaledLength;
            int j = 0;
            while (j < stroke.vectorPaths.Count - 1 && tStrokeDistance > stroke.vectorPaths[j].estimatedLength)
            {
                tStrokeDistance -= stroke.vectorPaths[j].estimatedLength;
                j++;
            }
            float tPathDistance = tStrokeDistance / stroke.vectorPaths[j].estimatedLength;
            tPathDistance = Mathf.Clamp(tPathDistance, 0, 1);
            result = GetPntOnCubicBezier(tPathDistance, stroke.vectorPaths[j]);
            return result;
        }

        public static List<Vector2> GetPointsForVectorStroke(Stroke stroke, int pntsInStroke)
        {
            // get the points across the whole vector path
            List<Vector2> pnts = new List<Vector2>();
            for (int i = 0; i <= pntsInStroke; i++)
            {
                float tStroke = i / (float)pntsInStroke; // should go from 0 - 1
                pnts.Add(GetPointOnVectorStroke(stroke, tStroke));
            }
            //Debug.Break();
            return pnts;
        }

        public static float GetLengthForPnts(List<Vector2> points)
        {
            float totalDist = 0;
            for (int i = 1; i < points.Count; i++)
            {
                totalDist += (points[i] - points[i - 1]).magnitude;
            }
            return totalDist;
        }

        public static float GetLengthForVectorStroke(List<CubicBezier> vectorPaths)
        {
            // get total length of line
            float totalDist = 0;
            for (int i = 0; i < vectorPaths.Count; i++)
            {
                totalDist += GetLengthOfCubicBezier(vectorPaths[i]);
            }
            return totalDist;

        }

        public static List<Vector2> GetKeyPointsForVectorStroke(Stroke stroke, float segmentLength)
        {
            float totalLength = stroke.unscaledLength;
            List<Vector2> keyPoints = new();
            float currentLength = totalLength;
            if (currentLength <= 0) return keyPoints;
            while (currentLength >= 0)
            {
                float t = currentLength / totalLength;
                keyPoints.Insert(0, GetPointOnVectorStroke(stroke, t));
                currentLength -= segmentLength;
            }
            return keyPoints;
        }

        public static Vector2 NormalizeAndConvertPointToUnityCoords(Vector2 p, float svgHeight, float svgWidth)
        {
            return new Vector2(p.x * (1 / svgWidth), 1 - (p.y * (1 / svgHeight)));
        }

        public static List<Vector2> NormalizeAndConvertPointsToUnityCoords(List<Vector2> points, float svgHeight, float svgWidth)
        {
            return points.ConvertAll(p => NormalizeAndConvertPointToUnityCoords(p, svgHeight, svgWidth));
        }

        public static List<CubicBezier> NormalizeAndConvertCurvesToUnityCoords(List<CubicBezier> curves, float svgHeight, float svgWidth)
        {
            foreach (CubicBezier b in curves)
            {
                b.p1 = NormalizeAndConvertPointToUnityCoords(b.p1, svgHeight, svgWidth);
                b.p2 = NormalizeAndConvertPointToUnityCoords(b.p2, svgHeight, svgWidth);
                b.p3 = NormalizeAndConvertPointToUnityCoords(b.p3, svgHeight, svgWidth);
                b.p4 = NormalizeAndConvertPointToUnityCoords(b.p4, svgHeight, svgWidth);
            }
            return curves;
        }

    }

    public static class CharacterConversionUtils
    {

        private static readonly Dictionary<char, char> KatakanaToHiraganaMap = new Dictionary<char, char>()
    {
        {'ア','あ'},
        {'イ','い'},
        {'ウ','う'},
        {'エ','え'},
        {'オ','お'},
        {'カ','か'},
        {'キ','き'},
        {'ク','く'},
        {'ケ','け'},
        {'コ','こ'},
        {'サ','さ'},
        {'シ','し'},
        {'ス','す'},
        {'セ','せ'},
        {'ソ','そ'},
        {'タ','た'},
        {'チ','ち'},
        {'ツ','つ'},
        {'テ','て'},
        {'ト','と'},
        {'ナ','な'},
        {'ニ','に'},
        {'ヌ','ぬ'},
        {'ネ','ね'},
        {'ノ','の'},
        {'ハ','は'},
        {'ヒ','ひ'},
        {'フ','ふ'},
        {'ヘ','へ'},
        {'ホ','ほ'},
        {'マ','ま'},
        {'ミ','み'},
        {'ム','む'},
        {'メ','め'},
        {'モ','も'},
        {'ヤ','や'},
        {'ユ','ゆ'},
        {'ヨ','よ'},
        {'ラ','ら'},
        {'リ','り'},
        {'ル','る'},
        {'レ','れ'},
        {'ロ','ろ'},
        {'ワ','わ'},
        {'ヰ','ゐ'},
        {'ヱ','ゑ'},
        {'ヲ','を'},
        {'ン','ん'},
        {'ガ','が'},
        {'ギ','ぎ'},
        {'グ','ぐ'},
        {'ゲ','げ'},
        {'ゴ','ご'},
        {'ザ','ざ'},
        {'ジ','じ'},
        {'ズ','ず'},
        {'ゼ','ぜ'},
        {'ゾ','ぞ'},
        {'ダ','だ'},
        {'ヂ','ぢ'},
        {'ヅ','づ'},
        {'デ','で'},
        {'ド','ど'},
        {'バ','ば'},
        {'ビ','び'},
        {'ブ','ぶ'},
        {'ベ','べ'},
        {'ボ','ぼ'},
        {'パ','ぱ'},
        {'ピ','ぴ'},
        {'プ','ぷ'},
        {'ペ','ぺ'},
        {'ポ','ぽ'},
        // yoon
        {'ャ','ゃ'},
        {'ュ','ゅ'},
        {'ョ','ょ'},
        // sokuon
        {'ッ','っ'},
        {'ー','ー'},
    };

        private static readonly Dictionary<char, char> HiraganaToKatakanaMap = new Dictionary<char, char>()
    {
        {'あ','ア'},
        {'い','イ'},
        {'う','ウ'},
        {'え','エ'},
        {'お','オ'},
        {'か','カ'},
        {'き','キ'},
        {'く','ク'},
        {'け','ケ'},
        {'こ','コ'},
        {'さ','サ'},
        {'し','シ'},
        {'す','ス'},
        {'せ','セ'},
        {'そ','ソ'},
        {'た','タ'},
        {'ち','チ'},
        {'つ','ツ'},
        {'て','テ'},
        {'と','ト'},
        {'な','ナ'},
        {'に','ニ'},
        {'ぬ','ヌ'},
        {'ね','ネ'},
        {'の','ノ'},
        {'は','ハ'},
        {'ひ','ヒ'},
        {'ふ','フ'},
        {'へ','ヘ'},
        {'ほ','ホ'},
        {'ま','マ'},
        {'み','ミ'},
        {'む','ム'},
        {'め','メ'},
        {'も','モ'},
        {'や','ヤ'},
        {'ゆ','ユ'},
        {'よ','ヨ'},
        {'ら','ラ'},
        {'り','リ'},
        {'る','ル'},
        {'れ','レ'},
        {'ろ','ロ'},
        {'わ','ワ'},
        {'ゐ','ヰ'},
        {'ゑ','ヱ'},
        {'を','ヲ'},
        {'ん','ン'},
        {'が','ガ'},
        {'ぎ','ギ'},
        {'ぐ','グ'},
        {'げ','ゲ'},
        {'ご','ゴ'},
        {'ざ','ザ'},
        {'じ','ジ'},
        {'ず','ズ'},
        {'ぜ','ゼ'},
        {'ぞ','ゾ'},
        {'だ','ダ'},
        {'ぢ','ヂ'},
        {'づ','ヅ'},
        {'で','デ'},
        {'ど','ド'},
        {'ば','バ'},
        {'び','ビ'},
        {'ぶ','ブ'},
        {'べ','ベ'},
        {'ぼ','ボ'},
        {'ぱ','パ'},
        {'ぴ','ピ'},
        {'ぷ','プ'},
        {'ぺ','ペ'},
        {'ぽ','ポ'},
        {'ゃ','ャ'},
        {'ゅ','ュ'},
        {'ょ','ョ'},
        {'っ','ッ'},
        {'ー','ー'},
    };

        public static char[] UnmodifiedHiragana = new char[]
        {
        'あ',
        'い',
        'う',
        'え',
        'お',
        'か',
        'き',
        'く',
        'け',
        'こ',
        'さ',
        'し',
        'す',
        'せ',
        'そ',
        'た',
        'ち',
        'つ',
        'て',
        'と',
        'な',
        'に',
        'ぬ',
        'ね',
        'の',
        'は',
        'ひ',
        'ふ',
        'へ',
        'ほ',
        'ま',
        'み',
        'む',
        'め',
        'も',
        'や',
        'ゆ',
        'よ',
        'ら',
        'り',
        'る',
        'れ',
        'ろ',
        'わ',
        'を',
        'ん',
        };

        public static char[] UnmodifiedKatakana = new char[]
        {
        'ア',
        'イ',
        'ウ',
        'エ',
        'オ',
        'カ',
        'キ',
        'ク',
        'ケ',
        'コ',
        'サ',
        'シ',
        'ス',
        'セ',
        'ソ',
        'タ',
        'チ',
        'ツ',
        'テ',
        'ト',
        'ナ',
        'ニ',
        'ヌ',
        'ネ',
        'ノ',
        'ハ',
        'ヒ',
        'フ',
        'ヘ',
        'ホ',
        'マ',
        'ミ',
        'ム',
        'メ',
        'モ',
        'ヤ',
        'ユ',
        'ヨ',
        'ラ',
        'リ',
        'ル',
        'レ',
        'ロ',
        'ワ',
        'ヲ',
        'ン'
        };


        public static char HiraganaToKatakana(char hiragana)
        {
            return HiraganaToKatakanaMap[hiragana];
        }

        public static char KatakanaToHiragana(char hiragana)
        {
            return KatakanaToHiraganaMap[hiragana];
        }

        public static string KanaToRomaji(string input)
        {
            return WanaKanaSharp.WanaKana.ToRomaji(input);
        }

    }
}