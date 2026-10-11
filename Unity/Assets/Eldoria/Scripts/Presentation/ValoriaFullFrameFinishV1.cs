using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    public static class ValoriaFullFrameFinishV1
    {
        public static bool Enabled=false;
        const string RootName="Valoria · Full Frame Finish v1";

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null)return;
            var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
            var root=new GameObject(RootName).transform;root.SetParent(parent,true);

            var camera=Camera.main;
            if(camera!=null)
            {
                camera.allowHDR=true;
                foreach(var component in camera.gameObject.GetComponents<Component>())
                    if(component!=null&&component.GetType().Name=="UniversalAdditionalCameraData")
                    {
                        var p=component.GetType().GetProperty("renderPostProcessing");
                        if(p!=null&&p.CanWrite)p.SetValue(component,true,null);
                    }
            }

            var volumeType=FindType("UnityEngine.Rendering.Volume");
            var profileType=FindType("UnityEngine.Rendering.VolumeProfile");
            if(volumeType==null||profileType==null)return;

            var go=new GameObject(RootName+" · global volume");
            go.transform.SetParent(root,true);
            var volume=go.AddComponent(volumeType);
            SetMember(volume,"isGlobal",true);
            SetMember(volume,"priority",120f);
            var profile=ScriptableObject.CreateInstance(profileType);
            SetMember(volume,"profile",profile);

            var tone=AddOverride(profile,"UnityEngine.Rendering.Universal.Tonemapping");
            OverrideEnum(tone,"mode","ACES");

            var color=AddOverride(profile,"UnityEngine.Rendering.Universal.ColorAdjustments");
            Override(color,"postExposure",.12f);
            Override(color,"contrast",14f);
            Override(color,"saturation",6f);
            Override(color,"colorFilter",new Color(1.00f,.985f,.955f,1f));

            var bloom=AddOverride(profile,"UnityEngine.Rendering.Universal.Bloom");
            Override(bloom,"threshold",1.05f);
            Override(bloom,"intensity",.16f);
            Override(bloom,"scatter",.52f);

            var vignette=AddOverride(profile,"UnityEngine.Rendering.Universal.Vignette");
            Override(vignette,"intensity",.045f);
            Override(vignette,"smoothness",.24f);

            var lift=AddOverride(profile,"UnityEngine.Rendering.Universal.LiftGammaGain");
            Override(lift,"lift",new Vector4(-.006f,-.004f,.002f,0f));
            Override(lift,"gamma",new Vector4(.015f,.008f,-.006f,0f));
            Override(lift,"gain",new Vector4(.035f,.018f,-.008f,0f));
        }

        static Type FindType(string full)
        {
            foreach(var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t=a.GetType(full,false);
                if(t!=null)return t;
            }
            return null;
        }

        static void SetMember(object target,string name,object value)
        {
            if(target==null)return;
            var type=target.GetType();
            var p=type.GetProperty(name);
            if(p!=null&&p.CanWrite){p.SetValue(target,value,null);return;}
            var f=type.GetField(name);
            if(f!=null)f.SetValue(target,value);
        }

        static object AddOverride(object profile,string typeName)
        {
            if(profile==null)return null;
            var t=FindType(typeName);if(t==null)return null;
            foreach(var m in profile.GetType().GetMethods())
            {
                if(m.Name!="Add")continue;
                var ps=m.GetParameters();
                if(!m.IsGenericMethod&&ps.Length==2&&ps[0].ParameterType==typeof(Type))
                    return m.Invoke(profile,new object[]{t,true});
                if(m.IsGenericMethodDefinition&&ps.Length==1&&ps[0].ParameterType==typeof(bool))
                    return m.MakeGenericMethod(t).Invoke(profile,new object[]{true});
            }
            return null;
        }

        static object GetMember(object target,string name)
        {
            if(target==null)return null;
            var t=target.GetType();
            var p=t.GetProperty(name);if(p!=null)return p.GetValue(target,null);
            var f=t.GetField(name);return f!=null?f.GetValue(target):null;
        }

        static void Override(object component,string parameterName,object value)
        {
            var parameter=GetMember(component,parameterName);if(parameter==null)return;
            foreach(var m in parameter.GetType().GetMethods())
            {
                if(m.Name!="Override")continue;
                var a=m.GetParameters();if(a.Length!=1)continue;
                var expected=a[0].ParameterType;object converted=value;
                if(value!=null&&!expected.IsInstanceOfType(value))
                {
                    try{converted=Convert.ChangeType(value,expected);}catch{continue;}
                }
                m.Invoke(parameter,new[]{converted});return;
            }
        }

        static void OverrideEnum(object component,string parameterName,string enumName)
        {
            var parameter=GetMember(component,parameterName);if(parameter==null)return;
            var p=parameter.GetType().GetProperty("value");
            if(p==null||!p.PropertyType.IsEnum)return;
            Override(component,parameterName,Enum.Parse(p.PropertyType,enumName));
        }
    }
}
