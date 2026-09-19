using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ImmersiveVRTools.Editor.Common.Utilities
{
    public class GUIRectHelper
    {
        public static List<Rect> GetInlineButtonRects(Rect propertyRect, out int totalWidthTaken, List<int> buttonSizes, int spacingBetweenButtons)
        {
            totalWidthTaken = buttonSizes.Sum();
            return buttonSizes.Select((buttonWidth, index) =>
            {
                var rect = new Rect(propertyRect);
                rect.xMin = propertyRect.xMax - (buttonSizes.Skip(index).Sum() + ((buttonSizes.Count - index - 1) * spacingBetweenButtons));
                rect.width = buttonWidth;
                return rect;
            }).ToList();
        }
    }
}