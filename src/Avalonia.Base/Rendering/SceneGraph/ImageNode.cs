﻿using Avalonia.Platform;
using Avalonia.Utilities;

namespace Avalonia.Rendering.SceneGraph
{
    /// <summary>
    /// A node in the scene graph which represents an image draw.
    /// </summary>
    internal class ImageNode : DrawOperationWithTransform
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageNode"/> class.
        /// </summary>
        /// <param name="transform">The transform.</param>
        /// <param name="source">The image to draw.</param>
        /// <param name="opacity">The draw opacity.</param>
        /// <param name="sourceRect">The source rect.</param>
        /// <param name="destRect">The destination rect.</param>
        public ImageNode(Matrix transform, IRef<IBitmapImpl> source, double opacity, Rect sourceRect, Rect destRect)
            : base(destRect, transform)
        {
            Source = source.Clone();
            Opacity = opacity;
            SourceRect = sourceRect;
            DestRect = destRect;
            SourceVersion = Source.Item.Version;
        }

        /// <summary>
        /// Gets the image to draw.
        /// </summary>
        public IRef<IBitmapImpl> Source { get; }

        /// <summary>
        /// Source bitmap Version
        /// </summary>
        public int SourceVersion { get; }

        /// <summary>
        /// Gets the draw opacity.
        /// </summary>
        public double Opacity { get; }

        /// <summary>
        /// Gets the source rect.
        /// </summary>
        public Rect SourceRect { get; }

        /// <summary>
        /// Gets the destination rect.
        /// </summary>
        public Rect DestRect { get; }
        
        /// <summary>
        /// Determines if this draw operation equals another.
        /// </summary>
        /// <param name="transform">The transform of the other draw operation.</param>
        /// <param name="source">The image of the other draw operation.</param>
        /// <param name="opacity">The opacity of the other draw operation.</param>
        /// <param name="sourceRect">The source rect of the other draw operation.</param>
        /// <param name="destRect">The dest rect of the other draw operation.</param>
        /// <returns>True if the draw operations are the same, otherwise false.</returns>
        /// <remarks>
        /// The properties of the other draw operation are passed in as arguments to prevent
        /// allocation of a not-yet-constructed draw operation object.
        /// </remarks>
        public bool Equals(Matrix transform, IRef<IBitmapImpl> source, double opacity, Rect sourceRect, Rect destRect)
        {
            return transform == Transform &&
                   Equals(source.Item, Source.Item) &&
                   source.Item.Version == SourceVersion &&
                   opacity == Opacity &&
                   sourceRect == SourceRect &&
                   destRect == DestRect;
        }

        /// <inheritdoc/>
        public override void Render(IDrawingContextImpl context)
        {
            context.DrawBitmap(Source, Opacity, SourceRect, DestRect);
        }

        /// <inheritdoc/>
        public override bool HitTestTransformed(Point p) => DestRect.ContainsExclusive(p);

        public override void Dispose()
        {
            Source?.Dispose();
        }
    }
}
