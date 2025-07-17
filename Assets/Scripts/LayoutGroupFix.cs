using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Unpainted.UI.Components
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class LayoutGroupFix : MonoBehaviour
    {
        [SerializeField] private bool fixOnEnable = true;
        [SerializeField] private bool fixWithDelay = true;
        float fixDelay = 0.1f;

        void OnEnable()
        {
#if UNITY_EDITOR
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            if (Application.isPlaying == false) { return; }
#endif
            if (fixWithDelay == false && fixOnEnable == true) { LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>()); }
            else if (fixWithDelay == true) { StartCoroutine(FixDelay()); }
        }

        public void FixLayout()
        {
            if (fixWithDelay == false) { LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>()); }
            else { StartCoroutine(FixDelay()); }
        }

        IEnumerator FixDelay()
        {
            GetComponent<LayoutGroup>().enabled = false;
            yield return new WaitForSecondsRealtime(fixDelay);
            GetComponent<LayoutGroup>().enabled = true;
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }
    }
}