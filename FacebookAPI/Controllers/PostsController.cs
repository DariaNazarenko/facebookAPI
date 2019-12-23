using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FacebookAPI.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Newtonsoft.Json.Linq;


namespace FacebookAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly FacebookAPIContext _context;
        FacebookWork facebook = new FacebookWork("EAAVm7uqfSPcBAIcqfOxiHcr8kSyfBhXXyp1eCvQ3OnpRZBcyJQxD4R50J2Tk2o9O3gBFVDhrZBiWs9GblL602t0GipWZA3ijxbVIoliXQZBHO8vPVsgkxzqgQNIf3LLYx9sfSDUjvafVcB5sYXlJQ07CwYZBj6tp2brWZC5hyspgZDZD", "111319367001521");
        public PostsController(FacebookAPIContext context)
        {
            _context = context;
        }

        // GET: api/Posts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetPost()
        {
            return await _context.Post.Where(p=>p.Deletion==false).ToListAsync();
        }

        // GET: api/Posts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> GetPost(int id)
        {
            var post = await _context.Post.FindAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            return post;
        }

        // PUT: api/Posts/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPost([FromBody]Post modelPost)
        {
            var model = await _context.Post.FirstOrDefaultAsync(e => e.ID == modelPost.ID);
            if (model == null)
            {
                return new JsonResult(400);
            }

            var res = facebook.PutPost(modelPost.PostID, modelPost.Text);

            if (res.Result.Item1 == 200)
            {
                model.Text = modelPost.Text;
                modelPost.Modification = DateTime.Now;
                _context.SaveChangesAsync();
                return new JsonResult(res.Result.Item1);
            }
            return new JsonResult(res.Result.Item1);

        }

        // POST: api/Posts
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<Post>> PostPost([FromBody]Post modelPost)
        {
            if (modelPost.Picture == null)
            {
                var res = facebook.PublishSimplePost(modelPost.Text);
                if (res.Result.Item1 == 200)
                {
                    var model = modelPost;
                    model.Creation = DateTime.Now;
                    var json = JObject.Parse(res.Result.Item2);

                    model.PostID = json["id"].ToString();
                    _context.Post.Add(modelPost);
                    await _context.SaveChangesAsync();
                    return new JsonResult(200);
                }
            }
            else if (modelPost.Picture.Contains("https"))
            {
                var res = facebook.PublishToFacebook(modelPost.Text, modelPost.Picture);
                if (res.Result.Item1 == 200)
                {
                    var model = modelPost;
                    model.Creation = DateTime.Now;

                    model.PostID = res.Result.Item2;
                    _context.Post.Add(modelPost);
                    await _context.SaveChangesAsync();
                    return new JsonResult(200);
                }
            }
            //else
            //{
            //    var res = facebook.PostImageFromPC(modelPost.Text, modelPost.Picture);
            //    if (res)
            //    {
            //        var model = modelPost;
            //        model.Creation = DateTime.Now;

            //        //model.PostID = res.Result.Item2;
            //        _context.Post.Add(modelPost);
            //        await _context.SaveChangesAsync();
            //        return new JsonResult(200);
            //    }
            //}
            return new JsonResult(400);

        }

        // DELETE: api/Posts/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Post>> DeletePost([FromBody]Post modelPost)
        {
            var model = await _context.Post.FirstOrDefaultAsync(e => e.PostID == modelPost.PostID);
            if (model == null)
            {
                return new JsonResult(400);
            }

            var res = facebook.DeletePost(model.PostID);
            if (res.Result.Item1 == 200)
            {
                model.Deletion = true;
                await _context.SaveChangesAsync();
                return new JsonResult(res.Result.Item1);
            }
            return new JsonResult(res.Result.Item1);
        }

        private bool PostExists(int id)
        {
            return _context.Post.Any(e => e.ID == id);
        }
    }
}
