﻿using MORPH3D;
using UnityEngine;
using MORPH3D.FOUNDATIONS;
using MORPH3D.COSTUMING;

public class CharacterCreationSystem : MonoBehaviour {

    public Gender gender;
    public GameObject[] prefabs;

    public RectTransform clothesDisplay;

    public RectTransform hairDisplay;

    public GameObject CCItemPrefab;
    public GameObject CCHairPrefab;

    [HideInInspector]
    public M3DCharacterManager character;

    public GameObject ConfirmationPanel;

    public Avatar mAva;
    public Avatar fAva;
    public RuntimeAnimatorController aCont;

    #region Scene Singleton

    public static CharacterCreationSystem instance;

    void Awake()
    {
        if (instance != null) Destroy(instance.gameObject);
        instance = this;
    }

    #endregion

    // Use this for initialization
    void Start () {
        character = Instantiate(prefabs[(int)gender], Vector3.zero, new Quaternion(0, 180, 0, 0)).GetComponent<M3DCharacterManager>();
        character.ForceJawShut = true;
        Animator anim = character.gameObject.AddComponent<Animator>();
        anim.avatar = gender == Gender.Male ? mAva : fAva;
        anim.runtimeAnimatorController = aCont;

        DisplayClothes();

        DisplayHair();
    }

    public void GenderChanged(int val) {
        gender = (Gender)val;

        GameObject obj = Instantiate(prefabs[val], Vector3.zero, new Quaternion(0, 180, 0, 0));
        Animator anim = obj.AddComponent<Animator>();

        anim.avatar = gender == Gender.Male ? mAva : fAva;
        anim.runtimeAnimatorController = aCont;

        CharacterCreationSystemGUI.instance.Tabs[1].SetActive(true);

        DisplayClothes();

        CharacterCreationSystemGUI.instance.Tabs[2].SetActive(true);

        DisplayHair();

        M3DCharacterManager objChar = obj.GetComponent<M3DCharacterManager>();

        M3DCharacterManager newChar = CopyCharacterVars(character, objChar);

        Destroy(character.gameObject);

        character = newChar;
        character.ForceJawShut = true;
    }

    public void BodyChanged(float val, string[] blendshapes)
    {
        foreach (string s in blendshapes) {
            character.SetBlendshapeValue(s, val);
        }
    }

    public void BodyChanged(float val, string blendshape)
    {
        character.SetBlendshapeValue(blendshape, val);
    }

    void DisplayHair()
    {

        if (hairDisplay.childCount > 0)
        {

            for (int i = 0; i < hairDisplay.childCount; i++)
            {

                Destroy(hairDisplay.GetChild(i).gameObject);

            }

        }

        ThreeDArray tttd = GameManager.data.hair[(int)gender];

        foreach (TwoDArray ttd in tttd.Array)
        {
            Object[] hairs = ttd.Array;

            CCHairScript hairScript = Instantiate(CCHairPrefab, hairDisplay).GetComponent<CCHairScript>();
            hairScript.prefab = (GameObject)hairs[0];
            hairScript.texture = (Texture2D)hairs[1];

            //Object[] tmp = (Object[])hairs.Clone();

            for (int i = 2; i < hairs.Length; i++) {
                hairScript.colors.Add((Material)hairs[i]);
            }
        }

    }

    void DisplayClothes() {

        if (clothesDisplay.childCount > 0) {

            for (int i = 0; i < clothesDisplay.childCount; i++) {

                Destroy(clothesDisplay.GetChild(i).gameObject);

            }

        }

        ThreeDArray tttd = GameManager.data.clothes[(int)gender];

        foreach (TwoDArray ttd in tttd.Array) {
            Object[] wear = ttd.Array;

            CCItemScript itemScript = Instantiate(CCItemPrefab, clothesDisplay).GetComponent<CCItemScript>();
            itemScript.prefab = (GameObject)wear[0];
            itemScript.texture = (Texture2D)wear[1];
        }

    }

    public void Confirmation(int state) {
        switch (state) {
            case (0): // Open the panel
                ConfirmationPanel.SetActive(true);
                break;
            case (1): // No
                ConfirmationPanel.SetActive(false);
                break;
            case (2): // Yes
                Character ch = ScriptableObject.CreateInstance<Character>();
                ch.gender = gender;

                foreach (CCItemScript itemScript in clothesDisplay.GetComponentsInChildren<CCItemScript>())
                {
                    if (itemScript.Mark.enabled) {
                        ch.clothes.Add(new ContentPack(itemScript.prefab));
                    }
                }
                foreach (CCHairScript itemScript in hairDisplay.GetComponentsInChildren<CCHairScript>())
                {
                    if (itemScript.Mark.enabled)
                    {
                        ch.clothes.Add(new ContentPack(itemScript.prefab));
                    }
                }
                /*foreach (CIhair hair in character.GetAllHair())
                {
                    CIhair cl = Instantiate(hair);
                    DontDestroyOnLoad(cl);
                    ch.hair.Add(cl);
                }*/

                ch.blendshapes = character.GetActiveBlendShapes();

                GameManager.instance.currentCharacter = ch;
                GameManager.levelLoader.LoadLevel(2);
                break;
        }
    }

    T CopyComponent<T>(T original, GameObject destination) where T : Component
    {
        System.Type type = original.GetType();
        Component copy = destination.AddComponent(type);
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(copy, field.GetValue(original));
        }
        return copy as T;
    }


    M3DCharacterManager CopyCharacterVars(M3DCharacterManager original, M3DCharacterManager destination)
    {
        foreach (Morph m in original.GetActiveBlendShapes()) {
            destination.SetBlendshapeValue(m.localName, m.value);
        }
        return destination;
    }

}

public enum Gender {

    Male,
    Female

}