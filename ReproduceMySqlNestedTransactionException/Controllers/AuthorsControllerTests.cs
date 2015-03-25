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
        public void PostSameAuthor()
        {
            var authorCon = new AuthorsController();

            string uniqueName = "An amazing author " + GetRandomInt();

            var author = new Author
            {
                Name = uniqueName
            };

            // Send a POST with this data. This request succeeds.
            authorCon.PostAuthor(author);

            // Now send several more POST calls with the same data. These should all result in an exception with
            // 'Duplicate entry'.
            // The second call gives the correct exception: duplicate values for column "Name" are not allowed.

            // A third attempt should give the same results. However on a MySQL 5.6 Windows instance it doesn't: 
            // it throws an exception with an inner MySqlException with message "Nested transactions are not 
            // supported."

            // On a different MySQL server (AWS RDS micro instance) the issue occured systematically after 4
            // attempts.

            for (int i = 2; i <= 20; i++)
                PostAndExpectDuplicateException(authorCon, author, i);
        }

        /// <summary>
        /// Post the given Author to the given AuthorsController, and expect an exception for a duplicate entry.
        /// </summary>
        /// <param name="authorCon"></param>
        /// <param name="author"></param>
        private static void PostAndExpectDuplicateException(AuthorsController authorCon, Author author, int callNumber)
        {
            try
            {
                authorCon.PostAuthor(author);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.InnerException != null
                    && ex.InnerException.InnerException != null
                    && ex.InnerException.InnerException.Message.Contains("Duplicate entry"), "Exception with 'Duplicate entry' is thrown on call number " + callNumber + ".");
            }
        }

        private static int GetRandomInt()
        {
            var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var randomBytes = new byte[8];
            rng.GetBytes(randomBytes);
            return BitConverter.ToInt32(randomBytes, 0);
        }
    }
}
