using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace PacMan
{
    public enum EntityState
    {
        Scatter,
        Chase,
        Frightened,
        Eaten,
        GhostPoints200,
        GhostPoints400,
        GhostPoints800,
        GhostPoints1600
    }

    public class Entity
    {
        public int X;
        public int Y;

        public Direction currentDirection = Direction.RIGHT;
        public Direction selectedDirection = Direction.RIGHT;
        public bool IsAtIntersection = false;
        public int currentSpriteNumber = 0;
        public bool IsPlayerEntity = false;
        public bool Visible = true;
        public float Speed = 5;
        public int DotCounter = 0;

        public EntityState State = EntityState.Scatter;

        //Bad
        public Direction prevDirection = Direction.RIGHT;



        private int startX;
        private int startY;
        private Direction startDirection = Direction.RIGHT;

        public string Name;

        public Entity(string name, int StartingX, int StartingY, Direction StartDirection)
        {
            Name = name;

            startX = StartingX;
            startY = StartingY;

            X = StartingX;
            Y = StartingY;

            startDirection = StartDirection;
            currentDirection = startDirection;
            selectedDirection = startDirection;
        }

        public void ResetToStartingPosition()
        {
            X = startX;
            Y = startY;
            currentDirection = startDirection;
            selectedDirection = startDirection;

        }

        public void ReverseDirection()
        {
            if (this.currentDirection == Direction.UP) this.currentDirection = Direction.DOWN;
            else if (this.currentDirection == Direction.DOWN) this.currentDirection = Direction.UP;
            else if (this.currentDirection == Direction.RIGHT) this.currentDirection = Direction.LEFT;
            else if (this.currentDirection == Direction.LEFT) this.currentDirection = Direction.RIGHT;
        }

    }
}
