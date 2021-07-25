namespace SoarCraft.QYun.UnityABStudio.Helpers {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Numerics;
    using AssetReader.Entities.Enums;
    using AssetReader.Unity3D.Objects.Sprites;
    using AssetReader.Unity3D.Objects.Texture2Ds;
    using AssetReader.Utils;
    using Core.Entities;
    using Extensions;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Drawing;
    using SixLabors.ImageSharp.Drawing.Processing;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using Vector2 = AssetReader.Math.Vector2;

    public static class SpriteHelper {
        public static MemoryStream GetImage(this Sprite m_Sprite, ImageFormat imageFormat) {
            var image = GetImage(m_Sprite);
            if (image != null) {
                using (image) {
                    return image.ConvertToStream(imageFormat);
                }
            }
            return null;
        }

        public static Image GetImage(this Sprite m_Sprite) {
            if (m_Sprite.m_SpriteAtlas != null && m_Sprite.m_SpriteAtlas.TryGet(out var m_SpriteAtlas)) {
                if (m_SpriteAtlas.m_RenderDataMap.TryGetValue(m_Sprite.m_RenderDataKey, out var spriteAtlasData) && spriteAtlasData.texture.TryGet(out var m_Texture2D)) {
                    return CutImage(m_Texture2D, m_Sprite, spriteAtlasData.textureRect, spriteAtlasData.textureRectOffset, spriteAtlasData.settingsRaw);
                }
            } else {
                if (m_Sprite.m_RD.texture.TryGet(out var m_Texture2D)) {
                    return CutImage(m_Texture2D, m_Sprite, m_Sprite.m_RD.textureRect, m_Sprite.m_RD.textureRectOffset, m_Sprite.m_RD.settingsRaw);
                }
            }
            return null;
        }

        private static Image CutImage(Texture2D m_Texture2D, Sprite m_Sprite, Rectf textureRect, Vector2 textureRectOffset, SpriteSettings settingsRaw) {
            var originalImage = m_Texture2D.ConvertToImage(false);
            if (originalImage != null) {
                using (originalImage) {
                    var rectX = (int)Math.Floor(textureRect.x);
                    var rectY = (int)Math.Floor(textureRect.y);
                    var rectRight = (int)Math.Ceiling(textureRect.x + textureRect.width);
                    var rectBottom = (int)Math.Ceiling(textureRect.y + textureRect.height);
                    rectRight = Math.Min(rectRight, m_Texture2D.m_Width);
                    rectBottom = Math.Min(rectBottom, m_Texture2D.m_Height);
                    var rect = new Rectangle(rectX, rectY, rectRight - rectX, rectBottom - rectY);
                    var spriteImage = originalImage.Clone(x => x.Crop(rect));
                    if (settingsRaw.packed == 1) {
                        //RotateAndFlip
                        switch (settingsRaw.packingRotation) {
                            case SpritePackingRotation.kSPRFlipHorizontal:
                                spriteImage.Mutate(x => x.Flip(FlipMode.Horizontal));
                                break;
                            case SpritePackingRotation.kSPRFlipVertical:
                                spriteImage.Mutate(x => x.Flip(FlipMode.Vertical));
                                break;
                            case SpritePackingRotation.kSPRRotate180:
                                spriteImage.Mutate(x => x.Rotate(180));
                                break;
                            case SpritePackingRotation.kSPRRotate90:
                                spriteImage.Mutate(x => x.Rotate(270));
                                break;
                        }
                    }

                    //Tight
                    if (settingsRaw.packingMode == SpritePackingMode.kSPMTight) {
                        try {
                            var triangles = GetTriangles(m_Sprite.m_RD);
                            var polygons = triangles.Select(x => new Polygon(new LinearLineSegment(x.Select(y => new PointF(y.X, y.Y)).ToArray()))).ToArray();
                            IPathCollection path = new PathCollection(polygons);
                            var matrix = Matrix3x2.CreateScale(m_Sprite.m_PixelsToUnits);
                            var version = m_Sprite.version;
                            if (version[0] < 5
                               || (version[0] == 5 && version[1] < 4)
                               || (version[0] == 5 && version[1] == 4 && version[2] <= 1)) //5.4.1p3 down
                            {
                                matrix *= Matrix3x2.CreateTranslation(m_Sprite.m_Rect.width * 0.5f - textureRectOffset.X, m_Sprite.m_Rect.height * 0.5f - textureRectOffset.Y);
                            } else {
                                matrix *= Matrix3x2.CreateTranslation(m_Sprite.m_Rect.width * m_Sprite.m_Pivot.X - textureRectOffset.X, m_Sprite.m_Rect.height * m_Sprite.m_Pivot.Y - textureRectOffset.Y);
                            }
                            path = path.Transform(matrix);
                            var options = new DrawingOptions {
                                GraphicsOptions = new GraphicsOptions {
                                    AlphaCompositionMode = PixelAlphaCompositionMode.DestOut
                                }
                            };
                            var rectP = new RectangularPolygon(0, 0, rect.Width, rect.Height);
                            spriteImage.Mutate(x => x.Fill(options, Color.Red, rectP.Clip(path)));
                            spriteImage.Mutate(x => x.Flip(FlipMode.Vertical));
                            return spriteImage;
                        } catch {
                            // ignored
                        }
                    }

                    //Rectangle
                    spriteImage.Mutate(x => x.Flip(FlipMode.Vertical));
                    return spriteImage;
                }
            }

            return null;
        }

        private static Vector2[][] GetTriangles(SpriteRenderData m_RD) {
            if (m_RD.vertices != null) //5.6 down
            {
                var vertices = m_RD.vertices.Select(x => (Vector2)x.pos).ToArray();
                var triangleCount = m_RD.indices.Length / 3;
                var triangles = new Vector2[triangleCount][];
                for (var i = 0; i < triangleCount; i++) {
                    var first = m_RD.indices[i * 3];
                    var second = m_RD.indices[i * 3 + 1];
                    var third = m_RD.indices[i * 3 + 2];
                    var triangle = new[] { vertices[first], vertices[second], vertices[third] };
                    triangles[i] = triangle;
                }
                return triangles;
            } else {//5.6 and up
                var triangles = new List<Vector2[]>();
                var m_VertexData = m_RD.m_VertexData;
                var m_Channel = m_VertexData.m_Channels[0]; //kShaderChannelVertex
                var m_Stream = m_VertexData.m_Streams[m_Channel.stream];
                using (var vertexReader = new UnityReader(new MemoryStream(m_VertexData.m_DataSize), false)) {
                    using (var indexReader = new UnityReader(new MemoryStream(m_RD.m_IndexBuffer), false)) {
                        foreach (var subMesh in m_RD.m_SubMeshes) {
                            vertexReader.BaseStream.Position = m_Stream.offset + subMesh.firstVertex * m_Stream.stride + m_Channel.offset;

                            var vertices = new Vector2[subMesh.vertexCount];
                            for (var v = 0; v < subMesh.vertexCount; v++) {
                                vertices[v] = vertexReader.ReadVector3();
                                vertexReader.BaseStream.Position += m_Stream.stride - 12;
                            }

                            indexReader.BaseStream.Position = subMesh.firstByte;

                            var triangleCount = subMesh.indexCount / 3u;
                            for (var i = 0; i < triangleCount; i++) {
                                var first = indexReader.ReadUInt16() - subMesh.firstVertex;
                                var second = indexReader.ReadUInt16() - subMesh.firstVertex;
                                var third = indexReader.ReadUInt16() - subMesh.firstVertex;
                                var triangle = new[] { vertices[first], vertices[second], vertices[third] };
                                triangles.Add(triangle);
                            }
                        }
                    }
                }
                return triangles.ToArray();
            }
        }
    }
}
