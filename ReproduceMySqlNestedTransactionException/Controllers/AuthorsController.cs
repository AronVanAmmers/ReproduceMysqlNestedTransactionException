﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using ReproduceMySqlNestedTransactionException.Models;

namespace ReproduceMySqlNestedTransactionException.Controllers
{
    public class AuthorsController : ApiController
    {
        private ReproduceMySqlNestedTransactionExceptionContext db = new ReproduceMySqlNestedTransactionExceptionContext();

        // POST: api/Authors
        [ResponseType(typeof(Author))]
        public IHttpActionResult PostAuthor(Author author)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Authors.Add(author);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = author.Id }, author);
        }
    }
}