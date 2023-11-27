using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Data;

namespace WishListAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WishController : ControllerBase
    {
        private IConfiguration _configuration;

        public WishController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("GetWishes")]
        public JsonResult GetWishes()
        {
            string query = "SELECT * FROM wish ORDER BY wishid";
            DataTable table = new DataTable();
            string sqlDatasource = _configuration.GetConnectionString("wishListDBCon");

            using (NpgsqlConnection connection = new NpgsqlConnection(sqlDatasource))
            {
                connection.Open();
                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(query, connection))
                {
                    adapter.Fill(table);
                }
                connection.Close();
            }

            return new JsonResult(table);
        }

        [HttpPost]
        [Route("AddWish")]
        public JsonResult AddWish(string wishText) 
        {
            string query = "INSERT INTO wish (wishtext) VALUES (@wishText) RETURNING wishid";
            string sqlDatasource = _configuration.GetConnectionString("wishListDBCon");

            using (NpgsqlConnection connection = new NpgsqlConnection(sqlDatasource))
            {
                connection.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@wishtext", wishText);
                    var wishId = cmd.ExecuteScalar();
                    if (wishId != null)
                    {
                        connection.Close();
                        return new JsonResult(wishId);
                    }
                }
                connection.Close();
            }

            return new JsonResult("Something went wrong.");
        }

        [HttpPut]
        [Route("UpdateIsComplete/{wishId}")]
        public JsonResult UpdateIsComplete(int wishId)
        {
            string query = "UPDATE wish SET iscomplete = NOT iscomplete WHERE wishid = @wishId";
            string sqlDatasource = _configuration.GetConnectionString("wishListDBCon");

            using (NpgsqlConnection connection = new NpgsqlConnection(sqlDatasource))
            {
                connection.Open();

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@wishid", wishId);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return new JsonResult("isComplete value updated successfully.");
                    }
                    else
                    {
                        return new JsonResult("Wish not found or could not update isComplete value.");
                    }
                }
            }
        }


        [HttpDelete]
        [Route("DeleteWish/{wishId}")]
        public JsonResult DeleteWish(int wishId)
        {
            string query = "DELETE FROM wish WHERE wishid = @wishId";
            string sqlDatasource = _configuration.GetConnectionString("wishListDBCon");

            using (NpgsqlConnection connection = new NpgsqlConnection(sqlDatasource))
            {
                connection.Open();

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@wishid", wishId);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return new JsonResult("Wish deleted successfully.");
                    }
                    else
                    {
                        return new JsonResult("Wish not found or could not be deleted.");
                    }
                }
            }
        }

    }
}