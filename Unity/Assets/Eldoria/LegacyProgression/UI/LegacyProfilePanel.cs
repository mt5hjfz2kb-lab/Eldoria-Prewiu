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
        // Inject only the authenticated server cache/read adapter; no provider means unavailable.
        // Host calls Refresh on completed fetch; never perform network IO inside a row click.
        public void BindRankings(Func<RankingKind,RankingDetailSnapshot> reader)
        {rankingReader=reader;if(IsOpen&&rankingKind.HasValue)Render();}

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
            RebuildTabs();Render();
        }
        public void Refresh()
        {
            if(!IsOpen||gateway==null)return;
            profile=LegacyPlayerAdapter.Read(gateway);
            if(!new List<ProfileSection>(ProfilePresentation.Sections(profile)).Contains(section))
            {section=ProfileSection.Overview;collectionId=null;rankingKind=null;}
            RebuildTabs();Render();
        }
        public void Close()
        {
            if(panelRoot!=null)panelRoot.SetActive(false);
            Clear(generatedTabs);Clear(generatedRows);
            profile=null;collectionId=null;rankingKind=null;
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
                tab.onClick.AddListener(()=>{section=s;collectionId=null;rankingKind=null;Render();});
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
                AddRow(new ProfileEntry("back-ranking","Volver a clasificaciones","",ProfileEntryState.Information),()=>{rankingKind=null;Render();});
                RankingDetailSnapshot data=null;
                try { if(rankingReader!=null)data=rankingReader(kind); }
                catch(Exception e) { Debug.LogWarning("Ranking read unavailable: "+e.GetType().Name);data=new RankingDetailSnapshot(kind,RankingDetailStatus.Error); }
                foreach(var row in ProfilePresentation.RankingDetailRows(profile,kind,data))AddRow(row,null);
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
                if(section==ProfileSection.Rankings&&ProfilePresentation.TryRankingKind(id,out var kind))action=()=>{rankingKind=kind;Render();};
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
