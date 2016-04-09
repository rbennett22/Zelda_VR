using UnityEngine;
using UnityEditor;
using System.Collections;

public class FindObjects : ScriptableWizard
{
    //search types
    public enum SearchType
    {
        Name,
        Component
    }

    //query parameters
    public string searchFor = "";
    public SearchType searchBy = SearchType.Name;
    public bool caseSensitive = false;
    public bool wholeWord = false;
    public bool inSelectionOnly = false;

    //stored parameters and results for Find Next
    static string lastSearch = "";
    static SearchType lastSearchType = SearchType.Name;
    static bool lastSearchWhole = false;
    static ArrayList foundItems;
    static int foundIndex = -1;


    //Menu item starts wizard
    [MenuItem("GameObject/Find... %&g")]
    static void FindMenuItem()
    {
        ScriptableWizard.DisplayWizard("Find", typeof(FindObjects), "", "Find");
    }

    //Main function
    void OnWizardOtherButton()
    {
        //Debug.Log("OnWizardCreate");

        //set static records
        lastSearch = searchFor;
        lastSearchType = searchBy;
        lastSearchWhole = wholeWord;

        //search space and results
        Object[] allObjects;
        foundItems = new ArrayList();

        if (inSelectionOnly)
            allObjects = Selection.objects;
        else
            allObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject));

        if (searchBy == SearchType.Name)
        {
            //name comparison
            if (wholeWord)
            {
                if (caseSensitive)
                {
                    foreach (GameObject anObject in allObjects)
                        if (anObject.name.ToLower().Equals(lastSearch.ToLower()))
                            foundItems.Add(anObject);
                }
                else
                {
                    foreach (GameObject anObject in allObjects)
                        if (anObject.name.Equals(lastSearch))
                            foundItems.Add(anObject);
                }
                if (foundItems.Count == 0)
                {
                    Debug.Log("No active objects were found with the name \"" + lastSearch + "\"");
                    foundIndex = -1;
                }
                else
                {
                    foundIndex = 0;
                    SelectObject(0);
                    AnnounceResult();
                }
            }
            else
            {
                if (caseSensitive)
                {
                    foreach (GameObject anObject in allObjects)
                        if (anObject.name.IndexOf(lastSearch) > -1)
                            foundItems.Add(anObject);
                }
                else
                {
                    foreach (GameObject anObject in allObjects)
                        if (anObject.name.ToLower().IndexOf(lastSearch.ToLower()) > -1)
                            foundItems.Add(anObject);
                }
                if (foundItems.Count == 0)
                {
                    Debug.Log("No active objects were found with names containing \"" + lastSearch + "\"");
                    foundIndex = -1;
                }
                else
                {
                    foundIndex = 0;
                    SelectObject(0);
                    AnnounceResult();
                }
            }
        }
        else
        { //component comparison
            foreach (GameObject objectByType in allObjects)
                if (objectByType.GetComponent(lastSearch))
                    foundItems.Add(objectByType);
            if (foundItems.Count == 0)
            {
                Debug.Log("No active objects were found with attached " +
                            "component \"" + lastSearch + "\"");
                foundIndex = -1;
            }
            else
            {
                foundIndex = 0;
                SelectObject(0);
                AnnounceResult();
            }

        }
    }

    void OnWizardUpdate()
    {
        //Debug.Log("OnWizardUpdate");

        //Make sure there is a search string
        if (searchFor.Equals(""))
        {
            errorString = "Enter a search and push enter";
            isValid = false;
        }
        else
        {
            errorString = "";
            isValid = true;
        }
        //make it obvious that you need an exact match for a Component search
        if (searchBy == SearchType.Name)
        {
            helpString = "";
        }
        else
        {
            if (!caseSensitive || !wholeWord)
            {
                caseSensitive = wholeWord = true;
            }
            helpString = "Component searches always require an exact match";
        }
    }

    //Next Result menu item
    [MenuItem("GameObject/Next Result %g")]
    static void NextResultMenuItem()
    {
        if (++foundIndex >= foundItems.Count)
            foundIndex = 0;
        SelectObject(foundIndex);
        AnnounceResult();
    }

    //Next is only available if there was a previous successful search
    [MenuItem("GameObject/Next Result %g", true)]
    static bool ValidateNextResult()
    {
        return foundIndex > -1;
    }

    //Previous Result menu item
    [MenuItem("GameObject/Previous Result #%g")]
    static void PreviousResultMenuItem()
    {
        if (--foundIndex < 0)
            foundIndex = foundItems.Count - 1;
        SelectObject(foundIndex);
        AnnounceResult();
    }

    //Find Next is only available if there was a previous successful search
    [MenuItem("GameObject/Previous Result #%g", true)]
    static bool ValidatePreviousResult()
    {
        return foundIndex > -1;
    }

    //tool for setting the selection by index in search results
    static void SelectObject(int newSelection)
    {
        Object[] newSelectionArray = { foundItems[newSelection] as Object };
        Selection.objects = newSelectionArray;
    }

    //Identifies the current search result
    static void AnnounceResult()
    {
        if (lastSearchType == SearchType.Component)
            Debug.Log("Object " + (foundIndex + 1) + " of " + foundItems.Count +
                        " with attached component \"" + lastSearch + "\"");
        else if (lastSearchWhole)
            Debug.Log("Object " + (foundIndex + 1) + " of " + foundItems.Count +
                        " with the name \"" + lastSearch + "\"");
        else
            Debug.Log("Object " + (foundIndex + 1) + " of " + foundItems.Count +
                        " with name containing \"" + lastSearch + "\"");
    }

}
