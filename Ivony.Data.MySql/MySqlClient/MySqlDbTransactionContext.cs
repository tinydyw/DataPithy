﻿using Ivony.Data.Common;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Data.MySqlClient
{
  public class MySqlDbTransactionContext : DbTransactionContextBase<MySqlDbExecutor, MySqlTransaction>
  {




    /// <summary>
    /// 获取数据库连接
    /// </summary>
    public MySqlConnection Connection
    {
      get;
      private set;
    }



    internal MySqlDbTransactionContext( DbEnv environment, string connectionString )
    {
      Connection = new MySqlConnection( connectionString );
      _executor = new MySqlDbExecutorWithTransaction( environment, this );
    }



    /// <summary>
    /// 打开数据库连接并创建数据库事务对象。
    /// </summary>
    /// <returns>SQL Server 数据库事务对象</returns>
    protected override MySqlTransaction CreateTransaction()
    {
      if ( Connection.State == ConnectionState.Closed )
        Connection.Open();

      return Connection.BeginTransaction();
    }



    private MySqlDbExecutor _executor;

    /// <summary>
    /// 获取用于在事务中执行查询的 MySql 查询执行器
    /// </summary>
    public override MySqlDbExecutor DbExecutor
    {
      get { return _executor; }
    }


    private class MySqlDbExecutorWithTransaction : MySqlDbExecutor
    {

      public MySqlDbExecutorWithTransaction( DbEnv environment, MySqlDbTransactionContext transaction )
        : base( environment, transaction.Connection.ConnectionString )
      {
        _transaction = transaction;
      }


      private MySqlDbTransactionContext _transaction;


      protected override IDbExecuteContext Execute( MySqlCommand command, IDbTracing tracing )
      {

        TryExecuteTracing( tracing, t => t.OnExecuting( command ) );

        command.Connection = _transaction.Connection;
        command.Transaction = _transaction.Transaction;

        if ( Configuration.QueryExecutingTimeout.HasValue )
          command.CommandTimeout = (int) Configuration.QueryExecutingTimeout.Value.TotalSeconds;


        var dataReader = command.ExecuteReader();
        var context = new MySqlExecuteContext( _transaction, dataReader, tracing );

        TryExecuteTracing( tracing, t => t.OnLoadingData( context ) );

        return context;
      }

    }
  }
}
