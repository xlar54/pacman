using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.Graphics;
using SFML.Window;

namespace PacMan
{
    public class StartScene :Scene
    {
        public StartScene(GameObject gameObject) : base(gameObject)
        {
        }


        public override void HandleInput(KeyEventArgs e)
        {
            if (e.Code == Keyboard.Key.Space)
            {
                SceneManager.Instance.StartScene("main");
            }

            if (e.Code == Keyboard.Key.Escape)
                this._gameObject.Window.Close();

            base.HandleInput(e);
        }

    }
}
