using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using ConfigManager.UI;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace ConfigManager.UI.InteractiveValues
{
    public class InteractiveString : InteractiveValue
    {
        public InteractiveString(object value, Type valueType) : base(value, valueType) { }

        internal InputFieldRef valueInput;
        internal GameObject hiddenObj;
        internal Text placeholderText;

        public override bool SupportsType(Type type) => type == typeof(string);

        public override void RefreshUIForValue()
        {
            if (!hiddenObj.gameObject.activeSelf)
                hiddenObj.gameObject.SetActive(true);

            if (!string.IsNullOrEmpty((string)Value))
            {
                string toString = (string)Value;
                if (toString.Length > 15000)
                    toString = toString.Substring(0, 15000);

                valueInput.Text = toString;
                placeholderText.text = toString;
            }
            else
            {
                string s = Value == null 
                            ? "null" 
                            : "empty";

                valueInput.Text = "";
                placeholderText.text = MenuChinese.Text(s);
            }
        }

        internal void SetValueFromInput()
        {
            Value = valueInput.Text;
            Owner.SetValueFromIValue();
        }

        public override void ConstructUI(GameObject parent)
        {
            base.ConstructUI(parent);

            hiddenObj = UIFactory.CreateLabel(mainContent, "HiddenLabel", "", TextAnchor.MiddleLeft).gameObject;
            hiddenObj.SetActive(false);
            Text hiddenText = hiddenObj.GetComponent<Text>();
            hiddenText.color = Color.clear;
            hiddenText.fontSize = 14;
            hiddenText.raycastTarget = false;
            hiddenText.supportRichText = false;
            ContentSizeFitter hiddenFitter = hiddenObj.AddComponent<ContentSizeFitter>();
            hiddenFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            UIFactory.SetLayoutElement(hiddenObj, minHeight: 25, flexibleHeight: 500, minWidth: 250, flexibleWidth: 9000);
            UIFactory.SetLayoutGroup<HorizontalLayoutGroup>(hiddenObj, true, true, true, true);

            valueInput = UIFactory.CreateInputField(hiddenObj, "StringInputField", "...");
            UIFactory.SetLayoutElement(valueInput.Component.gameObject, minWidth: 120, minHeight: 25, flexibleWidth: 5000, flexibleHeight: 5000);

            valueInput.Component.lineType = InputField.LineType.MultiLineNewline;

            placeholderText = valueInput.Component.placeholder.GetComponent<Text>();

            placeholderText.supportRichText = false;
            valueInput.Component.textComponent.supportRichText = false;

            valueInput.OnValueChanged += (string val) =>
            {
                if (Owner.ReadOnly)
                {
                    valueInput.Text = (string)Value;
                    return;
                }

                hiddenText.text = val ?? "";
                LayoutRebuilder.ForceRebuildLayoutImmediate(Owner.ContentRect);
                SetValueFromInput();
            };


            // Copy button

            if (Owner.AllowCopy)
            {
                ButtonRef copyButton = UIFactory.CreateButton(hiddenObj, "CopyButton", I18n.T("Copy"), new Color(0.3f, 0.3f, 0.3f));
                copyButton.OnClick += () =>
                {
                    var str = Value as string ?? "";
                    WindowsClipboard.SetText(str);
                    copyButton.ButtonText.text = $"<color=#03c03c>{I18n.T("Copied")}</color>";

                    new Thread(() =>
                    {
                        Thread.Sleep(500);
                        copyButton.ButtonText.text = I18n.T("Copy");
                    }).Start();
                };
                UIFactory.SetLayoutElement(copyButton.Component.gameObject, minWidth: 80, minHeight: 22, flexibleWidth: 0);
            }
            RefreshUIForValue();
        }
    }
}
