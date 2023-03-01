using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace WebApplicationClient.DTOs
{
    public class FastAPIImageDTO
    {
        public IFormFile Image { get; set; }
    }
}
