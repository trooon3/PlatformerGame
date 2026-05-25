#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public sealed class EnemyTypeAssigner : EditorWindow
{
    [MenuItem("Tools/Назначить EnemyType врагам")]
    private static void ShowWindow()
    {
        GetWindow<EnemyTypeAssigner>("Назначить EnemyType");
    }

    private void OnGUI()
    {
        GUILayout.Label("Назначение EnemyType врагам", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        GUILayout.Label("По имени объекта:", EditorStyles.boldLabel);

        DrawNameButtons();

        EditorGUILayout.Space();

        GUILayout.Label("Ручное назначение:", EditorStyles.boldLabel);

        DrawManualAssignmentButton();
    }

    private void DrawNameButtons()
    {
        DrawButton("Swamp (болотные)", "swamp", EnemyType.Swamp);
        DrawButton("Skeleton (скелеты)", "skeleton", EnemyType.Skeleton);
        DrawButton("Demon (демоны)", "demon", EnemyType.Demon);
        DrawButton("Spider (пауки)", "spider", EnemyType.Spider);
        DrawButton("Zombie (зомби)", "zombie", EnemyType.Zombie);
    }

    private void DrawButton(string buttonName, string namePart, EnemyType enemyType)
    {
        if (GUILayout.Button(buttonName))
        {
            AssignEnemyTypeByName(namePart, enemyType);
        }
    }

    private void DrawManualAssignmentButton()
    {
        if (GUILayout.Button("Назначить выбранным объектам"))
        {
            ShowEnemyTypeSelector();
        }
    }

    private static void AssignEnemyTypeByName(string namePart, EnemyType enemyType)
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        int count = 0;

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.ToLower().Contains(namePart))
            {
                AddOrUpdateEnemyTypeComponent(obj, enemyType);
                count++;
            }
        }
    }

    private static void ShowEnemyTypeSelector()
    {
        GenericMenu menu = new GenericMenu();

        AddMenuItem(menu, "Default", EnemyType.Default);
        AddMenuItem(menu, "Swamp", EnemyType.Swamp);
        AddMenuItem(menu, "Skeleton", EnemyType.Skeleton);
        AddMenuItem(menu, "Demon", EnemyType.Demon);
        AddMenuItem(menu, "Spider", EnemyType.Spider);
        AddMenuItem(menu, "Zombie", EnemyType.Zombie);
        AddMenuItem(menu, "Boss", EnemyType.Boss);
        AddMenuItem(menu, "Flying", EnemyType.Flying);

        menu.ShowAsContext();
    }

    private static void AddMenuItem(GenericMenu menu, string name, EnemyType enemyType)
    {
        menu.AddItem(new GUIContent(name), false, () =>
            AssignEnemyTypeToSelected(enemyType));
    }

    private static void AssignEnemyTypeToSelected(EnemyType enemyType)
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            AddOrUpdateEnemyTypeComponent(obj, enemyType);
        }
    }

    private static void AddOrUpdateEnemyTypeComponent(GameObject obj, EnemyType enemyType)
    {
        EnemyTypeComponent component = obj.GetComponent<EnemyTypeComponent>();

        if (component == null)
        {
            component = obj.AddComponent<EnemyTypeComponent>();
        }

        component.SetEnemyType(enemyType);

        EditorUtility.SetDirty(obj);
    }
}
#endif