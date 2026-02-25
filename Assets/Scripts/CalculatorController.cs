using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Globalization;

public class CalculatorController : MonoBehaviour
{
    [SerializeField] private TMP_Text displayText;
    [SerializeField] private TMP_Text historyText;

    private string currentInput = "0";   
    private double? a = null;                       
    private Op currentOp = Op.None;      

    private enum Op { None, Add, Sub, Mul, Div }

    private readonly CultureInfo inv = CultureInfo.InvariantCulture;

    private List<string> historyItems = new List<string>();
    private string fullExpression = "";

    private void Start()
    {
        UpdateDisplay();
    }


    public void PressDigit(int digit)
    {
        if (currentInput == "0") currentInput = digit.ToString();
        else currentInput += digit.ToString();

            UpdateDisplay();
    }

    
    public void PressDot()
    {
        if (!currentInput.Contains("."))
        {
            currentInput += ".";
            UpdateDisplay();
        }
    }
   
    public void PressClear()
    {
        currentInput = "0";
        a = null;
        currentOp = Op.None;
        fullExpression = "";
        UpdateDisplay();
    }

    public void PressBackspace()
    {
        if (currentInput.Length <= 1) currentInput = "0";
        else currentInput = currentInput.Substring(0, currentInput.Length - 1);

        UpdateDisplay();
    }

    public void PressOperator(string opSymbol)
    {
        if (!double.TryParse(currentInput, NumberStyles.Any, inv, out double parsed)) return;

        a = parsed;
        currentOp = opSymbol switch
        {
            "+" => Op.Add,
            "-" => Op.Sub,
            "×" => Op.Mul,
            "/" => Op.Div,
            _ => Op.None
        };

        fullExpression = $"{a} {opSymbol} ";
        currentInput = "0";
        UpdateDisplay();
    }

    public void PressEquals()
    {
        if (currentOp == Op.None || a == null) return;

        if (!double.TryParse(currentInput, NumberStyles.Any, inv, out double b)) return;

        double result = 0;

        switch (currentOp)
        {
            case Op.Add: result = a.Value + b; break;
            case Op.Sub: result = a.Value - b; break;
            case Op.Mul: result = a.Value * b; break;
            case Op.Div:
                if (b == 0) { ShowError("Ошибка: /0"); return; }
                result = a.Value / b;
                break;
        }

        string record = $"{fullExpression}{b} = {result}";
        historyItems.Add(record);
        UpdateHistory();

        currentInput = result.ToString(inv);
        a = null;
        currentOp = Op.None;
        UpdateDisplay();

    }

    private void UpdateDisplay() => displayText.text = currentInput;

    private void UpdateHistory()
    {
        historyText.text = string.Join("\n", historyItems);
    }

    private void ShowError(string msg)
    {
        displayText.text = msg;
        currentInput = "0";
    }
}
