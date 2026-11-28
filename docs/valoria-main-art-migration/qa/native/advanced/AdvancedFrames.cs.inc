        // Runs inside the canonical editor fixture in a disposable source checkout only.
        static System.Collections.IEnumerator AdvancedGuard()
        {
            string save = null;
            byte[] before = null;
            Exception failure = null;
            try
            {
                Need(host.City.Probe && host.City.Clock is ValoriaReferenceCleanGameplay.TestClock,
                    "Advanced inventory requires a disposable probe and test clock.");
                save = host.City.SaveFile;
                before = File.Exists(save) ? File.ReadAllBytes(save) : null;
            }
            catch (Exception error) { failure = error; }
            if (failure != null)
            {
                Debug.LogException(failure);
                EditorApplication.Exit(1);
                yield break;
            }
            var sequence = AdvancedFrames();
            for (;;)
            {
                bool more = false;
                try { more = sequence.MoveNext(); }
                catch (Exception error) { failure = error; }
                if (!more || failure != null) break;
                yield return sequence.Current;
            }
            try
            {
                if (before == null) File.Delete(save);
                else File.WriteAllBytes(save,before);
                Need(before == null ? !File.Exists(save) : before.SequenceEqual(File.ReadAllBytes(save)),
                    "Disposable probe save restoration failed.");
            }
            catch (Exception error) { Debug.LogException(error); failure = error; }
            if (failure != null) Debug.LogException(failure);
            EditorApplication.Exit(failure == null ? 0 : 1);
        }

        static System.Collections.IEnumerator AdvancedFrames()
        {
            var ui = BreachWatchHud.Install(host);
            Need(ui && ui.ProductionPresentation && ui.IndependentCanvas && ui.LegacyPresentationHidden,
                "Main production presentation is required.");
            var folder = Path.Combine(Environment.GetEnvironmentVariable("VALORIA_MIGRATION_OUTPUT"),"advanced-fixture");
            Directory.CreateDirectory(folder);
            void Frame(string label, Func<bool> verify)
            {
                Capture(ui,folder,label,390,844,verify);
                Capture(ui,folder,label,844,390,verify);
            }
            var seed = host.City.Gateway.Snapshot();
            var marchSeed = JsonUtility.FromJson<PlayerState>(JsonUtility.ToJson(seed));
            seed.EngendroDefeated = seed.ScoutDefeated = seed.JourneyComplete = true;
            seed.ChapterThree.FissureClosed = seed.ChapterThree.LyraRecruited = true;
            seed.ChapterProgress.MarchConfirmed = true;
            seed.Available = new ArmyRoster { ArcherT1 = 195 };
            seed.PreparedTroops = new ArmyRoster();
            seed.MarchConfigured = false;
            seed.March = new MarchState();
            seed.ExtraMarches = new List<MarchState>();
            seed.ChapterThree.InvestigationEndsUtcTicks = 0;
            seed.Retention.Orders = new List<ScoutOrder>();
            foreach (var beat in new[]{"start","world","mark","barracks","secured","frontier-open","fissure-closed",
                     "march-preparation","vigilants-introduction","scouts-introduction"})
                if (!seed.ChapterProgress.SeenNarrative.Contains(beat)) seed.ChapterProgress.SeenNarrative.Add(beat);
            foreach (int level in new[]{4,5})
            {
                seed.BastionLevel = level;
                seed.SawmillLevel = seed.BarracksLevel = seed.ChapterThree.GranaryLevel = level;
                seed.ChapterThree.StoneworksLevel = level == 5 ? 5 : 0;
                seed.ChapterThree.DevelopmentPriority = level == 4 ? "" : "civil";
                new Eldoria.Infrastructure.FileStateStore(host.City.SaveFile).Save(seed);
                host.City.ReloadForProbe();
                yield return new WaitForSecondsRealtime(.5f);
                for (int page=0; page<40 && host.Hud.NarrativeOpen; page++)
                {
                    host.Hud.Modal.Extra.onClick.Invoke();
                    yield return null;
                }
                Need(!host.Hud.NarrativeOpen,"Advanced city fixture still has an open narrative.");
                ui.SelectBuilding("");
                yield return null;
                Frame("city-level-"+level,()=>!host.InWorld && host.City.Gateway.Snapshot().BastionLevel == level);
                if (level == 4)
                {
                    ui.SelectBuilding("bastion");
                    yield return null;
                    Frame("bastion-before-priority",()=>ui.SelectedBuildingId=="bastion" &&
                          string.IsNullOrEmpty(host.City.Gateway.Snapshot().ChapterThree.DevelopmentPriority));
                    ui.SelectBuilding("");
                    ui.Reflow(390,844);
                    var objective = ui.NativeCanvas.GetComponentsInChildren<Button>()
                        .Single(x=>x.name=="Chapter side tablet");
                    var expanded=typeof(BreachWatchHud).GetField("fullObjective",BindingFlags.Instance|BindingFlags.NonPublic);
                    Need(expanded!=null && !(bool)expanded.GetValue(ui),"Expected collapsed objective.");
                    ClickActualMailControl(ui,objective,false);
                    Frame("objective-expanded",()=>(bool)expanded.GetValue(ui));
                    ClickActualMailControl(ui,objective,false);
                }
            }
            host.TrySetWorld(true);
            for (int i=0; i<600 && !host.InWorld; i++) yield return null;
            Need(host.InWorld,"Canonical world entry unavailable.");
            foreach (int preset in new[]{2,1,0})
            {
                host.WorldNavigation.SetZoomLevel(preset,true);
                yield return new WaitForSecondsRealtime(.25f);
                Frame("world-preset-"+preset,()=>host.InWorld && host.WorldCamera.enabled);
            }
            // Reuse the canonical three-march fixture's Barracks I troop capacity.
            marchSeed.SawmillLevel=1;
            marchSeed.BastionLevel=3;
            marchSeed.ChapterThree.LyraRecruited=true;
            marchSeed.BarracksLevel=1;
            marchSeed.Available=new ArmyRoster { ArcherT1=195 };
            marchSeed.MarchConfigured=false;
            marchSeed.PreparedTroops=new ArmyRoster();
            marchSeed.March=new MarchState();
            marchSeed.ExtraMarches=new List<MarchState>();
            marchSeed.ChapterThree.InvestigationEndsUtcTicks=0;
            marchSeed.Retention.Orders=new List<ScoutOrder>();
            new Eldoria.Infrastructure.FileStateStore(host.City.SaveFile).Save(marchSeed);
            host.City.ReloadForProbe();
            yield return new WaitForSecondsRealtime(.3f);
            if (!host.InWorld) host.TrySetWorld(true);
            for (int i=0; i<600 && !host.InWorld; i++) yield return null;
            Need(host.InWorld,"World view was not restored after installing the marches fixture.");
            var gateway = host.City.Gateway;
            void Command(string id,string kind,string target)
            {
                var live=gateway.Snapshot();
                Need(gateway.Execute(new GameCommand(id,live.PlayerId,kind,target,live.Revision)).Ok,
                    "Main gateway rejected advanced fixture command "+id);
            }
            Command("inventory-gather","Gather","forest-valoria");
            Command("inventory-prepare-lyra","ConfigureMarch","march-main:60:lyra");
            Command("inventory-quarry","Gather","quarry-valoria");
            Command("inventory-prepare-aldric","ConfigureMarch","march-main:60:aldric");
            Command("inventory-fight","Fight","corrupt-scout");
            ((ValoriaReferenceCleanGameplay.TestClock)host.City.Clock).Step(1);
            yield return new WaitForSecondsRealtime(.4f);
            Frame("three-active-marches",()=>host.InWorld &&
                MarchCapacityRules.ActiveCount(gateway.Snapshot(),host.City.Clock.UtcTicks)==3);
        }
