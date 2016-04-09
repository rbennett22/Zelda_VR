using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class MenuController : MonoBehaviour 
{
    public const string ClassName = "MenuController";


    #region GUI Properties

    // GUI Text Colors
    static Color NonSpeechColor = Color.white;
    static Color SpeechColor    = Color.cyan;


    // GUI Text
    const string MenuTitle_Text          = "Speech Demo: Making Choices";
    const string PresentChoices_Text1    = "\tChoose an Option by speaking it.";
    const string PresentChoices_Text2A   = "\t\t{0})\t";
    const string PresentChoices_Text2B   = "\t\t\t\t{0}";
    const string ShowResults_Text1       = "\tIt's official - you chose \"{0}\"!";
    const string SpeechRecIsLoading_Text = "Speech Recognizer is Loading.  Please Wait...";

    // GUI ColoredTextItems (for assigning different colors to different words in a sentence)
    ColoredTextItem[] AskForConfirmation_TextItems = 
    { 
        new ColoredTextItem("\tYou chose \"{0}\".  Is this correct? (", NonSpeechColor),
        new ColoredTextItem("Yes",     SpeechColor),
        new ColoredTextItem(" or",     NonSpeechColor),
        new ColoredTextItem(" No",     SpeechColor),
        new ColoredTextItem(")",       NonSpeechColor),
    };

    ColoredTextItem[] ShowResults_TextItems = 
    { 
        new ColoredTextItem("\tSay \"",         NonSpeechColor),
        new ColoredTextItem("Try Again",        SpeechColor),
        new ColoredTextItem("\" to try again.", NonSpeechColor)
    };


    // GUI Layout
    public float width = 340;
    public float height = 360;

    public int lineHeight = 30;
    public int fontSize = 16;

    public Rect GuiArea {
        get { return new Rect((Screen.width  - width)  * 0.5f,
                              (Screen.height - height) * 0.5f,
                               width, height); }
    }

    #endregion


    public bool verbose = true;


    public bool SpeechRecognizerIsLoading { get; set; }


    #region enum MenuPhase

    public enum MenuPhaseEnum
    {
        PresentChoices,
        AskForConfirmation,
        ShowResults
    }

    MenuPhaseEnum _menuPhase = MenuPhaseEnum.PresentChoices;
    public MenuPhaseEnum MenuPhase { 
        get { return _menuPhase; }
        private set {
            if (value == _menuPhase) { return; }
            _menuPhase = value;
            OnMenuPhaseChanged(_menuPhase);
        }
    }

    List<MenuPhaseEnum> _menuPhasesInOrder = new List<MenuPhaseEnum>() {
        MenuPhaseEnum.PresentChoices,
        MenuPhaseEnum.AskForConfirmation,
        MenuPhaseEnum.ShowResults
    };
    void GotoInitialMenuPhase()
    {
        MenuPhase = _menuPhasesInOrder[0];
    }
    void GotoNextMenuPhase()
    {
        int currIdx = _menuPhasesInOrder.IndexOf(MenuPhase);
        int nextIdx = Mathf.Min(currIdx + 1, _menuPhasesInOrder.Count - 1);
        MenuPhase = _menuPhasesInOrder[nextIdx];
    }
    void GotoPreviousMenuPhase()
    {
        int currIdx = _menuPhasesInOrder.IndexOf(MenuPhase);
        int prevIdx = Mathf.Max(0, currIdx - 1);
        MenuPhase = _menuPhasesInOrder[prevIdx];
    }

    #endregion


    #region enum Choice

    public enum Choice {
        One,
        Two,
        Three
    }

    // AllChoices
    static List<Choice> _allChoices = new List<Choice>() {
        Choice.One,
        Choice.Two,
        Choice.Three
    };

    // ChoicesForString
    static Dictionary<string, Choice> _ChoicesForString = new Dictionary<string, Choice> {
		{ Choice.One.ToString().ToUpper(),    Choice.One },
		{ Choice.Two.ToString().ToUpper(),    Choice.Two },
		{ Choice.Three.ToString().ToUpper(),  Choice.Three }
	};
    static public bool TryGetChoiceForString(string str, out Choice choice) {
        if (str == null) { str = string.Empty; }
        return _ChoicesForString.TryGetValue(str.ToUpper(), out choice);
    }

    Choice _pendingChoice;
    Choice _finalChoice;

    #endregion


    #region Custom Events

    // --- OnMenuPhaseChanged ---

    public sealed class MenuPhaseChanged_EventArgs : EventArgs
    {
        public MenuPhaseEnum NewMenuPhase { get; private set; }

        public MenuPhaseChanged_EventArgs(MenuPhaseEnum newMenuPhase)
        {
            NewMenuPhase = newMenuPhase;
        }
    }

    public event EventHandler<MenuPhaseChanged_EventArgs> MenuPhaseChanged;

    void OnMenuPhaseChanged(MenuPhaseEnum newMenuPhase)
    {
        if (verbose) { print(ClassName + " :: OnMenuPhaseChanged:  " + newMenuPhase); }

        EventHandler<MenuPhaseChanged_EventArgs> handler = MenuPhaseChanged;
        if (handler == null) { return; }
        handler(this, new MenuPhaseChanged_EventArgs(newMenuPhase));
    }

    #endregion


    void Awake()
    {
        SpeechRecognizerIsLoading = true;
    }


    #region Actions

    public void SelectChoice(Choice choice)
    {
        if (MenuPhase != MenuPhaseEnum.PresentChoices) { return; }
        if (verbose) { print(ClassName + " :: Choice Selected:  " + choice); }

        _pendingChoice = choice;
        GotoNextMenuPhase();
    }

    public void ConfirmPendingChoice()
    {
        if (MenuPhase != MenuPhaseEnum.AskForConfirmation) { return; }
        if (verbose) { print(ClassName + " :: Pending Choice Confirmed:  " + _pendingChoice); }

        _finalChoice = _pendingChoice;
        GotoNextMenuPhase();
    }

    public void DenyPendingChoice()
    {
        if (MenuPhase != MenuPhaseEnum.AskForConfirmation) { return; }
        if (verbose) { print(ClassName + " :: Pending Choice Denied:  " + _pendingChoice); }

        GotoPreviousMenuPhase();
    }

    public void Restart()
    {
        if (MenuPhase != MenuPhaseEnum.ShowResults) { return; }
        if (verbose) { print(ClassName + " :: Restart"); }

        GotoInitialMenuPhase();
    }

    #endregion


    #region GUI

    void OnGUI()
    {
        int oldFontSize = GUI.skin.GetStyle("Label").fontSize;
        GUI.skin.GetStyle("Label").fontSize = fontSize;

        Rect position = GuiArea;


        GUI_MenuTitleLabel(ref position);

        NextLine(ref position, 2);


        if (SpeechRecognizerIsLoading)
        {
            GUI_WaitingOnSpeechRecognizer(ref position);
        }
        else
        {
            switch (MenuPhase)
            {
                case MenuPhaseEnum.PresentChoices: GUI_PresentChoices(ref position); break;
                case MenuPhaseEnum.AskForConfirmation: GUI_AskForConfirmation(ref position); break;
                case MenuPhaseEnum.ShowResults: GUI_ShowResults(ref position); break;
            }
        }

        GUI.skin.GetStyle("Label").fontSize = oldFontSize;
    }

    void GUI_MenuTitleLabel(ref Rect position)
    {
        GUIStyle styleForTitle = new GUIStyle(GUI.skin.GetStyle("Label"));
        styleForTitle.fontStyle = FontStyle.Bold;
        styleForTitle.fontSize = fontSize + 4;
        GUI.Label(position, MenuTitle_Text, styleForTitle);
    }

    void GUI_WaitingOnSpeechRecognizer(ref Rect position)
    {
        NextLine(ref position, 1);

        string text = SpeechRecIsLoading_Text;
        Color color = new Color(0.9f, 0.8f, 0);
        GUI_ColoredLabel(position, text, color);
    }

    void GUI_PresentChoices(ref Rect position)
    {
        GUI.Label(position, PresentChoices_Text1);

        NextLine(ref position);

        GUI_ChoiceLabels(ref position, true);
    }

    void GUI_AskForConfirmation(ref Rect position)
    {
        GUI.Label(position, PresentChoices_Text1);

        NextLine(ref position);

        GUI_ChoiceLabels(ref position, false, true);

        NextLine(ref position);


        string storedText = AskForConfirmation_TextItems[0].text;
        {
            string text = string.Format(AskForConfirmation_TextItems[0].text, _pendingChoice);
            AskForConfirmation_TextItems[0].text = text;

            ColoredTextItem.GUI_ColoredTextItemsLabel(position, AskForConfirmation_TextItems);
        }
        AskForConfirmation_TextItems[0].text = storedText;
    }

    void GUI_ShowResults(ref Rect position)
    {
        NextLine(ref position, 1);

        string text = string.Format(ShowResults_Text1, _finalChoice);
        GUI.Label(position, text);

        NextLine(ref position, 2);

        ColoredTextItem.GUI_ColoredTextItemsLabel(position, ShowResults_TextItems);
    }


    void GUI_ChoiceLabels(ref Rect position, bool colorSpeechWords, bool highlightPendingChoice = false)
    {
        int i = 0;
        foreach (Choice choice in _allChoices)
        {
            i++;

            // ChoiceNumber (ie  "1)")
            string text = string.Format(PresentChoices_Text2A, i);
            Color color = NonSpeechColor;
            if (highlightPendingChoice)
            {
                color.a = (choice == _pendingChoice) ? 1.0f : 0.3f;
            }
            GUI_ColoredLabel(position, text, color);

            // ChoiceText (ie  "One")
            text = string.Format(PresentChoices_Text2B, choice);
            color = colorSpeechWords ? SpeechColor : NonSpeechColor;
            if (highlightPendingChoice)
            {
                color.a = (choice == _pendingChoice) ? 1.0f : 0.3f;
            }
            GUI_ColoredLabel(position, text, color);

            NextLine(ref position);
        }
    }

    void GUI_ColoredLabel(Rect position, string text, Color color )
    {
        Color oldColor = GUI.color;
        GUI.color = color;
        {
            GUI.Label(position, text);
        }
        GUI.color = oldColor;
    }

    void NextLine(ref Rect position, UInt32 numLines = 1)
    {
        position.x = GuiArea.x;
        position.y += numLines * lineHeight;
    }


    // This class serves to help show individual words of a given sentence in different colors
    class ColoredTextItem
    {
        public String text;
        public Color color;

        public ColoredTextItem(String text, Color color)
        {
            this.text = text;
            this.color = color;
        }

        static public void GUI_ColoredTextItemsLabel(Rect position, ColoredTextItem[] textItems)
        {
            Rect rect = position;
            GUIStyle style = new GUIStyle("Label");

            Color oldColor = GUI.color;

            foreach (ColoredTextItem thisItem in textItems)
            {
                GUIContent content = new GUIContent(thisItem.text);
                Vector2 size = style.CalcSize(content);

                GUI.color = thisItem.color;

                rect.width = size.x;

                GUI.Label(rect, content, style);

                rect.x += size.x;     // set the x-value of the next box to the end of the last box
            }

            GUI.color = oldColor;
        }
    }

    #endregion

}
