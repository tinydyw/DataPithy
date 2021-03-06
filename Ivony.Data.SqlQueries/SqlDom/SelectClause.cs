﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Ivony.Data.Queries;

namespace Ivony.Data.SqlQueries.SqlDom
{
  public class SelectClause
  {
    public SelectClause( List<SelectElement> elements )
    {
      Elements = new ReadOnlyCollection<SelectElement>( elements );
    }

    public IReadOnlyList<SelectElement> Elements { get; }

    public override string ToString()
    {
      return $"SELECT {string.Join( ", ", Elements )}";
    }
  }
}
