using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.Graphics;
using SFML.Window;

namespace PacMan
{
    public class AnimatedTexture
    {
        public Texture texture;
        public int rows;
        public int columns;
        public int currentFrame = 0;
        public int totalFrames
        {
            get { return rows * columns; }
        }
    }

    public class AnimatedSprite : Sprite
    {
        private Dictionary<string, AnimatedTexture> textures = new Dictionary<string, AnimatedTexture>();
        public Direction Facing = Direction.RIGHT;

        public AnimatedSprite(AnimatedTexture upTexture, AnimatedTexture downTexture, AnimatedTexture rightTexture, AnimatedTexture leftTexture)
        {
            textures.Add("right", rightTexture);
            textures.Add("left", leftTexture);
            textures.Add("up", upTexture);
            textures.Add("down", downTexture);
        }

        public void Update(Direction d)
        {
            AnimatedTexture t = textures["right"];

            if (d == Direction.RIGHT) t = textures["right"];
            if (d == Direction.LEFT) t = textures["left"];
            if (d == Direction.UP) t = textures["up"];
            if (d == Direction.DOWN) t = textures["down"];

            t.currentFrame++;
            if (t.currentFrame == t.totalFrames)
                t.currentFrame = 0;

            int width = (int)t.texture.Size.X / t.columns;
            int height = (int)t.texture.Size.Y / t.rows;
            int row = (int)((float)t.currentFrame / (float)t.columns);
            int column = t.currentFrame % t.columns;

            base.Texture = t.texture;
            base.TextureRect = new IntRect(width * column, height * row, width, height);

        }


    }
}
