using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using ExtremeVR;

namespace ExtremeVR
{

    /**
    *  \class FileTools 
    *  \author Sofiane Kerrakchou
    *  \brief Classe permettant de lire un script (fichier texte) et de retourner un objet Scene correspondant
    *  
    *  Note : Méthodes et attributs de classe uniquement
    *  \n Voir le manuel sur l'écriture de scripts 
    *  \n "Instruction" correspond à ce que le simulateur doit faire, "Tâche" correspond à ce que l'utilisateur doit faire
    */

    class FileTools
    {
        /** Extension des fichier de script (texte) */
        public const string FILE_EXTENSION = ".txt";

        /** Dossier contenant les scripts */
        public const string SCENE_FOLDER = "/mnt/sdcard/scenario/";

        /** Liste constante contenant les noms des différents blocs dans les scripts */
        public static readonly List<String> FUNCT_NAME = new List<String>() {"define:","tasks","when","config:"};

        /** Contient le nombre de tabulation attendu (permet de savoir quand un bloc est terminé) */
        private static int _currentTabNumber;

        /** Contient les valeurs des différentes constantes défini dans le script */
        private static Dictionary<String,String> _constantDict;

        /** Numéro de la dernière ligne lu */
        private static int _lineNumber;

        /** Pile des différents blocs dans lequel une ligne lu se trouve (une déclaration de bloc peut être constitué de plusieurs mots)*/
        private static Stack<String[]> _state;

        /** NON UTILISEE => A SUPPRIMER APRES VERIF */
        private static Stack<bool> _isStrictStack;

        /** Objet Scene qui sera retourné */
        private static Scene _scene;
        
        /** Lit le fichier texte */
        private static StreamReader _sr;

        /** Lit le script "file" et retourne un objet Scene correspondant */
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
            //TextAsset scenario = (TextAsset)Resources.Load(SCENE_FOLDER + file, typeof(TextAsset));

            //MemoryStream stream = new MemoryStream( scenario.bytes );
            _sr = new StreamReader(SCENE_FOLDER + file + FILE_EXTENSION);
            Debug.Log("SR Loaded");
            string line;

            while ((line = _read_file()) != null)
            {
                _read_line(line);
            }
            _sr.Close();

            return _scene;
        }


        /** Lit une ligne */
        private static void _read_line(string line)
        {
            int readLineTabNumber;
            string []splitLine;
            string []peekLine;
            string trimLine;
            
            //Enlève les espaces ou tabulations à la fin de la ligne
            line = line.TrimEnd();
            //On garde la taille de la ligne sans enlever les espaces/tabulation au début (permettra de calculer le nombre de tabulations)
            readLineTabNumber = line.Length;
            //Enlève les espaces ou tabulations en début de ligne
            line = line.TrimStart();

            //Si la ligne est vide ou qu'elle commence par # (commentaire), on passe à la ligne suivant 
            //(TrimEnd() et TrimStart() se font avant pour éviter qu'une ligne ne contenant que des espaces/tabulations soit considérée comme non vide)
            if(line.Length == 0) return;
            if(line[0] == '#') return;

            //nombre de tabulations sur la ligne en cours de lecture
            readLineTabNumber = readLineTabNumber - line.Length;

            //Si le nombre de tabulations est inférieur à celui de la ligne précédente, on quitte le bloc
            if(_currentTabNumber > readLineTabNumber)
            {
                _currentTabNumber = readLineTabNumber;
                _state.Pop();
            }

            splitLine = line.Split(' ');
            
            //Si la ligne est le début d'un bloc 
            //(dans le cas d'une déclaration de bloc contenant plusieurs mots, on vérifie seulement le premier, mais on garde en mémoire la déclaration entière)
            if(FUNCT_NAME.Contains(splitLine[0]))
            {
                _state.Push(splitLine);
                //On entre dans un bloc, le nombre de tabulation attendu est donc augmenté
                _currentTabNumber++;
                return;
            }

            //Si la ligne ne se trouve pas dans un bloc, on passe à la suivante -- PENSER A METTRE AVERTISSEMENT/ERREUR SI LIGNE HORS BLOC
            if(_state.Count == 0) return;

            //Le haut de la pile contient le nom du bloc dans lequel on se situe
            peekLine = _state.Peek();

            //On appele la méthode correspondant au bloc en cours
            switch(peekLine[0])
            {
                case "config:":
                    _config_funct(line);
                    break;
                case "define:":
                    _define_funct(line);
                    break;
                case "tasks":
                    //Un bloc tasks existe en deux versions: "without order" et "with order" (cf manuel script)
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
                    else if(peekLine[1] == "start:")
                    {
                        _add_instruction_to_start(line);
                    }
                    break;
                default:
                    _printDebug("Incorrect params \"" + String.Join(" ",peekLine) + "\"",_lineNumber,'e');
                    return;
                    
            }
        }

        /** Lit une ligne d'un bloc "config" */
        private static int _config_funct(string line)
        {
            String []tmp;

            //On vérifie si la ligne est une attribution de valeur (ex: unityScene = scene) 
            tmp = line.Split('=');
            tmp[0] = tmp[0].Trim();

            //Si, après avoir séparé la ligne avec '=', on obtient deux éléments, c'est que '=' est dans la ligne et qu'il s'agit d'une attribution de valeur
            if(tmp.Length == 2)
            {
                tmp[1] = tmp[1].Trim();
                if(tmp[0] == "unityScene")
                {
                    _scene.UnityScene = tmp[1].Trim();
                }
            }
            else
            {
                tmp = line.Split(' ');
                if(tmp.Length == 3)
                {
                    if(tmp[0] == "show")
                    {
                        if(tmp[1] == "objTag")
                        {
                            _scene.AddObjOptions(tmp[2],true,Scene.ObjectOptions.TYPE_OBJ_TAG);
                        }
                        if(tmp[1] == "objName")
                        {
                            _scene.AddObjOptions(tmp[2],true,Scene.ObjectOptions.TYPE_OBJ_NAME);
                        }
                    }
                    if(tmp[0] == "hide")
                    {
                        if(tmp[1] == "objTag")
                        {
                            _scene.AddObjOptions(tmp[2],false,Scene.ObjectOptions.TYPE_OBJ_TAG);
                        }
                        if(tmp[1] == "objName")
                        {
                            _scene.AddObjOptions(tmp[2],false,Scene.ObjectOptions.TYPE_OBJ_NAME);
                        }
                    }
                }
            }

            return 0;
        }

        /** Lit une ligne d'un bloc "define" */
        private static int _define_funct(String line)
        {
            // Rappel : déclaration de constantes dans le bloc "define" : $nomConstante = valeur
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

        /** Lit une ligne d'un bloc "tasks" 
        * \param withOrder Indique si il s'agit d'un bloc "tasks with order" (true) ou "tasks without order" (false)
        */
        private static int _add_task(String line, bool withOrder)
        {
            String []tmp = line.Split(' ');
            tmp[0] = tmp[0].Trim();
            //Le début d'une ligne dans un bloc "take" est le type de tâche, le reste contients les paramètres (ex: nom de l'objet)
            switch(tmp[0])
            {
                case "take":
                    //Dans le cas d'une tâche "take", il n'y a qu'un paramètre (le nom de l'objet)
                    tmp[1] = tmp[1].Trim();
                    //Si la ligne contient une constante (ex: take $constant), on la remplace par la valeur déclaré dans le bloc "define"
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

        private static void _add_instruction_to_start(string line)
        {
            Instruction i = _read_instruction(line);
            if(i != null) _scene.AddInstructionToStart(i);
        }

        /** Lit une ligne contenant une instruction */
        private static Instruction _read_instruction(string line)
        {
            line = line.Trim();
            string []splitLine = line.Split(' ');
            if(splitLine[0] == "print")
            {
                string printLine;
                string text;
                string timeString;
                double time;
                
                //On sépare le contenu entre guillemets (le texte à afficher) du reste (la façon dont le texte doit être affiché)
                foreach (Match match in Regex.Matches(line, "\"([^\"]*)\"\\s+(\\w+\\s*)\\s+(\\w+)"))
                {
                    text = match.Result("$1");
                    if(match.Result("$2") == "for")
                    {
                        timeString = match.Result("$3");
                        //Si ce paramètre se termine par 's', il s'agit d'une durée en secondes
                        if(timeString[timeString.Length - 1] == 's') timeString = timeString.Substring(0,timeString.Length - 1);
                        time = Double.Parse(timeString);
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
            else if(splitLine[0] == "restart")
            {
                return new RestartInst();
            }
            //Console.WriteLine("WARNING!!!!!! NULL INSTRUCTION !!!!!!");
            _printDebug("Null instruction from line \"" + line + "\"",_lineNumber,'w');
            return null;
        }
        
        /** Lit une instruction de type checkbox (questionnaire) */
        private static Instruction _read_checkbox(string line)
        {
            String []splitLine = line.Split(' ');
            int i = 1;
            //Une instruction de type checkbox est composé de plusieurs "sous-blocs"
            int beginTab = _currentTabNumber, newTabs;
            bool isStrict;
            //Message qui sera affiché avec le questionnaire
            string message;
            //Liste des options proposées à l'utilisateur
            List<string> options = new List<string>();

            //La clé List<int> contient le numéro des options sélectionnées par l'utilisateur 
            //La valeur List<Instruction> contient les instructions à effectuer en conséquences
            //(la clé correspondant au sous-bloc "else" contient seulement la valeur CheckBoxInstr.DEFAULT_CASE)
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

            //On récupère le message à afficher et les options à proposer à l'utilisateur
            //Le premier élément est le message et les autres sont les options
            //Les éléments sont entre guillemets et sont séparés les uns des autres par un espace
            line = String.Join(" ",splitLine,i,splitLine.Length - i);
            MatchCollection mc = Regex.Matches(line, "\"([^\"]*)\"");
            message = mc[0].Result("$1");
            for (i = 1; i< mc.Count; i++)
            {
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
                        //Liste des différents réponses devant être séléctionnées (cf manuel)
                        string []args = splitLine[1].Split(',');
                        caseL = new List<int>(); //Liste des réponses devant être séléctionnées
                        caseInst = new List<Instruction>(); //Liste des instructions
                        foreach(string st in args)
                        {
                            //On convertit la liste en int (la ligne se terminant par ':', on vérifie que le caractère n'est pas présent,sinon on l'enlève avant la conversion)
                            if(int.TryParse((st[st.Length - 1] == ':') ? st.Substring(0,st.Length - 1) : st,out caseNb))
                            caseL.Add(caseNb - 1);
                        }
                        //En entrant dans le "sous-bloc" case, il doit y avoir une tabulation supplémentaire, on retient celle de la déclaration (donc la dernière ligne lu)
                        newTabs = _currentTabNumber;
                        line = _read_file();
                        _currentTabNumber = _get_tab_number(line);
                        //Tant que l'on n'a pas quitté le bloc case
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
                        //Fonctionnement similaire au bloc case
                        caseL = new List<int>();
                        caseInst = new List<Instruction>();

                        //On ajoute le numéro de réponse CheckBoxInstr.DEFAULT_CASE
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

        /** Permet d'afficher un message d'avertissement ou d'erreur
        * \param mess Message à afficher
        * \param lineNumber Numéro de la ligne en cours de lecture
        * \param type Type de message : 'e' pour une erreur, 'w' pour un avertissement
        */
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
/*
        private static void _rewind_file()
        {
            
        }
*/
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
}