using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.Input;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace ConfigManager.UI.InteractiveValues
{
    public class InteractiveKeycode : InteractiveValue
    {
        internal Text labelText;
        internal ButtonRef rebindButton;
        internal ButtonRef confirmButton;
        internal ButtonRef cancelButton;
        internal ButtonRef clearButton;

        private readonly bool isInputSystem;

        public InteractiveKeycode(object value, Type valueType) : base(value, valueType)
        {
            isInputSystem = !(value is KeyCode);
        }

        public override bool SupportsType(Type type) => type == typeof(KeyCode) || type.FullName == "UnityEngine.InputSystem.Key";

        public override void RefreshUIForValue()
        {
            base.RefreshUIForValue();

            labelText.text = KeyCodeToText((KeyCode)Value);
        }

        public override void OnValueUpdated()
        {
            base.OnValueUpdated();
        }

        public void BeginRebind()
        {
            rebindButton.Component.gameObject.SetActive(false);
            confirmButton.Component.gameObject.SetActive(true);
            confirmButton.Component.interactable = false;
            cancelButton.Component.gameObject.SetActive(true);
            clearButton.Component.gameObject.SetActive(true);

            labelText.text = $"<i>{I18n.T("PressAKey")}</i>";

            InputManager.BeginRebind(OnRebindKeyPressed, OnKeycodeConfirmed);
        }

        private string KeyCodeToText(KeyCode keyCode)
        {
            if (keyCode == KeyCode.None)
                return I18n.T("NotSet");
            if (!isInputSystem)
                return keyCode.ToString();

            var key = InputSystem.KeyCodeToKeyEnum(keyCode);
            if (key == null)
                return I18n.T("NotSet");
            return key.ToString();
        }

        private void OnRebindKeyPressed(KeyCode kc)
        {
            labelText.text = $"<i>{KeyCodeToText(kc)}</i>";

            confirmButton.Component.interactable = true;
        }

        private void OnKeycodeConfirmed(KeyCode? kc)
        {
            if (kc != null)
            {
                if (!isInputSystem)
                    Value = kc;
                else
                    Value = InputSystem.KeyCodeToKeyEnum(kc.Value);
            }

            Owner.SetValueFromIValue();
            RefreshUIForValue();
        }

        public void ConfirmEndRebind()
        {
            InputManager.EndRebind();
            OnRebindEnd();
        }

        public void CancelEndRebind()
        {
            InputManager.LastRebindKey = null;
            InputManager.EndRebind();
            OnRebindEnd();
        }

        public void ClearEndRebind()
        {
            InputManager.LastRebindKey = KeyCode.None;
            OnRebindKeyPressed(KeyCode.None);
            InputManager.EndRebind();
            OnRebindEnd();
        }

        internal void OnRebindEnd()
        {
            rebindButton.Component.gameObject.SetActive(true);
            confirmButton.Component.gameObject.SetActive(false);
            cancelButton.Component.gameObject.SetActive(false);
            clearButton.Component.gameObject.SetActive(false);
        }

        public override void ConstructUI(GameObject parent)
        {
            base.ConstructUI(parent);

            labelText = UIFactory.CreateLabel(mainContent, "Label", KeyCodeToText((KeyCode)Value), TextAnchor.MiddleLeft);
            UIFactory.SetLayoutElement(labelText.gameObject, minWidth: 150, minHeight: 25);

            rebindButton = UIFactory.CreateButton(mainContent, "RebindButton", I18n.T("Rebind"));
            rebindButton.OnClick += BeginRebind;
            UIFactory.SetLayoutElement(rebindButton.Component.gameObject, minHeight: 25, minWidth: 100);

            confirmButton = UIFactory.CreateButton(mainContent, "ConfirmButton", I18n.T("Confirm"), new Color(0.1f, 0.4f, 0.1f));
            confirmButton.OnClick += ConfirmEndRebind;
            UIFactory.SetLayoutElement(confirmButton.Component.gameObject, minHeight: 25, minWidth: 100);
            confirmButton.Component.gameObject.SetActive(false);
            RuntimeHelper.SetColorBlock(confirmButton.Component, disabled: new Color(0.3f, 0.3f, 0.3f));

            cancelButton = UIFactory.CreateButton(mainContent, "EndButton", I18n.T("Cancel"), new Color(0.4f, 0.1f, 0.1f));
            cancelButton.OnClick += CancelEndRebind;
            UIFactory.SetLayoutElement(cancelButton.Component.gameObject, minHeight: 25, minWidth: 100);
            cancelButton.Component.gameObject.SetActive(false);

            clearButton = UIFactory.CreateButton(mainContent, "ClearButton", I18n.T("Clear"), new Color(0.54f, 0.17f, 0.89f));
            clearButton.OnClick += ClearEndRebind;
            UIFactory.SetLayoutElement(clearButton.Component.gameObject, minHeight: 25, minWidth: 100);
            clearButton.Component.gameObject.SetActive(false);
        }
}
}
