using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Tennis01.Objects
{
    class Object2D
    {
        Texture2D image;
        Vector2 position;
        public Object2D(string textureName,Vector2 position)
        {
            image = GameMain.Textures[textureName];
            this.position = position;
        }
        public bool Intersects(Object2D obj)
        {
            if (position.X >= obj.position.X + obj.image.Width && position.X + image.Width >= obj.position.X &&
               position.Y >= obj.position.Y + obj.image.Height && position.Y + image.Height >= obj.position.Y)
                return true;
            return false;
        }
        public virtual void Update()
        {
            
        }
    }
}
