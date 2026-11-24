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
        [SerializeField] Text heading;
        [SerializeField] GameObject panelRoot;
        readonly List<GameObject> generatedTabs=new List<GameObject>();
        readonly List<GameObject> generatedRows=new List<GameObject>();
        ICommandGateway gateway;
        LocalProfile profile;
        ProfileSection section;
        string collectionId;

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
            section=ProfileSection.Overview;collectionId=null;
            panelRoot.SetActive(true);
            RebuildTabs();Render();
        }
        public void Refresh()
        {
            if(!IsOpen||gateway==null)return;
            profile=LegacyPlayerAdapter.Read(gateway);
            if(!new List<ProfileSection>(ProfilePresentation.Sections(profile)).Contains(section))
            {section=ProfileSection.Overview;collectionId=null;}
            RebuildTabs();Render();
        }
        public void Close()
        {
            if(panelRoot!=null)panelRoot.SetActive(false);
            Clear(generatedTabs);Clear(generatedRows);
            profile=null;collectionId=null;
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
                tab.onClick.AddListener(()=>{section=s;collectionId=null;Render();});
                generatedTabs.Add(tab.gameObject);tab.gameObject.SetActive(true);
            }
        }
        void Render()
        {
            Clear(generatedRows);
            heading.supportRichText=false;heading.text=ProfilePresentation.Label(section);
            if(collectionId!=null)
            {
                AddRow(new ProfileEntry("back","Volver a hallazgos","",ProfileEntryState.Information),
                    ()=>{collectionId=null;Render();});
                foreach(var row in ProfilePresentation.DiscoveryEntries(profile,collectionId))AddRow(row,null);
                return;
            }
            foreach(var row in ProfilePresentation.Rows(profile,section))
            {
                string id=row.Key;
                AddRow(row,section==ProfileSection.Discoveries?new Action(()=>{collectionId=id;Render();}):null);
            }
        }
        void AddRow(ProfileEntry data,Action action)
        {
            var row=Instantiate(rowTemplate,rowsRoot,false);
            row.Bind(data,action);
            generatedRows.Add(row.gameObject);row.gameObject.SetActive(true);
        }
    }
}
