using Newtonsoft.Json;
using RTLTMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterElement : GridElement
{
    public enum Gender
    {
        Male,
        Female,
        Count
    }
    [Serializable]
    private class Output
    {
        public Data self;
        public string parent;
    }

    [Serializable]
    private class Data
    {
        public string name = "";
        public string details = "";
        public Gender gender = Gender.Male;
    }


    [SerializeField] TMP_InputField _name;
    [SerializeField] TMP_InputField _details;

    [SerializeField] Color maleColor;
    [SerializeField] Color femaleColor;

    private Image _image;


    private Gender _gender = Gender.Male;

    protected override void Start()
    {
        base.Start();

        _image = GetComponent<Image>();
    }

    protected void Update()
    {
        switch (_gender)
        {
            case Gender.Male:
                _image.color = maleColor;
                break;

            case Gender.Female:
                _image.color = femaleColor;
                break;
        }
    }

    public void MarriageClickCallback()
    {
        ArrowController.Instance.PrepareConnection(guid, ConnectionType.Marriage);
    }

    public void SwitchGenderCallback()
    {
        int index = (int)_gender;
        index++;
        index %= (int)Gender.Count;
        _gender = (Gender)index;
    }


    public override string Serialize()
    {
        Output output = new Output();
        output.parent = base.Serialize();

        Data data = new Data();
        if (_name.textComponent is RTLTextMeshPro)
        {
            data.name = ((RTLTextMeshPro)_name.textComponent).OriginalText;
        }
        else
        {
            data.name = _name.textComponent.text;
        }
        if (_details.textComponent is RTLTextMeshPro)
        {
            data.details = ((RTLTextMeshPro)_details.textComponent).OriginalText;
        }
        else
        {
            data.details = _details.textComponent.text;
        }

        data.gender = _gender;

        output.self = data;

        return JsonConvert.SerializeObject(output);
    }

    public override void Deserialize(string str)
    {
        Output output = JsonConvert.DeserializeObject<Output>(str);
        base.Deserialize(output.parent);

        _name.text = output.self.name;
        _details.text = output.self.details;
        _gender = output.self.gender;

    }
}
