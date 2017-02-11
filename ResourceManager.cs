using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.Graphics;
using SFML.Audio;

namespace PacMan
{
    public class ResourceManager
    {
        private static ResourceManager instance = null;
        Dictionary<string, Texture> _textures = new Dictionary<string, Texture>();
        Dictionary<string, AnimatedTexture> _animTextures = new Dictionary<string, AnimatedTexture>();
        Dictionary<string, Sound> _sounds = new Dictionary<string, Sound>();
        Dictionary<string, Font> _fonts = new Dictionary<string, Font>();

        public static ResourceManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ResourceManager();
                }
                return instance;
            }
        }

        public void LoadTextureFromFile(string name, string path)
        {
            Texture texture = new Texture(path);

            _textures.Add(name, texture);
        }

        public Texture GetTexture(string name)
        {
            return _textures[name];
        }

        public void LoadAnimatedTextureFromFile(string name, string path, int columns, int rows)
        {
            AnimatedTexture animTexture = new AnimatedTexture();
            animTexture.texture = new Texture(path);
            animTexture.columns = columns;
            animTexture.rows = rows;

            _animTextures.Add(name, animTexture);
        }

        public AnimatedTexture GetAnimatedTexture(string name)
        {
            return _animTextures[name];
        }

        public AnimatedSprite GetAnimatedSprite(string upTexture, string downTexture, string rightTexture, string leftTexture)
        {
            AnimatedTexture up = _animTextures[upTexture];
            AnimatedTexture down = _animTextures[downTexture];
            AnimatedTexture right = _animTextures[rightTexture];
            AnimatedTexture left = _animTextures[leftTexture];

            return new AnimatedSprite(up, down, right, left);

        }

        public bool LoadSoundFromFile(string name, string path)
        {
            SoundBuffer _soundBuffer = new SoundBuffer(path);
            Sound s = new Sound(_soundBuffer);
            _sounds.Add(name, s);

            return true;

        }

        public bool LoadFontFromFile(string name, string path)
        {
            Font font = new Font(path);
            _fonts.Add(name, font);

            return true;

        }


        public Sound GetSound(string name)
        {
            return _sounds[name];
        }

        public Font GetFont(string name)
        {
            return _fonts[name];
        }


    }
}
