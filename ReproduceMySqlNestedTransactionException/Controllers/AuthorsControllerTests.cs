using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReproduceMySqlNestedTransactionException.Controllers;
using ReproduceMySqlNestedTransactionException.Models;

namespace ReproduceMySqlNestedTransactionException.Controllers
{
    [TestClass]
    public class AuthorsControllerTests
    {
        [TestMethod]
        public void Post()
        {
            var authorCon = new AuthorsController();

            PostSameAuthor(authorCon.PostAuthor);
        }

        [TestMethod]
        public void PostWithTransactionScope()
        {
            var authorCon = new AuthorsController();

            PostSameAuthor(authorCon.PostAuthorWithTransactionScope);
        }

        public void PostSameAuthor(AuthorPostMethod authorPostMethod)
        {
            // Generate a unique value for this test session, to be sure that previous test data doesn't cause
            // conflicts.
            string uniqueName = "An amazing author " + Guid.NewGuid().ToString();

            var author = new Author
            {
                Name = uniqueName
            };

            // Send a POST with this data. This request succeeds.
            authorPostMethod(author);

            // Now send several more POST calls with the same data. These should all result in an exception with
            // 'Duplicate entry'.
            // The second call with the same data gives the correct exception: duplicate values for column "Name" 
            // are not allowed.

            // Further calls should give the same results. However on a MySQL 5.6 Windows instance it doesn't: 
            // it throws an exception with an inner MySqlException with message "Nested transactions are not 
            // supported."

            // On a different MySQL server (AWS RDS micro instance) the issue occured systematically at the 6th 
            // call.

            for (int i = 2; i <= 20; i++)
                PostAndExpectDuplicateException(authorPostMethod, author, i);
        }

        /// <summary>
        /// Post the given Author to the given AuthorsController, and expect an exception for a duplicate entry.
        /// </summary>
        /// <param name="authorCon"></param>
        /// <param name="author"></param>
        private static void PostAndExpectDuplicateException(AuthorPostMethod authorPostMethod, Author author, int callNumber)
        {
            try
            {
                authorPostMethod(author);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.InnerException != null
                    && ex.InnerException.InnerException != null
                    && ex.InnerException.InnerException.Message.Contains(
                    "Duplicate entry"), "Exception with 'Duplicate entry' is thrown on call number " + callNumber + ".");
            }
        }
    }
}
