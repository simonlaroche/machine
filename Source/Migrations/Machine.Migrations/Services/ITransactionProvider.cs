﻿using System.Data;

namespace Machine.Migrations.Services
{
  public interface ITransactionProvider
  {
    IDbTransaction Begin();
    void Enlist(IDbCommand command);
  }
}