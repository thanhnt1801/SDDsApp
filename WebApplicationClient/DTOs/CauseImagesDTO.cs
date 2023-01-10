using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using WebApplicationClient.Models;

namespace WebApplicationClient.DTOs
{
    public class CauseImagesDTO
    {
        public List<IFormFile> Images { get; set; }
        public Cause Cause { get; set; }
    }
}
