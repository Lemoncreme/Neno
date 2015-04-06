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
        public static double Time = 0;
        public static Random R = new Random();

        //Drawing
        public static SpriteFont font;
        public static SpriteFont consoleFont;
        public static Texture2D pix;
        public static int windowWidth = 0;
        public static int windowHeight = 0;
        public static string[] wordTileImg = new string[28];

        //Mouse
        MouseState mouse;
        MouseState mousealt;
        public static Vector2 mousePos = Vector2.Zero;
        public static Point mousePosP = Point.Zero;
        public static bool mouseLeftPressed = false;
        public static bool mouseRightPressed = false;
        public static bool mouseLeftDown = false;
        public static bool mouseRightDown = false;
        public static bool mouseScrollUp = false;
        public static bool mouseScrollDown = false;
        int scroll = 0;


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
                    Client = new GameClient();
                    Client.init();
                    Server = new GameServer();
                    Server.init();
                    break;
            }
        }
        public static SoundEffect sound(string name)
        {
            return self.Content.Load<SoundEffect>("Sounds/" + name);
        }
        public static Texture2D img(string name)
        {
            return self.Content.Load<Texture2D>("Textures/" + name);
        }
        public static Song music(string name)
        {
            return self.Content.Load<Song>("Music/" + name);
        }
        public static void drawText(SpriteFont font, string Text, Vector2 pos, Color color, float scale, TextOrient orientation)
        {
            if (orientation == TextOrient.Middle)
                pos.X -= scale * font.MeasureString(Text).X / 2;
            else
            if (orientation == TextOrient.Right)
                pos.X -= scale * font.MeasureString(Text).X;

            Main.sb.DrawString(font, Text, pos, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
        }
        public static T choose<T>(List<T> list)
        {
            return list[rInt(0, list.Count - 1)];
        }
        public static int rInt(int min, int max)
        {
            return R.Next(min, max + 1);
        }
        public static bool chance(int num)
        {
            return (R.Next(num) == 0);
        }

        #region Drawing

        public static void DrawRectangle(int x, int y, int width, int height, Color color, bool outline)
        {
            if (!outline)
            {
                //Full
                Main.sb.Draw(Main.pix, new Rectangle(x, y, width, height), color);
            }
            else
            {
                //Top
                Main.sb.Draw(Main.pix, new Rectangle(x, y, width, 1), Color.White);
                //Left
                Main.sb.Draw(Main.pix, new Rectangle(x, y, 1, height), Color.White);
                //Right
                Main.sb.Draw(Main.pix, new Rectangle(x + width, y, 1, height), Color.White);
                //Bottom
                Main.sb.Draw(Main.pix, new Rectangle(x, y + height, width, 1), Color.White);
            }
        }
        public static void DrawRectangle(Rectangle rect, Color color, bool outline)
        {
            int x = rect.X;
            int y = rect.Y;
            int width = rect.Width;
            int height = rect.Height;

            if (!outline)
            {
                //Full
                Main.sb.Draw(Main.pix, new Rectangle(x, y, width, height), color);
            }
            else
            {
                //Top
                Main.sb.Draw(Main.pix, new Rectangle(x, y, width, 1), Color.White);
                //Left
                Main.sb.Draw(Main.pix, new Rectangle(x, y, 1, height), Color.White);
                //Right
                Main.sb.Draw(Main.pix, new Rectangle(x + width, y, 1, height), Color.White);
                //Bottom
                Main.sb.Draw(Main.pix, new Rectangle(x, y + height, width, 1), Color.White);
            }
        }
        public static void DrawRectangle(Rectangle rect, Color color)
        {
            //Full
            Main.sb.Draw(Main.pix, rect, color);
        }
        public static void DrawLine(SpriteBatch spriteBatch, Vector2 begin, Vector2 end, Color color, int width = 1)
        {
            Rectangle r = new Rectangle((int)begin.X, (int)begin.Y, (int)(end - begin).Length() + width, width);
            Vector2 v = Vector2.Normalize(begin - end);
            float angle = (float)Math.Acos(Vector2.Dot(v, -Vector2.UnitX));
            if (begin.Y > end.Y) angle = MathHelper.TwoPi - angle;
            spriteBatch.Draw(Main.pix, r, null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
        }
        #endregion

        #endregion





        public Main()
        {
            self = this;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.IsMouseVisible = true;
            this.Window.AllowUserResizing = true;
            this.Window.Title = "Neno";
            MediaPlayer.IsMuted = true;
        }

        protected override void Initialize()
        {
            windowWidth = Window.ClientBounds.Width;
            windowHeight = Window.ClientBounds.Height;
            MediaPlayer.Play(Main.music("findingNeno"));
            MediaPlayer.IsRepeating = true;
            //MediaPlayer.IsMuted = true;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            sb = new SpriteBatch(GraphicsDevice);

            //Font
            font = Content.Load<SpriteFont>("font1");
            consoleFont = Content.Load<SpriteFont>("font2");
            pix = Main.img("pix");

            //Word Tile Images
            wordTileImg[1] = "Tiles/tile_a";
            wordTileImg[2] = "Tiles/tile_b";
            wordTileImg[3] = "Tiles/tile_c";
            wordTileImg[4] = "Tiles/tile_d";
            wordTileImg[5] = "Tiles/tile_e";
            wordTileImg[6] = "Tiles/tile_f";
            wordTileImg[7] = "Tiles/tile_g";
            wordTileImg[8] = "Tiles/tile_h";
            wordTileImg[9] = "Tiles/tile_i";
            wordTileImg[10] = "Tiles/tile_j";
            wordTileImg[11] = "Tiles/tile_k";
            wordTileImg[12] = "Tiles/tile_l";
            wordTileImg[13] = "Tiles/tile_m";
            wordTileImg[14] = "Tiles/tile_n";
            wordTileImg[15] = "Tiles/tile_o";
            wordTileImg[16] = "Tiles/tile_p";
            wordTileImg[17] = "Tiles/tile_q";
            wordTileImg[18] = "Tiles/tile_r";
            wordTileImg[19] = "Tiles/tile_s";
            wordTileImg[20] = "Tiles/tile_t";
            wordTileImg[21] = "Tiles/tile_u";
            wordTileImg[22] = "Tiles/tile_v";
            wordTileImg[23] = "Tiles/tile_w";
            wordTileImg[24] = "Tiles/tile_x";
            wordTileImg[25] = "Tiles/tile_y";
            wordTileImg[26] = "Tiles/tile_z";
            wordTileImg[27] = "Tiles/tile_wild";

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
            if (self.IsActive) Key.update();
            Time += 1;
            if (Time > int.MaxValue - 8)
                Time = 0;
            windowWidth = Window.ClientBounds.Width;
            windowHeight = Window.ClientBounds.Height;

            //Mouse
            if (self.IsActive)
            {
                //Get Mouse state
                mousealt = mouse;
                mouse = Mouse.GetState();

                //Buttons held
                if (mouse.LeftButton == ButtonState.Pressed)
                    mouseLeftDown = true;
                else 
                    mouseLeftDown = false;
                if (mouse.RightButton == ButtonState.Pressed)
                    mouseRightDown = true;
                else
                    mouseRightDown = false;

                //Buttons pressed
                if (mouse.LeftButton == ButtonState.Pressed && mousealt.LeftButton == ButtonState.Released)
                    mouseLeftPressed = true;
                else
                    mouseLeftPressed = false;
                if (mouse.RightButton == ButtonState.Pressed && mousealt.RightButton == ButtonState.Released)
                    mouseRightPressed = true;
                else
                    mouseRightPressed = false;

                //Mouse position
                mousePos = new Vector2(mouse.X, mouse.Y);
                mousePosP = new Point(mouse.X, mouse.Y);

                //Scrolling
                var newScroll = mouse.ScrollWheelValue;
                mouseScrollUp = false;
                mouseScrollDown = false;
                if (scroll > newScroll)
                    mouseScrollDown = true;
                if (scroll < newScroll)
                    mouseScrollUp = true;
                scroll = mouse.ScrollWheelValue;
            }
            else
            {
                mouseLeftPressed = false;
                mouseRightPressed = false;
                mouseScrollUp = false;
                mouseScrollDown = false;
            }

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
