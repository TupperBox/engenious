﻿using System;
using System.Collections.Generic;
using OpenTK;
using System.Linq;
using OpenTK.Graphics.OpenGL4;

namespace engenious.Graphics
{
    internal class SpriteBatcher: IDisposable
    {
		
        public class BatchItem
        {
            internal Vector2 texTopLeft;
            internal Vector2 texBottomRight;
            internal Vector3[] positions;
            internal Color color;
            internal Texture2D texture;
            internal float sortingKey;

            private void InitBatchItem(Vector2 position, Color color, float rotation, Vector2 origin, Vector2 size, SpriteBatch.SpriteEffects effects, float layerDepth, SpriteBatch.SpriteSortMode sortMode, Vector4 tempText)
            {

                if ((effects & SpriteBatch.SpriteEffects.FlipVertically) != 0)
                {
                    texTopLeft.X = tempText.X + tempText.Z;
                    texBottomRight.X = tempText.X;
                }
                else
                {
                    texTopLeft.X = tempText.X;
                    texBottomRight.X = tempText.X + tempText.Z;
                } 
                if ((effects & SpriteBatch.SpriteEffects.FlipHorizontally) != 0)
                {
                    texTopLeft.Y = tempText.Y + tempText.W;
                    texBottomRight.Y = tempText.Y;
                }
                else
                {
                    texTopLeft.Y = tempText.Y;
                    texBottomRight.Y = tempText.Y + tempText.W;
                }
                    
                positions = new Vector3[4];
                positions[0] = new Vector3(-origin.X, -origin.Y, layerDepth);
                positions[1] = new Vector3(-origin.X + size.X, -origin.Y, layerDepth);
                positions[2] = new Vector3(-origin.X, -origin.Y + size.Y, layerDepth);
                positions[3] = new Vector3(-origin.X + size.X, -origin.Y + size.Y, layerDepth);

                if (rotation != 0.0f || ((rotation = rotation % (float)(Math.PI * 2)) != 0.0f))
                {
                    float cosR = (float)Math.Cos(rotation);//TODO: correct rotation
                    float sinR = (float)Math.Sin(rotation);
                    for (int i = 0; i < positions.Length; i++)
                    {
                        positions[i] = new Vector3(positions[i].X * cosR - positions[i].Y * sinR, positions[i].Y * cosR + positions[i].X * sinR, positions[i].Z);
                    }
                }
                for (int i = 0; i < positions.Length; i++)
                {
                    positions[i] += new Vector3(origin.X + position.X, origin.Y + position.Y, 0.0f);
                }
                this.color = color;

                switch (sortMode)
                {
                    case SpriteBatch.SpriteSortMode.BackToFront:
                        this.sortingKey = -layerDepth;
                        break;
                    case SpriteBatch.SpriteSortMode.FrontToBack:
                        this.sortingKey = layerDepth;
                        break;
                    case SpriteBatch.SpriteSortMode.Texture:
                        this.sortingKey = texture.GetHashCode();
                        break;
                }
            }

            public BatchItem(Texture2D texture, Vector2 position, Nullable<RectangleF> sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 size, SpriteBatch.SpriteEffects effects, float layerDepth, SpriteBatch.SpriteSortMode sortMode)
            {
                this.texture = texture;
                Vector4 tempText;
                if (sourceRectangle.HasValue)
                    tempText = new Vector4(sourceRectangle.Value.X, sourceRectangle.Value.Y, sourceRectangle.Value.Width, sourceRectangle.Value.Height);
                else
                    tempText = new Vector4(0, 0, 1, 1);

                InitBatchItem(position, color, rotation, origin, new Vector2(size.X * tempText.Z, size.Y * tempText.W), effects, layerDepth, sortMode, tempText);
            }

            public BatchItem(Texture2D texture, Vector2 position, Nullable<Rectangle> sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 size, SpriteBatch.SpriteEffects effects, float layerDepth, SpriteBatch.SpriteSortMode sortMode)
            {
                this.texture = texture;
                Vector4 tempText;
                if (sourceRectangle.HasValue)
                    tempText = new Vector4((float)sourceRectangle.Value.X / (float)texture.Width, sourceRectangle.Value.Y / texture.Height, (float)sourceRectangle.Value.Width / texture.Width, (float)sourceRectangle.Value.Height / texture.Height);
                else
                    tempText = new Vector4(0, 0, 1, 1);
                InitBatchItem(position, color, rotation, origin, new Vector2(size.X, size.Y), effects, layerDepth, sortMode, tempText);
               
            }
        }

        private DynamicVertexBuffer vertexBuffer;
        private VertexPositionColorTexture[] vertexData = new VertexPositionColorTexture[MAX_BATCH * 4];
        private DynamicIndexBuffer indexBuffer;
        private ushort[] indexData = new ushort[MAX_BATCH * 6];
        private const int MAX_BATCH = 256;
        private SpriteBatch.SpriteSortMode sortMode;
        private GraphicsDevice graphicsDevice;

        public SpriteBatcher(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            Batches = new List<BatchItem>(MAX_BATCH);
            vertexBuffer = new DynamicVertexBuffer(graphicsDevice, VertexPositionColorTexture.VertexDeclaration, 4 * MAX_BATCH);
            indexBuffer = new DynamicIndexBuffer(graphicsDevice, DrawElementsType.UnsignedShort, 6 * MAX_BATCH);
        }

        public void Begin(SpriteBatch.SpriteSortMode sortMode)
        {
            Batches.Clear();
            this.sortMode = sortMode;
        }

        internal SamplerState samplerState;

        public void Flush(Effect effect, Texture texture, int batch, int batchCount)
        {
            if (batchCount == 0)
                return;


            //graphicsDevice.DrawPrimitives(OpenTK.Graphics.OpenGL4.PrimitiveType.Qu
            vertexBuffer.SetData<VertexPositionColorTexture>(VertexPositionColorTexture.VertexDeclaration.VertexStride * batch * 4, vertexData, 0, batchCount * 4, VertexPositionColorTexture.VertexDeclaration.VertexStride);
            indexBuffer.SetData<ushort>(sizeof(short) * 6 * batch, indexData, 0, batchCount * 6);


            graphicsDevice.VertexBuffer = vertexBuffer;
            graphicsDevice.IndexBuffer = indexBuffer;
            graphicsDevice.Textures[0] = texture;
            //graphicsDevice.SamplerStates[0] = samplerState;
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.Triangles, 0, 0, batchCount * 4, 0, batchCount * 2);
            }

            graphicsDevice.IndexBuffer = null;
            graphicsDevice.VertexBuffer = null;
        }

        public void End(Effect effect)
        {
            if (Batches.Count == 0)
                return;



            if (sortMode != SpriteBatch.SpriteSortMode.Immediate && sortMode != SpriteBatch.SpriteSortMode.Deffered)
            {
                Batches.Sort((x, y) =>
                    {
                        int first = y.sortingKey.CompareTo(x.sortingKey);
                        return first;
                    });
            }


            //Array.Sort (Batches, 0, Batches.Count);
            Texture currentTexture = Batches.First().texture;
            int batchStart = 0, batchCount = 0;

            ushort bufferIndex = 0;
            ushort indicesIndex = 0;
            foreach (BatchItem item in Batches)
            {
                if (item.texture != currentTexture || batchCount == MAX_BATCH)
                {
                    Flush(effect, currentTexture, batchStart, batchCount);
                    currentTexture = item.texture;
                    batchStart = batchCount = 0;
                    indicesIndex = bufferIndex = 0;
                }

                indexData[indicesIndex++] = (ushort)(bufferIndex + 0);
                indexData[indicesIndex++] = (ushort)(bufferIndex + 1);
                indexData[indicesIndex++] = (ushort)(bufferIndex + 2);

                indexData[indicesIndex++] = (ushort)(bufferIndex + 1);
                indexData[indicesIndex++] = (ushort)(bufferIndex + 3);
                indexData[indicesIndex++] = (ushort)(bufferIndex + 2);

                vertexData[bufferIndex++] = new VertexPositionColorTexture(item.positions[0], item.color, item.texTopLeft);
                vertexData[bufferIndex++] = new VertexPositionColorTexture(item.positions[1], item.color, new Vector2(item.texBottomRight.X, item.texTopLeft.Y));
                vertexData[bufferIndex++] = new VertexPositionColorTexture(item.positions[2], item.color, new Vector2(item.texTopLeft.X, item.texBottomRight.Y));
                vertexData[bufferIndex++] = new VertexPositionColorTexture(item.positions[3], item.color, item.texBottomRight);
                batchCount++;
            }
            if (batchCount > 0)
                Flush(effect, currentTexture, batchStart, batchCount);
        }

        public List<BatchItem> Batches{ get; private set; }

        public void Dispose()
        {
            vertexBuffer.Dispose();
            indexBuffer.Dispose();
        }
    }
}

