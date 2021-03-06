using System;
using System.Collections.Generic;
using System.Linq;

namespace Voodoo.Messages.Paging
{
    public interface IGridState
    {
        int PageNumber { get; set; }
        int PageSize { get; set; }
        int TotalRecords { get; set; }
        string SortMember { get; set; }
        string SortDirection { get; set; }
        string DefaultSortMember { get; }
        bool ResetPaging { get; set; }
    }
}