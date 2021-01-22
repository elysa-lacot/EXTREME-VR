using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

namespace ExtremeVR
{
    /**
    *  \class Scene 
    *  \author Sofiane Kerrakchou
    *  \brief Classe représentant un scénario
    *  
    */
    class Scene
    {
        /**
        *  \class Task 
        *  \author Sofiane Kerrakchou
        *  \brief Classe représentant une tâche
        */
        class Task
        {
            public const int TASK_TAKE = 1;
            public const int TASK_WAIT_UNORDERED = 2;
            public const int TASK_CHECK = 3;
            /** Prend une valeur parmi les constantes commençant par TASK */
            private int type;
            /** Contient des informations supplémentaires */
            private List<String> args;
            /** Dans le cas de tâches devant être accomplies dans un ordre précis, contient la position à laquelle la tâche doit être accomplies 
                (contient -1 si il n'y a pas d'ordre)  */
            private int order;
            /** Indique si la tâche a été accompli ou non */
            private bool isDone;
            /** Constructeur
            * \param type Prend une valeur parmi les constantes commençant par TASK
            * \param args Contient des informations supplémentaires 
            * \param order Dans le cas de tâches devant être accomplies dans un ordre précis, contient la position à laquelle la tâche doit être accomplies (contient -1 si il n'y a pas d'ordre)
            */
            public Task(int type, String []args, int order = -1)
            {
                this.type = type;
                this.args = new List<String>();
                foreach (String s in args)
                {
                    this.args.Add(s);
                }
                this.order = order;
                isDone = false;
            }

            /** Constructeur
            * \param type Prend une valeur parmi les constantes commençant par TASK
            * \param arg Contient une information supplémentaire
            * \param order Dans le cas de tâches devant être accomplies dans un ordre précis, contient la position à laquelle la tâche doit être accomplies (contient -1 si il n'y a pas d'ordre)
            */
            public Task(int type, String arg, int order = -1)
            {
                this.args = new List<String>();
                this.type = type;
                this.args.Add(arg);
                this.order = order;
                isDone = false;
            }

            public int getType() { return type; }
            public bool isTaskDone() { return isDone; }
            public void setTaskDone(bool b) { isDone = b; }
            public List<String> getArgs() { return args; }

            public override bool Equals(object obj)
            {
                if(this.type != ((Task)obj).type) return false;
                return args.Equals(((Task)obj).args);
            }
        }

        /**
        *  \class ObjectOptions 
        *  \author Sofiane Kerrakchou
        *  \brief Classe contenant les options (d'affichages pour l'instant, peut être amenée à contenir d'autres informations) concernant les objets de la scène Unity
        */
        public class ObjectOptions
        {
            /** Nom ou tag des objets concernés par les options */
            public String name;
            /** Indique que les options s'applique aux objets ayant le nom indiqué par l'attribut "name"*/
            public static int TYPE_OBJ_NAME = 1;
            /** Indique que les options s'applique aux objets ayant le tag indiqué par l'attribut "name"*/
            public static int TYPE_OBJ_TAG = 2;
            /** Prend comme valeur une des constantes commençants par TYPE */
            public int type;
            /** Indique si les objets doivent s'afficher ou non */
            public bool isShown;
        }
        /** Liste des tâches pouvant être effectuées dans n'importe quel ordre */
        private List<Task> _unorderedTasks; //<Task,isDone>
        /** Liste des tâches devant être effectuées dans un ordre précis */
        private List<Task> _orderedTasks;
        //private List<Task> _watchedAction;
        /** Liste des instructions à effectuer en cas de réussite */
        private List<Instruction> _successInst;
        /** Liste des instructions à effectuer en cas d'échec */
        private List<Instruction> _failInst;
        /** Liste des instructions à effectuer uniquement au premier lancement du scénario */
        private List<Instruction> _firstTryInst;
        /** Liste des instructions à effectuer au lancement du scénario (y compris en cas de redémarrage) */
        private List<Instruction> _atStartInst;
        /** Affichage des messages du scénario */
        private IPrintable _printOutput;
        /** List des objets pris */
        private List<String> _objInventory;
        /** Indique si l'utilisateur doit prendre uniquement les objets demandés (et pas un de plus) (cf manuel) */
        private bool _isTakeObjectStrict = false;

        public bool IsTakeObjectStrict { get{ return _isTakeObjectStrict;} set{ _isTakeObjectStrict = value;}}
        private bool _waitForValidation;
        /** Si vrai, la simulation attend que l'utilisateur vérifie manuellement si les tâches ont été effectuées */
        public bool WaitForValidation {get {return _waitForValidation;} set {_waitForValidation = value;}}
        public IPrintable PrintOutput { get { return _printOutput; } }
        private ISimulContext _simulContext;
        public ISimulContext SimulContext { get { return _simulContext; } set { _simulContext = value; }}
        private object _instLock;
        private bool _isSceneFunctionRunning;
        public bool IsSceneFunctionRunning { get { return _isSceneFunctionRunning; }}

        private Boolean _isUserInput;

        private Dictionary<List<int>,List<Instruction>> _checkbox_inst;
        private bool _checkbox_isStrict;

        /** Contient le nom de la scène Unity à charger */
        private String _unityScene;
        public String UnityScene { get { return _unityScene; } set { _unityScene = value; }}
        private List<ObjectOptions> _objOptions = new List<ObjectOptions>();
        public List<ObjectOptions> ObjOptions { get { return _objOptions; }}

        public Scene()
        {
            _unorderedTasks = new List<Task>();
            _orderedTasks = new List<Task>();
            _successInst = new List<Instruction>();
            _failInst = new List<Instruction>();
            _firstTryInst = new List<Instruction>();
            _atStartInst = new List<Instruction>();
            _objInventory = new List<String>();
            _isUserInput = false;
            WaitForValidation = false;
            _instLock = new object();
            _unityScene = "";
        }
        
        public void setPrintOutput(IPrintable p)
        {
            _printOutput = p;
        }

        public void AddInstructionToFirstTry(Instruction i)
        {
            if(i != null) _firstTryInst.Add(i);
        }

        public void AddInstructionToStart(Instruction i)
        {
            if(i != null) _atStartInst.Add(i);
        }

        public void AddObjOptions(String name, bool isShown, int type)
        {
            ObjectOptions ob = new ObjectOptions();
            ob.name = name;
            ob.isShown = isShown;
            ob.type = type;
            _objOptions.Add(ob);
        }

        /** Coroutine exécutant les instructions du bloc firsttry */
        public IEnumerator<WaitUntil> FirstTry()
        {
            lock(_instLock)
            {
                _isSceneFunctionRunning = true;
                foreach (Instruction i in _firstTryInst) 
                {
                    i.Execute(this);
                    if(i.Type == Instruction.PRINT_INST)
                    {
                            // On attend la confirmation de l'utilisateur avant de continuer (_isUserInput prend la valeur true en appelant la méthode SignalUserInput() )
                            _isUserInput = false;
                            yield return new WaitUntil(() => _isUserInput);
                            _isUserInput = false;
                    }
                    else if(i.Type == Instruction.LOAD_INST) break;
                    // L'interface des instructions checkbox n'est pas terminé et non fonctionnelle
                    else if(i.Type == Instruction.CHECKBOX_INST) 
                    {
                        yield return new WaitUntil( () => PrintOutput.IsWaitingForAnswer());
                        yield return new WaitUntil( () => !_checkbox_check_answers(PrintOutput.GetCheckboxAnswers()).MoveNext() );
                    }
                }
                _isSceneFunctionRunning = false;
                yield break;
            }
            yield break;
        }

        /** Coroutine exécutant les instructions du bloc start */
        public IEnumerator<WaitUntil> AtStart()
        {
            lock(_instLock)
            {
                _isSceneFunctionRunning = true;
                foreach (Instruction i in _atStartInst) 
                {
                    i.Execute(this);
                    if(i.Type == Instruction.PRINT_INST)
                    {
                            _isUserInput = false;
                            yield return new WaitUntil(() => _isUserInput);
                            _isUserInput = false;
                    }
                    else if(i.Type == Instruction.LOAD_INST) break;
                }
                _isSceneFunctionRunning = false;
            }
            yield break;
        }

        /** Redémarre le scénario */
        public void Restart()
        {
            _objInventory.Clear();
            // On annule toutes les tâches effectuées
            foreach(Task t in _orderedTasks )
            {
                t.setTaskDone(false);
            }
            foreach(Task t in _unorderedTasks )
            {
                t.setTaskDone(false);
            }
            _simulContext.RestartScene();
        }

        /** Ajoute une tâche "task" dans la liste des tâches sans ordre à respecter */
        public void addTakeObjectUnorderedTask(String objID)
        {
            Task tmp = new Task(Task.TASK_TAKE,objID);
            _unorderedTasks.Add(tmp);
        }

        /** Ajoute une tâche "task" dans la liste des tâches avec un ordre à respecter */
        public void addTakeObjectOrderedTask(String objID)
        {
            Task tmp = new Task(Task.TASK_TAKE,objID,_orderedTasks.Count + 1);
            _orderedTasks.Add(tmp);
        }
        
        /** Ajoute un objet avec l'identifiant objID à la liste des objets pris */
        public Boolean TakeObject(String objID)
        {
            Debug.Log("objID = " + objID);
            objID = objID.Split('.')[0];
            bool returnValue = false;
            // Cherche la première tâche "task" correspondant à l'objet "objID" et qui n'a pas déjà été effectuée dans la liste des tâches sans ordre
            Task tmp = _unorderedTasks.Find(t => t.getType() == Task.TASK_TAKE && t.getArgs()[0] == objID && !t.isTaskDone());
            if(tmp != null)
            {
                tmp.setTaskDone(true);
                returnValue = true;
            }
            // Cherche la première tâche "task" correspondant à l'objet "objID" et qui n'a pas déjà été effectuée dans la liste des tâches avec ordre
            int tmpIndex = _orderedTasks.FindIndex(t => t.getType() == Task.TASK_TAKE && t.getArgs()[0] == objID && !t.isTaskDone());
            if(tmpIndex >= 0)
            {
                // On vérifie si la tâche précédant celle que l'utilisateur vient d'accomplir a bien été effectuée */
                if(_orderedTasks[tmpIndex > 0 ? tmpIndex - 1 : 0].isTaskDone() || tmpIndex == 0)
                {
                    tmp.setTaskDone(true);
                    returnValue = true;
                }
            }
            _objInventory.Add(objID);

            if(!WaitForValidation) CheckTasks();

            return returnValue;
        }

        /* Enleve le premier objet trouvé avec l'identifiant objID de la liste des objets pris 
        *  Renvoie true si l'objet a été trouvé dans l'inventaire et supprimé, false sinon
        **/
        public Boolean DropObject(String objID)
        {
            bool returnValue = false;
            Task tmp = _unorderedTasks.Find(t => t.getType() == Task.TASK_TAKE && t.getArgs()[0] == objID);
            if(tmp != null)
            {
                tmp.setTaskDone(false);
                returnValue = true;
            }

            tmp = _orderedTasks.Find(t => t.getType() == Task.TASK_TAKE && t.getArgs()[0] == objID);
            if(tmp != null)
            {
                tmp.setTaskDone(false);
                returnValue = true;
            }
            if(_objInventory.Remove(objID)) returnValue = true;
            if(!WaitForValidation) CheckTasks();
            return returnValue;
        }

        /** Coroutine vérifiant si les tâches ont été effectuées et exéctute les instructions du bloc success ou fail selon la situation */
        public IEnumerator<IEnumerator<WaitUntil>> CheckTasks()
        {
            List<String> obj = new List<String>(_objInventory);
            foreach(Task t in _unorderedTasks)
            {
                if(!t.isTaskDone())
                {   
                    yield return fail();
                    yield break;
                }
                if(t.getType() == Task.TASK_TAKE)
                {
                    Debug.Log("Take " + t.getArgs()[0]);
                    obj.Remove(t.getArgs()[0]);
                }
            }

            foreach(Task t in _orderedTasks)
            {
                if(!t.isTaskDone())
                {
                    yield return fail();
                    yield break;
                }
                if(t.getType() == Task.TASK_TAKE)
                {
                    Debug.Log("Take " + t.getArgs()[0]);
                    obj.Remove(t.getArgs()[0]);
                }
            }

            if(IsTakeObjectStrict)
            {
                if(obj.Count > 0)
                {
                    yield return fail();
                    yield break;
                }
            }

            yield return success();
            yield break;
        }

        /** Coroutine exécutant les instructions du bloc success */
        private IEnumerator<WaitUntil> success()
        {
            _isSceneFunctionRunning = true;
            foreach (Instruction i in _successInst)
            {
                i.Execute(this);
                if(i.Type == Instruction.PRINT_INST)
                {
                        _isUserInput = false;
                        yield return new WaitUntil(() => _isUserInput);
                        _isUserInput = false;
                }
                else if(i.Type == Instruction.LOAD_INST) break;
            }
            _isSceneFunctionRunning = false;
            yield break;
        }
        
        /** Coroutine exécutant les instructions du bloc fail */
        private IEnumerator<WaitUntil> fail()
        {
            Debug.Log("Inside Fail");
            _isSceneFunctionRunning = true;
            foreach (Instruction i in _failInst)
            {
                i.Execute(this);
                if(i.Type == Instruction.PRINT_INST)
                {
                        _isUserInput = false;
                        yield return new WaitUntil(() => _isUserInput);
                        _isUserInput = false;
                }
                else if(i.Type == Instruction.LOAD_INST) break;
            }
            _isSceneFunctionRunning = false;
            yield break;
        }

        /** Ajoute une instruction au bloc success */
        public void AddInstructionToSuccess(Instruction i)
        {
            _successInst.Add(i);
        }

        /** Ajoute une instruction au bloc fail */
        public void AddInstructionToFail(Instruction i)
        {
            _failInst.Add(i);
        }

        /** Charge un nouveau scénario */
        public bool ChangeScene(string file)
        {
            return SimulContext.LoadScene(file);
        }

        /** Les instructions checkbox ne sont pas entièrement implémentés */
        public void CheckBox(string message, List<string> options, Dictionary<List<int>,List<Instruction>> inst, bool isStrict = false)
        {
            //List<int> input = new List<int>();
            PrintOutput.CheckboxToUser(message, options);
            _checkbox_inst = inst;
            _checkbox_isStrict = isStrict;
            //yield return new WaitUntil( () => { return PrintOutput.IsWaitingForAnswer(); });

        }

        /** Coroutine vérifiant les réponses données par l'utilisateur à une checkbox */
        private IEnumerator<WaitUntil> _checkbox_check_answers(List<int> input)
        {
            bool goToElse = true;
            List<int> elseKey = null;

            foreach (KeyValuePair<List<int>,List<Instruction>> kv in _checkbox_inst)
            {
                if(_checkbox_isStrict)
                {
                    if(Enumerable.SequenceEqual(input.OrderBy(e => e), kv.Key.OrderBy(e => e)))
                    {
                        goToElse = false;
                        foreach (Instruction i in kv.Value)
                        {
                            i.Execute(this);
                            if(i.Type == Instruction.PRINT_INST)
                            {
                                    _isUserInput = false;
                                    yield return new WaitUntil(() => _isUserInput);
                                    _isUserInput = false;
                            }
                            else if(i.Type == Instruction.LOAD_INST) break;
                            else if(i.Type == Instruction.CHECKBOX_INST) 
                            {
                                yield return new WaitUntil( () => PrintOutput.IsWaitingForAnswer());
                                //yield return _checkbox_check_answers(PrintOutput.GetCheckboxAnswers());
                            }
                        }
                    }
                    else if(kv.Key.Contains(CheckBoxInstr.DEFAULT_CASE)) elseKey = kv.Key;
                }
                else
                {
                    if(kv.Key.Intersect(input).ToList().Count == input.Count)
                    {
                        goToElse = false;
                        foreach (Instruction i in kv.Value)
                        {
                            i.Execute(this);
                            if(i.Type == Instruction.PRINT_INST)
                            {
                                    _isUserInput = false;
                                    yield return new WaitUntil(() => _isUserInput);
                                    _isUserInput = false;
                            }
                            else if(i.Type == Instruction.LOAD_INST) break;
                            else if(i.Type == Instruction.CHECKBOX_INST) 
                            {
                                yield return new WaitUntil( () => PrintOutput.IsWaitingForAnswer());
                                //yield return _checkbox_check_answers(PrintOutput.GetCheckboxAnswers());
                            }
                        }
                    }
                    else if(kv.Key.Contains(CheckBoxInstr.DEFAULT_CASE)) elseKey = kv.Key;
                }
            }

            if(goToElse && elseKey != null)
            {
                foreach(Instruction i in _checkbox_inst[elseKey])
                {
                    i.Execute(this);
                    if(i.Type == Instruction.PRINT_INST)
                    {
                            _isUserInput = false;
                            yield return new WaitUntil(() => _isUserInput);
                            _isUserInput = false;
                    }
                    else if(i.Type == Instruction.LOAD_INST) break;
                    else if(i.Type == Instruction.CHECKBOX_INST) 
                    {
                        yield return new WaitUntil( () => PrintOutput.IsWaitingForAnswer());
                        //yield return _checkbox_check_answers(PrintOutput.GetCheckboxAnswers());
                    }
                }
            }
        }
        public void SetUserFreeze(bool isFrozen)
        {
            SimulContext.SetUserFreeze(isFrozen);
        }

        public void WaitForUserInput()
        {
            _isUserInput = false;
        }

        public void SignalUserInput()
        {
            lock(_instLock)
            {
                _isUserInput = true;
            }
        }
    }
}