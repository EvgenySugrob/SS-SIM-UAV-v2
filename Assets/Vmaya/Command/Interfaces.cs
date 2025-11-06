using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Vmaya.Command
{
    public interface ICommand
    {
        bool isReady();
        bool execute();
        void undo();
        void redo();
        void destroy();
        string commandName();
        float executeTime();
    }   
}