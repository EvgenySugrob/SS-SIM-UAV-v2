using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using Vmaya.Collections;
using Vmaya.Entity;

namespace Vmaya.Command
{
    public interface ICommandCreator
    {
        ICommand createCommand(string commandPack);
        string packCommand(ICommand command);
    }

    [Serializable]
    public struct CommandList
    {
        public List<string> list;
        public int pointer;
    }

    public class CommandManager : MonoBehaviour, IListSource, IJsonObject
    {
        public enum LastAction { Execute, Fail, Undo, Redo, Destroy }

        private List<ICommand> _undoStack;
        private int _pointer;
        private bool _isListRun;
        public bool IsListRun => _isListRun;
        private bool _stopListRun;

        [SerializeField]
        private bool DisableStore = false;

        [SerializeField]
        [Tooltip("Work only when non-null value")]
        private int LimitCommandCount = 0;

        public bool isCommandList;

        [ConditionalField(nameof(isCommandList))] [SerializeField] private bool _fastExecuteFromList;
        [ConditionalField(nameof(isCommandList))] [TextArea] public string commandListData;

        public float waitSec;
        public UnityEvent onChange;
        public UnityEvent onStartExecuteList;
        public UnityEvent onEndExecuteList;

        private static CommandManager _instance;
        public static CommandManager instance => _instance ? _instance : _instance = FindObjectOfType<CommandManager>();
        public static bool isListRun => _instance ? _instance._isListRun : false;
        public static bool isCommandFast => _instance ? _instance._isListRun && _instance._fastExecuteFromList : false;

        private CommandList _commandsPack;
        private int _commandPackIndex = 0;

        private List<Action> _urStack; // Список команд ждущих очередь для исполнения Undo
        private bool _urWait = false;

        private ICommand _commandExecute = null;
        public ICommand commandExecute => _commandExecute;
        public bool isCommandExecute => _commandExecute != null;

        private ICommandCreator _commandCreator;
        public ICommandCreator commandCreator => _commandCreator != null ? _commandCreator : _commandCreator = GetComponent<ICommandCreator>();

        private LastAction _lastAction;
        public LastAction lastAction => _lastAction;

        private int _commandPackIndexCurrent;
        private ICommand _commandCurrent;

        virtual protected void Awake()
        {
            _urStack = new List<Action>();
            _undoStack = new List<ICommand>();
            _pointer = -1;
        }

        private void addPointer(int inc)
        {
            _pointer += inc;
            doChange();
        }

        virtual protected void doChange()
        {
            onChange.Invoke();
        }

        internal static void ExecuteCmd(ICommand command)
        {
            if (instance) instance.executeCmd(command);
            else command.execute();
        }

        private void executeCommandA(ICommand command)
        {
            Utils.PendingCondition(this, () => {
                return !isCommandExecute && command.isReady();
            }, () =>
            {
                _commandExecute = command;
                if (DisableStore) command.execute();
                else
                {
                    if (!poinerUp) clearTail();

                    if (command.execute())
                    {
                        _lastAction = LastAction.Execute;
                        _undoStack.Add(command);
                        if ((LimitCommandCount > 0) && (_undoStack.Count == LimitCommandCount))
                        {
                            _undoStack.RemoveAt(0);
                            addPointer(0);
                        } else addPointer(1);

                    } else _lastAction = LastAction.Fail;
                }
                if (isCommandFast) _commandExecute = null;
                else Utils.setTimeout(this, () => {
                    _commandExecute = null;
                }, command.executeTime());
            });
        }

        public void executeCmd(ICommand command)
        {
            if (!isListRun) executeCommandA(command);
        }

        public ICommand pointerCommand { get { return _undoStack.Count > 0 && _pointer > -1 ? _undoStack[_pointer] : null; } }
        public string pointerNameCommand { get { return _undoStack.Count > 0 && _pointer > -1 ? _undoStack[_pointer].commandName() : ""; } }
        public bool poinerUp { get { return _pointer >= _undoStack.Count - 1; } }
        public int pointer { get { return _pointer; } }
        public bool pointerBottom { get { return _pointer < 0; } }

        public ICommand this[int index]
        {
            get
            {
                return _undoStack[index];
            }
        }

        protected void clearTail()
        {
            clearTo(_pointer);
        }

        protected void clearTo(int a_pointer, bool undoCalling = false)
        {
            int index = a_pointer + 1;

            for (int i = _undoStack.Count - 1; i >= index; i--)
            {
                _lastAction = LastAction.Destroy;
                _instance = null; // Скрываем ссылку на менеджера, для того что бы во время разрушения, внутри, не создавались новые команды
                    
                if (undoCalling && _undoStack[i].isReady()) _undoStack[i].undo();
                _undoStack[i].destroy();
                _instance = this;
            }

            _undoStack.RemoveRange(index, _undoStack.Count - index);

            _pointer = a_pointer;
            doChange();

            if (isListRun) _stopListRun = true;
        }

        public void clearAll(bool undoCalling = false)
        {
            clearTo(-1, undoCalling);
        }

        IEnumerator checkURStack(float wait)
        {
            _urWait = true;
            yield return new WaitForSeconds(wait);
            _urWait = false;
            if (_urStack.Count > 0)
            {
                Action ura = _urStack[_urStack.Count - 1];
                _urStack.RemoveAt(_urStack.Count - 1);
                ura();
            }
        }

        public void undo()
        {
            if (_undoStack.Count > 0 && !pointerBottom && _commandExecute == null)
            {
                _commandExecute = pointerCommand;
                if (!_urWait)
                {
                    _lastAction = LastAction.Undo;

                    if (_commandExecute.isReady())
                        _commandExecute.undo();
                    else Debug.Log("It is no ready (" + pointerCommand + ")");

                    StartCoroutine(checkURStack(waitSec));
                    addPointer(-1);
                }
                else _urStack.Add(undo);
                _commandExecute = null;
            }
        }

        public void redo()
        {
            if (_undoStack.Count > 0 && !poinerUp)
            {
                if (!_urWait)
                {
                    addPointer(1);
                    _commandExecute = pointerCommand;
                    StartCoroutine(checkURStack(waitSec));
                    _lastAction = LastAction.Redo;
                    if (_commandExecute.isReady())
                        _commandExecute.redo();

                    _commandExecute = null;
                }
                else _urStack.Add(redo);
            }
        }

        public void executeList(string jsonList, float delay)
        {
            executeList(JsonUtility.FromJson<CommandList>(jsonList), commandCreator, delay);
        }

        public void executeList(CommandList commandsPack, ICommandCreator creator, float delay)
        {
            if (commandsPack.list.Count > 0)
            {
                _commandsPack = commandsPack;
                _isListRun = true;
                _stopListRun = false;
                _commandPackIndexCurrent = -1;
                _commandPackIndex = 0;
                onStartExecuteList.Invoke();
                StartCoroutine(nextCommand(creator, delay));
            }
        }

        public void stopExecuteList()
        {
            _stopListRun = true;
        }

        public void beginTo(int toIndex)
        {

            IEnumerator nextCommandA(int count)
            {
                if (count > 0)
                {
                    redo();
                    count--;
                }
                else
                {
                    undo();
                    count++;
                }

                if ((count != 0) && !_stopListRun)
                {
                    yield return new WaitForSeconds(waitSec);
                    StartCoroutine(nextCommandA(count));
                }
                else
                {
                    _isListRun = false;
                    onEndExecuteList.Invoke();
                }
            }

            if (!_isListRun)
            {
                int diff = Mathf.Clamp(toIndex, -1, getCount() - 1) - pointer;
                if (diff != 0)
                {
                    _stopListRun = false;
                    _isListRun = true;
                    onStartExecuteList.Invoke();
                    StartCoroutine(nextCommandA(diff));
                }
            }
        }

        private IEnumerator nextCommand(ICommandCreator creator, float delay)
        {
            if (!_stopListRun)
            {
                if (_commandPackIndexCurrent != _commandPackIndex)
                {
                    _commandCurrent = creator.createCommand(_commandsPack.list[_commandPackIndex]);
                    _commandPackIndexCurrent = _commandPackIndex;
                }

                if (_commandCurrent == null)
                    throw new NullReferenceException(_commandsPack.list[_commandPackIndex]);

                yield return new WaitForSeconds(delay);

                Utils.PendingCondition(this, () =>
                {
                    return (!isCommandExecute && _commandCurrent.isReady()) || _stopListRun;
                }, () =>
                {
                    if (!_stopListRun)
                    {
                        executeCommandA(_commandCurrent);

                        if (_commandPackIndex < _commandsPack.list.Count - 1)
                        {
                            _commandPackIndex++;
                            StartCoroutine(nextCommand(creator, delay));
                        }
                        else
                        {
                            _isListRun = false;
                            onEndExecuteList.Invoke();
                        }
                    }
                });
            }
            else
            {
                _isListRun = false;
                _stopListRun = false;
                onEndExecuteList.Invoke();
            }
        }

        public CommandList commandList(ICommandCreator creator)
        {
            CommandList commandsPack;
            commandsPack.list = new List<string>();

            for (int i = 0; i < _undoStack.Count; i++)
                commandsPack.list.Add(creator.packCommand(_undoStack[i]));

            commandsPack.pointer = _pointer;
            return commandsPack;
        }

        public string getCurrentListJson()
        {
            return commandCreator != null ? JsonUtility.ToJson(commandList(commandCreator)) : null;
        }

        public void setListJson(string jsonList)
        {
            if ((commandCreator != null) && !string.IsNullOrEmpty(jsonList))
            {
                clearAll();

                CommandList commandsPack = JsonUtility.FromJson<CommandList>(jsonList);
                foreach (string commandJson in commandsPack.list) 
                    _undoStack.Add(commandCreator.createCommand(commandJson));

                Utils.setTimeout(this, () => {
                    _pointer = commandsPack.pointer;
                    doChange();
                }, 0.1f);
            }
        }
        /*

        public void resetMenuItemCaption(MenuItem item)
        {
            if (item.data.action.Equals("Undo"))
                item.CaptionText = item.data.name +
                    ((pointer >= 0) ? " " + pointerNameCommand : "");
            else if (item.data.action.Equals("Redo"))
                item.CaptionText = item.data.name +
                    ((pointer < sizeStack - 1) ? " " + this[pointer + 1].commandName() : "");
        }

        internal bool isCommandMenuItem(MenuItem item)
        {
            return item.data.action.Equals("Undo") || item.data.action.Equals("Redo");
        }*/

        private void Update()
        {
            if (commandCreator != null)
            {
#if ENABLE_INPUT_SYSTEM
                if (VKeyboard.GetKey(Key.LeftCtrl))
                {
                    if (VKeyboard.GetKeyDown(Key.T))
                        Debug.Log(getCurrentListJson());
                    else if (VKeyboard.GetKeyDown(Key.G) && !string.IsNullOrEmpty(commandListData))
                        executeList(commandListData, 0.1f);
                }
#else
                if (VKeyboard.GetKey(KeyCode.LeftControl))
                {
                    if (VKeyboard.GetKeyDown(KeyCode.T))
                        Debug.Log(getCurrentListJson());
                    else if (VKeyboard.GetKeyDown(KeyCode.G) && !string.IsNullOrEmpty(commandListData))
                        executeList(commandListData, 0.1f);
                }
#endif
            }
        }

        public int getCount()
        {
            return _undoStack != null ? _undoStack.Count + 1 : 0;
        }

        public int IndexOf(string id)
        {
            return int.Parse(id);
        }

        public string getId(int idx)
        {
            return idx.ToString();
        }

        public string getName(int idx)
        {
            string result = null;
            if (idx == 0) result = "---";
            else if (idx <= _undoStack.Count)
            {
                ICommand command = _undoStack[idx - 1];
                if ((command != null) && command.isReady())
                    result = command.commandName();
                else result = "It is no ready (" + command + ")";
            }

            return result;
        }

        public string getData(int idx)
        {
            return idx == 0 ? null : commandCreator.packCommand(_undoStack[idx - 1]);
        }

        public void onAfterChange(UnityAction complete)
        {
            onChange.AddListener(complete);
            complete();
        }

        public void Refresh()
        {
        }

        public void setJson(string jsonData)
        {
            setListJson(jsonData);
        }

        public string getJson()
        {
            return getCurrentListJson();
        }

        public void setActive(bool value)
        {
            enabled = value;
        }

        public bool getActive()
        {
            return enabled;
        }
    }
}