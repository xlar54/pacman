using SFML;
using SFML.Audio;
using SFML.Graphics;
using SFML.Window;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

/// 
/// Pacman clone
/// Written By Scott Hutter
/// September 2014
/// 
/// Demonstration of a game written in C# and SFML
/// 

namespace PacMan
{
    public enum Direction
    {
        UP,
        DOWN,
        RIGHT,
        LEFT
    }

    internal sealed class PacGame
    {
        RenderWindow _window;

        object[] powerSprites = new object[3];

        int powerSpriteId = 0;
        int entityStateCounter = 0;

        Sprite spBack;
        Sprite spDot;
        Sprite spPower;
        Sprite spStart;

        List<Entity> entities = new List<Entity>();
        Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        static Random rnd = new Random();

        System.Timers.Timer stateTimer = new System.Timers.Timer();
        System.Timers.Timer frightenedTimer = new System.Timers.Timer();

        const int TILEHEIGHT = 20;
        const int TILEWIDTH = 20;
        const int SCREENOFFSET = 10;

        Font arial = new Font(@"resources\arial.ttf");
        Text txtScore;
        int score = 0;
        int playerLives = 3;

        SoundBuffer startBuffer, chompBuffer, energizerBuffer, sirenBuffer, playerDeathBuffer;
        Sound startSound, chompSound, energizerSound, sirenSound, playerDeathSound;

        Map map = new Map();

        public PacGame()
        {
            Initialize();
            LoadResources();
        }

        public void Initialize()
        {
            // Initialize values
            _window = new RenderWindow(new VideoMode(1024u, 768u), "PacMan");

            _window.SetVisible(true);
            _window.SetVerticalSyncEnabled(true);

            // Set up event handlers
            _window.Closed += new EventHandler(OnClosed);
            _window.KeyPressed += _window_KeyPressed;

            // Set up score text
            txtScore = new Text("Score: 0", arial);
            txtScore.Position = new Vector2f(0, 0);
            txtScore.CharacterSize = 20;

            // Set up player sprite
            Texture texture = new Texture(@"resources\pacman-sprites.png");
            Sprite sp = new Sprite(texture, new IntRect(0, 0, 40, 40));
            RegisterEntity("pacman", 26, 14, Direction.LEFT, true, texture, sp);

            // Set up a ghost
            texture = new Texture(@"resources\ghost-sprites-red.png");
            sp = new Sprite(texture, new IntRect(0, 0, 40, 40));
            RegisterEntity("red", 14, 13, Direction.RIGHT, false, texture, sp);

            // Set up a ghost
            texture = new Texture(@"resources\ghost-sprites-blue.png");
            sp = new Sprite(texture, new IntRect(0, 0, 40, 40));
            RegisterEntity("blue", 17, 12, Direction.UP, false, texture, sp);

            // Set up a ghost
            texture = new Texture(@"resources\ghost-sprites-pink.png");
            sp = new Sprite(texture, new IntRect(0, 0, 40, 40));
            RegisterEntity("pink", 17, 14, Direction.UP, false, texture, sp);

            // Set up a ghost
            texture = new Texture(@"resources\ghost-sprites-orange.png");
            sp = new Sprite(texture, new IntRect(0, 0, 40, 40));
            RegisterEntity("green", 17, 15, Direction.UP, false, texture, sp);

            frightenedTimer.Elapsed += frightenedTimer_Elapsed;
        }

        private void LoadResources()
        {

            // Set up start screen graphics
            Texture txStart = new Texture(@"resources\logo.png");
            spStart = new Sprite(txStart);

            // Main game screen background
            Texture txBack = new Texture(@"resources\back.png");
            spBack = new Sprite(txBack);

            Texture txDot = new Texture(@"resources\dot.png");
            spDot = new Sprite(txDot);

            powerSprites[0] = new Texture(@"resources\power-1.png");
            powerSprites[1] = new Texture(@"resources\power-2.png");
            powerSprites[2] = new Texture(@"resources\power-3.png");

            spPower = new Sprite((Texture)powerSprites[powerSpriteId]);

            startBuffer = new SFML.Audio.SoundBuffer(@"resources\pacman_beginning.wav");
            startSound = new SFML.Audio.Sound(startBuffer);
            
            chompBuffer = new SFML.Audio.SoundBuffer(@"resources\dot.wav");
            chompSound = new SFML.Audio.Sound(chompBuffer);

            energizerBuffer = new SFML.Audio.SoundBuffer(@"resources\energizer.wav");
            energizerSound = new SFML.Audio.Sound(energizerBuffer);

            sirenBuffer = new SFML.Audio.SoundBuffer(@"resources\siren.wav");
            sirenSound = new SFML.Audio.Sound(sirenBuffer);

            playerDeathBuffer = new SFML.Audio.SoundBuffer(@"resources\pacman_death.wav");
            playerDeathSound = new SFML.Audio.Sound(playerDeathBuffer);
        }

        private void RegisterEntity(string id, int startX, int startY, Direction startDirection, bool IsPlayerEntity, Texture texture, Sprite sprite)
        {
            Entity e = new Entity(id, startX, startY, startDirection);
            e.IsPlayerEntity = IsPlayerEntity;

            entities.Add(e);
            sprites.Add(e.Name, sprite);

            UpdateSpritePosition(e);
        }

        public void Run()
        {
            // Main game loop routine

            stateTimer.Interval = 7000;
            stateTimer.Elapsed += stateTimer_Elapsed;
            stateTimer.Enabled = true;

            Stopwatch timer = Stopwatch.StartNew();
            TimeSpan dt = TimeSpan.FromSeconds(3);
            TimeSpan elapsedTime = TimeSpan.Zero;

            timer.Restart();
            elapsedTime = timer.Elapsed;

            while (_window.IsOpen())
            {
                startSound.Play();

                while (playerLives > 0)
                {
                    if (elapsedTime >= dt)
                    {
                        if (sirenSound.Status != SoundStatus.Playing && startSound.Status != SoundStatus.Playing)
                            sirenSound.Play();

                        DrawMap();
                        UpdateScore();

                        foreach (Entity e in entities)
                        {
                            MoveEntity(e);
                            AnimateEntity(e);
                            CheckEntityForCollision(e);
                        }

                        foreach (KeyValuePair<string, Sprite> entry in sprites)
                            entry.Value.Draw(_window, RenderStates.Default);

                        AnimatePowerPellets();

                        CheckForEndOfLevel();

                        _window.Display();

                        while (startSound.Status == SFML.Audio.SoundStatus.Playing) { };

                        timer.Restart();
                        elapsedTime = timer.Elapsed;

                    }
                    else
                    {
                        _window.DispatchEvents();
                        elapsedTime += TimeSpan.FromSeconds(1.0 / 1000.0); // dt;
                    }
                }
                
                break;
            }

            
        }

        private void MoveEntity(Entity e)
        {

            List<Direction> possibleDirections = map.PossibleEntityDirections(e);

            // Perform AI if Ghost
            if (!e.IsPlayerEntity && e.IsAtIntersection)
                e.selectedDirection = EntityAI(e, possibleDirections);

            if (e.IsAtIntersection && possibleDirections.Contains(e.selectedDirection))
            {
                e.prevDirection = e.currentDirection;
                e.currentDirection = e.selectedDirection;
            }

            if (possibleDirections.Contains(e.currentDirection))
                UpdateEntityCoords(e);

            // Check for tunnels
            if (map.GetTileType(e.X, e.Y + 1) == Map.TileType.RIGHTTUNNEL && e.currentDirection == Direction.RIGHT)
            {
                e.Y = 2;
                UpdateSpritePosition(e);
            }

            if (map.GetTileType(e.X, e.Y - 1) == Map.TileType.LEFTTUNNEL && e.currentDirection == Direction.LEFT)
            {
                e.Y = 26;
                UpdateSpritePosition(e);
            }

            

        }

        private void CheckForEndOfLevel()
        {
            // Check for end
            // TODO: Add an actual end / restart
            if (map.GetDotCount() == 0)
            {
                map.Reset();

                foreach (Entity z in entities)
                {
                    z.ResetToStartingPosition();
                    UpdateSpritePosition(z);
                }

                System.Threading.Thread.Sleep(2000);
            }
        }

        private void CheckEntityForCollision(Entity e)
        {
            // Check for collisions
            if (!e.IsPlayerEntity)
            {
                Entity player = entities.Find(x => x.Name == "pacman");

                if (player.X == e.X && player.Y == e.Y)
                {
                    switch (e.State)
                    {
                        case EntityState.Frightened:
                            {
                                e.State = EntityState.Eaten;
                                stateTimer.Enabled = false;
                                break;
                            }
                        case EntityState.Chase:
                        case EntityState.Scatter:
                            {
                                playerDeathSound.Play();

                                while (playerDeathSound.Status == SoundStatus.Playing) { }

                                foreach (Entity z in entities)
                                {
                                    z.ResetToStartingPosition();
                                    //z.State = EntityState.Scatter;
                                    UpdateSpritePosition(z);
                                }

                                playerLives--;

                                System.Threading.Thread.Sleep(2000);
                                break;
                            }
                        default:
                            break;
                    }
                }
            }
        }

        private int GetManhattanDistance(int x1, int y1, int x2, int y2)
        {
            int d = Math.Abs(x1 - x2) + Math.Abs(y1 - y2);

            return d;
        }

        private void UpdateSpritePosition(Entity e)
        {
            Sprite sp = sprites[e.Name];
            sp.Position = new Vector2f(20 * e.Y, 20 * e.X);
        }

        private Direction EntityAI(Entity e, List<Direction> possibleDirections)
        {
            Direction d = Direction.RIGHT;

            Entity player = entities.Find(x => x.Name == "pacman");

            // Avoids ghost constantly reversing directions
            if (e.currentDirection == Direction.UP && possibleDirections.Contains(Direction.DOWN) && possibleDirections.Contains(Direction.UP))
                possibleDirections.Remove(Direction.DOWN);

            if (e.currentDirection == Direction.DOWN && possibleDirections.Contains(Direction.DOWN) && possibleDirections.Contains(Direction.UP))
                possibleDirections.Remove(Direction.UP);

            if (e.currentDirection == Direction.RIGHT && possibleDirections.Contains(Direction.RIGHT) && possibleDirections.Contains(Direction.LEFT))
                possibleDirections.Remove(Direction.LEFT);

            if (e.currentDirection == Direction.LEFT && possibleDirections.Contains(Direction.RIGHT) && possibleDirections.Contains(Direction.LEFT))
                possibleDirections.Remove(Direction.RIGHT);

            // Dont let ghosts go down into the ghost house if not eaten
            if (e.State != EntityState.Eaten && map.IsEntityIsAboveGate(e))
                possibleDirections.Remove(Direction.DOWN);


            int shortestDistance = 1000;

            int targetX = 0;
            int targetY = 0;


            // If ghosts are in the ghost house, special logic applies. We need them to exit
            if (map.GetTileType(e.X, e.Y) == Map.TileType.GHOSTONLY)
            {
                targetX = 14;
                targetY = 14;

                if (e.State == EntityState.Eaten)
                    e.State = EntityState.Chase;
            }
            else
            {
                bool retreatMode = (e.State == EntityState.Scatter || e.State == EntityState.Frightened);

                if (retreatMode && e.Name == "red")
                {
                    targetX = 0;
                    targetY = 26;
                }
                if (retreatMode && e.Name == "blue")
                {
                    targetX = 33;
                    targetY = 27;
                }
                if (retreatMode && e.Name == "green")
                {
                    targetX = 33;
                    targetY = 0;
                }
                if (retreatMode && e.Name == "pink")
                {
                    targetX = 0;
                    targetY = 2;
                }
                if (e.State == EntityState.Eaten)
                {
                    targetX = 14;
                    targetY = 14;
                }
                else if (e.State == EntityState.Chase)
                {
                    targetX = player.X;
                    targetY = player.Y;
                }
            }

            if (possibleDirections.Contains(Direction.UP) && e.prevDirection != Direction.DOWN)
            {
                int z = GetManhattanDistance(targetX, targetY, e.X - 1, e.Y);
                if (z < shortestDistance)
                {
                    shortestDistance = z;
                    d = Direction.UP;
                }
            }

            if (possibleDirections.Contains(Direction.DOWN) && e.prevDirection != Direction.UP)
            {
                int z = GetManhattanDistance(targetX, targetY, e.X + 1, e.Y);
                if (z < shortestDistance) 
                {
                    shortestDistance = z;
                    d = Direction.DOWN;
                }
            }

            if (possibleDirections.Contains(Direction.LEFT) && e.prevDirection != Direction.RIGHT)
            {
                int z = GetManhattanDistance(targetX, targetY, e.X, e.Y - 1);
                if (z < shortestDistance) 
                {
                    shortestDistance = z;
                    d = Direction.LEFT;
                }
            }

            if (possibleDirections.Contains(Direction.RIGHT) && e.prevDirection != Direction.LEFT)
            {
                int z = GetManhattanDistance(targetX, targetY, e.X, e.Y + 1);
                if (z < shortestDistance) 
                {
                    shortestDistance = z;
                    d = Direction.RIGHT;
                }
            }

            return d;
        }

        private void UpdateScore()
        {
            Entity player = entities.Find(x => x.Name == "pacman");
            string text = "Score: " + score.ToString("000") + " "; // +" - X: " + player.X.ToString("0#") + " Y: " + player.Y.ToString("0#");

            txtScore.DisplayedString = text; 
            txtScore.Draw(_window, RenderStates.Default);
        }

        private void AnimateEntity(Entity e)
        {
            // Locate the sprite
            Sprite sp = sprites[e.Name];

            if (e.IsPlayerEntity)
            {
                switch (e.currentDirection)
                {
                    case Direction.RIGHT:
                        sp.TextureRect = new IntRect(e.currentSpriteNumber * 40, 0, 40, 40);
                        e.currentSpriteNumber++;
                        if (e.currentSpriteNumber == 4) e.currentSpriteNumber = 0;
                        break;
                    case Direction.LEFT:
                        sp.TextureRect = new IntRect(e.currentSpriteNumber * 40 + 160, 0, 40, 40);
                        e.currentSpriteNumber++;
                        if (e.currentSpriteNumber == 4) e.currentSpriteNumber = 0;
                        break;
                    case Direction.UP:
                        sp.TextureRect = new IntRect(e.currentSpriteNumber * 40 + 320, 0, 40, 40);
                        e.currentSpriteNumber++;
                        if (e.currentSpriteNumber == 4) e.currentSpriteNumber = 0;
                        break;
                    case Direction.DOWN:
                        sp.TextureRect = new IntRect(e.currentSpriteNumber * 40 + 480, 0, 40, 40);
                        e.currentSpriteNumber++;
                        if (e.currentSpriteNumber == 4) e.currentSpriteNumber = 0;
                        break;

                }
            }
            else
            {
                if (e.State != EntityState.Frightened)
                {
                    if (e.State == EntityState.Eaten)
                        e.currentSpriteNumber = 4;
                    else
                        e.currentSpriteNumber = 0;


                    switch (e.currentDirection)
                    {
                        case Direction.RIGHT:
                            e.currentSpriteNumber += 0;
                            break;
                        case Direction.LEFT:
                            e.currentSpriteNumber += 1;
                            break;
                        case Direction.UP:
                            e.currentSpriteNumber += 2;
                            break;
                        case Direction.DOWN:
                            e.currentSpriteNumber += 3;
                            break;
                    }

                    
                    sp.TextureRect = new IntRect(e.currentSpriteNumber * 40, 0, 40, 40);

                }
                else
                {
                    if (energizerSound.Status != SoundStatus.Playing)
                        energizerSound.Play();

                    sp.TextureRect = new IntRect(320, 0, 40, 40);
                }
            }

        }

        private void AnimatePowerPellets()
        {
            spPower.Texture = (Texture)powerSprites[powerSpriteId];
            powerSpriteId++;
            if (powerSpriteId == 3) powerSpriteId = 0;

        }

        void UpdateEntityCoords(Entity e)
        {
            int mx = 0;
            int my = 0;
            int speed = 5;

            //if (e.State == EntityState.Frightened)
            //    speed = 1;


            // Pixels to move the character
            if (e.currentDirection == Direction.RIGHT) mx = +speed;
            if (e.currentDirection == Direction.LEFT) mx = -speed;
            if (e.currentDirection == Direction.UP) my = -speed;
            if (e.currentDirection == Direction.DOWN) my = +speed;

            // Locate the sprite
            Sprite sp = sprites[e.Name];

            Vector2f v = new Vector2f(sp.Position.X, sp.Position.Y);
            v.X += mx;
            v.Y += my;

            // Update its position
            sp.Position = v;


            // if the character has moved directly into a tile
            // (Movement happens anyway in order to have a smooth transition from one tile to another)
            if (Math.Round(sp.Position.Y / TILEWIDTH) == (sp.Position.Y / TILEWIDTH) &&
                Math.Round(sp.Position.X / TILEHEIGHT) == (sp.Position.X / TILEHEIGHT))
            {
                // Update the player's tile number
                e.X = (int)(sp.Position.Y / TILEWIDTH);
                e.Y = (int)(sp.Position.X / TILEHEIGHT);

                if (e.IsPlayerEntity)
                {
                    // If we have eaten a dot, add to the score and play the chomp sound
                    if (map.GetTileType(e.X, e.Y) == Map.TileType.DOT)
                    {
                        score = score + 10;

                        if (chompSound.Status != SoundStatus.Playing)
                            chompSound.Play();
                    }

                    if (map.GetTileType(e.X, e.Y) == Map.TileType.POWERPELLET)
                    {
                        score = score + 50;

                        foreach (Entity entity in entities)
                            if (!entity.IsPlayerEntity)
                                entity.State = EntityState.Frightened;

                        stateTimer.Enabled = false;
                        frightenedTimer.Interval = 5000;
                        frightenedTimer.Start();
                    }

                    // Set the space we are on to empty (eaten)
                    if (map.GetTileType(e.X, e.Y) == Map.TileType.DOT || map.GetTileType(e.X, e.Y) == Map.TileType.POWERPELLET)
                        map.ClearTile(e.X, e.Y);
                }

                // We can safely process directional changes now
                e.IsAtIntersection = true;


            }
            else
                e.IsAtIntersection = false;


        }

        void frightenedTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            stateTimer.Enabled = true;

            foreach (Entity entity in entities)
                if (!entity.IsPlayerEntity && entity.State != EntityState.Eaten)
                    entity.State = EntityState.Chase;

            stateTimer.Enabled = true;

            frightenedTimer.Enabled = false;
        }

        void stateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs el)
        {
            EntityState newState = EntityState.Scatter;
            entityStateCounter++;

            // Switch states for ghosts 
            if (entityStateCounter == 1)
            {
                newState = EntityState.Chase;
                stateTimer.Interval = 20000;
                //stateTimer.Enabled = true;
                
            }

            if (entityStateCounter == 2)
            {
                newState = EntityState.Scatter;
                stateTimer.Interval = 7000;
                //stateTimer.Enabled = true;
            }

            if (entityStateCounter == 3)
            {
                newState = EntityState.Chase;
                stateTimer.Interval = 20000;
                //stateTimer.Enabled = true;
            }

            if (entityStateCounter == 4)
            {
                newState = EntityState.Scatter;
                stateTimer.Interval = 5000;
                //stateTimer.Enabled = true;
            }

            if (entityStateCounter > 4)
            {
                newState = EntityState.Chase;
                stateTimer.Enabled = false;
                //stateTimer.Enabled = true;
            }
            foreach (Entity e in entities)
            {
                e.State = newState;
            }
        }

        void _window_KeyPressed(object sender, KeyEventArgs e)
        {

            Entity player = entities.Find(x => x.Name == "pacman");

            if (e.Code == Keyboard.Key.Left)
                player.selectedDirection = Direction.LEFT;

            if (e.Code == Keyboard.Key.Right)
                player.selectedDirection = Direction.RIGHT;

            if (e.Code == Keyboard.Key.Up)
                player.selectedDirection = Direction.UP;

            if (e.Code == Keyboard.Key.Down)
                player.selectedDirection = Direction.DOWN;
        }

        void OnClosed(object sender, EventArgs e)
        {
            _window.Close();
        }

        void DrawMap()
        {
            _window.Clear(Color.Black);

            // Draw the background
            spBack.Position = new Vector2f(SCREENOFFSET, SCREENOFFSET);
            spBack.Draw(_window, RenderStates.Default);

            // Draw the dots
            for (int x = 0; x < 33; x++)
            {
                for (int y = 0; y < 28; y++)
                {
                    if (map.GetTileType(x, y) == Map.TileType.DOT)
                    {
                        spDot.Position = new Vector2f((TILEHEIGHT * y) + SCREENOFFSET, (TILEWIDTH * x) + SCREENOFFSET);
                        spDot.Draw(_window, RenderStates.Default);
                    }
                    if (map.GetTileType(x, y) == Map.TileType.POWERPELLET)
                    {
                        spPower.Position = new Vector2f((TILEHEIGHT * y) + SCREENOFFSET, (TILEWIDTH * x) + SCREENOFFSET);
                        spPower.Draw(_window, RenderStates.Default);
                    }

                }
            }

        }


    }
}
