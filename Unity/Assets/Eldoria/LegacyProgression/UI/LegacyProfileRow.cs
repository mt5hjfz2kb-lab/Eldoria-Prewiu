using System;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.LegacyProgression.UI
{
    // Assign existing approved text/button/state graphics in the prefab.
    public sealed class LegacyProfileRow : MonoBehaviour
    {
        [SerializeField] Text title,detail;
        [SerializeField] Button button;
        [SerializeField] GameObject earnedMark,lockedMark,unavailableMark;
        public void Bind(ProfileEntry entry,Action activate)
        {
            if(!title||!detail)throw new InvalidOperationException("Profile row missing themed labels");
            title.supportRichText=false;detail.supportRichText=false;
            title.text=entry.Title;detail.text=entry.Detail;
            if(earnedMark)earnedMark.SetActive(entry.State==ProfileEntryState.Earned);
            if(lockedMark)lockedMark.SetActive(entry.State==ProfileEntryState.Locked);
            if(unavailableMark)unavailableMark.SetActive(entry.State==ProfileEntryState.Unavailable);
            if(button)
            {
                button.onClick.RemoveAllListeners();
                button.interactable=activate!=null;
                if(activate!=null)button.onClick.AddListener(()=>activate());
            }
        }
    }
}
