using Moths.Serialization;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class UIInteractableByCondition : MonoBehaviour
{
    [SerializeField] InterfaceReference<ICondition>[] _conditions;

    private CanvasGroup _group;

    private void Awake()
    {
        _group = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        if (_conditions == null) return;

        var interactable = true;
        foreach (var condition in _conditions) 
        {
            interactable = interactable && condition.Value.Test();
        }

        if (_group)
        {
            _group.interactable = interactable;
            _group.alpha = interactable ? 1 : 0.5f;
        }
    }
}
