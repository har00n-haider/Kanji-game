using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Manabu.Core;

namespace Manabu.Examples
{

    public class CharacterGrid : MonoBehaviour
    {
        public LineRenderer gridLinePrefab;
        [SerializeField]
        private float charSizeToGridSizePercentage;
        private float gridThickness;
        [SerializeField]
        private float charSizeToGridSpacingPercentage;
        private float gridSpacing;
        private DrawableCharacter drawChar;
        private List<LineRenderer> lines = new List<LineRenderer>();
        [SerializeField]
        private Color color;

        private Vector3[] gridPnts = new Vector3[]
        {
            new Vector3( 0.0f, 0.0f, 0.5f), // 0 - bottom left
            new Vector3( 0.5f, 0.0f, 0.5f), // 1 - bottom  middle
            new Vector3( 1.0f, 0.0f, 0.5f), // 2 - bottom right
            new Vector3( 0.0f, 0.5f, 0.5f), // 3 - middle left
            new Vector3( 0.5f, 0.5f, 0.5f), // 4 - middle middle
            new Vector3( 1.0f, 0.5f, 0.5f), // 5 - middle right
            new Vector3( 0.0f, 1.0f, 0.5f), // 6 - top left
            new Vector3( 0.5f, 1.0f, 0.5f), // 7 - top middle
            new Vector3( 1.0f, 1.0f, 0.5f), // 8 - top right
        };

        public void Init(DrawableCharacter drawChar)
        {
            this.drawChar = drawChar;
            GenerateGrid();
        }

        // Update is called once per frame
        void Update()
        {
            gridThickness = drawChar.boxCollider.size.magnitude * charSizeToGridSizePercentage;
            gridSpacing = drawChar.boxCollider.size.magnitude * charSizeToGridSpacingPercentage;
            UpdateLines();
        }

        // origin assumed at 0,0,0
        void GenerateGrid()
        {
            for (int i = 0; i < 6; i++)
            {
                lines.Add(Instantiate(gridLinePrefab, transform));
            }
        }

        void UpdateLines()
        {
            Vector3 size = drawChar.boxCollider.size;
            for (int i = 0; i < gridPnts.Length; i++)
            {
                gridPnts[i].Scale(size);
            }

            // horizontal lines
            UpdateLine(lines[0], gridPnts, 0, 2);
            UpdateLine(lines[1], gridPnts, 3, 5);
            UpdateLine(lines[2], gridPnts, 6, 8);

            // vertical lines
            UpdateLine(lines[3], gridPnts, 0, 6);
            UpdateLine(lines[4], gridPnts, 1, 7);
            UpdateLine(lines[5], gridPnts, 2, 8);
        }

        void UpdateLine(LineRenderer line, Vector3[] pnts, int sIdx, int eIdx)
        {
            line.material.mainTextureScale = new Vector2(gridSpacing, gridSpacing);
            line.material.color = color;
            line.positionCount = 2;
            line.SetPositions(new Vector3[] { pnts[sIdx], pnts[eIdx] });
            line.startWidth = gridThickness;
            line.endWidth = gridThickness;
        }
    }

}