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

        public Vector2 MeasureString(string s)
        {
            int width = 0;

            for (int i = 0; i < s.Length; i++)
            {
                Rectangle srcRect = characterRects[s[i] % 256];
                width += srcRect.Width;
            }

            int height = rowHeight;

            return new Vector2(width, height);
        }
    }

    public static class SpriteBatchExtensions
    {
        public static void DrawString(this SpriteBatch spriteBatch, HLFont hlFont, string text, Vector2 position, Color color)
        {
            int xOff = 0;
            for (int i = 0; i < text.Length; i++)
            {
                Rectangle srcRect = hlFont.characterRects[text[i] % 256];
                spriteBatch.Draw(hlFont.texture, new Vector2(position.X + xOff, position.Y), srcRect, color);
                xOff += srcRect.Width;
            }
        }

        public static void DrawString(this SpriteBatch spriteBatch, HLFont hlFont, StringBuilder text, Vector2 position, Color color)
        {
            int xOff = 0;
            for (int i = 0; i < text.Length; i++)
            {
                Rectangle srcRect = hlFont.characterRects[text[i] % 256];
                spriteBatch.Draw(hlFont.texture, new Vector2(position.X + xOff, position.Y), srcRect, color);
                xOff += srcRect.Width;
            }
        }
    }
}
