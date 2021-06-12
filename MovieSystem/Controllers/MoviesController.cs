using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieSystem.Data;
using MovieSystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace MovieSystem.Controllers
{
    [Authorize]
    public class MoviesController : Controller
    {
        private readonly MvcMovieContext _context;

        private const string accessKey = "AKIAVGK3U2NUBRNMBIPA";
        private const string secretKey = "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";

        public MoviesController(MvcMovieContext context)
        {
            _context = context;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            //Task t = CreateDynamoDb();
            //t.Wait();

            return View(await _context.Movie.ToListAsync());
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //var records = await GetCommentRecords(id);
            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(new MovieCommentViewModel
            {
                comments = null,
                movie = movie
            });
        }

        public ActionResult Comment()
        {
            return View();
        }

        public async Task<IActionResult> Rating(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        [HttpPost]
        public async Task<IActionResult> Rating(int id, [Bind("Id,Title,ReleaseDate,Genre,Price,Customer,Rating")] Movie movie)
        {
            _context.Update(movie);
            await _context.SaveChangesAsync();

            var records = await GetCommentRecords(id);
            if (movie == null)
            {
                return NotFound();
            }

            return View("Details", new MovieCommentViewModel
            {
                comments = records,
                movie = movie
            });
        }


        public async Task<IEnumerable<Comment>> GetCommentRecords(int? id)
        {
            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            var client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast2);
            var context = new DynamoDBContext(client);

            /*
            var conditions = new List<ScanCondition>();
            // you can add scan conditions, or leave empty
            var allDocs = await context.ScanAsync<Comment>(conditions).GetRemainingAsync();
            */

            var conditions = new List<ScanCondition>();
            conditions.Add(new ScanCondition("MovieId", ScanOperator.Equal, id.ToString()));
            var allDocs = await context.ScanAsync<Comment>(conditions).GetRemainingAsync();

            return allDocs;
        }

        [HttpPost]
        public async Task<IActionResult> Comment(Comment comment, int? id)
        {
            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            var client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast2);

            if (ModelState.IsValid)
            {
                var item = new Dictionary<string, AttributeValue>
                {
                    {"Id", new AttributeValue{S = Guid.NewGuid().ToString()} },
                    {"Description", new AttributeValue{S = comment.Description} },
                    {"MovieId", new AttributeValue{S = $"{id}" } }
             };

                PutItemRequest request = new PutItemRequest
                {
                    TableName = "Comment",
                    Item = item
                };

                await client.PutItemAsync(request);
            }


            var records = await GetCommentRecords(id);
            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View("Details", new MovieCommentViewModel
            {
                comments = records,
                movie = movie
            });
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View(new Movie
            {
                Genre = "Action",
                Price = 1.99M,
                ReleaseDate = DateTime.Now,
                Title = "Conan",
                Customer = User.Identity.Name
            });
        }

        // POST: Movies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,ReleaseDate,Genre,Price,Customer")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ReleaseDate,Genre,Price,Customer,Rating")] Movie movie)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movie.FindAsync(id);
            _context.Movie.Remove(movie);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.Id == id);
        }

        public static async Task CreateDynamoDb()
        {
            string tableName = "Comment";
            string hashKey = "Id";

            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            var client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast2);

            var tableResponse = await client.ListTablesAsync();
            if (!tableResponse.TableNames.Contains(tableName))
            {
                await client.CreateTableAsync(new CreateTableRequest
                {
                    TableName = tableName,
                    ProvisionedThroughput = new ProvisionedThroughput
                    {
                        ReadCapacityUnits = 3,
                        WriteCapacityUnits = 1
                    },
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement
                        {
                            AttributeName = hashKey,
                            KeyType = KeyType.HASH
                        }
                    },
                    AttributeDefinitions = new List<AttributeDefinition>
                    {
                        new AttributeDefinition { AttributeName = hashKey, AttributeType=ScalarAttributeType.S }
                    }
                });

                bool isTableAvailable = false;
                while (!isTableAvailable)
                {
                    var tableStatus = await client.DescribeTableAsync(tableName);
                    isTableAvailable = tableStatus.Table.TableStatus == "ACTIVE";
                }
            }
        }
    }
}