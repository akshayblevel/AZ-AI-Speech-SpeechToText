using AZ_AI_Speech_SpeechToText.Interfaces;
using AZ_AI_Speech_SpeechToText.Models;
using Microsoft.AspNetCore.Mvc;

namespace AZ_AI_Speech_SpeechToText.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpeechController(ISpeechToTextService speechToTextService) : ControllerBase
    {
        [HttpPost("ToText")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ConvertToText([FromForm] UploadFileModel model)
        {
            var result = await speechToTextService.SpeechToTextAsync(model.File);
            return Ok(result);
        }
    }
}
