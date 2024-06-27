using System;
using MissionLibrary.HotKey;
using MissionSharedLibrary.Utilities;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.InputSystem;

namespace MissionSharedLibrary.Config.HotKey
{
    public class GameKeySequence : IGameKeySequence
    {
        public int Id;
        public string StringId;
        public string CategoryId;

        public List<Key> Keys;

        public bool Mandatory = false;

        private readonly List<InputKey> _defaultGameKeys;

        private int _progress = 0;

        public GameKeySequence(int id, string stringId, string categoryId ,List<InputKey> gameKeys, bool mandatory = false)
        {
            Id = id;
            StringId = stringId;
            CategoryId = categoryId;
            gameKeys.RemoveAll(inputKey => inputKey == InputKey.Invalid);
            _defaultGameKeys = gameKeys;
            Mandatory = mandatory;
            SetGameKeys(gameKeys);
        }

        public SerializedGameKeySequence ToSerializedGameKeySequence()
        {
            return new SerializedGameKeySequence
            {
                StringId = StringId,
                KeyboardKeys = Keys.Where(key => key.InputKey != InputKey.Invalid).Select(key => key.InputKey).ToList()
            };
        }

        public void SetGameKeys(List<InputKey> inputKeys)
        {
            var keys = inputKeys.Where(inputKey => inputKey != InputKey.Invalid).Select(inputKey => new Key(inputKey)).ToList();
            if (Mandatory && keys.Count == 0)
                return;
            Keys = keys;
        }

        public void ClearInvalidKeys()
        {
            Keys.RemoveAll(key => key.InputKey == InputKey.Invalid);
        }

        public void ResetToDefault()
        {
            SetGameKeys(_defaultGameKeys);
        }

        public bool IsKeyDownInOrder(IInputContext input = null)
        {
            if (!CheckCurrentProgress(input))
                return false;

            for (int i = _progress; i < Keys.Count; ++i)
            {
                if (IsKeyDown(input, i))
                    ++_progress;
                else
                    return false;
            }

            return true;
        }

        public bool IsKeyPressedInOrder(IInputContext input = null)
        {
            if (!CheckCurrentProgress(input))
                return false;

            for (int i = _progress; i < Keys.Count - 1; ++i)
            {
                if (IsKeyDown(input, i))
                    ++_progress;
                else
                    return false;
            }

            return IsKeyPressed(input, Keys.Count - 1);
        }

        public bool IsKeyReleasedInOrder(IInputContext input = null)
        {
            if (!CheckCurrentProgress(input))
                return false;

            for (int i = _progress; i < Keys.Count - 1; ++i)
            {
                if (IsKeyDown(input, i))
                    ++_progress;
                else
                    return false;
            }

            return IsKeyReleased(input, Keys.Count - 1);
        }

        public bool IsKeyDown(IInputContext input = null)
        {
            if (Keys.Count == 0)
                return false;

            for (int i = 0; i < Keys.Count; ++i)
            {
                if (!IsKeyDown(input, i))
                    return false;
            }

            return true;
        }

        public bool IsKeyPressed(IInputContext input = null)
        {
            if (Keys.Count == 0)
                return false;

            for (int i = 0; i < Keys.Count - 1; ++i)
            {
                if (!IsKeyDown(input, i))
                    return false;
            }

            return IsKeyPressed(input, Keys.Count - 1);
        }

        public bool IsKeyReleased(IInputContext input = null)
        {
            if (Keys.Count == 0)
                return false;

            for (int i = 0; i < Keys.Count - 1; ++i)
            {
                if (!IsKeyDown(input, i))
                    return false;
            }

            return IsKeyReleased(input, Keys.Count - 1);
        }

        public string ToSequenceString()
        {
            if (Keys.Count == 0)
            {
                return "[No key]";
            }
            string result = "";
            for (int i = 0; i < Keys.Count - 1; ++i)
            {
                result += Utility.TextForKey(Keys[i].InputKey) + " ";
            }

            result += Utility.TextForKey(Keys[Keys.Count - 1].InputKey);
            return result;
        }

        private bool CheckCurrentProgress(IInputContext input)
        {
            if (Keys == null || Keys.Count == 0)
                return false;
            for (int i = 0; i < _progress; ++i)
            {
                if (!IsKeyDown(input, i))
                {
                    _progress = i;
                    return false;
                }
            }

            return true;
        }

        private bool IsKeyDown(IInputContext input, int i)
        {
            return input?.IsKeyDown(Keys[i].InputKey) ?? Input.IsKeyDown(Keys[i].InputKey);
        }

        private bool IsKeyPressed(IInputContext input, int i)
        {
            return input?.IsKeyPressed(Keys[i].InputKey) ?? Input.IsKeyPressed(Keys[i].InputKey);
        }

        private bool IsKeyReleased(IInputContext input, int i)
        {
            return input?.IsKeyReleased(Keys[i].InputKey) ?? Input.IsKeyReleased(Keys[i].InputKey);
        }
    }
}
