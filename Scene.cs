using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.Graphics;
using SFML.Window;
using System.Diagnostics;

namespace PacMan
{
    public class Scene
    {
        public string Name;
        public Texture BackgroundTexture;

        DateTime currentTime = System.DateTime.Now;
        DateTime targetTime = System.DateTime.Now;
        private bool pause = false;
        private int pauseSeconds = 0;

        protected GameObject _gameObject;
        protected Sprite BackSprite;

        public Scene(GameObject gameObject)
        {
            _gameObject = gameObject;
        }

        public virtual void Initialize()
        {
            // This method is called when the Scene object is created

            if (this.BackgroundTexture == null)
                BackSprite = new Sprite();
            else
                BackSprite = new Sprite(this.BackgroundTexture);
        }

        public virtual void Reset()
        {
            // This method is called when the Scene is reset
        }

        public void Run()
        {
            // This is the main loop for the scene

            Stopwatch timer = Stopwatch.StartNew();
            TimeSpan dt = TimeSpan.FromSeconds(3);
            TimeSpan elapsedTime = TimeSpan.Zero;


            timer.Restart();
            elapsedTime = timer.Elapsed;

            while (_gameObject.Window.IsOpen())
            {
                currentTime = System.DateTime.Now;

                if (elapsedTime >= dt)
                {

                    _gameObject.Window.Clear(Color.Black);

                    this.DrawBackground();

                    if (!pause)
                    {
                        this.Update();
                    }
                    else
                    {
                        if (currentTime > targetTime)
                        {
                            pause = false;
                            this.AfterPause();
                        }
                        else
                            this.OnPause();
                    }
                    
                    this.Draw();

                    _gameObject.Window.Display();

                    this.AfterDraw();


                    timer.Restart();
                    elapsedTime = timer.Elapsed;

                }
                else
                {
                    _gameObject.Window.DispatchEvents();
                    elapsedTime += TimeSpan.FromSeconds(1.0 / 1000.0); // dt;
                }
            }
        }

        public virtual void HandleInput(KeyEventArgs e)
        {
            // This is the input handler for the scene
        }

        public virtual void DrawBackground()
        {
            BackSprite.Draw(_gameObject.Window, RenderStates.Default);
        }

        public virtual void Update()
        {
            // This is the update method for the scene.  It will call entity update methods
        }

        public virtual void Draw()
        {
            // This is the draw method for the scene. It will call entity draw methods
        }

        public virtual void AfterDraw()
        {
            // This method is called after the window drawing is complete
        }

        public void Pause(int milliseconds)
        {
            targetTime = currentTime.AddMilliseconds(milliseconds);
            pause = true;
        }

        public virtual void OnPause()
        {
            // This method is called each frame during the pause
        }

        public virtual void AfterPause()
        {
        }
    }
}
