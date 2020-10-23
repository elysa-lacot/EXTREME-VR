using System;
using System.Collections.Generic;
using System.Threading;
using static IPrintable;
using UnityEngine;
using UnityEngine.UI;

abstract class Instruction
{
    public const int PRINT_INST = 1;
    public const int LOAD_INST = 2;
    public const int FREEZE_USER_INST = 3;
    public const int UNFREEZE_USER_INST = 4;
    public const int RESTART_INST = 5;
    public const int CHECKBOX_INST = 6;
    protected int _type;
    public int Type
    {
        get { return _type; }
    }

    public abstract void Execute(Scene s);
}
class PrintInst : Instruction
{
    private string _text;
    private int _printType;
    private double _time;
    public PrintInst(String text, int type, double time = -1)
    {
        _type = Instruction.PRINT_INST;
        _text = text;
        _printType = type;
        _time = time;
    }
    public override void Execute(Scene s)
    {
        if(s == null) Console.WriteLine("/////////////////");
        s.PrintOutput.PrintToUser(_text,_printType,_time);
    }
}

class RestartInst : Instruction
{
    public RestartInst()
    {
        _type = Instruction.RESTART_INST;
    }

    public override void Execute(Scene s)
    {
        s.Restart();
    }
}

class LoadSceneInst : Instruction
{
    private string _file;

    public LoadSceneInst(string file)
    {
        _type = Instruction.LOAD_INST;
        _file = file;
    }

    public override void Execute(Scene s)
    {
        s.ChangeScene(_file);
    }
}

class CheckBoxInstr : Instruction
{
    private string _message;
    private List<string> _options;
    private Dictionary<List<int>,List<Instruction>> _inst;
    private bool _isStrict;
    public const int DEFAULT_CASE = -1;

    public CheckBoxInstr(string message, List<string> options, Dictionary<List<int>,List<Instruction>> inst, bool isStrict = false)
    {
        _type = CHECKBOX_INST;
        _message = message;
        _options = options;
        _inst = inst;
        _isStrict = isStrict;
    }

    public override void Execute(Scene s)
    {
        s.CheckBox(_message,_options,_inst,_isStrict);
    }
}

class FreezeUserInst : Instruction
{
    private bool _isFrozen;

    public FreezeUserInst(bool isFrozen)
    {
        _type = FREEZE_USER_INST;
        _isFrozen = isFrozen;
    }

    public override void Execute(Scene s)
    {
        s.SetUserFreeze(_isFrozen);
    }
}
