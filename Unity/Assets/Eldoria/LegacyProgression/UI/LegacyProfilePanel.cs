using System;
using System.Collections.Generic;
using Eldoria.Application;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.LegacyProgression.UI
{
    // Reusable binding for the approved UI kit, not a competing runtime HUD.
    // Explicitly bind actual themed controls and the SAME gateway as the city.
    // No automatic bootstrap, new global canvas, save or network access.
    public sealed class LegacyProfilePanel : MonoBehaviour
    {
        [SerializeField] RectTransform tabsRoot, rowsRoot;
        [SerializeField] Button tabTemplate, closeButton;
        [SerializeField] LegacyProfileRow rowTemplate;
        [SerializeField] Text heading, feedback;
        [SerializeField] GameObject panelRoot;
        readonly List<GameObject> generatedTabs=new List<GameObject>();
        readonly List<GameObject> generatedRows=new List<GameObject>();
        ICommandGateway gateway;
        LocalProfile profile;
        ProfileSection section;
        string collectionId;
        RankingKind? rankingKind;
        Func<RankingKind,RankingDetailSnapshot> rankingReader;
        Func<RankingKind,int,RankingDetailSnapshot> rankingPageReader;
        int rankOffset;
        float hubScroll=1f;
        ScrollRect RankingScroll => rowsRoot ? rowsRoot.GetComponentInParent<ScrollRect>() : null;
        float ScrollPosition => RankingScroll ? RankingScroll.verticalNormalizedPosition : 1f;
        void ScrollTo(float value)
        {
            var scroll=RankingScroll;if(!scroll)return;
            Canvas.ForceUpdateCanvases();LayoutRebuilder.ForceRebuildLayoutImmediate(rowsRoot);
            scroll.StopMovement();scroll.verticalNormalizedPosition=Mathf.Clamp01(value);
        }
        public void BindRankingPages(Func<RankingKind,int,RankingDetailSnapshot> reader)
        {rankingPageReader=reader;rankOffset=0;if(IsOpen&&rankingKind.HasValue){Render();ScrollTo(1f);}}
        // Inject only the authenticated server cache/read adapter; no provider means unavailable.
        // Host calls Refresh on completed fetch; never perform network IO inside a row click.
        public void BindRankings(Func<RankingKind,RankingDetailSnapshot> reader)
        {rankingReader=reader;rankingPageReader=null;rankOffset=0;if(IsOpen&&rankingKind.HasValue){Render();ScrollTo(1f);}}

        public bool IsOpen => panelRoot!=null&&panelRoot.activeSelf;
        public void Bind(ICommandGateway source)
        {
            gateway=source??throw new ArgumentNullException(nameof(source));
            if(!tabsRoot||!rowsRoot||!tabTemplate||!rowTemplate||!heading||!panelRoot||!closeButton)
                throw new InvalidOperationException("Bind the approved UI templates before opening");
            if(tabTemplate.gameObject.activeSelf||rowTemplate.gameObject.activeSelf)
                throw new InvalidOperationException("Templates must be inactive");
            closeButton.onClick.RemoveListener(Close);
            closeButton.onClick.AddListener(Close);
            panelRoot.SetActive(false);
        }
        public void Open()
        {
            if(gateway==null)throw new InvalidOperationException("Profile has no canonical gateway");
            profile=LegacyPlayerAdapter.Read(gateway);
            if(feedback)feedback.text="";
            section=ProfileSection.Overview;collectionId=null;rankingKind=null;
            panelRoot.SetActive(true);
            rankOffset=0;hubScroll=1f;RebuildTabs();Render();ScrollTo(1f);
        }
        public void Refresh()
        {
            if(!IsOpen||gateway==null)return;
            float scrollPosition=ScrollPosition;
            profile=LegacyPlayerAdapter.Read(gateway);
            if(!new List<ProfileSection>(ProfilePresentation.Sections(profile)).Contains(section))
            {section=ProfileSection.Overview;collectionId=null;rankingKind=null;rankOffset=0;scrollPosition=1f;}
            RebuildTabs();Render();ScrollTo(scrollPosition);
        }
        public void Close()
        {
            if(panelRoot!=null)panelRoot.SetActive(false);
            Clear(generatedTabs);Clear(generatedRows);
            profile=null;collectionId=null;rankingKind=null;rankOffset=0;hubScroll=1f;
        }
        void OnDestroy()
        {
            if(closeButton)closeButton.onClick.RemoveListener(Close);
        }
        static void Clear(List<GameObject> objects)
        {
            foreach(var go in objects)if(go){go.SetActive(false);Destroy(go);}
            objects.Clear();
        }
        void RebuildTabs()
        {
            Clear(generatedTabs);
            foreach(var s in ProfilePresentation.Sections(profile))
            {
                var tab=Instantiate(tabTemplate,tabsRoot,false);
                tab.onClick.RemoveAllListeners();
                var label=tab.GetComponentInChildren<Text>(true);
                if(!label){Destroy(tab.gameObject);throw new InvalidOperationException("Tab template needs a text label");}
                label.supportRichText=false;label.text=ProfilePresentation.Label(s);
                tab.onClick.AddListener(()=>{section=s;collectionId=null;rankingKind=null;rankOffset=0;Render();ScrollTo(1f);});
                generatedTabs.Add(tab.gameObject);tab.gameObject.SetActive(true);
            }
        }
        void Render()
        {
            Clear(generatedRows);
            heading.supportRichText=false;heading.text=ProfilePresentation.Label(section);
            if(rankingKind.HasValue)
            {
                var kind=rankingKind.Value;
                heading.text="Ranking de "+ProfilePresentation.RankingLabel(kind);
                AddRow(new ProfileEntry("back-ranking","Volver a clasificaciones","",ProfileEntryState.Information),()=>{rankingKind=null;rankOffset=0;Render();ScrollTo(hubScroll);});
                RankingDetailSnapshot data=null;
                try
                {
                    if(rankingPageReader!=null)data=rankingPageReader(kind,rankOffset);
                    else if(rankingReader!=null&&rankOffset==0)data=rankingReader(kind);
                    // A stale response must never be displayed for a different category/page/account.
                    if(data!=null&&(data.Kind!=kind||data.Offset!=rankOffset||
                        (data.OwnRow!=null&&data.OwnRow.PlayerId!=gateway.Snapshot().PlayerId)))
                        data=new RankingDetailSnapshot(kind,RankingDetailStatus.Error,offset:rankOffset);
                }
                catch(Exception e) { Debug.LogWarning("Ranking read unavailable: "+e.GetType().Name);data=new RankingDetailSnapshot(kind,RankingDetailStatus.Error,offset:rankOffset); }
                foreach(var row in ProfilePresentation.RankingDetailRows(profile,kind,data))AddRow(row,null);
                if(rankingPageReader!=null&&rankOffset>0)
                    AddRow(new ProfileEntry("page-prev","Página anterior","",ProfileEntryState.Information),()=>{rankOffset-=RankingDetailSnapshot.PageSize;Render();ScrollTo(1f);});
                if(rankingPageReader!=null&&data!=null&&data.Status==RankingDetailStatus.Ready&&data.HasMore)
                    AddRow(new ProfileEntry("page-next","Siguiente página","",ProfileEntryState.Information),()=>{rankOffset+=RankingDetailSnapshot.PageSize;Render();ScrollTo(1f);});
                return;
            }
            if(collectionId!=null)
            {
                AddRow(new ProfileEntry("back","Volver a hallazgos","",ProfileEntryState.Information),
                    ()=>{collectionId=null;Render();});
                foreach(var row in ProfilePresentation.DiscoveryEntries(profile,collectionId))AddRow(row,null);
                return;
            }
            foreach(var row in ProfilePresentation.Rows(profile,section,DateTime.UtcNow))
            {
                string id=row.Key;
                Action action=null;
                if(section==ProfileSection.Rankings&&ProfilePresentation.TryRankingKind(id,out var kind))action=()=>{hubScroll=ScrollPosition;rankingKind=kind;rankOffset=0;Render();ScrollTo(1f);};
                else if(section==ProfileSection.Discoveries)action=()=>{collectionId=id;Render();};
                else if(!string.IsNullOrEmpty(row.CommandKind))action=()=>Select(row);
                AddRow(row,action);
            }
        }
        void Select(ProfileEntry entry)
        {
            var s=gateway.Snapshot();
            var result=gateway.Execute(new GameCommand("legacy:"+Guid.NewGuid().ToString("N"),
                s.PlayerId,entry.CommandKind,entry.CommandTarget,s.Revision));
            if(feedback){feedback.supportRichText=false;feedback.text=result.Message;}
            Refresh();
        }
        void AddRow(ProfileEntry data,Action action)
        {
            var row=Instantiate(rowTemplate,rowsRoot,false);
            row.Bind(data,action);
            generatedRows.Add(row.gameObject);row.gameObject.SetActive(true);
        }
    }
}
