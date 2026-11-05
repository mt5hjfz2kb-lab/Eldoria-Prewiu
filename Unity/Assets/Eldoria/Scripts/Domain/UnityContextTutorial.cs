namespace Eldoria.Domain
{
    // Text guidance for an existing actionable objective; no modal, extra save,
    // timed task or reward. The actual player state controls the objective key.
    public static class UnityContextTutorial
    {
        // Teach first Mailbox use only after a genuine message arrives.
        // Opening and reading reports cannot grant duplicate loot.
        public static bool NeedsMailboxOpening(PlayerState state)
        {
            var box=state?.Mailbox;
            if(box?.Messages==null)return false;
            bool unread=false, read=false;
            foreach(var message in box.Messages)
            {
                if(message==null)continue;
                if(message.Read)read=true;else unread=true;
            }
            return unread&&!read;
        }

        public static string MailboxFirstUse(bool en) => en?
            "New Mailbox: open Mail and select your first message. Reading does not claim extra rewards.":
            "Nuevo Buzón: abre Correo y lee tu primer mensaje. Leer no concede recompensas adicionales.";

        public static string ForObjective(string key,bool en)
        {
            switch(key)
            {
                case "b2.train-archers": return en?
                    "Open the Barracks, choose how many archers to train and start. Wait until they are ready.":
                    "En el Cuartel elige cuántos arqueros entrenar e inicia la tarea. Espera a que terminen.";
                case "b2.prepare-march": return en?
                    "Open March, assign Aldric and troops, then Confirm. Preparing does not send the army.":
                    "Abre Marcha, asigna a Aldric y sus tropas y confirma. Preparar no envía el ejército.";
                case "b2.raise-expedition-power": return en?
                    "Expedition Power comes from heroes and assigned troops, not the Bastion. Adjust your March.":
                    "El Poder de expedición depende del héroe y sus tropas, no del Bastión. Ajusta la Marcha.";
                case "b3.food": return en?
                    "The Granary produces food; hunting adds more. Gather 500 to continue.":
                    "El Granero produce comida; la caza aporta más. Reúne 500 para continuar.";
                case "b3.hunt": return en?
                    "Choose a hunt in the World. Hunting yields food and materials, not kingdom Power.":
                    "Elige una cacería en el Mundo. Aporta comida y materiales, no Poder del reino.";
                case "b3.investigate": return en?
                    "Investigate the fissure for 20 minutes before taking further action.":
                    "Investiga la fisura durante 20 minutos antes de intentar otra acción.";
                case "b3.lyra": return en?
                    "Human tracks were found near the fissure. Approach to discover who left them.":
                    "Aldric ha hallado huellas humanas junto a la fisura. Acércate y averigua quién está allí.";
                case "b3.fissure": return en?
                    "Prepare Aldric for the assault alongside Lyra; the fissure must be closed.":
                    "Prepara a Aldric para el asalto junto a Lyra; aún queda cerrar la fisura.";
                case "b3.infrastructure": return en?
                    "Upgrade the Sawmill, Barracks and Granary to level 2 before ascending.":
                    "Mejora Aserradero, Cuartel y Granero a nivel 2 antes de ascender.";
                case "b4.priority": return en?
                    "Choose Production, Defence or Shelter. The choice does not lock the other paths.":
                    "Elige Producción, Defensa o Refugio. Las otras opciones seguirán disponibles.";
                case "b5.stoneworks": return en?
                    "Build Valoria's Stoneworks to start continuous stone production.":
                    "Construye la Cantera de Valoria para producir piedra de forma continua.";
                case "b5.stone": return en?
                    "The Stoneworks produces stone. You can also gather it in the World.":
                    "La Cantera produce piedra. También puedes recolectarla en el Mundo.";
                case "b5.army": return en?
                    "Train archers in the Barracks until you have 60, including troops on marches.":
                    "Entrena arqueros en el Cuartel hasta reunir 60; también cuentan los que están en marcha.";
                case "b5.rare": return en?
                    "The Ash Stalker is a combat threat, not a hunt. Prepare a March and review its report.":
                    "El Acechador es un enemigo, no una cacería. Prepara la Marcha y revisa el informe.";
                default: return null;
            }
        }
    }
}
