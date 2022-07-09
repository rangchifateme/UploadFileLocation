using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using UploadFileLocation.Models;
using CrossPlatformLibrary.Geolocation;
using Microsoft.Practices.ServiceLocation;
using RestSharp;
using System.Net;
using File = UploadFileLocation.Models.File;
using System.Data.SqlClient;
using System.Data;

namespace UploadFileLocation.Controllers
{

    public class FileLocationInput
    {

        public HttpPostedFileBase File { get; set; }
        public string longitude { get; set; }
    }
    public class FileController : Controller
    {
        public static string cityName = "";
        public static int lastLocationId = 0;
        public static int currentLocationId = 0;

        SqlConnection sconn = new SqlConnection("Data Source=.;Initial Catalog=DatabaseFileLocation;User ID=sa;Password=***");
        [HttpGet]
        public ActionResult UploadFile()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase File)
        {
            string fileName = Path.GetFileNameWithoutExtension(File.FileName);
            string extension = Path.GetExtension(File.FileName);
            fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
            string path = "~/Files/" + cityName;
            
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(Server.MapPath(path));
            }
            fileName = Path.Combine(Server.MapPath("~/Files/" + cityName), fileName);
            File.SaveAs(fileName);
            File files = new File
            {
                Fk_LocationId = currentLocationId,
                Filename = File.FileName,
                //CurrentFile = File,
                FilePath = fileName,
                FileType = (File.FileName.Split('.'))[1]
            };
            using (DatabaseFileLocationEntities6 db = new DatabaseFileLocationEntities6())
            {
                db.Files.Add(files);
                db.SaveChanges();
            }
            ModelState.Clear();
            return View();
        }
        [HttpPost]
        public ActionResult Addcity(string cityNameInput)
        {
            try
            {
                SqlCommand command;
                command = new SqlCommand("select * from Location where LocationCity='" + cityNameInput + "'", sconn);
                SqlDataAdapter da = new SqlDataAdapter(command);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    currentLocationId = Convert.ToInt32(dt.Rows[0][0]);
                    cityName = cityNameInput;
                }
                else
                {
                    if (lastLocationId == 0)
                    {
                        command = new SqlCommand("select TOP 1 LocationId from Location ORDER BY LocationId DESC", sconn);
                        sconn.Open();
                        lastLocationId = command.ExecuteNonQuery();
                        if (lastLocationId < 0)
                        {
                            lastLocationId = 0;
                        }
                    }
                    lastLocationId++;
                    sconn.Close();
                    Location location = new Location()
                    {
                        Latitude = "",
                        Longitude = "",
                        LocationCity = cityName,
                        LocationId = lastLocationId,
                    };
                    command = new SqlCommand("INSERT INTO Location(LocationId, LocationCity, Longitude, Latitude) VALUES(" + location.LocationId + ", '" + location.LocationCity + "', '" + location.Longitude + "', '" + location.Latitude + "'); ", sconn);
                    sconn.Open();
                    command.ExecuteNonQuery();
                    sconn.Close();
                }
                return Json(true);
            }
            catch
            {
                return Json(false);
            }
        }

        [HttpPost]
        public ActionResult GetCurrentLocation(string LatAndLong)
        {
            //return Json(false); //just for test
            try {

                var client = new RestClient
                {
                    Timeout = 1500000,
                    BaseUrl = new System.Uri("https://maps.googleapis.com")
                };

                //The key is unique for google maps
                var a = "https://maps.googleapis.com/maps/api/geocode/json?latlng=" + LatAndLong + "=true&key=";
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
                var request = new RestRequest
                {
                    Resource = "https://maps.googleapis.com/maps/api/geocode/json?latlng=" + LatAndLong + "&sensor==true&key=my personal key",
                    Method = Method.GET,
                    RequestFormat = DataFormat.Json,
                };
                request.AddHeader("Content-Type", "application/json");
                //     RestResponse<Root> response = (RestResponse<Root>)client.Execute<Root>(request);

                var result = client.Execute(request);
                Root googleMapResult = new Root();
                googleMapResult = Newtonsoft.Json.JsonConvert.DeserializeObject<Root>(result.Content);
                foreach (var addressComponent in googleMapResult.results[1].address_components)
                {
                    if (addressComponent.types[0] == "locality")
                    {
                        cityName = addressComponent.long_name;
                        break;
                    }
                }

                SqlCommand command;
                command = new SqlCommand("select * from Location where LocationCity='" + cityName + "'", sconn);
                SqlDataAdapter da = new SqlDataAdapter(command);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    currentLocationId = Convert.ToInt32(dt.Rows[0][0]);
                }
                else
                {
                    if (lastLocationId == 0)
                    {
                        command = new SqlCommand("select TOP 1 LocationId from Location ORDER BY LocationId DESC", sconn);
                        sconn.Open();
                        lastLocationId = command.ExecuteNonQuery();
                        if (lastLocationId < 0)
                        {
                            lastLocationId = 0;
                        }
                    }
                    lastLocationId++;
                    sconn.Close();
                    Location location = new Location()
                    {
                        Latitude = LatAndLong.Split(',')[0],
                        Longitude = LatAndLong.Split(',')[1],
                        LocationCity = cityName,
                        LocationId = lastLocationId,
                    };
                    command = new SqlCommand("INSERT INTO Location(LocationId, LocationCity, Longitude, Latitude) VALUES(" + location.LocationId + ", '" + location.LocationCity + "', '" + location.Longitude + "', '" + location.Latitude + "'); ", sconn);
                    sconn.Open();
                    command.ExecuteNonQuery();
                    sconn.Close();
                   
                }
                return Json(true);
            }
            catch {
                return Json(false);
            }
        }

        [HttpPost]
        public ActionResult GetSubDirectories()

        {

            string root = @"D:\IOT project\UploadFileLocation\UploadFileLocation\Files";

            // Get all subdirectories

            string[] subdirectoryEntries = Directory.GetDirectories(root);
            List<string> directorynames = new List<string>();
            foreach(var directoryname in subdirectoryEntries)
            {
                var pathname = directoryname.Split('\\').Last();
                directorynames.Add(pathname);
            }

            // Loop through them to see if they have any other subdirectories

            return Json(directorynames);

        }

        [HttpPost]
        public ActionResult GetFileNames(string cityName)
        {
            string root = @"D:\IOT project\UploadFileLocation\UploadFileLocation\Files\"+cityName;
            string[] uploadedFiles = Directory.GetFiles(root);
            List<string> FileNames = new List<string>();
            foreach (var fileName in uploadedFiles)
            {
                var pathname = fileName.Split('\\').Last();
                FileNames.Add(pathname);
            }
            return Json(FileNames);
        }
    }
}