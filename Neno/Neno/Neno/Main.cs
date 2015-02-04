using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Neno
{
    public enum Focus
    {
        //Focus of game
        Menu
    }

    public class Main : Microsoft.Xna.Framework.Game
    {


        #region Variables

        //Main
        public GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        public static Focus focus = Focus.Menu;
        public static Menu GameMenu;

        #endregion


        #region Methods

        public void Switch(Focus newFocus)
        {
            //Switches between game areas
            if (newFocus == focus) return;

            //End previous focus
            switch(focus)
            {
                case Focus.Menu:
                    GameMenu.end();
                    GameMenu = null;
                    break;
            }

            //Change focus
            focus = newFocus;

            //Start new focus
            switch(newFocus)
            {
                case Focus.Menu:
                    GameMenu = new Menu();
                    GameMenu.init();
                    break;
            }
        }

        #endregion


        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {


            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);


        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) this.Exit();


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);



            base.Draw(gameTime);
        }
    }
}
