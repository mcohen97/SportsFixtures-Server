using System;
using System.Collections.Generic;
using System.Text;
using BusinessLogic;

namespace DataRepositoryInterfaces
{
    public interface ICommentRepository
    {
        void Clear();
        bool IsEmpty();
        void AddComment(Commentary comment, Match aMatch);
    }
}
