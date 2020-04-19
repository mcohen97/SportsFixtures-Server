using System;
using System.Collections.Generic;
using System.Text;

namespace ObligatorioDA2.Data.Entities
{
    public class EncounterEntity
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public ICollection<CommentEntity> Commentaries { get; set; }
        public SportEntity SportEntity { get; set; }
        public bool HasResult { get; set; }
    }
}
