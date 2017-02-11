using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.Audio;
using SFML.Graphics;
using SFML.Window;
using System.Diagnostics;

namespace PacMan
{
    public class MainScene : Scene
    {   
        object[] powerSprites = new object[3];

        int powerSpriteId = 0;
        int entityStateCounter = 0;
        int ghostEatCounter = 0;
        bool reset = true;
        bool gotExtraLife = false;
        bool playerPause = false;

        Sprite spDot;
        Sprite spPower;
        Sprite spReady;
        Sprite spGhostPoints;
        Sprite spLivesRemaining;

        List<Entity> entities = new List<Entity>();
        Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        static Random rnd = new Random();

        System.Timers.Timer stateTimer = new System.Timers.Timer();
        System.Timers.Timer frightenedTimer = new System.Timers.Timer();

        const int TILEHEIGHT = 20;
        const int TILEWIDTH = 20;
        const int SCREENOFFSET = 10;

        Text txtScore;
        int score = 0;
        int playerLives = 3;

        Sound startSound, chompSound, energizerSound, sirenSound, playerDeathSound, eatGhostSound, extraPacSound;

        Map map = new Map();

        // Override Methods for the Scene

        public MainScene(GameObject gameObject) : base(gameObject)
        {

        }

        public override void Initialize()
        {
            // Set up score text
            txtScore = new Text("Score: 0", ResourceManager.Instance.GetFont("arial"));
            txtScore.Position = new Vector2f(0, 0);
            txtScore.CharacterSize = 20;

            // Set up player sprite
            Texture texture = new Texture(@"resources\pacman-sprites.png");
            Sprite sp = new Sprite(texture, new IntRect(0, 0, 40, 40));
            RegisterEntity("pacman", 26, 14, Direction.LEFT, true, texture, sp);

            // And lives remaining...
            spLivesRemaining = new Sprite(texture);
            spLivesRemaining.TextureRect = new IntRect(2 * 40 + 160, 0, 40, 40);

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
            RegisterEntity("orange", 17, 15, Direction.UP, false, texture, sp);

            // Set up ready sprite
            texture = new Texture(@"resources\ready.png");
            spReady = new Sprite(texture);
            spReady.Position = new Vector2f(240, 410);

            frightenedTimer.Elapsed += frightenedTimer_Elapsed;

            texture = new Texture(@"resources\dot.png");
            spDot = new Sprite(texture);

            texture = new Texture(@"resources\energizer.png");
            spPower = new Sprite(texture);
            spPower.TextureRect = new IntRect(0, 0, 20, 20);


            startSound = ResourceManager.Instance.GetSound("begin");
            chompSound = ResourceManager.Instance.GetSound("eatdot");
            energizerSound = ResourceManager.Instance.GetSound("eatenergizer");
            sirenSound = ResourceManager.Instance.GetSound("siren");
            playerDeathSound = ResourceManager.Instance.GetSound("pacdie");
            eatGhostSound = ResourceManager.Instance.GetSound("eatghost");
            extraPacSound = ResourceManager.Instance.GetSound("extrapac");

            base.Initialize();
        }

        public override void Reset()
        {
            map = new Map();
            playerLives = 3;
            score = 0;
            stateTimer.Enabled = false;
            frightenedTimer.Enabled = false;
            gotExtraLife = false;
            playerPause = false;
            reset = true;
            entityStateCounter = 0;

            foreach (Entity e in entities)
            {
                e.ResetToStartingPosition();
                this.UpdateSpritePosition(e);
                e.Visible = false;
                if (e.Name == "blue") e.DotCounter = 30;
                if (e.Name == "orange") e.DotCounter = 60;
            }

            startSound.Play();
            this.Pause(5000);
        }

        public override void HandleInput(KeyEventArgs e)
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

            if (e.Code == Keyboard.Key.P)
                playerPause = (playerPause ? false : true);

            if (e.Code == Keyboard.Key.Escape)
                SceneManager.Instance.GotoScene("start");
        }

        public override void Update()
        {
            if (!playerPause)
            {
                if (playerLives == 0)
                    SceneManager.Instance.GotoScene("start");
                else
                {
                    UpdateScore();

                    foreach (Entity e in entities)
                    {
                        MoveEntity(e);

                        if (e.IsAtIntersection)
                            UpdateDirection(e);

                        AnimateEntity(e);
                        CollisionCheck(e);
                    }

                    AnimatePowerPellets();

                    CheckForEndOfLevel();

                    if (!reset && sirenSound.Status != SoundStatus.Playing)
                        sirenSound.Play();
                }
            }
        }

        public override void Draw()
        {
            DrawMap();

            txtScore.Draw(_gameObject.Window, RenderStates.Default);

            // Draw each entity
            foreach(Entity e in entities)
                if (e.Visible)
                    sprites[e.Name].Draw(_gameObject.Window, RenderStates.Default);


            // Draw lives remaining
            int lifePosition = 10;
            for (int x = playerLives-1; x > 0; x--)
            {
                spLivesRemaining.Position = new Vector2f(lifePosition, 700);
                lifePosition += 50;
                spLivesRemaining.Draw(_gameObject.Window, RenderStates.Default);
            }

        }

        public override void AfterPause()
        {
            foreach (Entity e in entities)
            {
                // We added a pause whenever a ghost is eaten so that
                // the points could be displayed.  Its a crude solution, but works
                // Anyway, after the pause ends, if any ghost is in a points state, 
                // change them to an eaten state   
                // Ghosts and Pacman are temporarily hidden when the points are displayed also
                
                if (!e.Visible)
                    e.Visible = true;

                if (e.State == EntityState.GhostPoints200 ||
                    e.State == EntityState.GhostPoints400 ||
                    e.State == EntityState.GhostPoints800 ||
                    e.State == EntityState.GhostPoints1600)
                    e.State = EntityState.Eaten;

            }

            reset = false;
        }

        public override void OnPause()
        {
            if (reset)
            {
                // A pause is called so that the READY! message is displayed when a player starts
                spReady.Draw(_gameObject.Window, RenderStates.Default);
            }

            // A pause is called when PacMan eats a ghost, so that the points are displayed
            foreach (Entity e in entities)
            {
                if (e.State == EntityState.GhostPoints200)
                {
                    spGhostPoints = new Sprite();
                    spGhostPoints.Texture = ResourceManager.Instance.GetTexture("ghostpoints");
                    spGhostPoints.TextureRect = new IntRect(0, 0, 80, 20);
                    spGhostPoints.Position = sprites[e.Name].Position;
                    spGhostPoints.Draw(_gameObject.Window, RenderStates.Default);
                }

                if (e.State == EntityState.GhostPoints400)
                {
                    spGhostPoints = new Sprite();
                    spGhostPoints.Texture = ResourceManager.Instance.GetTexture("ghostpoints");
                    spGhostPoints.TextureRect = new IntRect(81, 0, 80, 20);
                    spGhostPoints.Position = sprites[e.Name].Position;
                    spGhostPoints.Draw(_gameObject.Window, RenderStates.Default);
                }

                if (e.State == EntityState.GhostPoints800)
                {
                    spGhostPoints = new Sprite();
                    spGhostPoints.Texture = ResourceManager.Instance.GetTexture("ghostpoints");
                    spGhostPoints.TextureRect = new IntRect(161, 0, 80, 20);
                    spGhostPoints.Position = sprites[e.Name].Position;
                    spGhostPoints.Draw(_gameObject.Window, RenderStates.Default);
                }

                if (e.State == EntityState.GhostPoints1600)
                {
                    spGhostPoints = new Sprite();
                    spGhostPoints.Texture = ResourceManager.Instance.GetTexture("ghostpoints");
                    spGhostPoints.TextureRect = new IntRect(241, 0, 80, 20);
                    spGhostPoints.Position = sprites[e.Name].Position;
                    spGhostPoints.Draw(_gameObject.Window, RenderStates.Default);
                }
            }

        }

        // Private Methods

        private void RegisterEntity(string id, int startX, int startY, Direction startDirection, bool IsPlayerEntity, Texture texture, Sprite sprite)
        {
            Entity e = new Entity(id, startX, startY, startDirection);
            e.IsPlayerEntity = IsPlayerEntity;

            if (e.Name == "red" || e.Name == "pink") e.DotCounter = 0;
            if (e.Name == "blue") e.DotCounter = 30;
            if (e.Name == "orange") e.DotCounter = 60;

            entities.Add(e);
            sprites.Add(e.Name, sprite);

            UpdateSpritePosition(e);
        }
        
        private void MoveEntity(Entity e)
        {
            int angle = 0;

            if (e.currentDirection == Direction.RIGHT) angle = 0;
            if (e.currentDirection == Direction.DOWN) angle = 90;
            if (e.currentDirection == Direction.LEFT) angle = 180;
            if (e.currentDirection == Direction.UP) angle = 270;

            var scalex = Math.Round(Math.Cos(angle * (Math.PI / 180.0)));
            var scaley = Math.Round(Math.Sin(angle * (Math.PI / 180.0)));

            var velocityx = (float)(e.Speed * scalex);
            var velocityy = (float)(e.Speed * scaley);

            // Update the characters sprite position
            Sprite sp = sprites[e.Name];
            Vector2f v = new Vector2f(sp.Position.X + velocityx, sp.Position.Y + velocityy);
            sp.Position = v;

            // Get the center screen position of the character
            var eCenterX = sp.Position.X + sp.TextureRect.Width / 2;
            var eCenterY = sp.Position.Y + sp.TextureRect.Height / 2;

            // Determine which tile the center of the character is in
            var tileY = Math.Floor((eCenterY-SCREENOFFSET) / TILEHEIGHT);
            var tileX = Math.Floor((eCenterX-SCREENOFFSET) / TILEWIDTH);

            // Determine the center screen position of the tile
            var tileXpos = TILEWIDTH  * Math.Floor(tileX+1);
            var tileYpos = TILEHEIGHT * Math.Floor(tileY+1);

            // Update the entity's tile number
            e.X = (int)tileY;
            e.Y = (int)tileX;

            // This flag determines if we can actually change directions now
            if (eCenterX == tileXpos && eCenterY == tileYpos)
                e.IsAtIntersection = true;
            else
                e.IsAtIntersection = false;
        }
        
        private void UpdateDirection(Entity e)
        {
            List<Direction> possibleDirections = map.PossibleEntityDirections(e);

            // Perform AI if Ghost
            if (!e.IsPlayerEntity)
                e.selectedDirection = EntityAI(e, possibleDirections);

            if (possibleDirections.Contains(e.selectedDirection))
            {
                e.prevDirection = e.currentDirection;
                e.currentDirection = e.selectedDirection;

                if (e.State == EntityState.Frightened)
                    e.Speed = 2.5F; // 1, 2, 2.5, 4, 5 (evenly divisible by tile size - 20)
                else
                    e.Speed = 5;
            }

            if (!possibleDirections.Contains(e.currentDirection))
                e.Speed = 0;

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

        private void CollisionCheck(Entity e)
        {
            if (e.IsPlayerEntity)
            {
                // If we have eaten a dot, add to the score and play the chomp sound
                if (map.GetTileType(e.X, e.Y) == Map.TileType.DOT)
                {
                    score = score + 10;
                    map.ClearTile(e.X, e.Y);

                    if (chompSound.Status != SoundStatus.Playing)
                        chompSound.Play();

                    Entity eBlue = entities.Find(x => x.Name == "blue");
                    Entity eOrange = entities.Find(x => x.Name == "orange");

                    // Dot counters are used to determine when some ghosts can leave the pen
                    if (eBlue.DotCounter > 0)
                    {
                        eBlue.DotCounter--;
                    }
                    else if (eBlue.DotCounter == 0 && eOrange.DotCounter > 0)
                    {
                        eOrange.DotCounter--;
                    }

                }

                if (map.GetTileType(e.X, e.Y) == Map.TileType.POWERPELLET)
                {
                    score = score + 50;
                    map.ClearTile(e.X, e.Y);

                    foreach (Entity entity in entities)
                        if (!entity.IsPlayerEntity && entity.State != EntityState.Eaten)
                        {
                            entity.State = EntityState.Frightened;
                            entity.ReverseDirection();
                        }

                    stateTimer.Enabled = false;
                    frightenedTimer.Interval = 5000;
                    frightenedTimer.Start();

                    // This flag tells us how many ghosts have been eaten during this
                    // instance of the power pellet / energizer
                    // Used to determine score
                    ghostEatCounter = 0;
                }
            }


            if (!e.IsPlayerEntity)
            {
                Entity player = entities.Find(x => x.Name == "pacman");

                if (player.X == e.X && player.Y == e.Y)
                {
                    switch (e.State)
                    {
                        case EntityState.Frightened:
                            {
                                stateTimer.Enabled = false;
                                
                                switch (ghostEatCounter)
                                {
                                    case 0: 
                                        e.State = EntityState.GhostPoints200;
                                        score += 200;
                                        break;
                                    case 1: 
                                        e.State = EntityState.GhostPoints400;
                                        score += 400;
                                        break;
                                    case 2: 
                                        e.State = EntityState.GhostPoints800;
                                        score += 800;
                                        break;
                                    case 3: 
                                        e.State = EntityState.GhostPoints1600;
                                        score += 1600;
                                        break;
                                }

                                ghostEatCounter++;
                                if (ghostEatCounter > 3) ghostEatCounter = 3;
                                                                
                                e.Visible = false;
                                player.Visible = false;

                                eatGhostSound.Play();

                                this.Pause(500);
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
                                    UpdateSpritePosition(z);
                                }

                                playerLives--;
                                reset = true;

                                this.Pause(3000);
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
            sp.Position = new Vector2f(TILEWIDTH * e.Y, TILEHEIGHT * e.X);
        }

        private Direction EntityAI(Entity e, List<Direction> possibleDirections)
        {
            // Simple AI.  Much more needs to be done here.  Each ghost has a different
            // method of finding PacMan.  This code basically just does a simple pathfind

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

            if (!gotExtraLife && score > 9999)
            {
                gotExtraLife = true;
                playerLives++;

                extraPacSound.Play();
            }
            
        }

        private void AnimateEntity(Entity e)
        {
            // Selects the sprite for the entity depending on the state

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
                if (e.State == EntityState.Frightened)
                {
                    if (energizerSound.Status != SoundStatus.Playing)
                        energizerSound.Play();

                    sp.TextureRect = new IntRect(320, 0, 40, 40);
                }
                else
                {
                    e.currentSpriteNumber = (e.State == EntityState.Eaten ? 4 : 0);

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
            }

        }

        private void AnimatePowerPellets()
        {
            spPower.TextureRect = new IntRect(powerSpriteId * 21, 0, 20, 20);
            powerSpriteId++;
            if (powerSpriteId == 4) powerSpriteId = 0;

        }

        private void DrawMap()
        {
            // Draw the dots
            for (int x = 0; x < 33; x++)
            {
                for (int y = 0; y < 28; y++)
                {
                    if (map.GetTileType(x, y) == Map.TileType.DOT)
                    {
                        spDot.Position = new Vector2f((TILEHEIGHT * y) + SCREENOFFSET, (TILEWIDTH * x) + SCREENOFFSET);
                        spDot.Draw(_gameObject.Window, RenderStates.Default);
                    }
                    if (map.GetTileType(x, y) == Map.TileType.POWERPELLET)
                    {
                        spPower.Position = new Vector2f((TILEHEIGHT * y) + SCREENOFFSET, (TILEWIDTH * x) + SCREENOFFSET);
                        spPower.Draw(_gameObject.Window, RenderStates.Default);
                    }

                }
            }

        }

        // Timers

        void frightenedTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            frightenedTimer.Enabled = false;

            foreach (Entity entity in entities)
                if (!entity.IsPlayerEntity && entity.State == EntityState.Frightened)
                    entity.State = EntityState.Chase;

            stateTimer.Enabled = true;
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
                stateTimer.Enabled = true;
                
            }

            if (entityStateCounter == 2)
            {
                newState = EntityState.Scatter;
                stateTimer.Interval = 7000;
                stateTimer.Enabled = true;
            }

            if (entityStateCounter == 3)
            {
                newState = EntityState.Chase;
                stateTimer.Interval = 20000;
                stateTimer.Enabled = true;
            }

            if (entityStateCounter == 4)
            {
                newState = EntityState.Scatter;
                stateTimer.Interval = 5000;
                stateTimer.Enabled = true;
            }

            if (entityStateCounter > 4)
            {
                newState = EntityState.Chase;
                stateTimer.Enabled = false;
                stateTimer.Enabled = true;
            }
            foreach (Entity e in entities)
            {
                e.State = newState;
            }
        }




    }
}
