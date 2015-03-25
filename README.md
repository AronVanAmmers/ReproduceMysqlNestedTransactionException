# Reproduce MySQL "nested transactions" exception
This example project shows how to reproduce an exception regarding "nested transactions" in a simple .NET Web API + Entity Framework 6 + MySQL project.

**Update**: the cause of the issue is this bug in MySQL .NET connector: http://bugs.mysql.com/bug.php?id=71502. It can be reproduced without Entity Framework / Web API.

## Description
The project uses .NET Web API, Entity Framework 6 with code-first migrations and the MySQL Entity Framework connector. It contains a single entity `Author` with a column `Name` which has a unique constraint. 

When repeatedly inserting an entity with the same value for `Name`, I would expect to get an exception that the unique constraint is violated every time. However in my experience every second attempt a MySqlException with message "Nested transactions are not supported." is thrown.

## Prerequisites
* Microsoft Visual Studio 2013 (tested on Community edition)
* A MySQL server. Tested on:
 * 5.6.23-log Community / Win64 (local machine)
 * 5.6.22-log Community / Linux (x86_64) (Amazon RDS micro instance)

## Instructions

### Clone and build

1. Clone the repository in a new folder.
1. Open the solution in Visual Studio 2013.
1. Build. VS will download the missing NuGet packages on first build.

### Prepare database

1. Create a database and user on your MySQL server. 
2. Adapt `Web.config` for your MySQL connection string, here:

  ```xml
  <connectionStrings>
    <add name="ReproduceMySqlNestedTransactionExceptionContext" providerName="MySql.Data.MySqlClient" connectionString="server=localhost;port=3306;database=reproduce_nested_transactions_error;uid=reproduce_user;password=test1234" />
  </connectionStrings>
  ```

3. In the Package Manager Console of Visual Studio, run `Update-Database`. The database structure will be created from the migrations.

### Run

Run the test "PostSameAuthor" from the Test Explorer. This test repeatedly calls `Post()` with an `Author` with the same name, and expects an exception with message "Duplicate entry" from the second call onwards.

## Output

The test fails. The 2nd call gives the expected result, but at the 3rd call a `MySqlException` with message "Nested transactions are not supported." is thrown.

![Test output](/test-output.png "Test output")

Details of the exception can be seen by debugging the test instead of running it (Test Explorer -> Debug Selected Tests). Stack trace below.

```
System.Data.Entity.Core.EntityException: An error occurred while starting a transaction on the provider connection. See the inner exception for details. ---> System.InvalidOperationException: Nested transactions are not supported.
   at MySql.Data.MySqlClient.ExceptionInterceptor.Throw(Exception exception)
   at MySql.Data.MySqlClient.MySqlConnection.Throw(Exception ex)
   at MySql.Data.MySqlClient.MySqlConnection.BeginTransaction(IsolationLevel iso)
   at MySql.Data.MySqlClient.MySqlConnection.BeginTransaction()
   at MySql.Data.MySqlClient.MySqlConnection.BeginDbTransaction(IsolationLevel isolationLevel)
   at System.Data.Common.DbConnection.BeginTransaction(IsolationLevel isolationLevel)
   at System.Data.Entity.Infrastructure.Interception.DbConnectionDispatcher.<BeginTransaction>b__0(DbConnection t, BeginTransactionInterceptionContext c)
   at System.Data.Entity.Infrastructure.Interception.InternalDispatcher`1.Dispatch[TTarget,TInterceptionContext,TResult](TTarget target, Func`3 operation, TInterceptionContext interceptionContext, Action`3 executing, Action`3 executed)
   at System.Data.Entity.Infrastructure.Interception.DbConnectionDispatcher.BeginTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext)
   at System.Data.Entity.Core.EntityClient.EntityConnection.<>c__DisplayClassf.<BeginDbTransaction>b__d()
   at System.Data.Entity.Infrastructure.DefaultExecutionStrategy.Execute[TResult](Func`1 operation)
   at System.Data.Entity.Core.EntityClient.EntityConnection.BeginDbTransaction(IsolationLevel isolationLevel)
   --- End of inner exception stack trace ---
   at System.Data.Entity.Core.EntityClient.EntityConnection.BeginDbTransaction(IsolationLevel isolationLevel)
   at System.Data.Common.DbConnection.BeginTransaction()
   at System.Data.Entity.Core.EntityClient.EntityConnection.BeginTransaction()
   at System.Data.Entity.Core.Objects.ObjectContext.ExecuteInTransaction[T](Func`1 func, IDbExecutionStrategy executionStrategy, Boolean startLocalTransaction, Boolean releaseConnectionOnSuccess)
   at System.Data.Entity.Core.Objects.ObjectContext.SaveChangesToStore(SaveOptions options, IDbExecutionStrategy executionStrategy, Boolean startLocalTransaction)
   at System.Data.Entity.Core.Objects.ObjectContext.<>c__DisplayClass2a.<SaveChangesInternal>b__27()
   at System.Data.Entity.Infrastructure.DefaultExecutionStrategy.Execute[TResult](Func`1 operation)
   at System.Data.Entity.Core.Objects.ObjectContext.SaveChangesInternal(SaveOptions options, Boolean executeInExistingTransaction)
   at System.Data.Entity.Core.Objects.ObjectContext.SaveChanges(SaveOptions options)
   at System.Data.Entity.Internal.InternalContext.SaveChanges()
   at System.Data.Entity.Internal.LazyInternalContext.SaveChanges()
   at System.Data.Entity.DbContext.SaveChanges()
   at ReproduceMySqlNestedTransactionException.Controllers.AuthorsController.PostAuthor(Author author) in ReproduceMysqlNestedTransactionException\ReproduceMySqlNestedTransactionException\Controllers\AuthorsController.cs:line 29
   at ReproduceMySqlNestedTransactionException.Controllers.AuthorsControllerTests.PostAndExpectDuplicateException(AuthorsController authorCon, Author author, Int32 callNumber) in ReproduceMysqlNestedTransactionException\ReproduceMySqlNestedTransactionException\Controllers\AuthorsControllerTests.cs:line 53
```
