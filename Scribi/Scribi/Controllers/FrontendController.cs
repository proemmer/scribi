﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Scribi.Interfaces;

namespace Scribi.Controllers
{
    [Route("api/[controller]")]
    public class FrontendController : Controller
    {
        private readonly IScriptFactoryService _scriptFactory;

        public FrontendController(IScriptFactoryService scriptFactory)
        {
            _scriptFactory = scriptFactory;
        }


        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return _scriptFactory.GetScriptNames();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public Model.ScriptMetaData Get(string script)
        {
            return _scriptFactory.GetScriptMetaInfo(script);
        }

        //// POST api/values
        //[HttpPost]
        //public void Post([FromBody]string value)
        //{
        //}

        //// PUT api/values/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE api/values/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
