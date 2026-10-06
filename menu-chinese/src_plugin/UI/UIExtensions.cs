using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.Input;

namespace ConfigManager.UI
{
    public static class UIExtensions
    {
        /// <summary>
        /// Enables click-outside-to-close behavior for a Dropdown.
        /// Works around UniverseLib's Canvas hierarchy preventing the default Dropdown blocker from receiving clicks.
        /// </summary>
        public static void EnableClickOutsideToClose(this Dropdown dropdown)
        {
            RuntimeHelper.StartCoroutine(MonitorDropdownClickOutside(dropdown));
        }

        private static IEnumerator MonitorDropdownClickOutside(Dropdown dropdown)
        {
            while (dropdown && dropdown.gameObject)
            {
                yield return null;

                // "Dropdown List" is created by Unity's Dropdown.Show() as a child of the dropdown
                Transform dropdownList = dropdown.transform.Find("Dropdown List");
                if (dropdownList == null || !dropdownList.gameObject.activeSelf)
                    continue;

                if (!InputManager.GetMouseButtonDown(0))
                    continue;

                Vector2 mousePos = InputManager.MousePosition;
                RectTransform listRect = dropdownList.GetComponent<RectTransform>();

                // Close if clicking anywhere outside the dropdown list (including the dropdown button itself to act as toggle)
                if (!RectTransformUtility.RectangleContainsScreenPoint(listRect, mousePos))
                {
                    dropdown.Hide();
                }
            }
        }

        public static void SnapToSection(this ScrollRect @this, RectTransform section)
        {
            Canvas.ForceUpdateCanvases();

            var contentPos = (Vector2)@this.transform.InverseTransformPoint(@this.content.position);
            var childPos = (Vector2)@this.transform.InverseTransformPoint(section.position);
            var endPos = contentPos - childPos - (Vector2)section.rect.size * 0.5f;
            // If no horizontal scroll, then don't change contentPos.x
            if (!@this.horizontal)
                endPos.x = @this.content.anchoredPosition.x;
            // If no vertical scroll, then don't change contentPos.y
            if (!@this.vertical)
                endPos.y = @this.content.anchoredPosition.y;
            @this.content.anchoredPosition = endPos;
        }

        public static bool IsInSection(this ScrollRect @this, RectTransform section)
        {
            var contentPos = (Vector2)@this.transform.InverseTransformPoint(@this.content.position);
            var childPos = (Vector2)@this.transform.InverseTransformPoint(section.position);
            var endPos = contentPos - childPos + (Vector2)section.rect.size * 0.5f;
            // If horizontal scroll, then compare with contentPos.x
            if (@this.horizontal)
                return endPos.x >= @this.content.anchoredPosition.x;
            // If vertical scroll, then compare with contentPos.y
            if (@this.vertical)
                return endPos.y >= @this.content.anchoredPosition.y;
            return false;
        }
    }
}