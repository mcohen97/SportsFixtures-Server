

namespace ObligatorioDA2.Data.Entities
{
    public class EncounterTeam
    {
        public TeamEntity Team { get; set; }
        public int TeamNumber { get; set; }
        public EncounterEntity Encounter { get; set; }
        public int EncounterId { get; set; }
        public int Position { get; set; }
    }
}
