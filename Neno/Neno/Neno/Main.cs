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
        Menu, Client, Server
    }

    public class Main : Microsoft.Xna.Framework.Game
    {


        #region Variables

        //Main
        public static Main self;
        public GraphicsDeviceManager graphics;
        public static SpriteBatch sb;
        public static Focus focus = Focus.Menu;
        public static Menu GameMenu;
        public static GameClient Client;
        public static GameServer Server;

        //Drawing
        public static SpriteFont font;
        public static Texture2D pix;

        //Mouse
        MouseState mouse;
        MouseState mousealt;
        public static Vector2 mousePos = Vector2.Zero;
        public static bool mouseLeftPressed = false;
        public static bool mouseRightPressed = false;

        #endregion


        #region Methods

        public static void Switch(Focus newFocus)
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
                case Focus.Client:
                    Client.end();
                    Client = null;
                    break;
                case Focus.Server:
                    Client.end();
                    Client = null;
                    Server.end();
                    Server = null;
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
                case Focus.Client:
                    Client = new GameClient();
                    Client.init();
                    break;
                case Focus.Server:
                    Server = new GameServer();
                    Server.init();
                    Client = new GameClient();
                    Client.init();
                    break;
            }
        }
        public static SoundEffect sound(string name)
        {
            return self.Content.Load<SoundEffect>(name);
        }
        public static Texture2D img(string name)
        {
            return self.Content.Load<Texture2D>(name);
        }

        #endregion



        

        public Main()
        {
            self = this;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.IsMouseVisible = true;
            this.Window.AllowUserResizing = true;
            this.Window.Title = "Neno";
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            sb = new SpriteBatch(GraphicsDevice);

            //Font
            font = Content.Load<SpriteFont>("font1");
            pix = Content.Load<Texture2D>("pix");

            GameMenu = new Menu();
            GameMenu.init();
        }

        protected override void UnloadContent()
        {
            Content.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            //Global
            Timer.Update();
            Key.update();

            //Mouse
            mousealt = mouse;
            mouse = Mouse.GetState();
            if (mouse.LeftButton == ButtonState.Pressed && mousealt.LeftButton == ButtonState.Released)
                mouseLeftPressed = true;
            else
                mouseLeftPressed = false;
            if (mouse.RightButton == ButtonState.Pressed && mousealt.RightButton == ButtonState.Released)
                mouseRightPressed = true;
            else
                mouseRightPressed = false;
            mousePos = new Vector2(mouse.X, mouse.Y);

            //Step
            switch (focus) 
            { 
                case Focus.Menu:
                    GameMenu.step();
                    break;
                case Focus.Client:
                    Client.step();
                    break;
                case Focus.Server:
                    if (Server != null)
                    Server.step();
                    Client.step();
                    break;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            //Step
            switch (focus)
            {
                case Focus.Menu:
                    GameMenu.draw();
                    break;
                case Focus.Client:
                    Client.draw();
                    break;
                case Focus.Server:
                    Client.draw();
                    Server.draw();
                    break;
            }

            base.Draw(gameTime);
        }
    }
}
