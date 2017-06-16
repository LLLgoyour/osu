﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;

namespace osu.Game.Graphics.UserInterface
{
    public class LineGraph : Container
    {
        /// <summary>
        /// Manually set the max value, otherwise <see cref="Enumerable.Max(IEnumerable{float})"/> will be used.
        /// </summary>
        public float? MaxValue { get; set; }

        /// <summary>
        /// Manually set the min value, otherwise <see cref="Enumerable.Min(IEnumerable{float})"/> will be used.
        /// </summary>
        public float? MinValue { get; set; }

        public float ActualMaxValue { get; private set; } = float.NaN;
        public float ActualMinValue { get; private set; } = float.NaN;

        private const double transform_duration = 500;

        /// <summary>
        /// Hold an empty area if values are less.
        /// </summary>
        public int DefaultValueCount;

        private readonly Container<Path> maskingContainer;
        private readonly Path path;

        private float[] values;

        /// <summary>
        /// A list of floats decides position of each line node.
        /// </summary>
        public IEnumerable<float> Values
        {
            get { return values; }
            set
            {
                values = value.ToArray();
                applyPath();
                maskingContainer.Width = 0;
                maskingContainer.ResizeWidthTo(1, transform_duration, EasingTypes.OutQuint);
            }
        }

        public LineGraph()
        {
            Add(maskingContainer = new Container<Path>
            {
                Masking = true,
                RelativeSizeAxes = Axes.Both
            });
            maskingContainer.Add(path = new Path { RelativeSizeAxes = Axes.Both, PathWidth = 1 });
        }

        public override bool Invalidate(Invalidation invalidation = Invalidation.All, Drawable source = null, bool shallPropagate = true)
        {
            if ((invalidation & Invalidation.DrawSize) != 0)
                applyPath();
            return base.Invalidate(invalidation, source, shallPropagate);
        }

        private void applyPath()
        {
            if (values == null) return;

            path.ClearVertices();
            int count = Math.Max(values.Length, DefaultValueCount);

            float max = values.Max(), min = values.Min();
            if (MaxValue > max) max = MaxValue.Value;
            if (MinValue < min) min = MinValue.Value;

            ActualMaxValue = max;
            ActualMinValue = min;

            for (int i = 0; i < values.Length; i++)
            {
                float x = (i + count - values.Length) / (float)(count - 1) * DrawWidth - 1;
                float y = GetYPosition(values[i]) * DrawHeight - 1;
                // the -1 is for inner offset in path (actually -PathWidth)
                path.AddVertex(new Vector2(x, y));
            }
        }

        protected float GetYPosition(float value)
        {
            if (ActualMaxValue == ActualMinValue) return 0;
            return (ActualMaxValue - value) / (ActualMaxValue - ActualMinValue);
        }
    }
}
