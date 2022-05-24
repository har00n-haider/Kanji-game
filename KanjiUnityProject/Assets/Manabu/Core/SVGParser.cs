// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// Do test the code! You usually need to change a few small bits.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text.RegularExpressions;

namespace Manabu.Core
{


    public class SVGParser
    {

        #region  public 

        /// <summary>
        /// Points are return in unity coordinate system.
        /// </summary>
        /// <param name="svgContent"></param>
        /// <param name="pntsInStroke">The number of points per stroke to use</param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static DrawData GetStrokesFromSvg(string svgContent, int pntsInStroke = 50)
        {
            if (svgContent == "") return null;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(svgContent);

            var pathElems = xmlDoc.GetElementsByTagName("path");

            // get stroke from raw data
            List<Stroke> strokes = new List<Stroke>();
            foreach (XmlNode pathElem in pathElems)
            {
                string pathStr = pathElem.Attributes.GetNamedItem("d").Value;
                var vectorPaths = GetVectorStroke(pathStr);
                Stroke rawStroke = new Stroke
                {
                    orderNo = int.Parse(pathElem.Attributes.GetNamedItem("id").Value.Split('-')[1].Replace("s", "")),
                    points = SVGUtils.GetPointsForVectorStroke(vectorPaths, pntsInStroke),
                };
                rawStroke.points = SVGUtils.SVGToUnityCoords(rawStroke.points);
                // HACK: Hardcoded for now as it doesn't seem that these values change in the
                // source kanji svg files
                rawStroke.points = SVGUtils.NormalizeAndConvertToUnity(rawStroke.points, 109, 109);
                strokes.Add(rawStroke);
            }

            return new DrawData(1, 1, 1, strokes);
        }

        #endregion


        #region  private 

        /**
         * Uses regex to get the SVG parametric objects from the d 
         */
        private static List<CubicBezier> GetVectorStroke(string pathStr)
        {
            List<CubicBezier> vectorPaths = new List<CubicBezier>();
            var lastPnt = new Vector2();
            Vector2? lastCbCtrlPnt = null;
            char? lastComChar = null;

            float pullVal()
            {
                Regex valRegex = new Regex(@"-?\d*\.?\d+");
                Match match = valRegex.Match(pathStr);
                if (match.Success)
                {
                    pathStr = valRegex.Replace(pathStr, "", 1);
                    return float.Parse(match.Value);
                }
                return 0f;
            }
            char? pullCommand()
            {
                Regex comRegex = new Regex(@"[^ 0-9,.-]");
                Match match = comRegex.Match(pathStr);
                if (match.Success)
                {
                    pathStr = comRegex.Replace(pathStr, "", 1);
                    return match.Value[0];
                }
                return null;
            }
            void cleanStr()
            {
                Regex clnRegex = new Regex(@"[ ,]*");
                Match match = clnRegex.Match(pathStr, 0);
                pathStr = clnRegex.Replace(pathStr, "", 1);
            }
            void runCommand(char? commChar)
            {
                switch (commChar)
                {
                    case 'M':
                        float moveX = pullVal();
                        float moveY = pullVal();
                        lastPnt = new Vector2(moveX, moveY);
                        cleanStr();
                        break;
                    case 'C':
                    case 'c':
                    case 'S':
                    case 's':
                        //cubic beziers
                        Vector2 p2 = new Vector2();
                        if (commChar == 'S' || commChar == 's')
                        {
                            //use reflected control point from previous cB
                            if (lastCbCtrlPnt == null)
                            {
                                throw new System.Exception("last cubic bezier control point not set");
                            }
                            Vector2 rflVec = lastPnt - lastCbCtrlPnt.Value;
                            if (commChar == 'S')
                            {
                                p2 = rflVec + lastPnt;
                            }
                            else
                            {
                                p2 = rflVec;
                            }
                        }
                        else
                        {
                            //standard method to get control point
                            float p2x = pullVal();
                            float p2y = pullVal();
                            p2 = new Vector2(p2x, p2y);
                        }
                        float p3x = pullVal();
                        float p3y = pullVal();
                        float p4x = pullVal();
                        float p4y = pullVal();
                        Vector2 p3 = new Vector2(p3x, p3y);
                        Vector2 p4 = new Vector2(p4x, p4y);
                        cleanStr();
                        CubicBezier cubeBez = new CubicBezier();
                        if (commChar == 'C' || commChar == 'S')
                        {
                            cubeBez.p1 = lastPnt;
                            cubeBez.p2 = p2;
                            cubeBez.p3 = p3;
                            cubeBez.p4 = p4;
                        }
                        else
                        {
                            cubeBez.p1 = lastPnt;
                            cubeBez.p2 = p2 + lastPnt;
                            cubeBez.p3 = p3 + lastPnt;
                            cubeBez.p4 = p4 + lastPnt;
                        }
                        lastCbCtrlPnt = cubeBez.p3;
                        lastPnt = cubeBez.p4;
                        vectorPaths.Add(cubeBez);
                        break;
                    default:
                        throw new System.Exception("unknown SVG command char: " + commChar);
                }
            }
            while (pathStr.Length > 0)
            {
                char? commChar = pullCommand();
                if (commChar == null && lastComChar == null)
                {
                    return vectorPaths;
                }
                else if (commChar == null)
                {
                    commChar = lastComChar;
                }
                runCommand(commChar);
                lastComChar = commChar;
            }
            return vectorPaths;
        }

        #endregion












    }

}