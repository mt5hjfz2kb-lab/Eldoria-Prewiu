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
        // Optional themed rank marks in the shared row template, no new canvas.
        [SerializeField] GameObject firstPlaceMark,secondPlaceMark,thirdPlaceMark;
        public void Bind(ProfileEntry entry,Action activate)
        {
            if(!title||!detail)throw new InvalidOperationException("Profile row missing themed labels");
            title.supportRichText=false;detail.supportRichText=false;
            title.text=entry.Title;detail.text=entry.Detail;
            if(earnedMark)earnedMark.SetActive(entry.State==ProfileEntryState.Earned);
            if(lockedMark)lockedMark.SetActive(entry.State==ProfileEntryState.Locked);
            if(unavailableMark)unavailableMark.SetActive(entry.State==ProfileEntryState.Unavailable);
            if(firstPlaceMark)firstPlaceMark.SetActive(entry.RankingPosition==1);
            if(secondPlaceMark)secondPlaceMark.SetActive(entry.RankingPosition==2);
            if(thirdPlaceMark)thirdPlaceMark.SetActive(entry.RankingPosition==3);
            if(activate!=null&&!button)throw new InvalidOperationException("Navigable row requires a real themed Button");
            if(button)
            {
                button.onClick.RemoveAllListeners();
                button.interactable=activate!=null;
                if(activate!=null)button.onClick.AddListener(()=>activate());
            }
        }
    }
}
