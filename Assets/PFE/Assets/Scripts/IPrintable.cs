using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPrintable
{
    void PrintToUser(string text, int type, double time = -1);
    List<int> CheckboxToUser(string message,List<string> choices);
    bool IsWaitingForAnswer();
    List<int> GetCheckboxAnswers();
}

public static class PrintType
{
    public const int WITH_CONFIRMATION = 1;
    public const int WITH_TIMEOUT = 2;
}