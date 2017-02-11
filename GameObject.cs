using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.Window;
using SFML.Graphics;
using SFML.Audio;

namespace PacMan
{
    public class GameObject
    {
        private RenderWindow _window;

        public RenderWindow Window { get { return this._window; } }

        public GameObject()
        {
            // Initialize values
            _window = new RenderWindow(new VideoMode(1024u, 768u), "PacMan");

            _window.SetVisible(true);
            _window.SetVerticalSyncEnabled(true);

            // Set up event handlers
            _window.Closed += _window_Closed;
            _window.KeyPressed += _window_KeyPressed;
        }

        void _window_KeyPressed(object sender, KeyEventArgs e)
        {
            SceneManager.Instance.CurrentScene.HandleInput(e);
        }

        void _window_Closed(object sender, EventArgs e)
        {
            _window.Close();
        }

        private void Initialize()
        {
            ResourceManager.Instance.LoadTextureFromFile("start", @"resources\logo.png");
            ResourceManager.Instance.LoadTextureFromFile("main", @"resources\back.png");
            ResourceManager.Instance.LoadTextureFromFile("ghostpoints", @"resources\ghostpoints.png");

            ResourceManager.Instance.LoadSoundFromFile("begin", @"resources\pacman_beginning.wav");
            ResourceManager.Instance.LoadSoundFromFile("eatdot", @"resources\dot.wav");
            ResourceManager.Instance.LoadSoundFromFile("eatenergizer", @"resources\energizer.wav");
            ResourceManager.Instance.LoadSoundFromFile("siren", @"resources\siren.wav");
            ResourceManager.Instance.LoadSoundFromFile("pacdie", @"resources\pacman_death.wav");
            ResourceManager.Instance.LoadSoundFromFile("eatghost", @"resources\pacman_eatghost.wav");
            ResourceManager.Instance.LoadSoundFromFile("extrapac", @"resources\pacman_extrapac.wav");

            ResourceManager.Instance.LoadFontFromFile("arial", @"resources\arial.ttf");
        }

        public void Run()
        {
            this.Initialize();
                        
            // Build the startup menu scene
            StartScene s = new StartScene(this);
            s.Name = "start";
            s.BackgroundTexture = ResourceManager.Instance.GetTexture("start");
            SceneManager.Instance.AddScene(s);

            // Build the main game scene
            MainScene d = new MainScene(this);
            d.Name = "main";
            d.BackgroundTexture = ResourceManager.Instance.GetTexture("main");
            SceneManager.Instance.AddScene(d);

            // Start the game
            SceneManager.Instance.GotoScene("start");
        }
    }
}
