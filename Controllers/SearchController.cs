using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StackOverflow.Data;
using StackOverflow.Models;

namespace StackOverflow.Controllers
{
    public class SearchController : Controller
    {
        private readonly AppDbContext _context;

        private readonly ILogger<SearchController> _logger;

        public SearchController(AppDbContext context, ILogger<SearchController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult _SearchResults(string searchString, int page = 1)
        {
            List<SearchResultViewModel> searchResults = new List<SearchResultViewModel>();
            string conString = "Data Source=localhost;Initial Catalog=StackOverflow2010;Integrated Security=True;Trust Server Certificate=True";

            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                {
                    con.Open();
                    string sql = @"
                                    SELECT 
                        p.Id AS PostId, 
                        p.Title, 
                        LEFT(p.Body, 140) AS Description, 
                        p.Score AS Votes, 
                        p.AnswerCount, 
                        u.Id AS UserId, 
                        u.DisplayName AS UserName, 
                        u.Reputation, 
                        COUNT(b.Id) AS Badges
                    FROM (select * from Posts p WHERE p.Title LIKE @searchString OR p.Tags LIKE @searchString AND (p.PostTypeId IN (select pt.Id from PostTypes AS pt WHERE pt.Type IN ('Question','Answer'))) ORDER BY p.Id OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY) as p
                    LEFT JOIN Users u ON p.OwnerUserId = u.Id
                    LEFT JOIN Badges b ON u.Id = b.UserId
                    GROUP BY p.Id, p.Title, p.Body, p.Score, p.AnswerCount, u.Id, u.DisplayName, u.Reputation
                    ORDER BY p.Score DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@searchString", $"%{searchString}%");
                        cmd.Parameters.AddWithValue("@offset", (page - 1) * 10);
                        cmd.Parameters.AddWithValue("@pageSize", 10);

                        SqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            SearchResultViewModel result = new SearchResultViewModel
                            {
                                PostId = reader.GetInt32(reader.GetOrdinal("PostId")),
                                Title = reader.GetString(reader.GetOrdinal("Title")),
                                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? string.Empty : reader.GetString(reader.GetOrdinal("Description")),
                                Votes = reader.GetInt32(reader.GetOrdinal("Votes")),
                                AnswerCount = reader.GetInt32(reader.GetOrdinal("AnswerCount")),
                                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                UserName = reader.GetString(reader.GetOrdinal("UserName")),
                                Reputation = reader.GetInt32(reader.GetOrdinal("Reputation")),
                                Badges = reader.GetInt32(reader.GetOrdinal("Badges"))
                            };

                            searchResults.Add(result);
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing the database query.");
                // Handle the exception, log it, or return an error response
            }


            //var model = _context.Posts
            //    .Where(p => ((p.Title != null && EF.Functions.Like(p.Title, $"%{searchString}%")) ||
            //                 (p.Tags != null && EF.Functions.Like(p.Tags, $"%{searchString}%"))))
            //    .Skip((page - 1) * 10)
            //    .Take(10)
            //    .ToList();

            ViewData["PageNumber"] = page + 1;
            ViewData["SearchString"] = searchString;
            return PartialView("_SearchResults", searchResults);
        }
    }
}
