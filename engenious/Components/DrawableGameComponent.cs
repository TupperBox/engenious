﻿using System;
using engenious.Graphics;

namespace engenious
{
    public abstract class DrawableGameComponent : GameComponent, IDrawable
    {
        public DrawableGameComponent(Game game)
            : base(game)
        {
            this.GraphicsDevice = game.GraphicsDevice;
            Visible = true;
        }

        public GraphicsDevice GraphicsDevice{ get; private set; }

        #region IDrawable implementation

        public virtual void Draw(GameTime gameTime)
        {
        }

        public bool Visible
        {
            get;
            set;
        }

        public int DrawOrder
        {
            get;
            set;
        }


        #endregion

        protected override void LoadContent()
        {
           
        }

        protected override void UnloadContent()
        {

        }
    }
}

