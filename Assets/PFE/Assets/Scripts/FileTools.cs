using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;


class FileTools
{
    public const string FILE_EXTENSION = "";
    public const string SCENE_FOLDER = "Scenario/";
    public static readonly List<String> FUNCT_NAME = new List<String>() {"define:","tasks","when"};
    private static int _currentTabNumber;
    private static Dictionary<String,String> _constantDict;
    private static int _lineNumber;

    private static Stack<String[]> _state;
    private static Stack<bool> _isStrictStack;
    private static Scene _scene;
    private static StreamReader _sr;
    public static Scene LoadTextFile(String file)
    {
        _scene = new Scene();
        _currentTabNumber = 0;
        _lineNumber = 0;
        _state = new Stack<String[]>();
        _isStrictStack = new Stack<bool>();
        _constantDict = new Dictionary<string, string>();
        int state = -1;
        Debug.Log("Load " + SCENE_FOLDER + file);
        TextAsset scenario = (TextAsset)Resources.Load(SCENE_FOLDER + file, typeof(TextAsset));
        //Debug.Log("Is null ? " + scenario.text == null);
        //Debug.Log(scenario.text);
        //byte[] byteArray = Encoding.ASCII.GetBytes( scenario );
        MemoryStream stream = new MemoryStream( scenario.bytes );
        _sr = new StreamReader(stream);
        Debug.Log("SR Loaded");
        string line;

        while ((line = _read_file()) != null)
        {
            _read_line(line);
            //Debug.Log("|||TAB = " + tmp);
            //Debug.Log(line);
        }
        _sr.Close();
        /*foreach (KeyValuePair<String,String> k in _constantDict)
        {
            Debug.Log(k.Key + " => " + k.Value);
        }*/

        return _scene;
    }


    private static void _read_line(string line)
    {
        int tmp;
        string []splitLine;
        string []peekLine;
        string trimLine;
        line = line.TrimEnd();
        tmp = line.Length;
        line = line.TrimStart();
        if(line.Length == 0) return;
        if(line[0] == '#') return;
        tmp = tmp - line.Length;

        if(_currentTabNumber > tmp)
        {
            //Console.WriteLine("/// BLOCK END");
            _currentTabNumber = tmp;
            _state.Pop();
        }
        /*if(_currentTabNumber < tmp)
        {
            Console.WriteLine("/// BLOCK START");
            _currentTabNumber = tmp;
        }*/
        splitLine = line.Split(' ');
        if(FUNCT_NAME.Contains(splitLine[0]))
        {
            _state.Push(splitLine);
            _currentTabNumber++;
            return;
        }
        if(_state.Count == 0) return;
        peekLine = _state.Peek();
        switch(peekLine[0])
        {
            case "define:":
                _define_funct(line);
                break;
            case "tasks":
                if(peekLine[1] == "without" && peekLine[2] == "order:")
                    _add_task(line,false);
                else if(peekLine[1] == "with" && peekLine[2] == "order:")
                    _add_task(line,true);
                else goto default;
                break;
            case "when":
                if(peekLine[1] == "success:")
                {
                    _add_instruction_to_success(line);
                }
                else if(peekLine[1] == "fail:")
                {
                    _add_instruction_to_fail(line);
                }
                else if(peekLine[1] == "firsttry:")
                {
                    _add_instruction_to_first_try(line);
                }
                break;
            default:
                //Console.WriteLine("error");
                _printDebug("Incorrect params \"" + String.Join(" ",peekLine) + "\"",_lineNumber,'e');
                return;
                //_state.Pop();
                break;
        }
    }

    private static int _define_funct(String line)
    {
        String []tmp;
        tmp = line.Split('=');
        if(tmp.Length != 2 || line[0] != '$')
        {
            _printDebug("Incorrect \"define\" instruction ",_lineNumber,'e');
            return 1;
        }
        tmp[0] = tmp[0].Trim();
        tmp[1] = tmp[1].Trim();
        _constantDict[tmp[0]] = tmp[1];

        return 0;
    }

    private static int _add_task(String line, bool withOrder)
    {
        String []tmp = line.Split(' ');
        tmp[0] = tmp[0].Trim();
        switch(tmp[0])
        {
            case "take":
                tmp[1] = tmp[1].Trim();
                if(_constantDict.ContainsKey(tmp[1])) tmp[1] = _constantDict[tmp[1]];

                if(withOrder) _scene.addTakeObjectOrderedTask(tmp[1]);
                else _scene.addTakeObjectUnorderedTask(tmp[1]);
                break;
            case "wait":
                if(tmp[1] == "for")
                {
                    if(tmp[2] == "validation")
                    {
                        _scene.WaitForValidation = true;
                    }
                }
                break;
            case "strict":
            {
                if(tmp[1] == "take")
                {
                    _scene.IsTakeObjectStrict = true;
                }
                break;
            }
        }

        return 0;
    }

    private static void _add_instruction_to_success(string line)
    {
        Instruction i = _read_instruction(line);
        if(i != null) _scene.AddInstructionToSuccess(i);
    }

    private static void _add_instruction_to_fail(string line)
    {
        Instruction i = _read_instruction(line);
        if(i != null) _scene.AddInstructionToFail(i);
    }

    private static void _add_instruction_to_first_try(string line)
    {
        Instruction i = _read_instruction(line);
        if(i != null) _scene.AddInstructionToFirstTry(i);
    }

    private static Instruction _read_instruction(string line)
    {
        line = line.Trim();
        string []splitLine = line.Split(' ');
        //Console.WriteLine("read inst");
        if(splitLine[0] == "print")
        {
            string printLine;
            string text;
            string timeString;
            double time;
            //Regex searchTerm = new Regex("\"([^\"\\\\]*(\\\\.)*)*\"");
            //Match mc = searchTerm.Match(line);
            //Console.WriteLine(line);
            foreach (Match match in Regex.Matches(line, "\"([^\"]*)\"\\s+(\\w+\\s*)\\s+(\\w+)"))
            {
                //string result = match.Result("$2");
                //Console.WriteLine(match.Result("$1") + "////");
                //Console.WriteLine(match.Result("$2") + "////");
                //Console.WriteLine(match.Result("$3") + "////");
                text = match.Result("$1");
                if(match.Result("$2") == "for")
                {
                    timeString = match.Result("$3");
                    //Console.WriteLine(timeString);
                    if(timeString[timeString.Length - 1] == 's') timeString = timeString.Substring(0,timeString.Length - 1);
                    //Console.WriteLine(timeString);
                    time = Double.Parse(timeString);
                    //Console.WriteLine(text);
                    return new PrintInst(text,PrintType.WITH_TIMEOUT,time);
                }
                if(match.Result("$2") == "with")
                {
                    if(match.Result("$3") == "confirmation")
                        return new PrintInst(text,PrintType.WITH_CONFIRMATION,-1);
                }
            }
        }
        else if(splitLine[0] == "load")
        {
            return new LoadSceneInst(splitLine[1] + FILE_EXTENSION);
        }
        else if(splitLine[0] == "checkbox")
        {
            return _read_checkbox(line);
        }
        else if(splitLine[0] == "freeze")
        {
            if(splitLine[1] == "user")
            {
                return new FreezeUserInst(true);
            }
        }
        else if(splitLine[0] == "unfreeze")
        {
            if(splitLine[1] == "user")
            {
                return new FreezeUserInst(false);
            }
        }
        //Console.WriteLine("WARNING!!!!!! NULL INSTRUCTION !!!!!!");
        _printDebug("Null instruction from line \"" + line + "\"",_lineNumber,'w');
        return null;
    }
    
    private static Instruction _read_checkbox(string line)
    {
        String []splitLine = line.Split(' ');
        int i = 1;
        int beginTab = _currentTabNumber, newTabs;
        bool isStrict;
        string message;
        List<string> options = new List<string>();
        Dictionary<List<int>,List<Instruction>> inst = new Dictionary<List<int>, List<Instruction>>();
        CheckBoxInstr checkBox;
        List<int> caseL;
        List<Instruction> caseInst;
        Instruction tmp;
        int caseNb;


        if(splitLine[i] == "strict")
        {
            isStrict = true;
            i++;
        }
        else isStrict = false;

        line = String.Join(" ",splitLine,i,splitLine.Length - i);
        //Console.WriteLine("read checkbox => " + line);
        MatchCollection mc = Regex.Matches(line, "\"([^\"]*)\"");
        message = mc[0].Result("$1");
        for (i = 1; i< mc.Count; i++)
        {
            //Console.WriteLine(mc[i].Result("$1"));
            options.Add(mc[i].Result("$1"));
        }
        line = _read_file();
        _currentTabNumber = _get_tab_number(line);
        line = line.Trim();
        splitLine = line.Split(' ');
        
        if(splitLine[0] != "case" && splitLine[0] != "else:") _printDebug("checkbox error: missing \"case\" or \"else\" statement",_lineNumber,'e');
        if(splitLine.Length != 2) _printDebug("checkbox error: incorrect statement",_lineNumber,'e');
        

        while(_currentTabNumber > beginTab)
        {
            switch(splitLine[0])
            {
                case "case":
                    string []args = splitLine[1].Split(',');
                    caseL = new List<int>();
                    caseInst = new List<Instruction>();
                    foreach(string st in args)
                    {
                        if(int.TryParse((st[st.Length - 1] == ':') ? st.Substring(0,st.Length - 1) : st,out caseNb))
                            caseL.Add(caseNb - 1);
                    }
                    newTabs = _currentTabNumber;
                    line = _read_file();
                    _currentTabNumber = _get_tab_number(line);
                    while(_currentTabNumber > newTabs)
                    {
                        tmp = _read_instruction(line);
                        if(tmp != null) caseInst.Add(tmp);
                        line = _read_file();
                        _currentTabNumber = _get_tab_number(line);
                        line = line.Trim();
                    }
                    inst[caseL] = caseInst;
                    break;
                case "else:":
                    caseL = new List<int>();
                    caseInst = new List<Instruction>();

                    caseL.Add(CheckBoxInstr.DEFAULT_CASE);
                    newTabs = _currentTabNumber;
                    line = _read_file();
                    _currentTabNumber = _get_tab_number(line);
                    while(_currentTabNumber > newTabs)
                    {
                        tmp = _read_instruction(line);
                        if(tmp != null) caseInst.Add(tmp);
                        line = _read_file();
                        _currentTabNumber = _get_tab_number(line);
                        line = line.Trim();
                    }
                    inst[caseL] = caseInst;

                    line = _read_file();
                    _currentTabNumber = _get_tab_number(line);
                    break;
                default:
                    line = _read_file();
                    _currentTabNumber = _get_tab_number(line);
                    break;
            }
            splitLine = line.Split(' ');
        }
        return new CheckBoxInstr(message,options,inst,isStrict);
    }

    private static void _printDebug(String mess, int lineNumber, char type)
    {
        if(type == 'e') 
        {
            //Console.ForegroundColor = ConsoleColor.Red; 
            Debug.Log("Error at line " + lineNumber + " : " + mess);
            //Console.ResetColor();
            //Environment.Exit(1);
        }
        if(type == 'w')
        {
            //Console.ForegroundColor = ConsoleColor.Magenta; 
            Debug.Log("Warning at line " + lineNumber + " : " + mess);
            //Console.ResetColor();
        }
    }

    private static String _read_file()
    {
        string s = _sr.ReadLine();
        if(s != null)
            _lineNumber++;
        return s;
    }

    private static void _rewind_file()
    {
        
    }

    private static int _get_tab_number(string line)
    {
        int tmp;
        line = line.TrimEnd();
        tmp = line.Length;
        line = line.TrimStart();
        tmp = tmp - line.Length;
        return tmp;
    }
}

