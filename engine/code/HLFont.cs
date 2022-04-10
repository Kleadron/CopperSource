using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace CopperSource
{
    public class HLFont
    {
        public int width;
        public int height;

        public int rowCount;
        public int rowHeight;

        public Rectangle[] characterRects;
        public Texture2D texture;

        public int LineSpacing
        {
            get
            {
                return rowHeight;
            }
        }

        public HLFont(GraphicsDevice device, WadFile.Font fontData)
        {
            width = 256;//fontData.width;
            height = fontData.height;

            rowCount = fontData.rowCount;
            rowHeight = fontData.rowHeight;

            characterRects = new Rectangle[256];

            for (int i = 0; i < characterRects.Length; i++)
            {
                int charX = fontData.info[i].texOffset % width;
                int charY = fontData.info[i].texOffset / width;

                characterRects[i] = new Rectangle(
                    charX, 
                    charY,
                    fontData.info[i].charWidth, 
                    rowHeight
                    );
            }

            // create font texture
            fontData.colorPalette[255] = Color.Transparent;
            texture = new Texture2D(device, width, height, false, SurfaceFormat.Color);
            Color[] colors = new Color[width * height];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = fontData.colorPalette[fontData.data[i]];
            }
            texture.SetData(0, null, colors, 0, colors.Length);
        }

        /// <summary>
        /// Measures a string with the font information and returns the bounding size.
        /// </summary>
        /// <param name="s">The input string to measure.</param>
        /// <returns>The bounding size of the string.</returns>
        public Vector2 MeasureString(string s)
        {
            int width = 0;
            int height = rowHeight;

            int rowWidth = 0;

            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '\n')
                {
                    rowWidth = 0;
                    height += rowHeight;
                }
                Rectangle srcRect = characterRects[c % 256];
                rowWidth += srcRect.Width;
                if (rowWidth > width)
                    width = rowWidth;
            }

            return new Vector2(width, height);
        }

        /// <summary>
        /// A variant of MeasureString that uses Point instead of Vector2.
        /// </summary>
        /// <param name="s">The input string to measure.</param>
        /// <returns>The bounding size of the string.</returns>
        public Point MeasureStringPoint(string s)
        {
            int width = 0;
            int height = rowHeight;

            int rowWidth = 0;

            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '\n')
                {
                    rowWidth = 0;
                    height += rowHeight;
                }
                Rectangle srcRect = characterRects[c % 256];
                rowWidth += srcRect.Width;
                if (rowWidth > width)
                    width = rowWidth;
            }

            return new Point(width, height);
        }
    }

    public static class SpriteBatchExtensions
    {
        public static void DrawString(this SpriteBatch spriteBatch, HLFont hlFont, string text, int x, int y, Color color, int scaler = 1)
        {
            int xOff = 0;
            int yOff = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '\n')
                {
                    xOff = 0;
                    yOff += hlFont.rowHeight * scaler;
                }
                Rectangle srcRect = hlFont.characterRects[c % 256];
                spriteBatch.Draw(hlFont.texture, new Rectangle(x + xOff, y + yOff, srcRect.Width * scaler, srcRect.Height * scaler), srcRect, color);
                xOff += srcRect.Width * scaler;
            }
        }

        public static void DrawString(this SpriteBatch spriteBatch, HLFont hlFont, StringBuilder text, int x, int y, Color color, int scaler = 1)
        {
            int xOff = 0;
            int yOff = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '\n')
                {
                    xOff = 0;
                    yOff += hlFont.rowHeight * scaler;
                }
                Rectangle srcRect = hlFont.characterRects[c % 256];
                spriteBatch.Draw(hlFont.texture, new Rectangle(x + xOff, y + yOff, srcRect.Width * scaler, srcRect.Height * scaler), srcRect, color);
                xOff += srcRect.Width * scaler;
            }
        }
    }
}
