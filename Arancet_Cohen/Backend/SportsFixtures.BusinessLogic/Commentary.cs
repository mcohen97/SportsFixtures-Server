using System;
using ObligatorioDA2.BusinessLogic.Exceptions;

namespace ObligatorioDA2.BusinessLogic
{
    public class Commentary
    {
        private int id;
        private string text;
        public User Maker { get; private set; }

        public Commentary(string aText, User aUser)
        {
            text = aText;
            Maker = aUser;
        }
        public Commentary(int id, string text, User aUser) : this(text, aUser)
        {
            Id = id;
        }

        public int Id { get; set; }
        public string Text { get { return text; } set { SetText(value); } }

        private void SetText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidCommentaryDataException("Text can't be empty");
            }

            text = value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is Commentary))
            {
                return false;
            }

            Commentary commentaryCompared = (Commentary)obj;

            return Id == commentaryCompared.Id;
        }
    }
}