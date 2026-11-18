using Eldoria.Domain;
namespace Eldoria.Valoria25D
{
    /// <summary>
    /// Read-only onboarding labels for the CANONICAL mission key. Never grants,
    /// completes or advances a mission. The original SliceRules decides all order.
    /// </summary>
    public static class City25DObjectiveCopy
    {
        public static string Read(PlayerState s)
        {
            if(s==null)return "Partida no disponible";
            string key=SliceRules.CurrentObjectiveKey(s);
            switch(key)
            {
                case "b1.build-sawmill":return "OBJETIVO: construye Aserradero · EDIFICIOS";
                case "b1.gather-wood":return "OBJETIVO: reúne madera · MUNDO";
                case "b1.gather-stone":return "OBJETIVO: reúne piedra · MUNDO";
                case "b1.clear-route":return "OBJETIVO: despeja la ruta · MUNDO";
                case "b1.return":return "OBJETIVO: regresa a Valoria · CIUDAD";
                case "b1.ascend":return "OBJETIVO: asciende a Bastión II";
                case "b2.build-barracks":return "OBJETIVO: construye Cuartel · EDIFICIOS";
                case "b2.train-archers":return "OBJETIVO: recluta arqueros · CUARTEL";
                case "b2.prepare-march":return "OBJETIVO: prepara tu MARCHA · CUARTEL";
                case "b2.raise-expedition-power":return "OBJETIVO: refuerza y prepara MARCHA";
                case "b2.defeat-engendro":return "OBJETIVO: derrota al Engendro · MUNDO";
                case "b2.complete":return "OBJETIVO: asciende a Bastión III";
                case "b3.granary":return "OBJETIVO: construye Granero · EDIFICIOS";
                case "b3.food":return "OBJETIVO: produce comida · GRANERO";
                case "b3.hunt":return "OBJETIVO: caza 2 presas · MUNDO > MISIONES";
                case "b3.investigate":return "OBJETIVO: investiga La Brecha · MUNDO";
                case "b3.lyra":return "OBJETIVO: contacta a Lyra · MUNDO";
                case "b3.fissure":return "OBJETIVO: cierra La Brecha · MUNDO";
                case "b3.infrastructure":return "OBJETIVO: Aserradero, Cuartel, Granero II";
                case "b3.complete":return "OBJETIVO: asciende a Bastión IV";
                case "b4.priority":return "OBJETIVO: elige prioridad · BASTIÓN IV";
                case "b4.ascend":return "OBJETIVO: asciende a Bastión V";
                case "b5.stoneworks":return "OBJETIVO: construye Cantera · EDIFICIOS";
                case "b5.stone":return "OBJETIVO: afianza la producción de piedra";
                case "b5.army":return "OBJETIVO: refuerza las tropas · CUARTEL";
                case "b5.rare":return "OBJETIVO: la amenaza rara · MUNDO";
                case "b5.complete":return "BASTIÓN V: capítulo consolidado";
                default:return "OBJETIVO: "+key;
            }
        }
    }
}
